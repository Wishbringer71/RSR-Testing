using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using AttackType = RotationSolver.Engine.Data.AttackType;
using CombatRole = RotationSolver.Engine.Data.CombatRole;
using static RotationSolver.Configuration.ConfigTypes;
using Player = ECommons.GameHelpers.Player;

namespace RotationSolver.Engine.Actions;

public partial struct ActionTargetInfo
{
	#region Get Most Target

	private readonly IEnumerable<IBattleChara> GetMostCanTargetObjects(IEnumerable<IBattleChara> canTargets, IEnumerable<IBattleChara> canAffects, int aoeCount)
	{
		if (canTargets == null || canAffects == null)
		{
			yield break;
		}

		if (IsSingleTarget || EffectRange <= 0)
		{
			foreach (var target in canTargets)
			{
				yield return target;
			}

			yield break;
		}

		if (!action.Setting.IsFriendly && Service.Config.AoEType == AoEType.Off)
		{
			yield break;
		}

		// Cleave mode. Attacks only, as Off already is: the setting is about AoE attacks, and a heal or
		// a party mitigation that wants several members in reach is not one (owner's reading, A147).
		// Without the IsFriendly test a healer on Cleave never cast Medica, Helios or Succor.
		if (!action.Setting.IsFriendly && aoeCount > 1
			&& (Service.Config.AoEType == AoEType.Cleave || (DataCenter.IsInM9S && Service.Config.M9SCleaveOnly)))
		{
			yield break;
		}

		var targets = canTargets as List<IBattleChara> ?? [.. canTargets];

		List<IBattleChara> best = [];
		var bestCount = Math.Max(0, aoeCount);

		foreach (var t in targets)
		{
			// Concept 03 (A572): no centre whose area catches an enemy no attack may hit.
			if (!action.Setting.IsFriendly && HitsUntouchable(t))
			{
				continue;
			}

			var count = GetCanTargetCount(t, canAffects);
			if (count < bestCount)
			{
				continue;
			}

			if (count > bestCount)
			{
				bestCount = count;
				best.Clear();
			}
			best.Add(t);
		}

		for (var i = 0; i < best.Count; i++)
		{
			yield return best[i];
		}
	}

	/// <summary>
	/// Concept 03, "Gegner, die keine Fläche treffen darf" (A572): whether this area attack, centred on
	/// <paramref name="centre"/> (the player for an area around him), would catch an enemy no attack may hit - by the
	/// action's shape, and for a cone around the player by his facing.
	/// </summary>
	private readonly bool HitsUntouchable(IBattleChara centre)
	{
		var untouchable = DataCenter.UntouchableHostiles;
		if (untouchable.Count == 0 || IsSingleTarget || EffectRange <= 0 || action.Setting.IsFriendly || centre == null)
		{
			return false;
		}

		List<IBattleChara> caught = [];
		foreach (var enemy in untouchable)
		{
			if (enemy != null && enemy.GameObjectId != centre.GameObjectId && GetCanTarget(centre, enemy))
			{
				caught.Add(enemy);
			}
		}

		return caught.Count > 0 && (centre.GameObjectId != Player.Object?.GameObjectId || FilterAffectsByFacing(caught).Count > 0);
	}

	private readonly int GetCanTargetCount(IBattleChara target, IEnumerable<IBattleChara> canAffects)
	{
		if (target == null || canAffects == null)
		{
			return 0;
		}

		var count = 0;
		foreach (var t in canAffects)
		{
			if (target != t && !GetCanTarget(target, t))
			{
				continue;
			}

			if (Service.Config.NoNewHostiles && t.TargetObject == null)
			{
				return 0;
			}
			count++;
		}

		return count;
	}

	private const double _alpha = Math.PI / 3;
	private static readonly float SinAlpha = (float)Math.Sin(_alpha);
	private static readonly double CosAlpha = Math.Cos(_alpha);

