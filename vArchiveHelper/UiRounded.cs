using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class UiRounded
{
	public const int Radius = 12;

	private static readonly Dictionary<ButtonBase, RoundButtonStyle> ButtonStyles = new Dictionary<ButtonBase, RoundButtonStyle>();

	private static readonly Dictionary<ButtonBase, UiInteractionState> ButtonInteractionStates = new Dictionary<ButtonBase, UiInteractionState>();

	private static readonly HashSet<Control> CardPaintHooked = new HashSet<Control>();

	private static readonly HashSet<ButtonBase> ButtonPaintHooked = new HashSet<ButtonBase>();

	private static readonly HashSet<ButtonBase> ButtonInteractionHooked = new HashSet<ButtonBase>();

	public static GraphicsPath CreatePath(Rectangle bounds, int radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
		{
			graphicsPath.AddRectangle(bounds);
			return graphicsPath;
		}
		int num = radius * 2;
		if (bounds.Width < num || bounds.Height < num)
		{
			graphicsPath.AddRectangle(bounds);
			return graphicsPath;
		}
		Rectangle rect = new Rectangle(bounds.Location, new Size(num, num));
		graphicsPath.AddArc(rect, 180f, 90f);
		rect.X = bounds.Right - num;
		graphicsPath.AddArc(rect, 270f, 90f);
		rect.Y = bounds.Bottom - num;
		graphicsPath.AddArc(rect, 0f, 90f);
		rect.X = bounds.Left;
		graphicsPath.AddArc(rect, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static void DrawFilledBorder(Graphics graphics, Rectangle bounds, Color fill, Color border, int radius = 12)
	{
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		using GraphicsPath path = CreatePath(bounds, radius);
		using SolidBrush brush = new SolidBrush(fill);
		graphics.FillPath(brush, path);
		if (border.A > 0)
		{
			using (Pen pen = new Pen(border))
			{
				graphics.DrawPath(pen, path);
				return;
			}
		}
	}

	public static Color Blend(Color from, Color to, float amount)
	{
		amount = Math.Max(0f, Math.Min(1f, amount));
		int red = (int)((float)(int)from.R + (float)(to.R - from.R) * amount);
		int green = (int)((float)(int)from.G + (float)(to.G - from.G) * amount);
		int blue = (int)((float)(int)from.B + (float)(to.B - from.B) * amount);
		return Color.FromArgb(from.A, red, green, blue);
	}

	public static void AttachCardPaint(Control card)
	{
		card.BackColor = UiTheme.Palette.WindowBackground;
		if (CardPaintHooked.Add(card))
		{
			card.Paint += OnCardPaint;
			card.Disposed += delegate
			{
				CardPaintHooked.Remove(card);
			};
		}
	}

	public static void AttachButtonPaint(ButtonBase button, RoundButtonStyle style)
	{
		ButtonStyles[button] = style;
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderSize = 0;
		button.UseVisualStyleBackColor = false;
		button.BackColor = UiTheme.Palette.WindowBackground;
		ButtonInteractionStates[button] = UiInteractionState.Normal;
		if (ButtonPaintHooked.Add(button))
		{
			button.Paint += OnButtonPaint;
		}
		if (ButtonInteractionHooked.Add(button))
		{
			button.MouseEnter += OnButtonMouseEnter;
			button.MouseLeave += OnButtonMouseLeave;
			button.MouseDown += OnButtonMouseDown;
			button.MouseUp += OnButtonMouseUp;
			button.EnabledChanged += delegate
			{
				button.Invalidate();
			};
			button.Disposed += delegate
			{
				DetachButton(button);
			};
		}
		button.Invalidate();
	}

	private static void DetachButton(ButtonBase button)
	{
		ButtonStyles.Remove(button);
		ButtonInteractionStates.Remove(button);
		ButtonPaintHooked.Remove(button);
		ButtonInteractionHooked.Remove(button);
	}

	private static void OnButtonMouseEnter(object sender, EventArgs e)
	{
		SetButtonInteractionState((ButtonBase)sender, UiInteractionState.Hovered);
	}

	private static void OnButtonMouseLeave(object sender, EventArgs e)
	{
		SetButtonInteractionState((ButtonBase)sender, UiInteractionState.Normal);
	}

	private static void OnButtonMouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			SetButtonInteractionState((ButtonBase)sender, UiInteractionState.Pressed);
		}
	}

	private static void OnButtonMouseUp(object sender, MouseEventArgs e)
	{
		ButtonBase obj = (ButtonBase)sender;
		Point pt = obj.PointToClient(Cursor.Position);
		SetButtonInteractionState(obj, obj.ClientRectangle.Contains(pt) ? UiInteractionState.Hovered : UiInteractionState.Normal);
	}

	private static void SetButtonInteractionState(ButtonBase button, UiInteractionState state)
	{
		if (!button.Enabled)
		{
			state = UiInteractionState.Normal;
		}
		ButtonInteractionStates[button] = state;
		button.Invalidate();
	}

	private static void OnCardPaint(object sender, PaintEventArgs e)
	{
		Control control = (Control)sender;
		UiThemePalette palette = UiTheme.Palette;
		DrawFilledBorder(bounds: new Rectangle(0, 0, control.Width - 1, control.Height - 1), graphics: e.Graphics, fill: palette.CardBackground, border: palette.CardBorder);
	}

	private static void OnButtonPaint(object sender, PaintEventArgs e)
	{
		ButtonBase buttonBase = (ButtonBase)sender;
		RoundButtonStyle value;
		RoundButtonStyle style = (ButtonStyles.TryGetValue(buttonBase, out value) ? value : RoundButtonStyle.Standard);
		UiInteractionState value2;
		UiInteractionState interaction = (ButtonInteractionStates.TryGetValue(buttonBase, out value2) ? value2 : UiInteractionState.Normal);
		(Color Fill, Color Border, Color Text) buttonColors = GetButtonColors(style, interaction);
		Color item = buttonColors.Fill;
		Color item2 = buttonColors.Border;
		Color foreColor = buttonColors.Text;
		Color color = buttonBase.Parent?.BackColor ?? UiTheme.Palette.WindowBackground;
		e.Graphics.Clear(color);
		Rectangle bounds = new Rectangle(0, 0, buttonBase.Width - 1, buttonBase.Height - 1);
		DrawFilledBorder(e.Graphics, bounds, item, item2);
		if (!buttonBase.Enabled)
		{
			foreColor = SystemColors.GrayText;
		}
		TextRenderer.DrawText(e.Graphics, buttonBase.Text, buttonBase.Font, bounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
	}

	private static (Color Fill, Color Border, Color Text) GetButtonColors(RoundButtonStyle style, UiInteractionState interaction)
	{
		UiThemePalette palette = UiTheme.Palette;
		var (color, color2, color3) = style switch
		{
			RoundButtonStyle.Accent => (palette.ButtonBackground, palette.Accent, palette.Accent), 
			RoundButtonStyle.Primary => (palette.ButtonSelectedBackground, palette.ButtonSelectedBackground, palette.ButtonSelectedText), 
			RoundButtonStyle.ThemeSwitchOn => (palette.ButtonSelectedBackground, palette.ButtonSelectedBackground, palette.ButtonSelectedText), 
			RoundButtonStyle.ThemeSwitchOff => (palette.ButtonBackground, palette.ButtonBorder, palette.PrimaryText), 
			_ => (palette.ButtonBackground, palette.ButtonBorder, palette.PrimaryText), 
		};
		return interaction switch
		{
			UiInteractionState.Hovered => GetHoveredButtonColors(style, color, color2, color3, palette), 
			UiInteractionState.Pressed => GetPressedButtonColors(style, color, color2, color3, palette), 
			_ => (Fill: color, Border: color2, Text: color3), 
		};
	}

	private static (Color Fill, Color Border, Color Text) GetHoveredButtonColors(RoundButtonStyle style, Color fill, Color border, Color text, UiThemePalette palette)
	{
		switch (style)
		{
		case RoundButtonStyle.Accent:
			return (Fill: Blend(fill, palette.Accent, 0.14f), Border: palette.Accent, Text: palette.SectionTitle);
		case RoundButtonStyle.Primary:
		case RoundButtonStyle.ThemeSwitchOn:
			return (Fill: Blend(fill, Color.White, 0.14f), Border: Blend(palette.ButtonSelectedBackground, Color.White, 0.1f), Text: palette.ButtonSelectedText);
		case RoundButtonStyle.Standard:
		case RoundButtonStyle.ThemeSwitchOff:
			return (Fill: Blend(fill, palette.Accent, 0.1f), Border: palette.Accent, Text: palette.PrimaryText);
		default:
			return (Fill: fill, Border: border, Text: text);
		}
	}

	private static (Color Fill, Color Border, Color Text) GetPressedButtonColors(RoundButtonStyle style, Color fill, Color border, Color text, UiThemePalette palette)
	{
		switch (style)
		{
		case RoundButtonStyle.Accent:
			return (Fill: Blend(fill, palette.Accent, 0.28f), Border: palette.SectionTitle, Text: palette.SectionTitle);
		case RoundButtonStyle.Primary:
		case RoundButtonStyle.ThemeSwitchOn:
			return (Fill: palette.SectionTitle, Border: palette.SectionTitle, Text: palette.ButtonSelectedText);
		case RoundButtonStyle.Standard:
		case RoundButtonStyle.ThemeSwitchOff:
			return (Fill: Blend(fill, palette.Accent, 0.22f), Border: palette.SectionTitle, Text: palette.PrimaryText);
		default:
			return (Fill: fill, Border: border, Text: text);
		}
	}
}
