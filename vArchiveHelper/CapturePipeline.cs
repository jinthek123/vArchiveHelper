using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class CapturePipeline
{
	public static string Run(HelperConfig config)
	{
		return RunAsync(config).GetAwaiter().GetResult();
	}

	public static async Task<string> RunAsync(HelperConfig config)
	{
		try
		{
			(Bitmap, string, string) tuple = await Task.Run(delegate
			{
				string error;
				string methodUsed;
				Bitmap bitmap = ScreenCapture.CaptureMonitorToTarget(config, out error, out methodUsed);
				return ((Bitmap Image, string Error, string Method))((bitmap == null) ? (Image: null, Error: error, Method: null) : (Image: bitmap, Error: null, Method: methodUsed));
			}).ConfigureAwait(continueOnCapturedContext: true);
			if (tuple.Item1 == null)
			{
				return EnrichFailure(tuple.Item2);
			}
			using (tuple.Item1)
			{
				return FinishOnUiThread(config, tuple.Item1, tuple.Item3);
			}
		}
		catch (Exception ex)
		{
			return EnrichFailure("오류가 발생하였습니다. " + ex.Message);
		}
	}

	private static string FinishOnUiThread(HelperConfig config, Bitmap bitmap, string method)
	{
		VArchiveLauncher.EnsureRunning(config);
		Clipboard.SetImage(bitmap);
		Thread.Sleep(config.ClipboardSettleMs);
		Thread.Sleep(config.AfterClipboardBeforeRecognizeMs);
		InputHelper.SendAltInsert();
		return $"완료 — {bitmap.Width}×{bitmap.Height}, [{method}], Index {config.MonitorIndex + 1} · 클립보드→인식";
	}

	private static string EnrichFailure(string error)
	{
		if (string.IsNullOrWhiteSpace(error))
		{
			return "캡처 실패 — 원인을 알 수 없습니다.";
		}
		if (error.IndexOf("DXGI", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return error + " → DXGI·물리 픽셀을 확인하고, 사용 매뉴얼의 단축키 그림을 보세요.";
		}
		return error;
	}
}