	// The cone test of a cone cast from the player: the direction to the enemy, flat and unit length, lies within the
	// half-angle of his facing.
	private static bool InFacingCone(Vector3 faceVec, Vector3 toTargetUnit)
	{
		return Vector3.Dot(faceVec, toTargetUnit) >= (float)CosAlpha;
	}

	private readonly bool GetCanTarget(IBattleChara target, IBattleChara subTarget)
	{
		var player = Player.Object;
		if (target == null || subTarget == null || player == null)
		{
			return false;
		}

		var pPos = player.Position;
		var targetPos = target.Position;
		var subPos = subTarget.Position;
		var subRadius = subTarget.HitboxRadius;
		var effectRange = EffectRange;
		var castType = (CastType)action.Action.CastType;
		var dir = targetPos - pPos;
		var tdir = subPos - pPos;

		var dirLen = dir.Length();
		var tdirLen = tdir.Length();

		if (dirLen <= 0.001f)
		{
			switch (castType)
			{
				case CastType.Circle:
					return Vector3.Distance(targetPos, subPos) - subRadius <= effectRange;

				case CastType.Cone:
					return (tdirLen - subRadius) <= effectRange;

				case CastType.StraightLine:
					{
						if (tdirLen - subRadius > effectRange)
						{
							return false;
						}

						var face = player.GetFaceVector();
						var lineDir2 = Vector3.Normalize(new Vector3(face.X, 0, face.Z));
						Vector3 tdirXZ2 = new(tdir.X, 0, tdir.Z);
						var perp2 = Vector3.Cross(lineDir2, tdirXZ2).Length();
						var halfWidth2 = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
						return perp2 <= halfWidth2 + subRadius && Vector3.Dot(lineDir2, tdirXZ2) >= 0;
					}

				case CastType.Donut:
					{
						var d = Vector3.Distance(targetPos, subPos) - subRadius;
						var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
						return d <= effectRange && d >= innerRadius;
					}

				default:
					return false;
			}
		}

		switch (castType)
		{
			case CastType.Circle:
				return Vector3.Distance(targetPos, subPos) - subRadius <= effectRange;

			case CastType.Cone:
				{
					if (tdirLen - subRadius > effectRange)
					{
						return false;
					}

					var dirN = dir / dirLen;

					var tdirAdj = tdir + dirN * (target.HitboxRadius / SinAlpha);

					var tlen = tdirAdj.Length();
					if (tlen <= 0.001f)
					{
						return true;
					}

					var cos = Vector3.Dot(dir, tdirAdj) / (dirLen * tlen);
					return cos >= CosAlpha;
				}

			case CastType.StraightLine:
				{
					if (tdirLen - subRadius > effectRange)
					{
						return false;
					}

					// A line with a target goes towards it: using the action turns the player to the target, and
					// BossMod's autorotation measures it the same way (AkechiTools.LineTargetCheck). Measured along
					// the facing of the moment (upstream 2c080bbc2), the count followed where the player looked, not
					// where the line will go (A399). The facing stays for a target straight above or below him.
					var flatDir = new Vector3(dir.X, 0, dir.Z);
					Vector3 lineDir;
					if (flatDir.LengthSquared() > 0f)
					{
						lineDir = Vector3.Normalize(flatDir);
					}
					else
					{
						var face = player.GetFaceVector();
						lineDir = Vector3.Normalize(new Vector3(face.X, 0, face.Z));
					}

					Vector3 tdirXZ = new(tdir.X, 0, tdir.Z);
					var perp = Vector3.Cross(lineDir, tdirXZ).Length();

					var halfWidth = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier / 2f : 2f;
					var width = halfWidth + subRadius;

					return perp <= width && Vector3.Dot(lineDir, tdirXZ) >= 0;
				}

			case CastType.Donut:
				{
					var dis = Vector3.Distance(targetPos, subPos) - subRadius;
					var innerRadius = action.Action.XAxisModifier > 0 ? action.Action.XAxisModifier : 8f;
					return dis <= effectRange && dis >= innerRadius;
				}

			default:
				return false;
		}
	}

	#endregion
}
