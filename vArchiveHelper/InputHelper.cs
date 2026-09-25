using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace vArchiveHelper;

internal static class InputHelper
{
	private const byte VK_MENU = 18;

	private const byte VK_INSERT = 45;

	private const uint KEYEVENTF_KEYUP = 2u;

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	public static void SendAltInsert()
	{
		keybd_event(18, 0, 0u, UIntPtr.Zero);
		Thread.Sleep(30);
		keybd_event(45, 0, 0u, UIntPtr.Zero);
		keybd_event(45, 0, 2u, UIntPtr.Zero);
		Thread.Sleep(30);
		keybd_event(18, 0, 2u, UIntPtr.Zero);
	}
}
