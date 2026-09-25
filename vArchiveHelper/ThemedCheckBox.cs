using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class ThemedCheckBox : CheckBox
{
	private const int BoxSize = 14;

	private UiInteractionState _interactionState;

	public ThemedCheckBox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		AutoSize = true;
		base.MouseEnter += delegate
		{
			SetInteractionState(UiInteractionState.Hovered);
		};
		base.MouseLeave += delegate
		{
			SetInteractionState(UiInteractionState.Normal);
		};
		base.MouseDown += delegate(object _, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				SetInteractionState(UiInteractionState.Pressed);
			}
		};
		base.MouseUp += delegate
		{
			Point pt = PointToClient(Cursor.Position);
			SetInteractionState(base.ClientRectangle.Contains(pt) ? UiInteractionState.Hovered : UiInteractionState.Normal);
		};
	}

	private void SetInteractionState(UiInteractionState state)
	{
		if (!base.Enabled)
		{
			state = UiInteractionState.Normal;
		}
		_interactionState = state;
		Invalidate();
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size size = TextRenderer.MeasureText(Text, Font, proposedSize, TextFormatFlags.SingleLine);
		int num = 20 + size.Width;
		int num2 = Math.Max(16, size.Height + 2);
		return new Size(num, num2);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		UiThemePalette palette = UiTheme.Palette;
		Color foreColor = (base.Enabled ? palette.PrimaryText : SystemColors.GrayText);
		Color color = (base.Enabled ? palette.Accent : SystemColors.GrayText);
		e.Graphics.Clear(BackColor);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle rectangle = new Rectangle(0, (base.Height - 14) / 2, 14, 14);
		var (color2, color3) = GetCheckBoxBoxColors(palette, color);
		Color mark = base.Checked ? palette.ButtonSelectedText : color;
		using (GraphicsPath path = UiRounded.CreatePath(rectangle, 4))
		{
			using SolidBrush brush = new SolidBrush(base.Checked ? color : color2);
			e.Graphics.FillPath(brush, path);
			using Pen pen = new Pen(base.Checked ? color : color3, (_interactionState == UiInteractionState.Pressed) ? 2f : 1f);
			e.Graphics.DrawPath(pen, path);
		}
		if (base.Checked)
		{
			DrawCheckMark(e.Graphics, rectangle, mark);
		}
		Rectangle bounds = new Rectangle(20, 0, Math.Max(0, base.Width - 14 - 6), base.Height);
		TextRenderer.DrawText(e.Graphics, Text, Font, bounds, foreColor, TextFormatFlags.VerticalCenter);
	}

	private (Color Fill, Color Border) GetCheckBoxBoxColors(UiThemePalette palette, Color accent)
	{
		return _interactionState switch
		{
			UiInteractionState.Hovered => (Fill: UiRounded.Blend(palette.InputBackground, accent, 0.12f), Border: accent), 
			UiInteractionState.Pressed => (Fill: UiRounded.Blend(palette.InputBackground, accent, 0.24f), Border: palette.SectionTitle), 
			_ => (Fill: palette.InputBackground, Border: accent), 
		};
	}

	private static void DrawCheckMark(Graphics graphics, Rectangle box, Color color)
	{
		Point[] points = new Point[3]
		{
			new Point(box.Left + 3, box.Top + 7),
			new Point(box.Left + 6, box.Top + 10),
			new Point(box.Left + 11, box.Top + 4)
		};
		using Pen pen = new Pen(color, 2f)
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round,
			LineJoin = LineJoin.Round
		};
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.DrawLines(pen, points);
	}
}
