namespace RotationSolver.Engine.Helpers;

public static partial class ObjectHelper
{
	private static bool _crawlingBindTraced;

	// Concept 21, K12a (A913): the crawling piece's bind lasts until it falls, so it comes first while it holds you;
	// unbound, the danger order decides - drawn first, it took the damage from the shambling pieces and led you into it.
	private static bool BoundByCrawlingPiece()
	{
		var bound = StatusHelper.PlayerHasStatus(false, StatusID.Bind_5710);
		if (bound && !_crawlingBindTraced)
		{
			DefenseTrace.Line("crucible siren: bound - the crawling piece comes first until it falls, its fall ends the bind");
		}

		_crawlingBindTraced = bound;
		return bound;
	}
}
