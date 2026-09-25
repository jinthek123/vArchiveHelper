using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
	private const int WM_HOTKEY = 786;

	private const int HOTKEY_ID = 22081;

	private readonly HelperConfig _config;

	private readonly Action<string> _onStatus;

	private bool _registered;

	private bool _captureInProgress;

	public HotkeyWindow(HelperConfig config, Action<string> onStatus)
	{
		_config = config;
		_onStatus = onStatus;
		CreateHandle(new CreateParams());
		RegisterFromConfig();
	}

	public bool RegisterFromConfig()
	{
		if (_registered)
		{
			UnregisterHotKey(base.Handle, 22081);
			_registered = false;
		}
		int captureHotkeyModifiers = _config.CaptureHotkeyModifiers;
		int captureHotkeyVirtualKey = _config.CaptureHotkeyVirtualKey;
		if (!CaptureHotkey.IsValidForRegistration(captureHotkeyModifiers, captureHotkeyVirtualKey, out var error))
		{
			_onStatus(error);
			return false;
		}
		int fsModifiers = captureHotkeyModifiers | 0x4000;
		if (!RegisterHotKey(base.Handle, 22081, fsModifiers, captureHotkeyVirtualKey))
		{
			_onStatus("단축키(" + CaptureHotkey.FormatDisplay(captureHotkeyModifiers, captureHotkeyVirtualKey) + ")를 등록하지 못했습니다. 다른 프로그램과 충돌했을 수 있습니다.");
			return false;
		}
		_registered = true;
		string arg = CaptureHotkey.FormatDisplay(captureHotkeyModifiers, captureHotkeyVirtualKey);
		_onStatus(
			$"{arg} → Index {_config.MonitorIndex + 1} 모니터 캡처 → v-archive 인식" +
			Environment.NewLine +
			"v-archive 단축키는 「사용 매뉴얼」 그림과 같은지 확인하세요.");
		return true;
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 786 && m.WParam.ToInt32() == 22081)
		{
			RunCaptureAsync();
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private async Task RunCaptureAsync()
	{
		if (_captureInProgress)
		{
			return;
		}
		_captureInProgress = true;
		try
		{
			string obj = await CapturePipeline.RunAsync(_config).ConfigureAwait(continueOnCapturedContext: true);
			_onStatus(obj);
		}
		finally
		{
			_captureInProgress = false;
		}
	}

	[DllImport("user32.dll")]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

	[DllImport("user32.dll")]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	public void Dispose()
	{
		if (_registered)
		{
			UnregisterHotKey(base.Handle, 22081);
		}
		DestroyHandle();
	}
}
