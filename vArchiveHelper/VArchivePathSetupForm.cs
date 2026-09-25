using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class VArchivePathSetupForm : Form
{
	private readonly HelperConfig _config;

	private readonly TextBox _pathInput;

	private readonly Label _statusLabel;

	public VArchivePathSetupForm(HelperConfig config)
	{
		_config = config;
		base.Icon = AppIcon.Get();
		Text = Application.ProductName + " — v-archive 경로";
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = true;
		AutoSize = true;
		base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		base.Padding = new Padding(14);
		Font = UiFonts.Regular();
		Label control = new Label
		{
			AutoSize = true,
			MaximumSize = new Size(420, 0),
			Text = "처음 사용 전에 v-archive.exe 위치를 지정해 주세요.\n캡처 후 자동 실행·인식에 필요합니다."
		};
		_pathInput = new TextBox
		{
			Width = 360,
			Text = (File.Exists(config.VArchiveExePath?.Trim() ?? "") ? config.VArchiveExePath : "")
		};
		Button button = new Button
		{
			Text = "찾기…",
			AutoSize = true
		};
		button.Click += delegate
		{
			Browse();
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false
		};
		flowLayoutPanel.Controls.Add(_pathInput);
		flowLayoutPanel.Controls.Add(button);
		_statusLabel = new Label
		{
			AutoSize = true,
			Tag = "settings-status",
			Margin = new Padding(0, 6, 0, 0)
		};
		Button button2 = new Button
		{
			Text = "저장",
			DialogResult = DialogResult.None,
			AutoSize = true
		};
		button2.Click += delegate
		{
			SaveAndClose();
		};
		Button button3 = new Button
		{
			Text = "나중에",
			DialogResult = DialogResult.Cancel,
			AutoSize = true
		};
		FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
		{
			AutoSize = true,
			FlowDirection = FlowDirection.RightToLeft,
			WrapContents = false,
			Margin = new Padding(0, 10, 0, 0)
		};
		flowLayoutPanel2.Controls.Add(button3);
		flowLayoutPanel2.Controls.Add(button2);
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			ColumnCount = 1,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink
		};
		tableLayoutPanel.Controls.Add(control, 0, 0);
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 1);
		tableLayoutPanel.Controls.Add(_statusLabel, 0, 2);
		tableLayoutPanel.Controls.Add(flowLayoutPanel2, 0, 3);
		base.Controls.Add(tableLayoutPanel);
		UiTheme.ApplyTo(this);
		UpdateStatus("", SettingsStatusTone.Neutral);
		base.AcceptButton = button2;
		base.CancelButton = button3;
	}

	private void UpdateStatus(string text, SettingsStatusTone tone)
	{
		_statusLabel.Text = text;
		_statusLabel.ForeColor = UiTheme.StatusColor(tone);
	}

	public static bool IsSetupRequired(HelperConfig config)
	{
		if (string.IsNullOrWhiteSpace(config.VArchiveExePath) || HelperConfig.IsPlaceholderPath(config.VArchiveExePath))
		{
			return true;
		}
		return !File.Exists(config.VArchiveExePath.Trim());
	}

	private void Browse()
	{
		if (VArchivePathPicker.TryBrowse(this, _pathInput.Text, out var selectedPath))
		{
			_pathInput.Text = selectedPath;
			UpdateStatus("", SettingsStatusTone.Neutral);
		}
	}

	private void SaveAndClose()
	{
		string text = _pathInput.Text.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			UpdateStatus("v-archive.exe 경로를 선택하세요.", SettingsStatusTone.Error);
			return;
		}
		if (!File.Exists(text))
		{
			UpdateStatus("파일을 찾을 수 없습니다.", SettingsStatusTone.Error);
			return;
		}
		try
		{
			_config.VArchiveExePath = text;
			_config.Save();
		}
		catch (Exception ex)
		{
			UpdateStatus("저장 실패: " + ex.Message, SettingsStatusTone.Error);
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
