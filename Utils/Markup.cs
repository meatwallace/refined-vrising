using VampireCommandFramework;

namespace Refined.Utils;

internal static class Markup
{
	public static string Highlight(int i) => Highlight(i.ToString());

	public static string Highlight(string s) => s.Bold().Color(HighlightColor);

	public const string HighlightColor = "#def";

	public static string Prefix = $"[refined] ".Color("#ed1").Bold();
}
