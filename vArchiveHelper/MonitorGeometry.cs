using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class MonitorGeometry
{
	private struct POINT
	{
		public int X;

		public int Y;
	}

	private struct DEVMODE
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public short dmSpecVersion;

		public short dmDriverVersion;

		public short dmSize;

		public short dmDriverExtra;

		public int dmFields;

		public int dmPositionX;

		public int dmPositionY;

		public int dmDisplayOrientation;

		public int dmDisplayFixedOutput;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public short dmLogPixels;

		public int dmBitsPerPel;

		public int dmPelsWidth;

		public int dmPelsHeight;

		public int dmDisplayFlags;

		public int dmDisplayFrequency;
	}

	private const int ENUM_CURRENT_SETTINGS = -1;

	private const int MDT_EFFECTIVE_DPI = 0;

	private const uint MONITOR_DEFAULTTONEAREST = 2u;

	public static MonitorCaptureInfo GetCaptureInfo(Screen screen, bool usePhysicalPixels)
	{
		Rectangle bounds = screen.Bounds;
		GetScale(screen, out var scaleX, out var scaleY);
		int width = bounds.Width;
		int height = bounds.Height;
		if (TryGetNativeResolution(screen.DeviceName, out var width2, out var height2))
		{
			width = width2;
			height = height2;
		}
		else if (usePhysicalPixels)
		{
			width = (int)Math.Round((double)bounds.Width * scaleX);
			height = (int)Math.Round((double)bounds.Height * scaleY);
		}
		Size nativeResolution = new Size(width, height);
		Rectangle captureBounds = new Rectangle(bounds.X, bounds.Y, width, height);
		if (!usePhysicalPixels)
		{
			captureBounds = bounds;
			nativeResolution = bounds.Size;
		}
		return new MonitorCaptureInfo(bounds, captureBounds, nativeResolution, scaleX, scaleY);
	}

	public static bool OutputMatchesMonitor(Rectangle outputBounds, MonitorCaptureInfo info)
	{
		if (outputBounds == info.LogicalBounds || outputBounds == info.CaptureBounds)
		{
			return true;
		}
		if (outputBounds.Width == info.NativeResolution.Width && outputBounds.Height == info.NativeResolution.Height && Math.Abs(outputBounds.Left - info.LogicalBounds.Left) <= 64)
		{
			return Math.Abs(outputBounds.Top - info.LogicalBounds.Top) <= 64;
		}
		return false;
	}

	public static void AppendDiagnostics(MonitorCaptureInfo info, StringBuilder log)
	{
		log.AppendLine($"논리: {info.LogicalBounds.Width}x{info.LogicalBounds.Height} @ ({info.LogicalBounds.Left},{info.LogicalBounds.Top})");
		log.AppendLine($"캡처: {info.CaptureBounds.Width}x{info.CaptureBounds.Height} @ ({info.CaptureBounds.Left},{info.CaptureBounds.Top})  (네이티브 {info.NativeResolution.Width}x{info.NativeResolution.Height}, 배율 {info.ScaleX:0.##}x)");
	}

	private static void GetScale(Screen screen, out double scaleX, out double scaleY)
	{
		scaleX = 1.0;
		scaleY = 1.0;
		IntPtr intPtr = MonitorFromPoint(new POINT
		{
			X = screen.Bounds.Left + screen.Bounds.Width / 2,
			Y = screen.Bounds.Top + screen.Bounds.Height / 2
		}, 2u);
		if (!(intPtr == IntPtr.Zero) && GetDpiForMonitor(intPtr, 0, out var dpiX, out var dpiY) == 0)
		{
			scaleX = (double)dpiX / 96.0;
			scaleY = (double)dpiY / 96.0;
		}
	}

	private static bool TryGetNativeResolution(string deviceName, out int width, out int height)
	{
		width = 0;
		height = 0;
		DEVMODE lpDevMode = new DEVMODE
		{
			dmSize = (short)Marshal.SizeOf<DEVMODE>()
		};
		if (!EnumDisplaySettings(deviceName, -1, ref lpDevMode))
		{
			return false;
		}
		width = lpDevMode.dmPelsWidth;
		height = lpDevMode.dmPelsHeight;
		if (width > 0)
		{
			return height > 0;
		}
		return false;
	}

	[DllImport("user32.dll")]
	private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

	[DllImport("Shcore.dll")]
	private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

	[DllImport("user32.dll", CharSet = CharSet.Ansi)]
	private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
}
