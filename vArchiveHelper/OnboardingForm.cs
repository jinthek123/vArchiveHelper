using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace vArchiveHelper;

/// <summary>첫 실행 온보딩.</summary>
internal sealed class OnboardingForm : Form
{
	private readonly HelperConfig _config;

	private readonly Label _bodyLabel;

	public OnboardingForm(HelperConfig config)
	{
		_config = config;
		base.Icon = AppIcon.Get();
		Text = Application.ProductName + " — 시작하기";
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.AutoScaleMode = AutoScaleMode.None;
		base.ClientSize = new Size(500, 380);
		base.Padding = new Padding(20, 18, 20, 12);
		Font = UiFonts.Regular(9.5f);

		FlowLayoutPanel titleLines = new FlowLayoutPanel
		{
			AutoSize = true,
			Dock = DockStyle.Top,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0, 2, 0, 10)
		};
		Label titleLine1 = new Label
		{
			AutoSize = true,
			Font = UiFonts.Bold(11f),
			Tag = "accent",
			Text = "v-archive 클라이언트 캡쳐 오류 방지 보조 프로그램",
			Margin = new Padding(0),
			Padding = new Padding(0)
		};
		Label titleLine2 = new Label
		{
			AutoSize = true,
			Font = UiFonts.Bold(11f),
			Tag = "accent",
			Text = "vArchiveHelper입니다.",
			Margin = new Padding(0),
			Padding = new Padding(0)
		};
		titleLines.Controls.Add(titleLine1);
		titleLines.Controls.Add(titleLine2);

		_bodyLabel = new Label
		{
			AutoSize = true,
			Dock = DockStyle.Top,
			MaximumSize = new Size(460, 0),
			Text =
				"「전체 화면 최적화 사용 중지」 옵션은 DJMAX RESPECT V 의 프레임 개선에 도움이 된다고 알려져 있습니다." +
				" 하지만 해당 옵션을 켜면 곡 인식을 제대로 하지 못하는 경우가 발생하여 해당 프로그램을 제작하게 되었습니다.\n\n" +
				"해당 프로그램에 관련해서 문의가 있다면 github에 남겨주시면 최대한 빠르게 답변드리겠습니다.",
			Padding = new Padding(0, 6, 0, 8),
			Margin = new Padding(0)
		};

		Button nextButton = new Button
		{
			Text = "시작하기",
			AutoSize = true,
			DialogResult = DialogResult.None,
			Tag = "accent",
			Font = UiFonts.Bold(),
			Padding = new Padding(12, 4, 12, 4)
		};
		nextButton.Click += delegate
		{
			Finish();
		};

		Button skipButton = new Button
		{
			Text = "건너뛰기",
			AutoSize = true,
			Padding = new Padding(10, 4, 10, 4)
		};
		skipButton.Click += delegate
		{
			Finish();
		};

		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.RightToLeft,
			AutoSize = true,
			Dock = DockStyle.Bottom,
			WrapContents = false,
			Padding = new Padding(0, 12, 0, 0),
			Margin = new Padding(0)
		};
		buttons.Controls.Add(nextButton);
		buttons.Controls.Add(skipButton);

		LinkLabel repoLink = new LinkLabel
		{
			AutoSize = true,
			Dock = DockStyle.Top,
			MaximumSize = new Size(460, 0),
			Text = "https://github.com/jinthek123/vArchiveHelper",
			LinkBehavior = LinkBehavior.HoverUnderline,
			Padding = new Padding(0, 8, 0, 4),
			Margin = new Padding(0)
		};
		repoLink.Links.Add(0, repoLink.Text.Length, repoLink.Text);
		repoLink.LinkClicked += delegate(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData is string url)
			{
				Process.Start(url);
			}
		};

		base.Controls.Add(repoLink);
		base.Controls.Add(_bodyLabel);
		base.Controls.Add(titleLines);
		base.Controls.Add(buttons);

		UiTheme.ApplyTo(this);
		repoLink.LinkColor = UiTheme.Palette.AccentSecondary;
		repoLink.ActiveLinkColor = UiTheme.Palette.Accent;
		repoLink.VisitedLinkColor = UiTheme.Palette.AccentSecondary;
		repoLink.BackColor = UiTheme.Palette.WindowBackground;
		base.AcceptButton = nextButton;
	}

	private void Finish()
	{
		_config.OnboardingCompleted = true;
		try
		{
			_config.Save();
		}
		catch
		{
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
