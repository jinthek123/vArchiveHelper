using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class UiThemedInputs
{
	private const int SpinnerButtonWidth = 17;

	private static readonly HashSet<ComboBox> ComboPaintHooked = new HashSet<ComboBox>();

	private static readonly HashSet<NumericUpDown> NumericPaintHooked = new HashSet<NumericUpDown>();

	private static readonly HashSet<Control> SpinnerChildPaintHooked = new HashSet<Control>();

	public static void AttachComboBox(ComboBox comboBox)
	{
		comboBox.FlatStyle = FlatStyle.Flat;
		if (comboBox is ThemedComboBox)
		{
			comboBox.Invalidate();
			return;
		}
		if (ComboPaintHooked.Add(comboBox))
		{
			comboBox.Paint += OnComboBoxPaint;
			comboBox.Disposed += delegate
			{
				ComboPaintHooked.Remove(comboBox);
			};
		}
		comboBox.Invalidate();
	}

	internal static void PaintComboDropdownButton(ComboBox comboBox)
	{
		if (!comboBox.IsHandleCreated || comboBox.Width <= 0 || comboBox.Height <= 0)
		{
			return;
		}
		using Graphics graphics = comboBox.CreateGraphics();
		UiThemePalette palette = UiTheme.Palette;
		(Color ButtonFill, Color ArrowColor) spinnerColors = GetSpinnerColors(comboBox.Enabled, palette);
		Color item = spinnerColors.ButtonFill;
		Color item2 = spinnerColors.ArrowColor;
		int num = Math.Min(17, comboBox.Width / 3);
		Rectangle rectangle = new Rectangle(comboBox.Width - num - 1, 0, num, comboBox.Height);
		using SolidBrush brush = new SolidBrush(item);
		graphics.FillRectangle(brush, rectangle);
		DrawDownArrow(graphics, Inset(rectangle, 4), item2);
	}

	public static void AttachNumericUpDown(NumericUpDown numericUpDown)
	{
		if (NumericPaintHooked.Add(numericUpDown))
		{
			numericUpDown.Paint += OnNumericUpDownPaint;
			numericUpDown.HandleCreated += delegate
			{
				AttachSpinnerButtons(numericUpDown);
			};
			numericUpDown.Disposed += delegate
			{
				NumericPaintHooked.Remove(numericUpDown);
				foreach (Control control in numericUpDown.Controls)
				{
					SpinnerChildPaintHooked.Remove(control);
				}
			};
		}
		if (numericUpDown.IsHandleCreated)
		{
			AttachSpinnerButtons(numericUpDown);
		}
		numericUpDown.Invalidate(invalidateChildren: true);
	}

	private static void AttachSpinnerButtons(NumericUpDown numericUpDown)
	{
		foreach (Control child in numericUpDown.Controls)
		{
			if (SpinnerChildPaintHooked.Add(child))
			{
				child.Paint += OnSpinnerChildPaint;
				child.Disposed += delegate
				{
					SpinnerChildPaintHooked.Remove(child);
				};
				child.Invalidate();
			}
		}
	}

	private static void OnComboBoxPaint(object sender, PaintEventArgs e)
	{
		PaintComboDropdownButton((ComboBox)sender);
	}

	private static void OnNumericUpDownPaint(object sender, PaintEventArgs e)
	{
		NumericUpDown numericUpDown = (NumericUpDown)sender;
		UiThemePalette palette = UiTheme.Palette;
		(Color ButtonFill, Color ArrowColor) spinnerColors = GetSpinnerColors(numericUpDown.Enabled, palette);
		Color item = spinnerColors.ButtonFill;
		Color item2 = spinnerColors.ArrowColor;
		int num = Math.Min(17, numericUpDown.Width / 3);
		PaintSpinnerButtons(buttonArea: new Rectangle(numericUpDown.Width - num - 1, 0, num, numericUpDown.Height), graphics: e.Graphics, buttonFill: item, arrowColor: item2);
	}

	private static void OnSpinnerChildPaint(object sender, PaintEventArgs e)
	{
		Control control = (Control)sender;
		if (control.Parent is NumericUpDown numericUpDown)
		{
			UiThemePalette palette = UiTheme.Palette;
			(Color ButtonFill, Color ArrowColor) spinnerColors = GetSpinnerColors(numericUpDown.Enabled, palette);
			Color item = spinnerColors.ButtonFill;
			Color item2 = spinnerColors.ArrowColor;
			Rectangle buttonArea = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
			e.Graphics.Clear(item);
			PaintSpinnerButtons(e.Graphics, buttonArea, item, item2);
		}
	}

	private static void PaintSpinnerButtons(Graphics graphics, Rectangle buttonArea, Color buttonFill, Color arrowColor)
	{
		using SolidBrush brush = new SolidBrush(buttonFill);
		graphics.FillRectangle(brush, buttonArea);
		int num = buttonArea.Top + buttonArea.Height / 2;
		using Pen pen = new Pen(Color.FromArgb(140, arrowColor));
		graphics.DrawLine(pen, buttonArea.Left, num, buttonArea.Right, num);
		Rectangle bounds = new Rectangle(buttonArea.Left, buttonArea.Top, buttonArea.Width, buttonArea.Height / 2);
		Rectangle bounds2 = new Rectangle(buttonArea.Left, num, buttonArea.Width, buttonArea.Height / 2);
		DrawUpArrow(graphics, Inset(bounds, 3), arrowColor);
		DrawDownArrow(graphics, Inset(bounds2, 3), arrowColor);
	}

	private static (Color ButtonFill, Color ArrowColor) GetSpinnerColors(bool enabled, UiThemePalette palette)
	{
		if (!enabled)
		{
			return (ButtonFill: SystemColors.ControlDark, ArrowColor: SystemColors.ControlLight);
		}
		return (ButtonFill: palette.Accent, ArrowColor: palette.ButtonSelectedText);
	}

	private static Rectangle Inset(Rectangle bounds, int padding)
	{
		return new Rectangle(bounds.Left + padding, bounds.Top + padding, Math.Max(0, bounds.Width - padding * 2), Math.Max(0, bounds.Height - padding * 2));
	}

	private static void DrawDownArrow(Graphics graphics, Rectangle bounds, Color color)
	{
		if (bounds.Width <= 0 || bounds.Height <= 0)
		{
			return;
		}
		int num = bounds.Left + bounds.Width / 2;
		int num2 = bounds.Top + bounds.Height / 2;
		int num3 = Math.Max(2, bounds.Width / 2);
		int num4 = Math.Max(2, bounds.Height / 3);
		Point[] points = new Point[3]
		{
			new Point(num - num3, num2 - num4 / 2),
			new Point(num + num3, num2 - num4 / 2),
			new Point(num, num2 + num4)
		};
		using SolidBrush brush = new SolidBrush(color);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.FillPolygon(brush, points);
	}

	private static void DrawUpArrow(Graphics graphics, Rectangle bounds, Color color)
	{
		if (bounds.Width <= 0 || bounds.Height <= 0)
		{
			return;
		}
		int num = bounds.Left + bounds.Width / 2;
		int num2 = bounds.Top + bounds.Height / 2;
		int num3 = Math.Max(2, bounds.Width / 2);
		int num4 = Math.Max(2, bounds.Height / 3);
		Point[] points = new Point[3]
		{
			new Point(num, num2 - num4),
			new Point(num - num3, num2 + num4 / 2),
			new Point(num + num3, num2 + num4 / 2)
		};
		using SolidBrush brush = new SolidBrush(color);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.FillPolygon(brush, points);
	}
}
