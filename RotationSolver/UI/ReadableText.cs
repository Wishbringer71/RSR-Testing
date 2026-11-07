namespace RotationSolver.UI;

/// <summary>
/// The readable line length of RSR's text, in one place for windows and tooltips (concept 03, "Hilfsfenster", A654):
/// a text of up to <see cref="LongestSingleLine"/> characters stays on one line, a longer one breaks at
/// <see cref="LineMeasure"/>, and a window never sets one narrower than <see cref="ShortestLine"/>. Characters are those of
/// the text itself - its width over its length, paragraph by paragraph - so no character width is assumed.
/// </summary>
internal static class ReadableText
{
	// The readable range is 50 to 75 characters (Emil Ruder, Baymard Institute); the Windows UX Guide sets 65; WCAG 2.2
	// 1.4.8 at most 80.
	private const int ShortestLine = 50;
	private const int LineMeasure = 65;
	private const int LongestSingleLine = 75;

	/// <summary>The width, in pixels of the current font, the text takes at its readable line length.</summary>
	public static float Column(string text)
	{
		return Widest(text, LineMeasure, LongestSingleLine);
	}

	/// <summary>The width below which the text's lines would get shorter than readable.</summary>
	public static float Shortest(string text)
	{
		return Widest(text, ShortestLine, ShortestLine);
	}

	/// <summary>
	/// The text, broken at its readable line length from where it starts. The break comes from the text, not from the
	/// window's edge, so a window that sizes itself to its content (a tooltip) grows with it instead of shrinking it.
	/// </summary>
	public static void Draw(string text)
	{
		ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + Column(text));
		ImGui.TextUnformatted(text);
		ImGui.PopTextWrapPos();
	}

	/// <summary>Per paragraph: its whole width up to <paramref name="single"/> characters, else <paramref name="measure"/> of them.</summary>
	private static float Widest(string text, int measure, int single)
	{
		var widest = 0f;
		var start = 0;
		while (start <= text.Length)
		{
			var end = text.IndexOf('\n', start);
			if (end < 0)
			{
				end = text.Length;
			}

			var chars = end - start;
			if (chars > 0)
			{
				var width = ImGui.CalcTextSize(text.Substring(start, chars)).X;
				widest = MathF.Max(widest, chars <= single ? width : width * measure / chars);
			}

			start = end + 1;
		}

		return widest;
	}
}
