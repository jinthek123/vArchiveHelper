using System;
using System.Drawing;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class UiTheme
{
	private static readonly UiThemePalette LightPalette = new UiThemePalette
	{
		WindowBackground = Color.FromArgb(247, 242, 236),
		PrimaryText = Color.FromArgb(42, 32, 30),
		SecondaryText = Color.FromArgb(138, 112, 104),
		Accent = Color.FromArgb(224, 122, 106),
		AccentSecondary = Color.FromArgb(196, 92, 78),
		Success = Color.FromArgb(62, 140, 104),
		Error = Color.FromArgb(196, 72, 78),
		InputBackground = Color.FromArgb(255, 252, 249),
		InputText = Color.FromArgb(42, 32, 30),
		InputBorder = Color.FromArgb(228, 210, 200),
		CardBackground = Color.FromArgb(255, 252, 249),
		CardBorder = Color.FromArgb(228, 210, 200),
		SectionTitle = Color.FromArgb(176, 78, 66),
		CardBody = Color.FromArgb(92, 72, 66),
		ButtonBackground = Color.FromArgb(255, 252, 249),
		ButtonBorder = Color.FromArgb(228, 210, 200),
		ButtonSelectedBackground = Color.FromArgb(224, 122, 106),
		ButtonSelectedText = Color.FromArgb(255, 250, 247)
	};

	private static readonly UiThemePalette DarkPalette = new UiThemePalette
	{
		WindowBackground = Color.FromArgb(26, 20, 18),
		PrimaryText = Color.FromArgb(245, 236, 230),
		SecondaryText = Color.FromArgb(186, 156, 146),
		Accent = Color.FromArgb(240, 156, 142),
		AccentSecondary = Color.FromArgb(240, 168, 154),
		Success = Color.FromArgb(122, 196, 160),
		Error = Color.FromArgb(232, 120, 124),
		InputBackground = Color.FromArgb(38, 30, 28),
		InputText = Color.FromArgb(245, 236, 230),
		InputBorder = Color.FromArgb(78, 58, 52),
		CardBackground = Color.FromArgb(38, 30, 28),
		CardBorder = Color.FromArgb(78, 58, 52),
		SectionTitle = Color.FromArgb(240, 168, 154),
		CardBody = Color.FromArgb(210, 186, 176),
		ButtonBackground = Color.FromArgb(38, 30, 28),
		ButtonBorder = Color.FromArgb(78, 58, 52),
		ButtonSelectedBackground = Color.FromArgb(196, 102, 88),
		ButtonSelectedText = Color.FromArgb(255, 246, 242)
	};

	public static UiThemeMode Current { get; private set; } = UiThemeMode.Light;

	public static UiThemePalette Palette
	{
		get
		{
			if (Current != UiThemeMode.Dark)
			{
				return LightPalette;
			}
			return DarkPalette;
		}
	}

	public static void SetMode(UiThemeMode mode)
	{
		Current = mode;
	}

	public static UiThemeMode Parse(string value)
	{
		if (!string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase))
		{
			return UiThemeMode.Light;
		}
		return UiThemeMode.Dark;
	}

	public static string ToConfigValue(UiThemeMode mode)
	{
		if (mode != UiThemeMode.Dark)
		{
			return "Light";
		}
		return "Dark";
	}

	public static Color StatusColor(SettingsStatusTone tone)
	{
		return tone switch
		{
			SettingsStatusTone.Success => Palette.Success, 
			SettingsStatusTone.Error => Palette.Error, 
			_ => Palette.SecondaryText, 
		};
	}

	public static void ApplyTo(Form form)
	{
		UiThemePalette palette = Palette;
		form.BackColor = palette.WindowBackground;
		form.ForeColor = palette.PrimaryText;
		ApplyRecursive(form, palette);
		UiFonts.ApplyTo(form);
	}

	public static void ApplyThemeSwitch(RadioButton light, RadioButton dark)
	{
		if (light != null && dark != null)
		{
			ApplySwitchButton(light, light.Checked);
			ApplySwitchButton(dark, dark.Checked);
		}
	}

	private static void ApplySwitchButton(RadioButton radioButton, bool selected)
	{
		radioButton.Font = UiFonts.Regular();
		radioButton.Padding = new Padding(10, 3, 10, 3);
		UiRounded.AttachButtonPaint(radioButton, selected ? RoundButtonStyle.ThemeSwitchOn : RoundButtonStyle.ThemeSwitchOff);
	}

	private static void ApplyRecursive(Control control, UiThemePalette palette)
	{
		if (!(control is Label label))
		{
			if (!(control is TextBox textBox))
			{
				if (!(control is ComboBox comboBox))
				{
					if (!(control is NumericUpDown numericUpDown))
					{
						if (!(control is ThemedCheckBox themedCheckBox))
						{
							if (!(control is CheckBox checkBox))
							{
								if (!(control is RadioButton radioButton))
								{
									if (!(control is Button button))
									{
										if (!(control is TableLayoutPanel tableLayoutPanel))
										{
											if (!(control is FlowLayoutPanel flowLayoutPanel))
											{
												if (control is Panel panel)
												{
													if (string.Equals(panel.Tag as string, "theme-card", StringComparison.Ordinal))
													{
														UiRounded.AttachCardPaint(panel);
													}
													else
													{
														panel.BackColor = palette.WindowBackground;
													}
												}
											}
											else
											{
												flowLayoutPanel.BackColor = palette.WindowBackground;
											}
										}
										else if (string.Equals(tableLayoutPanel.Tag as string, "theme-card", StringComparison.Ordinal))
										{
											UiRounded.AttachCardPaint(tableLayoutPanel);
										}
										else
										{
											tableLayoutPanel.BackColor = palette.WindowBackground;
										}
									}
									else if (string.Equals(button.Tag as string, "accent", StringComparison.Ordinal))
									{
										ApplyAccentButton(button, palette);
									}
									else
									{
										Button button2 = button;
										if (string.Equals(button2.Tag as string, "primary", StringComparison.Ordinal))
										{
											ApplyPrimaryButton(button2, palette);
										}
										else
										{
											ApplyStandardButton(button, palette);
										}
									}
								}
								else if (!string.Equals(radioButton.Tag as string, "theme-switch", StringComparison.Ordinal))
								{
									radioButton.ForeColor = palette.PrimaryText;
									radioButton.BackColor = palette.WindowBackground;
								}
							}
							else
							{
								checkBox.ForeColor = palette.PrimaryText;
								checkBox.BackColor = palette.WindowBackground;
							}
						}
						else
						{
							themedCheckBox.ForeColor = palette.PrimaryText;
							themedCheckBox.BackColor = palette.WindowBackground;
							themedCheckBox.Invalidate();
						}
					}
					else
					{
						numericUpDown.BackColor = palette.InputBackground;
						numericUpDown.ForeColor = palette.InputText;
						numericUpDown.BorderStyle = BorderStyle.FixedSingle;
						UiThemedInputs.AttachNumericUpDown(numericUpDown);
					}
				}
				else
				{
					comboBox.BackColor = palette.InputBackground;
					comboBox.ForeColor = palette.InputText;
					UiThemedInputs.AttachComboBox(comboBox);
				}
			}
			else
			{
				textBox.BackColor = palette.InputBackground;
				textBox.ForeColor = palette.InputText;
				textBox.BorderStyle = BorderStyle.FixedSingle;
			}
		}
		else if (string.Equals(label.Tag as string, "accent", StringComparison.Ordinal))
		{
			label.ForeColor = palette.AccentSecondary;
		}
		else if (string.Equals(label.Tag as string, "hint", StringComparison.Ordinal))
		{
			label.ForeColor = palette.SecondaryText;
		}
		else if (!string.Equals(label.Tag as string, "settings-status", StringComparison.Ordinal))
		{
			Label label2 = label;
			if (string.Equals(label2.Tag as string, "section-header", StringComparison.Ordinal))
			{
				label2.ForeColor = palette.SectionTitle;
				label2.Font = UiFonts.Bold(10f);
			}
			else
			{
				Label label3 = label;
				label3.ForeColor = palette.PrimaryText;
				Font font = label3.Font;
				if (font != null && font.Style == FontStyle.Bold)
				{
					label3.Font = UiFonts.Bold(label3.Font.Size);
				}
			}
		}
		foreach (Control control2 in control.Controls)
		{
			ApplyRecursive(control2, palette);
		}
	}

	private static void ApplyStandardButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Regular();
		button.Padding = new Padding(10, 4, 10, 4);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Standard);
	}

	private static void ApplyAccentButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Bold();
		button.Padding = new Padding(12, 5, 12, 5);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Accent);
	}

	private static void ApplyPrimaryButton(Button button, UiThemePalette palette)
	{
		button.Font = UiFonts.Bold();
		button.Padding = new Padding(14, 6, 14, 6);
		UiRounded.AttachButtonPaint(button, RoundButtonStyle.Primary);
	}
}
