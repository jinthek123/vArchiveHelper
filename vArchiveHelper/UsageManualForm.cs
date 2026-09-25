using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class UsageManualForm : Form
{
	private sealed class VerticalOnlyScrollPanel : Panel
	{
		private const int WmHscroll = 276;

		private const int WmMousehwheel = 526;

		public VerticalOnlyScrollPanel()
		{
			Dock = DockStyle.Fill;
			AutoScroll = true;
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SuppressHorizontalScroll();
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if ((msg != 276 && msg != 526) || 1 == 0)
			{
				base.WndProc(ref m);
			}
		}

		public void SuppressHorizontalScroll()
		{
			base.HorizontalScroll.Enabled = false;
			base.HorizontalScroll.Visible = false;
			if (base.AutoScrollMinSize.Width != 0)
			{
				base.AutoScrollMinSize = new Size(0, base.AutoScrollMinSize.Height);
			}
			try
			{
				base.HorizontalScroll.Value = base.HorizontalScroll.Minimum;
			}
			catch (ArgumentException)
			{
			}
		}
	}

	private const int FormContentWidth = 600;

	private const int FormClientHeight = 500;

	private const int FormPaddingHorizontal = 20;

	private const int FormChromeMargin = 16;

	private const int FormMinClientHeight = 360;

	private static UsageManualForm _openInstance;

	private VerticalOnlyScrollPanel _scrollPanel;

	private FlowLayoutPanel _contentFlow;

	private Label _headline;

	private string _layoutScreenDeviceName;

	private int _lastLayoutWidth = -1;

	private int _lastLayoutHeight = -1;

	private int _cardWidth = -1;

	public static void ShowFor(IWin32Window owner)
	{
		if (_openInstance != null && !_openInstance.IsDisposed)
		{
			if (_openInstance.WindowState == FormWindowState.Minimized)
			{
				_openInstance.WindowState = FormWindowState.Normal;
			}
			_openInstance.ApplyPreferredClientSize();
			_openInstance.BringToFront();
			_openInstance.Activate();
			ApplyCurrentThemeIfOpen();
		}
		else
		{
			_openInstance = new UsageManualForm();
			_openInstance.FormClosed += delegate
			{
				_openInstance = null;
			};
			_openInstance.Show(owner);
		}
	}

	public static void ApplyCurrentThemeIfOpen()
	{
		if (_openInstance != null && !_openInstance.IsDisposed)
		{
			_openInstance.ApplyTheme();
		}
	}

	private UsageManualForm()
	{
		base.Icon = AppIcon.Get();
		Text = "사용 매뉴얼 — " + Application.ProductName;
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.AutoScaleMode = AutoScaleMode.None;
		base.Padding = new Padding(18, 16, 18, 14);
		base.ClientSize = GetPreferredClientSize();
		Font = UiFonts.Regular(10f);
		_headline = new Label
		{
			AutoSize = true,
			Dock = DockStyle.Top,
			Text = "'전체 화면 최적화 사용 중지' 기능 사용과 동시에 v-archive에 업로드하는 방법",
			Font = UiFonts.Bold(11f),
			Margin = new Padding(0, 0, 0, 14),
			Padding = new Padding(0, 2, 0, 2),
			MinimumSize = new Size(0, 24)
		};
		_scrollPanel = new VerticalOnlyScrollPanel();
		_contentFlow = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0)
		};
		foreach (UsageGuideSection section in UsageGuide.Sections)
		{
			_contentFlow.Controls.Add(BuildSectionCard(section));
		}
		_contentFlow.Location = new Point(0, 0);
		_scrollPanel.Controls.Add(_contentFlow);
		Button button = new Button
		{
			Text = "닫기",
			AutoSize = true,
			Enabled = true,
			Font = UiFonts.Regular(10f),
			Padding = new Padding(12, 4, 12, 4),
			Margin = new Padding(0)
		};
		button.Click += delegate
		{
			Close();
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.RightToLeft,
			WrapContents = false,
			Dock = DockStyle.Bottom,
			Margin = new Padding(0, 8, 0, 0)
		};
		flowLayoutPanel.Controls.Add(button);
		base.Controls.Add(_scrollPanel);
		base.Controls.Add(flowLayoutPanel);
		base.Controls.Add(_headline);
		base.AcceptButton = button;
		base.CancelButton = button;
		base.Load += delegate
		{
			ApplyTheme();
			LayoutManualContent();
		};
		base.Shown += delegate
		{
			_layoutScreenDeviceName = Screen.FromControl(this).DeviceName;
			RelayoutManualContentDeferred();
		};
		base.Move += delegate
		{
			OnMovedToAnotherScreen();
		};
		_scrollPanel.Resize += delegate
		{
			LayoutManualContent();
		};
	}

	private void OnMovedToAnotherScreen()
	{
		if (base.IsHandleCreated && base.WindowState == FormWindowState.Normal)
		{
			string deviceName = Screen.FromControl(this).DeviceName;
			if (!string.Equals(_layoutScreenDeviceName, deviceName, StringComparison.Ordinal))
			{
				_layoutScreenDeviceName = deviceName;
				OnScreenEnvironmentChanged();
			}
		}
	}

	private void OnScreenEnvironmentChanged()
	{
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		base.ClientSize = GetPreferredClientSize();
		EnsureWithinWorkingArea();
		RefreshHeadlineLayout();
		RelayoutManualContentDeferred();
	}

	private Size GetPreferredClientSize()
	{
		int val = 620 + SystemInformation.VerticalScrollBarWidth;
		int val2 = 500;
		if (!base.IsHandleCreated)
		{
			return new Size(val, val2);
		}
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		int num = Math.Max(0, base.Width - base.ClientSize.Width);
		int num2 = Math.Max(0, base.Height - base.ClientSize.Height);
		val = Math.Min(val, Math.Max(280, workingArea.Width - num - 16));
		val2 = Math.Min(val2, Math.Max(360, workingArea.Height - num2 - 16));
		return new Size(val, val2);
	}

	private void RelayoutManualContentDeferred()
	{
		LayoutManualContent();
		BeginInvoke((Action)delegate
		{
			_lastLayoutWidth = -1;
			_lastLayoutHeight = -1;
			LayoutManualContent();
		});
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		OnScreenEnvironmentChanged();
	}

	private void ApplyPreferredClientSize()
	{
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		base.ClientSize = GetPreferredClientSize();
		EnsureWithinWorkingArea();
		RefreshHeadlineLayout();
		LayoutManualContent();
	}

	private void RefreshHeadlineLayout()
	{
		if (_headline != null)
		{
			int cardWidth = _cardWidth > 0 ? _cardWidth : MeasureCardWidth();
			_headline.MaximumSize = new Size(cardWidth, 0);
			_headline.PerformLayout();
			_headline.Invalidate();
		}
	}

	private void EnsureWithinWorkingArea()
	{
		Rectangle workingArea = Screen.FromControl(this).WorkingArea;
		if (base.Width > workingArea.Width - 8)
		{
			base.Width = workingArea.Width - 8;
		}
		if (base.Height > workingArea.Height - 8)
		{
			base.Height = workingArea.Height - 8;
		}
		if (base.Left < workingArea.Left)
		{
			base.Left = workingArea.Left;
		}
		if (base.Top < workingArea.Top)
		{
			base.Top = workingArea.Top;
		}
		if (base.Left + base.Width > workingArea.Right)
		{
			base.Left = workingArea.Right - base.Width;
		}
		if (base.Top + base.Height > workingArea.Bottom)
		{
			base.Top = workingArea.Bottom - base.Height;
		}
	}

	private void ApplyTheme()
	{
		UiThemePalette palette = UiTheme.Palette;
		UiTheme.ApplyTo(this);
		_headline.Font = UiFonts.Bold(11f);
		_headline.ForeColor = palette.SectionTitle;
		_scrollPanel.BackColor = palette.WindowBackground;
		foreach (Control control in _contentFlow.Controls)
		{
			if (!(control is TableLayoutPanel tableLayoutPanel))
			{
				continue;
			}
			tableLayoutPanel.BackColor = palette.WindowBackground;
			StyleManualLabels(tableLayoutPanel, palette);
			tableLayoutPanel.Invalidate();
		}
		_lastLayoutWidth = -1;
		_lastLayoutHeight = -1;
		LayoutManualContent();
	}

	private void LayoutManualContent()
	{
		if (_scrollPanel == null || _contentFlow == null)
		{
			return;
		}
		int inner = _scrollPanel.ClientSize.Width;
		if (inner < 1)
		{
			return;
		}
		int num = inner;
		if (!_scrollPanel.VerticalScroll.Visible)
		{
			ApplyCardWidth(inner);
			_contentFlow.PerformLayout();
			if (_contentFlow.PreferredSize.Height + 2 > _scrollPanel.ClientSize.Height)
			{
				num = Math.Max(1, inner - SystemInformation.VerticalScrollBarWidth);
			}
		}
		ApplyCardWidth(num);
		RefreshHeadlineLayout();
		_contentFlow.PerformLayout();
		int num3 = _contentFlow.PreferredSize.Height + 2;
		if (num == _lastLayoutWidth && num3 == _lastLayoutHeight)
		{
			_scrollPanel.SuppressHorizontalScroll();
			return;
		}
		_lastLayoutWidth = num;
		_lastLayoutHeight = num3;
		_contentFlow.Location = new Point(0, 0);
		_scrollPanel.AutoScrollPosition = new Point(0, 0);
		_scrollPanel.AutoScrollMinSize = new Size(0, num3);
		_scrollPanel.SuppressHorizontalScroll();
	}

	private int MeasureCardWidth()
	{
		if (_scrollPanel == null)
		{
			return Math.Max(1, base.ClientSize.Width - base.Padding.Horizontal);
		}
		int width = _scrollPanel.ClientSize.Width;
		if (width < 1)
		{
			width = Math.Max(1, base.ClientSize.Width - base.Padding.Horizontal);
		}
		return width;
	}

	private void ApplyCardWidth(int width)
	{
		_cardWidth = width;
		_contentFlow.Width = width;
		_contentFlow.MaximumSize = new Size(width, 0);
		foreach (Control control in _contentFlow.Controls)
		{
			if (!(control is TableLayoutPanel tableLayoutPanel))
			{
				continue;
			}
			int textWidth = Math.Max(1, width - tableLayoutPanel.Padding.Horizontal - tableLayoutPanel.Margin.Horizontal);
			tableLayoutPanel.MinimumSize = new Size(width, 0);
			tableLayoutPanel.MaximumSize = new Size(width, 0);
			tableLayoutPanel.Width = width;
			LimitLabelWidth(tableLayoutPanel, textWidth);
			tableLayoutPanel.PerformLayout();
		}
	}

	private static void StyleManualLabels(Control root, UiThemePalette palette)
	{
		foreach (Control control in root.Controls)
		{
			if (control is Label label)
			{
				label.BackColor = Color.Transparent;
				if (string.Equals(label.Tag as string, "manual-title", StringComparison.Ordinal))
				{
					label.Font = UiFonts.Bold(10.5f);
					label.ForeColor = palette.SectionTitle;
				}
				else
				{
					label.Font = UiFonts.Regular(9.5f);
					label.ForeColor = palette.CardBody;
				}
			}
			else if (control is FlowLayoutPanel)
			{
				control.BackColor = palette.CardBackground;
			}
			StyleManualLabels(control, palette);
		}
	}

	private static void LimitLabelWidth(Control root, int textWidth)
	{
		foreach (Control control in root.Controls)
		{
			if (control is Label label)
			{
				label.MaximumSize = new Size(textWidth, 0);
			}
			else if (control is PictureBox picture && picture.Image != null)
			{
				int height = Math.Max(1, picture.Image.Height * textWidth / Math.Max(1, picture.Image.Width));
				picture.Size = new Size(textWidth, height);
			}
			else if (control is FlowLayoutPanel flow)
			{
				int inner = Math.Max(1, textWidth - flow.Margin.Horizontal);
				flow.MaximumSize = new Size(inner, 0);
				flow.Width = inner;
				LimitLabelWidth(flow, inner);
			}
			else
			{
				LimitLabelWidth(control, textWidth);
			}
		}
	}

	private static Image _hotkeyShot;

	private static MemoryStream _hotkeyShotStream;

	private static Image LoadHotkeyShot()
	{
		if (_hotkeyShot != null)
		{
			return _hotkeyShot;
		}
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("vArchiveHelper.manual.varchive-hotkey.png");
		if (stream == null)
		{
			return null;
		}
		_hotkeyShotStream = new MemoryStream();
		stream.CopyTo(_hotkeyShotStream);
		_hotkeyShotStream.Position = 0;
		_hotkeyShot = Image.FromStream(_hotkeyShotStream);
		return _hotkeyShot;
	}

	private static Control BuildSectionCard(UsageGuideSection section)
	{
		UiThemePalette palette = UiTheme.Palette;
		TableLayoutPanel obj = new TableLayoutPanel
		{
			ColumnCount = 1,
			RowCount = 2,
			Tag = "theme-card",
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = palette.WindowBackground,
			Margin = new Padding(0, 0, 0, 10),
			Padding = new Padding(16, 14, 16, 14),
			RowStyles =
			{
				new RowStyle(SizeType.AutoSize),
				new RowStyle(SizeType.AutoSize)
			},
			ColumnStyles =
			{
				new ColumnStyle(SizeType.Percent, 100f)
			}
		};
		Label title = new Label
		{
			AutoSize = true,
			Tag = "manual-title",
			Text = section.Title,
			Font = UiFonts.Bold(10.5f),
			ForeColor = palette.SectionTitle,
			BackColor = Color.Transparent,
			Margin = new Padding(0, 0, 0, 10),
			UseMnemonic = false
		};
		FlowLayoutPanel lines = new FlowLayoutPanel
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0),
			BackColor = palette.CardBackground
		};
		int count = section.Lines?.Count ?? 0;
		for (int i = 0; i < count; i++)
		{
			lines.Controls.Add(new Label
			{
				AutoSize = true,
				Text = section.Lines[i],
				Font = UiFonts.Regular(9.5f),
				ForeColor = palette.CardBody,
				BackColor = Color.Transparent,
				Margin = new Padding(0, 0, 0, i == count - 1 ? 0 : 6),
				UseMnemonic = false
			});
			if (section.Lines[i].IndexOf("그림처럼", StringComparison.Ordinal) >= 0)
			{
				Image shot = LoadHotkeyShot();
				if (shot != null)
				{
					lines.Controls.Add(new PictureBox
					{
						Image = shot,
						SizeMode = PictureBoxSizeMode.Zoom,
						Size = new Size(360, Math.Max(1, shot.Height * 360 / Math.Max(1, shot.Width))),
						Margin = new Padding(0, 2, 0, 10),
						BackColor = palette.CardBackground
					});
				}
			}
		}
		obj.Controls.Add(title, 0, 0);
		obj.Controls.Add(lines, 0, 1);
		UiRounded.AttachCardPaint(obj);
		return obj;
	}
}
