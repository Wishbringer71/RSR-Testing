using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.ExcelServices;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System.Collections.Concurrent;

namespace RotationSolver.Engine.Helpers;

public static partial class ObjectHelper
{
	// A788, A983 (concept 11, "Ein Toter in Bewegung, einmal je Bild"): the answer is kept for the sample, so every caller
	// in a frame gets the same one; the sample is a fixed time, so the distance threshold does not depend on the frame rate.
	private const long MovingSampleIntervalMs = 75;
	private static readonly Dictionary<ulong, (long Tick, Vector3 Pos, bool IsMoving)> LastPositions = [];

	internal static bool IsTargetMoving(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		if (Svc.Condition[ConditionFlag.BetweenAreas] || LastPositions.Count > 4096)
		{
			LastPositions.Clear();
		}

		var id = battleChara.GameObjectId;
		var currentPos = battleChara.Position;
		var now = Environment.TickCount64;
		if (LastPositions.TryGetValue(id, out var entry))
		{
			if (now - entry.Tick < MovingSampleIntervalMs)
			{
				return entry.IsMoving;
			}

			var isMoving = Vector3.Distance(currentPos, entry.Pos) > 0.01f;
			LastPositions[id] = (now, currentPos, isMoving);
			return isMoving;
		}
		else
		{
			LastPositions[id] = (now, currentPos, false);
			return false;
		}
	}

	/// <summary>
	/// Calculates the estimated time to kill the specified battle character from its damage rate over the last 30 seconds. Known once it has taken damage for about 2 seconds.
	/// A party member is read from the party's own health history instead (<see cref="DataCenter.RecordedHP"/>).
	/// </summary>
	/// <param name="battleChara">The battle character to calculate the time to kill for.</param>
	/// <param name="wholeTime">If set to <c>true</c>, calculates the total time to kill; otherwise, calculates the remaining time to kill.</param>
	/// <returns>
	/// The estimated time to kill the battle character in seconds, or <see cref="float.NaN"/> if the calculation cannot be performed.
	/// </returns>
	internal static float GetTTK(this IBattleChara battleChara, bool wholeTime = false)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		if (battleChara.IsDummy())
		{
			return 999.99f;
		}

		// A party member is read from the fork's own history - four minutes, net of every heal (A91), which the heal
		// forecast and its self-correction rest on. Upstream's tracker keeps hostiles only and starts over at every
		// rise in health, so for a member being healed it would hold nothing to read (A341).
		var id = battleChara.GameObjectId;
		if (DataCenter.RecordedPartyIds.Contains(id))
		{
			// Once per member and frame: the heal target selection asks it several times per member, and each answer
			// walks the whole recorded history (A325). Within a frame the history does not change, except where a
			// new sample is recorded, and that clears the cache (ForgetTimeToKill). Both readings come out of the
			// same walk, so both are kept.
			var now = Environment.TickCount64;
			if (_ttkCache.TryGetValue(id, out var cached) && now - cached.Tick < DataCenter.FrameCacheMs)
			{
				return wholeTime ? cached.Whole : cached.Remaining;
			}

			var whole = ComputeTTK(battleChara, out var remaining);
			_ttkCache[id] = (now, whole, remaining);
			return wholeTime ? whole : remaining;
		}

		var rate = TimeToKillTracker.GetRate(id);
		if (float.IsNaN(rate))
		{
			return float.NaN;
		}

