using ECommons.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RotationSolver.Updaters;

/// <summary>
/// Compiles methods ahead of their first call, on a background thread (A739, concept 24, "Der erste Pull").
/// </summary>
/// <remarks>
/// .NET compiles a method on its first call. The first fight of a session calls hundreds of rules for the first time on the
/// game's thread, and the frame waits: the stalls of 21 to 64 ms in the traces fell into the first fight or onto a new
/// path, never with a garbage collection. Here the core - everything but the windows and the rotations - is compiled once
/// the plugin has loaded, and a rotation with its base classes when it is chosen; the game's thread then finds them
/// compiled. The cost line after each fight names the compile time that was left on the game's thread.
/// </remarks>
internal static class WarmUp
{
	private static readonly object _lock = new();
	private static readonly HashSet<Type> _prepared = [];
	private static readonly List<Thread> _threads = [];
	private static CancellationTokenSource _cancel = new();

	/// <summary>The core: every type of this library outside the windows and the rotations.</summary>
	public static void Core()
	{
		var assembly = typeof(WarmUp).Assembly;
		Start("the core", () =>
		{
			List<Type> types = [];
			foreach (var type in TypesOf(assembly))
			{
				var space = type.Namespace ?? string.Empty;
				if (!space.StartsWith("RotationSolver.UI", StringComparison.Ordinal)
					&& !space.StartsWith("RotationSolver.RebornRotations", StringComparison.Ordinal)
					&& !space.StartsWith("RotationSolver.ExtraRotations", StringComparison.Ordinal)
					&& !typeof(ICustomRotation).IsAssignableFrom(type))
				{
					types.Add(type);
				}
			}

			return types;
		});
	}

	/// <summary>A rotation when it is chosen: its class and the classes it derives from, with their nested types.</summary>
	public static void Rotation(Type rotation)
	{
		lock (_lock)
		{
			if (_prepared.Contains(rotation))
			{
				return;
			}
		}

		Start(rotation.Name, () =>
		{
			List<Type> types = [];
			for (var type = rotation; type != null && type != typeof(object); type = type.BaseType)
			{
				types.Add(type);
			}

			return types;
		});
	}

	/// <summary>Stops the threads still compiling; called when the plugin unloads.</summary>
	public static void Dispose()
	{
		List<Thread> threads;
		lock (_lock)
		{
			_cancel.Cancel();
			threads = [.. _threads];
			_threads.Clear();
		}

		foreach (var thread in threads)
		{
			_ = thread.Join(TimeSpan.FromSeconds(1));
		}

		lock (_lock)
		{
			_cancel.Dispose();
			_cancel = new CancellationTokenSource();
			_prepared.Clear();
		}
	}

	private static void Start(string what, Func<List<Type>> select)
	{
		CancellationToken token;
		lock (_lock)
		{
			token = _cancel.Token;
		}

		var thread = new Thread(() => Run(what, select, token))
		{
			IsBackground = true,
			Priority = ThreadPriority.BelowNormal,
			Name = "RSR warm-up",
		};

		lock (_lock)
		{
			_ = _threads.RemoveAll(t => !t.IsAlive);
			_threads.Add(thread);
		}

		thread.Start();
	}

	private static void Run(string what, Func<List<Type>> select, CancellationToken token)
	{
		try
		{
			var start = Stopwatch.GetTimestamp();
			var jitBefore = System.Runtime.JitInfo.GetCompilationTime(currentThread: true);
			int methods = 0, refused = 0;
			Stack<Type> pending = new(select());
			while (pending.Count > 0)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var type = pending.Pop();
				lock (_lock)
				{
					if (!_prepared.Add(type))
					{
						continue;
					}
				}

				// Lambdas and iterators live in nested types the compiler generates; they are the bodies of the rules.
				foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
				{
					pending.Push(nested);
				}

				if (type.ContainsGenericParameters)
				{
					continue;
				}

				const BindingFlags members = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static
					| BindingFlags.Public | BindingFlags.NonPublic;
				foreach (var method in type.GetMethods(members))
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					Prepare(method, ref methods, ref refused);
				}

				foreach (var constructor in type.GetConstructors(members))
				{
					Prepare(constructor, ref methods, ref refused);
				}
			}

			var jit = System.Runtime.JitInfo.GetCompilationTime(currentThread: true) - jitBefore;
			DefenseTrace.Line($"warm-up of {what}: {methods} method(s) compiled ahead in"
				+ $" {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F0} ms on a background thread (JIT {jit.TotalMilliseconds:F0} ms),"
				+ $" {refused} refused");
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"Warm-up of {what} stopped: {ex.Message}");
		}
	}

	private static void Prepare(MethodBase method, ref int methods, ref int refused)
	{
		if (method.IsAbstract || method.ContainsGenericParameters
			|| (method.Attributes & MethodAttributes.PinvokeImpl) != 0
			|| (method.MethodImplementationFlags & (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime)) != 0)
		{
			return;
		}

		try
		{
			RuntimeHelpers.PrepareMethod(method.MethodHandle);
			methods++;
		}
		catch (Exception)
		{
			// A method the runtime cannot compile ahead (a missing dependency, a special signature) is compiled on its
			// first call as before.
			refused++;
		}
	}

	private static Type[] TypesOf(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			List<Type> loaded = [];
			foreach (var type in ex.Types)
			{
				if (type != null)
				{
					loaded.Add(type);
				}
			}

			return [.. loaded];
		}
	}
}
