using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Hooks.ActionEffectTypes;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Damage the game has reported on the player and not yet taken from his health (concept 09, "Ein gemeldeter Treffer, der
/// tötet", A617).
/// </summary>
/// <remarks>
/// <para>The game reports a hit (its action effect) before it takes the health: in his logs of 25.10.-05.11.2026 the
/// result came 0.58 s later in the median, within 1.34 s for 95 %, at the longest 3.6 s (690 hits on him, line 21/22
/// against line 37 of the same sequence). The client keeps the reported effects per character and marks each confirmed
/// once its result arrives (<c>BattleChara.ActionEffectHandler.IncomingEffects</c>, <c>TargetConfirmed</c>; ClientStructs
/// 0adb2e5). This reads that list for the player.</para>
///
/// <para>An entry counts until the game confirms it or the player's health and barrier have fallen by it since it was first
/// seen. One that never lands - damage past death, under an invulnerability, all of it in a barrier - ends after the longest
/// wait from report to damage measured in this session, a GCD before the first. What stood in the list when
/// the reading began - on loading, after a death, in a new area - never counts: the server's sequence only rises.</para>
///
/// <para>This answers what is on its way; what to do about it is the tank rule's
/// (<c>CustomRotation.InvulnerabilityAgainstReportedDeath</c>).</para>
/// </remarks>
internal static unsafe class ReportedDamage
{
	// When each unconfirmed entry was first seen, and the player's health and barrier then: its landing is a fall by its amount.
	private readonly record struct Seen(long Tick, long EffectiveHp);

	private static readonly Dictionary<uint, Seen> _firstSeen = [];
	// Entries that have landed by the fall of health and barrier, kept until the list drops them: measured once, never again
	// counted (his trace of 06.11.2026: measured every frame, the longest wait grew by a frame at a time to 100 s, A634).
	private static readonly HashSet<uint> _landed = [];
	private static readonly HashSet<uint> _present = [];
	private static readonly List<uint> _gone = [];
	private static uint _baseline;
	private static bool _reading;
	private static uint _territory;
	private static long _longestWaitMs;

	/// <summary>The damage reported on the player and not yet taken from his health, in HP.</summary>
	internal static uint Pending { get; private set; }

	/// <summary>
	/// Reads the client's list once per frame, on the game thread.
	/// </summary>
	internal static void Update()
	{
		Pending = 0;
		var player = Player.Object;
		if (player == null || player.IsDead || player.Address == nint.Zero || DataCenter.TerritoryID != _territory)
		{
			_territory = DataCenter.TerritoryID;
			_firstSeen.Clear();
			_landed.Clear();
			_reading = false;
			return;
		}

		var handler = &player.Struct()->ActionEffectHandler;
		if (!_reading)
		{
			// What stands in the list already - an earlier death's overkill, an area left - is never confirmed and never counts.
			_baseline = 0;
			foreach (ref readonly var entry in handler->IncomingEffects)
			{
				_baseline = Math.Max(_baseline, entry.GlobalSequence);
			}

			_reading = true;
			return;
		}

		var now = Environment.TickCount64;
		long effectiveHp = player.CurrentHp + player.GetObjectShield();
		var window = _longestWaitMs > 0 ? _longestWaitMs : (long)TimeSpan.FromSeconds(DataCenter.DefaultGCDTotal).TotalMilliseconds;
		_present.Clear();
		foreach (ref readonly var entry in handler->IncomingEffects)
		{
			var sequence = entry.GlobalSequence;
			// The list is the one of effects coming in on the player, as BossModReborn reads it per actor (WorldStateGameSync).
			if (sequence <= _baseline)
			{
				continue;
			}

			if (_landed.Contains(sequence))
			{
				_ = _present.Add(sequence);
				continue;
			}

			if (entry.TargetConfirmed)
			{
				if (_firstSeen.Remove(sequence, out var confirmed))
				{
					MeasureWait(now - confirmed.Tick);
				}

				continue;
			}

			var amount = DamageOf(entry.Effects);
			if (amount == 0)
			{
				continue;
			}

			_ = _present.Add(sequence);
			if (!_firstSeen.TryGetValue(sequence, out var first))
			{
				first = new Seen(now, effectiveHp);
				_firstSeen[sequence] = first;
			}

			// Health and barrier have fallen by it since it was first seen: it has landed, whether or not the confirmation flag
			// was set - what counts on the flag alone would linger a whole window and add itself to the next hit.
			if (effectiveHp + amount <= first.EffectiveHp)
			{
				MeasureWait(now - first.Tick);
				_ = _firstSeen.Remove(sequence);
				_ = _landed.Add(sequence);
				continue;
			}

			if (now - first.Tick <= window)
			{
				Pending += amount;
			}
		}

		// An entry overwritten in the list before it was confirmed is gone with it.
		_gone.Clear();
		foreach (var sequence in _firstSeen.Keys)
		{
			if (!_present.Contains(sequence))
			{
				_gone.Add(sequence);
			}
		}

		foreach (var sequence in _gone)
		{
			_ = _firstSeen.Remove(sequence);
		}

		_landed.IntersectWith(_present);
	}

	private static void MeasureWait(long waitMs)
	{
		if (waitMs > _longestWaitMs)
		{
			_longestWaitMs = waitMs;
			DefenseTrace.Line($"reported damage: the longest wait from report to damage is now {_longestWaitMs} ms");
		}
	}

	private static uint DamageOf(FFXIVClientStructs.FFXIV.Client.Game.Character.ActionEffectHandler.TargetEffects effects)
	{
		uint sum = 0;
		foreach (ref readonly var effect in effects.Effects)
		{
			// The client's effect and ECommons' entry are the same eight bytes; ECommons calls Param3 mult and Param4 flags.
			sum += ActionEffectReading.DamageOnTarget(new EffectEntry
			{
				type = (ActionEffectType)effect.Type,
				param0 = effect.Param0,
				param1 = effect.Param1,
				param2 = effect.Param2,
				mult = effect.Param3,
				flags = effect.Param4,
				value = effect.Value,
			});
		}

		return sum;
	}
}
