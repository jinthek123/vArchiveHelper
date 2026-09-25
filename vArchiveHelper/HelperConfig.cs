using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class HelperConfig
{
	public const int CurrentConfigVersion = 3;

	public const string PlaceholderExePath = @"C:\path\to\v-archive.exe";

	public int ConfigVersion { get; set; } = CurrentConfigVersion;

	public int MonitorIndex { get; set; } = 1;

	public int TargetWidth { get; set; } = 2560;

	public int TargetHeight { get; set; } = 1440;

	public string VArchiveExePath { get; set; } = PlaceholderExePath;

	public string VArchiveProcessName { get; set; } = "v-archive";

	public int ClipboardSettleMs { get; set; } = 80;

	public int AfterClipboardBeforeRecognizeMs { get; set; } = 120;

	public int VArchiveStartupWaitMs { get; set; } = 2500;

	public bool UseDxgiCapture { get; set; } = true;

	public bool RequireDxgiCapture { get; set; } = true;

	public bool UsePhysicalPixels { get; set; } = true;

	public string GameProcessName { get; set; } = "";

	public bool UseGameWindowCapture { get; set; }

	public int CaptureHotkeyVirtualKey { get; set; } = 45;

	public int CaptureHotkeyModifiers { get; set; }

	public string UiTheme { get; set; } = "Light";

	public bool OnboardingCompleted { get; set; }

	/// <summary>true면 창을 닫을 때 종료 대신 트레이로 숨김. 기본 false(선택).</summary>
	public bool RunInSystemTray { get; set; }

	public static string SettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

	public static string ExampleSettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.example.json");

	public static HelperConfig Load()
	{
		EnsureSettingsFileExists();
		if (!File.Exists(SettingsPath))
		{
			return new HelperConfig();
		}
		try
		{
			HelperConfig obj = JsonSerializer.Deserialize<HelperConfig>(File.ReadAllText(SettingsPath)) ?? new HelperConfig();
			obj.NormalizeAfterLoad();
			return obj;
		}
		catch
		{
			return new HelperConfig();
		}
	}

	public static void EnsureSettingsFileExists()
	{
		if (!File.Exists(SettingsPath) && File.Exists(ExampleSettingsPath))
		{
			File.Copy(ExampleSettingsPath, SettingsPath);
		}
	}

	public void NormalizeAfterLoad()
	{
		if (ConfigVersion < CurrentConfigVersion)
		{
			ConfigVersion = CurrentConfigVersion;
		}
		if (!string.Equals(UiTheme, "Dark", StringComparison.OrdinalIgnoreCase))
		{
			UiTheme = "Light";
		}
		if (string.IsNullOrWhiteSpace(VArchiveExePath))
		{
			VArchiveExePath = PlaceholderExePath;
		}
	}

	public HelperConfig Clone()
	{
		return JsonSerializer.Deserialize<HelperConfig>(JsonSerializer.Serialize(this)) ?? new HelperConfig();
	}

	public bool TryValidate(out string error)
	{
		if (string.IsNullOrWhiteSpace(VArchiveExePath) || IsPlaceholderPath(VArchiveExePath))
		{
			error = "v-archive.exe 경로를 입력하세요.";
			return false;
		}
		if (!File.Exists(VArchiveExePath.Trim()))
		{
			error = "v-archive.exe 파일을 찾을 수 없습니다: " + VArchiveExePath;
			return false;
		}
		if (!CaptureHotkey.IsValidForRegistration(CaptureHotkeyModifiers, CaptureHotkeyVirtualKey, out error))
		{
			return false;
		}
		Screen[] allScreens = Screen.AllScreens;
		if (allScreens.Length == 0)
		{
			error = "연결된 모니터가 없습니다.";
			return false;
		}
		if (MonitorIndex < 0 || MonitorIndex >= allScreens.Length)
		{
			error = $"Index {MonitorIndex + 1} 모니터가 없습니다. (1~{allScreens.Length})";
			return false;
		}
		error = null;
		return true;
	}

	public static bool IsPlaceholderPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return true;
		}
		string trimmed = path.Trim();
		if (string.Equals(trimmed, PlaceholderExePath, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return trimmed.IndexOf("path\\to", StringComparison.OrdinalIgnoreCase) >= 0
			|| trimmed.IndexOf("path/to", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public void Save()
	{
		ConfigVersion = CurrentConfigVersion;
		JsonSerializerOptions options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, options));
	}
}
