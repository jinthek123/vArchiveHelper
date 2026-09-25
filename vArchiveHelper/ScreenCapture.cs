using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class ScreenCapture
{
	public static Bitmap CaptureMonitorToTarget(HelperConfig config, out string error, out string methodUsed)
	{
		using CaptureResult captureResult = CaptureInternal(config, scaleToTarget: true);
		error = captureResult.Error;
		methodUsed = captureResult.Method;
		if (!captureResult.Success)
		{
			return null;
		}
		return new Bitmap(captureResult.Image);
	}

	private static CaptureResult CaptureInternal(HelperConfig config, bool scaleToTarget)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Screen[] allScreens = Screen.AllScreens;
		if (allScreens.Length == 0)
		{
			return Fail(Rectangle.Empty, Size.Empty, Size.Empty, stringBuilder, "연결된 모니터를 찾을 수 없습니다.");
		}
		if (config.MonitorIndex < 0 || config.MonitorIndex >= allScreens.Length)
		{
			return Fail(Rectangle.Empty, Size.Empty, Size.Empty, stringBuilder, $"지정한 Index {config.MonitorIndex + 1} 모니터를 찾을 수 없습니다. (범위: 1~{allScreens.Length})");
		}
		MonitorCaptureInfo captureInfo = MonitorGeometry.GetCaptureInfo(allScreens[config.MonitorIndex], config.UsePhysicalPixels);
		Rectangle captureBounds = captureInfo.CaptureBounds;
		stringBuilder.AppendLine($"Index {config.MonitorIndex + 1}:");
		MonitorGeometry.AppendDiagnostics(captureInfo, stringBuilder);
		if (config.UseDxgiCapture)
		{
			string error;
			using Bitmap bitmap = DxgiMonitorCapture.TryCapture(captureInfo, out error, writeDiagnosticLog: true);
			if (bitmap != null)
			{
				stringBuilder.AppendLine($"DXGI: 성공 ({bitmap.Width}x{bitmap.Height}, 모니터 전체)");
				return Success(bitmap, "DXGI", captureBounds, captureInfo, config, scaleToTarget, stringBuilder);
			}
			stringBuilder.AppendLine("DXGI: 실패 — " + (error ?? "알 수 없음"));
			if (config.RequireDxgiCapture)
			{
				return Fail(captureBounds, Size.Empty, Size.Empty, stringBuilder,
					"DXGI 캡처 실패: " + (error ?? "원인을 알 수 없습니다.") +
					" — 「전체 화면 최적화 사용 중지」는 그대로 두고, DXGI 설정과 사용 매뉴얼을 확인하세요.");
			}
		}
		else
		{
			stringBuilder.AppendLine("DXGI: 사용 안 함 (UseDxgiCapture=false)");
			if (config.RequireDxgiCapture)
			{
				return Fail(captureBounds, Size.Empty, Size.Empty, stringBuilder, "DXGI 캡처가 필요하나 설정에서 비활성화되어 있습니다.");
			}
		}
		string error2;
		using Bitmap bitmap2 = CaptureGdi(captureBounds, out error2);
		if (bitmap2 != null)
		{
			stringBuilder.AppendLine($"GDI: 성공 ({bitmap2.Width}x{bitmap2.Height}, 모니터 전체)");
			return Success(bitmap2, "GDI", captureBounds, captureInfo, config, scaleToTarget, stringBuilder);
		}
		stringBuilder.AppendLine("GDI: 실패 — " + (error2 ?? "알 수 없음"));
		if (config.UseGameWindowCapture && !string.IsNullOrWhiteSpace(config.GameProcessName))
		{
			string error3;
			using Bitmap bitmap3 = GameWindowCapture.TryCapture(config.GameProcessName, captureInfo, out error3);
			if (bitmap3 != null)
			{
				stringBuilder.AppendLine($"Window: 성공 ({bitmap3.Width}x{bitmap3.Height}, 창 크롭 — 비권장)");
				return Success(bitmap3, "Window", captureBounds, captureInfo, config, scaleToTarget, stringBuilder);
			}
			stringBuilder.AppendLine("Window: 실패 — " + (error3 ?? "알 수 없음"));
		}
		else
		{
			stringBuilder.AppendLine("Window: 생략 (UseGameWindowCapture=false)");
		}
		return Fail(captureBounds, Size.Empty, Size.Empty, stringBuilder, "모든 캡처 방식이 실패하였습니다.");
	}

	private static CaptureResult Success(Bitmap raw, string method, Rectangle bounds, MonitorCaptureInfo monitor, HelperConfig config, bool scaleToTarget, StringBuilder log)
	{
		using Bitmap bitmap = EnsurePhysicalSize(raw, monitor, config.UsePhysicalPixels, log);
		Bitmap bitmap2 = (scaleToTarget ? ScaleToTarget(bitmap, config) : new Bitmap(bitmap));
		if (scaleToTarget && (bitmap.Width != bitmap2.Width || bitmap.Height != bitmap2.Height))
		{
			log.AppendLine($"v-archive 리사이즈: {bitmap.Width}x{bitmap.Height} → {bitmap2.Width}x{bitmap2.Height}");
		}
		return new CaptureResult(bitmap2, method, null, log.ToString().TrimEnd(), bounds, bitmap.Size, bitmap2.Size);
	}

	private static Bitmap EnsurePhysicalSize(Bitmap raw, MonitorCaptureInfo monitor, bool usePhysicalPixels, StringBuilder log)
	{
		if (!usePhysicalPixels)
		{
			return new Bitmap(raw);
		}
		int width = monitor.NativeResolution.Width;
		int height = monitor.NativeResolution.Height;
		if (raw.Width == width && raw.Height == height)
		{
			return new Bitmap(raw);
		}
		log.AppendLine($"물리 보정: {raw.Width}x{raw.Height} → {width}x{height}");
		return BitmapScale.ResizeHighQuality(raw, width, height);
	}

	private static CaptureResult Fail(Rectangle bounds, Size source, Size output, StringBuilder log, string error)
	{
		return new CaptureResult(null, null, error, log.ToString().TrimEnd(), bounds, source, output);
	}

	private static Bitmap CaptureGdi(Rectangle bounds, out string error)
	{
		error = null;
		try
		{
			using Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
			}
			return new Bitmap(bitmap);
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return null;
		}
	}

	private static Bitmap ScaleToTarget(Bitmap source, HelperConfig config)
	{
		if (source.Width == config.TargetWidth && source.Height == config.TargetHeight)
		{
			return new Bitmap(source);
		}
		return BitmapScale.ResizeHighQuality(source, config.TargetWidth, config.TargetHeight);
	}
}
