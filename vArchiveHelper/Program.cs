using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class Program
{
	private static readonly IntPtr PerMonitorDpiAwareV2 = new IntPtr(-4);

	[DllImport("user32.dll")]
	private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

	[STAThread]
	private static void Main()
	{
		if (!SingleInstanceApp.TryAcquire())
		{
			return;
		}
		try
		{
			SetProcessDpiAwarenessContext(PerMonitorDpiAwareV2);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			HelperConfig helperConfig = HelperConfig.Load();
			UiTheme.SetMode(UiTheme.Parse(helperConfig.UiTheme));
			using SettingsForm settingsForm = new SettingsForm(helperConfig);
			using HotkeyWindow hotkey = new HotkeyWindow(helperConfig, settingsForm.SetCaptureStatus);
			settingsForm.AttachHotkey(hotkey);
			SingleInstanceApp.StartShowListener(settingsForm.ShowFromTray);
			settingsForm.Show();
			Application.Run(settingsForm);
		}
		finally
		{
			SingleInstanceApp.Release();
		}
	}
}
