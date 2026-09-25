using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class UiFonts
{
	public const string FamilyName = "Noto Sans KR";

	private const float DefaultSize = 9f;

	private static readonly Dictionary<(float Size, FontStyle Style), Font> Cache = new Dictionary<(float, FontStyle), Font>();

	public static Font Regular(float size = 9f)
	{
		return Get(size, FontStyle.Regular);
	}

	public static Font Bold(float size = 9f)
	{
		return Get(size, FontStyle.Bold);
	}

	public static void ApplyTo(Control root)
	{
		float size = root.Font?.Size ?? 9f;
		ApplyRecursive(root, Regular(size));
	}

	private static Font Get(float size, FontStyle style)
	{
		(float, FontStyle) key = (size, style);
		if (!Cache.TryGetValue(key, out var value))
		{
			value = new Font("Noto Sans KR", size, style, GraphicsUnit.Point);
			Cache[key] = value;
		}
		return value;
	}

	private static void ApplyRecursive(Control control, Font regular)
	{
		if (control.Font == null || control.Font.Style != FontStyle.Bold)
		{
			control.Font = regular;
		}
		foreach (Control control2 in control.Controls)
		{
			ApplyRecursive(control2, regular);
		}
	}
}
