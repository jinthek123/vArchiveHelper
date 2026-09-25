using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace vArchiveHelper;

internal static class GameWindowCapture
{
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	private struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private const uint PW_RENDERFULLCONTENT = 2u;

	private const int GW_OWNER = 4;

	public static Bitmap TryCapture(string processName, MonitorCaptureInfo monitor, out string error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(processName))
		{
			error = "GameProcessName 없음";
			return null;
		}
		IntPtr intPtr = FindTopLevelWindow(processName.Trim());
		if (intPtr == IntPtr.Zero)
		{
			error = "프로세스 '" + processName + "' 창 없음";
			return null;
		}
		if (!GetWindowRect(intPtr, out var lpRect))
		{
			error = "GetWindowRect 실패";
			return null;
		}
		Rectangle rectangle = new Rectangle(lpRect.Left, lpRect.Top, lpRect.Right - lpRect.Left, lpRect.Bottom - lpRect.Top);
		if (rectangle.Width <= 0 || rectangle.Height <= 0)
		{
			error = "창 크기가 0";
			return null;
		}
		Rectangle rectangle2 = Rectangle.Intersect(rectangle, monitor.LogicalBounds);
		if (rectangle2.Width <= 0 || rectangle2.Height <= 0)
		{
			error = "게임 창이 대상 모니터와 겹치지 않음";
			return null;
		}
		using Bitmap bitmap = CaptureWindowBitmap(intPtr, rectangle.Width, rectangle.Height, out error);
		if (bitmap == null)
		{
			return null;
		}
		if (rectangle2 == rectangle)
		{
			return new Bitmap(bitmap);
		}
		Rectangle region = new Rectangle(rectangle2.Left - rectangle.Left, rectangle2.Top - rectangle.Top, rectangle2.Width, rectangle2.Height);
		return BitmapCrop.Crop(bitmap, region);
	}

	private static Bitmap CaptureWindowBitmap(IntPtr hwnd, int width, int height, out string error)
	{
		error = null;
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		IntPtr hdc = graphics.GetHdc();
		try
		{
			if (!PrintWindow(hwnd, hdc, 2u))
			{
				error = "PrintWindow 실패";
				bitmap.Dispose();
				return null;
			}
			return bitmap;
		}
		finally
		{
			graphics.ReleaseHdc(hdc);
		}
	}

	private static IntPtr FindTopLevelWindow(string processName)
	{
		Process[] processesByName = Process.GetProcessesByName(processName);
		try
		{
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				if (process.MainWindowHandle != IntPtr.Zero && IsWindowVisible(process.MainWindowHandle))
				{
					return process.MainWindowHandle;
				}
			}
			IntPtr found = IntPtr.Zero;
			array = processesByName;
			foreach (Process process2 in array)
			{
				uint pid = (uint)process2.Id;
				EnumWindows(delegate(IntPtr hwnd, IntPtr _)
				{
					if (!IsWindowVisible(hwnd))
					{
						return true;
					}
					GetWindowThreadProcessId(hwnd, out var lpdwProcessId);
					if (lpdwProcessId != pid)
					{
						return true;
					}
					if (GetWindow(hwnd, 4) != IntPtr.Zero)
					{
						return true;
					}
					found = hwnd;
					return false;
				}, IntPtr.Zero);
				if (found != IntPtr.Zero)
				{
					return found;
				}
			}
		}
		finally
		{
			Process[] array = processesByName;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
		}
		return IntPtr.Zero;
	}

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
}
