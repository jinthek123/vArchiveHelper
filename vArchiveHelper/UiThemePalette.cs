using System.Drawing;

namespace vArchiveHelper;

internal readonly struct UiThemePalette
{
	public Color WindowBackground { get; init; }

	public Color PrimaryText { get; init; }

	public Color SecondaryText { get; init; }

	public Color Accent { get; init; }

	public Color AccentSecondary { get; init; }

	public Color Success { get; init; }

	public Color Error { get; init; }

	public Color InputBackground { get; init; }

	public Color InputText { get; init; }

	public Color InputBorder { get; init; }

	public Color CardBackground { get; init; }

	public Color CardBorder { get; init; }

	public Color SectionTitle { get; init; }

	public Color CardBody { get; init; }

	public Color ButtonBackground { get; init; }

	public Color ButtonBorder { get; init; }

	public Color ButtonSelectedBackground { get; init; }

	public Color ButtonSelectedText { get; init; }
}