		return wholeTime ? 1f / rate : battleChara.GetHealthRatio() / rate;
	}

	// The party history's own start-up: no time to death before a member has been losing health this long.
	// Upstream's former figure, kept with the history it belonged to (A341).
	private static readonly TimeSpan CheckSpan = TimeSpan.FromSeconds(2.5);

	/// <summary>
	/// Seconds until the next DoT tick on the target, or <see cref="float.NaN"/> until one of its ticks has been seen.
	/// </summary>
	/// <param name="battleChara">The target.</param>
	public static float TimeToNextDotTick(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		var now = Environment.TickCount64;
		return DotTickTracker.TryGetNextTick(battleChara.GameObjectId, now, out var next) ? (next - now) / 1000f : float.NaN;
	}

	/// <summary>
	/// DoT ticks on the target between two times in seconds from now. Exact once one of its ticks has been seen, otherwise one per 3 seconds on average.
	/// </summary>
	/// <param name="battleChara">The target.</param>
	/// <param name="fromSeconds">Start of the window; a tick exactly at this time is not counted.</param>
	/// <param name="toSeconds">End of the window; a tick exactly at this time is counted.</param>
	public static float DotTicksBetween(this IBattleChara battleChara, float fromSeconds, float toSeconds)
	{
		if (battleChara == null || !float.IsFinite(fromSeconds) || !float.IsFinite(toSeconds))
		{
			return float.NaN;
		}

		var now = Environment.TickCount64;
		return DotTickTracker.CountTicks(battleChara.GameObjectId, now + (long)(fromSeconds * 1000), now + (long)(toSeconds * 1000));
	}

	private static readonly ConcurrentDictionary<ulong, float> _ttkBias = [];

	// Three samples at the 1 Hz recording rate. A forecast older than that belongs to a character
	// the sampling loop no longer visits - out of the object table, or out of the party - and a
	// stale time to death is worse than none, because it would keep answering confidently.
	private static readonly TimeSpan ForecastFreshness = TimeSpan.FromSeconds(3);

	// Same frame cache as DataCenter's party HP statistics, and for the same reason: the heal target
	// selection asks this several times per member per frame, and each read of the GCD figures goes
	// to the action manager. One value per frame is as current as the question can be - read from the
	// shared figure instead of a copy of it (A340), in upstream's frame cache (A354).
	private static readonly FrameCached<float> _healLead =
		new(static () => DataCenter.DefaultGCDRemain + DataCenter.DefaultGCDTotal, DataCenter.FrameCacheMs);

	private static readonly TimeSpan TtkForecastLifetime = TimeSpan.FromSeconds(DataCenter.HP_RECORD_TIME);

	private static readonly ConcurrentDictionary<ulong, DateTime> _aliveStartTimes = [];
	private static readonly ConcurrentDictionary<ulong, DateTime> _deadStartTimes = [];

	internal static void ClearTrackingCaches()
	{
		LastPositions.Clear();
		_aliveStartTimes.Clear();
		_deadStartTimes.Clear();
		_healthRatioCache.Clear();
		_losCache.Clear();
	}

	internal static float TimeAlive(this IBattleChara? battleChara)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		if (battleChara.IsDead || Svc.Condition[ConditionFlag.BetweenAreas])
		{
			_ = _aliveStartTimes.TryRemove(battleChara.GameObjectId, out _);
			return 0;
		}

		if (_aliveStartTimes.Count > 4096)
		{
			_aliveStartTimes.Clear();
		}

		return SecondsSinceFirstSeen(_aliveStartTimes, battleChara.GameObjectId);
	}

	internal static float TimeDead(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.NaN;
		}

		if (!battleChara.IsDead || Svc.Condition[ConditionFlag.BetweenAreas])
		{
			_ = _deadStartTimes.TryRemove(battleChara.GameObjectId, out _);
			return 0;
		}

		if (_deadStartTimes.Count > 4096)
		{
			_deadStartTimes.Clear();
		}

		return SecondsSinceFirstSeen(_deadStartTimes, battleChara.GameObjectId);
	}

	private static float SecondsSinceFirstSeen(ConcurrentDictionary<ulong, DateTime> startTimes, ulong id)
	{
		var start = startTimes.GetOrAdd(id, static _ => DateTime.Now);
		return MathF.Min(30, (float)(DateTime.Now - start).TotalSeconds);
	}

	internal static bool IsEngagedByPlayer(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return false;
		}

		var now = DateTime.Now;
		var targetId = battleChara.GameObjectId;
		lock (DataCenter._attackedTargetsLock)
		{
			foreach ((var id, var time) in DataCenter.AttackedTargets)
			{
				if (id == targetId)
				{
					return now - time >= TimeSpan.FromSeconds(1);
				}
			}
		}
		return false;
	}

	private struct LosCacheEntry
	{
		public Vector3 PlayerPos;
		public Vector3 TargetPos;
		public long ExpiresAt;
		public bool Visible;
	}
	private static readonly ConcurrentDictionary<ulong, LosCacheEntry> _losCache = [];
	private const float LosPosEpsilonSq = 0.04f;
	private const long LosTtlMs = 33;

	private static void MaybeResetLosCache()
	{
		if (Svc.Condition[ConditionFlag.BetweenAreas] || _losCache.Count > 4096)
		{
			_losCache.Clear();

			// The game's answers hold only friendly targets, fewer than the ray's; they go with it (A621).
			_gameSightCache.Clear();
			_sightDisagreement.Clear();
		}
	}

	internal static unsafe bool CanSeeFrom(this IBattleChara battleChara, Vector3 playerEyePos, float targetYOffset = 2.0f)
	{
		if (battleChara == null || Player.Object == null)
		{
			return false;
		}

		var targetStruct = battleChara.Struct();
		if (targetStruct == null)
		{
			return false;
		}

		MaybeResetLosCache();

		var targetPos = battleChara.Position;
		targetPos.Y += targetYOffset;
		var id = battleChara.GameObjectId;
		var now = Environment.TickCount64;

		if (_losCache.TryGetValue(id, out var entry))
		{
			if (now <= entry.ExpiresAt &&
				Vector3.DistanceSquared(playerEyePos, entry.PlayerPos) <= LosPosEpsilonSq &&
				Vector3.DistanceSquared(targetPos, entry.TargetPos) <= LosPosEpsilonSq)
			{
				return entry.Visible;
			}
		}

		var offset = targetPos - playerEyePos;
		var maxDist = offset.Length();
		if (maxDist < 0.01f)
		{
			return true;
		}

		var visible = !Raycast(playerEyePos, offset / maxDist, maxDist, out _);
		_losCache[id] = new LosCacheEntry
		{
			PlayerPos = playerEyePos,
			TargetPos = targetPos,
			ExpiresAt = now + LosTtlMs,
			Visible = visible
		};
		return visible;
	}

	// The game's last clear answer on whether the player sees a target (in sight or blocked), as long as both stand still.
	private static readonly Dictionary<ulong, LosCacheEntry> _gameSightCache = [];

	// The targets on which the game's answer and RSR's ray disagree right now: a trace line marks where it begins.
	private static readonly HashSet<ulong> _sightDisagreement = [];
	private static unsafe bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 point)
	{
		point = default;
		var framework = Framework.Instance();
		if (framework == null)
		{
			return false;
		}

		var module = framework->BGCollisionModule;
		if (module == null)
		{
			return false;
		}

		RaycastHit hit;
		var materialFilter = stackalloc int[] { 0x4000, 0, 0x4000, 0 };
		if (!module->RaycastMaterialFilter(&hit, &origin, &direction, maxDistance, 1, materialFilter))
		{
			return false;
		}

		point = hit.Point;
		return true;
	}

	internal static Vector3 SnapToGround(Vector3 position, float above = 2.5f, float below = 6f)
	{
		var origin = position + (Vector3.UnitY * above);
		if (Raycast(origin, -Vector3.UnitY, above + below, out var ground))
		{
			position.Y = ground.Y;
		}

		return position;
	}

	internal static bool CanSeePosition(Vector3 position, float playerYOffset = 2.0f, float targetYOffset = 1.0f)
	{
		if (Player.Object == null)
		{
			return false;
		}

		var eye = Player.Object.Position + (Vector3.UnitY * playerYOffset);
		var offset = position + (Vector3.UnitY * targetYOffset) - eye;
		var maxDist = offset.Length();
		return maxDist < 0.01f || !Raycast(eye, offset / maxDist, maxDist, out _);
	}

	internal static bool CanSee(this IBattleChara battleChara, float playerYOffset = 2.0f, float targetYOffset = 2.0f)
	{
		if (battleChara == null || Player.Object == null)
		{
			return false;
		}

		var playerPos = Player.Object.Position;
		playerPos.Y += playerYOffset;
		return CanSeeFrom(battleChara, playerPos, targetYOffset);
	}

	/// <summary>
	/// Get the Player's current MP percentage.
	/// </summary>
	/// <returns></returns>
	public static float GetPlayerMPRatio()
	{
		if (Player.Object == null)
		{
			return 0;
		}

		if (Player.Object.MaxMp == 0)
		{
			return 0;
		}

		return (float)Player.Object.CurrentMp / Player.Object.MaxMp;
	}

	/// <summary>
	/// Get the Player's current HP percentage.
	/// </summary>
	/// <returns></returns>
	public static float GetPlayerHealthRatio()
	{
		if (Player.Object == null)
		{
			return 0;
		}

		if (DataCenter.RefinedHP.TryGetValue(Player.Object.GameObjectId, out var hp))
		{
			return hp;
		}

		if (Player.Object.MaxHp == 0)
		{
			return 0;
		}

		return (float)Player.Object.CurrentHp / Player.Object.MaxHp;
	}

	/// <summary>
	/// Get the <paramref name="battleChara"/>'s current HP percentage.
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float GetHealthRatio(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return 0;
		}

		if (!battleChara.IsValid())
		{
			return 0;
		}

		var id = battleChara.GameObjectId;
		var now = Environment.TickCount64;
		if (_healthRatioCache.TryGetValue(id, out var cached) && now - cached.Tick < HealthRatioTtlMs)
		{
			return cached.Ratio;
		}

		if (_healthRatioCache.Count > 512)
		{
			_healthRatioCache.Clear();
		}

		var ratio = ComputeHealthRatio(battleChara);
		_healthRatioCache[id] = (now, ratio);
		return ratio;
	}

	private const long HealthRatioTtlMs = 15;
	private static readonly Dictionary<ulong, (long Tick, float Ratio)> _healthRatioCache = [];

	private static float ComputeHealthRatio(IBattleChara battleChara)
	{
		if (battleChara.IsParty() && battleChara.DoomNeedHealing())
		{
			return 0.01f;
		}

		if (DataCenter.RefinedHP.TryGetValue(battleChara.GameObjectId, out var hp))
		{
			return hp;
		}

		try
		{
			return battleChara.MaxHp == 0 ? 0 : (float)battleChara.CurrentHp / battleChara.MaxHp;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	/// <summary>
	/// Determines the positional relationship of the player relative to the enemy.
	/// </summary>
	/// <param name="enemy">The enemy game object.</param>
	/// <returns>
	/// An <see cref="EnemyPositional"/> value indicating whether the player is in front, at the rear, or on the flank of the enemy.
	/// </returns>
	public static EnemyPositional FindEnemyPositional(this IBattleChara enemy)
	{
		if (enemy == null || !enemy.IsValid() || enemy.Address == nint.Zero || Player.Object == null)
		{
			return EnemyPositional.None;
		}

		var faceVec = enemy.GetFaceVector();

		var offset = Player.Object.Position - enemy.Position;
		var dir = new Vector2(offset.X, offset.Z);
		if (dir.LengthSquared() < 0.0001f)
		{
			return EnemyPositional.None;
		}

		var dotProduct = Vector2.Dot(Vector2.Normalize(dir), new Vector2(faceVec.X, faceVec.Z));

		const float sectorCos = 0.70710677f;

		if (dotProduct > sectorCos)
		{
			return EnemyPositional.Front;
		}
		else if (dotProduct < -sectorCos)
		{
			return EnemyPositional.Rear;
		}

		return EnemyPositional.Flank;
	}

	internal static Vector3 GetFaceVector(this IBattleChara battleChara)
	{
		if (battleChara == null || !battleChara.IsValid() || battleChara.Address == nint.Zero)
		{
			return Vector3.Zero;
		}

		var rotation = battleChara.Rotation;
		return new Vector3((float)Math.Sin(rotation), 0, (float)Math.Cos(rotation));
	}

	internal static double AngleTo(this Vector3 vec1, Vector3 vec2)
	{
		double lengthProduct = vec1.Length() * vec2.Length();
		if (lengthProduct == 0)
		{
			return 0;
		}

		double dotProduct = Vector3.Dot(vec1, vec2);
		return Math.Acos(Math.Clamp(dotProduct / lengthProduct, -1.0, 1.0));
	}

	/// <summary>
	/// The distance from <paramref name="battleChara"/> to the player
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float DistanceToPlayer(this IBattleChara battleChara)
	{
		if (battleChara == null || Player.Object == null)
		{
			return float.MaxValue;
		}

		var playerPos = Player.Object.Position;
		var targetPos = battleChara.Position;

		var dy = MathF.Abs(targetPos.Y - playerPos.Y);
		if (dy > 30f)
		{
			return dy;
		}

		var dx = targetPos.X - playerPos.X;
		var dz = targetPos.Z - playerPos.Z;
		var distance = MathF.Sqrt(dx * dx + dz * dz) - (Player.Object.HitboxRadius + battleChara.HitboxRadius);
		return MathF.Max(0f, distance);
	}

	/// <summary>
	/// The distance from <paramref name="battleChara"/> to the player's pet
	/// </summary>
	/// <param name="battleChara"></param>
	/// <returns></returns>
	public static float DistanceToPet(IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return float.MaxValue;
		}
		var pet = DataCenter.GetPet();
		if (pet == null)
		{
			return float.MaxValue;
		}

		var pdx = battleChara.Position.X - pet.Position.X;
		var pdz = battleChara.Position.Z - pet.Position.Z;
		return MathF.Max(0f, MathF.Sqrt(pdx * pdx + pdz * pdz) - battleChara.HitboxRadius);
	}

}
