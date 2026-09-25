using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace vArchiveHelper;

internal static class VArchiveLauncher
{
	public static void EnsureRunning(HelperConfig config)
	{
		if (!IsRunning(config.VArchiveProcessName))
		{
			if (!File.Exists(config.VArchiveExePath))
			{
				throw new FileNotFoundException("v-archive.exe 를 찾을 수 없습니다.", config.VArchiveExePath);
			}
			string directoryName = Path.GetDirectoryName(config.VArchiveExePath);
			Process.Start(new ProcessStartInfo
			{
				FileName = config.VArchiveExePath,
				WorkingDirectory = (directoryName ?? AppDomain.CurrentDomain.BaseDirectory),
				UseShellExecute = true
			});
			Thread.Sleep(config.VArchiveStartupWaitMs);
		}
	}

	private static bool IsRunning(string processName)
	{
		return Process.GetProcessesByName(processName).Any();
	}
}
