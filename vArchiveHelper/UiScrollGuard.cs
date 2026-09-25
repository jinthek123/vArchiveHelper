using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class UiScrollGuard
{
	private const int WM_MOUSEWHEEL = 522;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	public static bool TryInterceptWheel(Control source, ref Message m)
	{
		if (m.Msg != 522 || ShouldAllowWheelOnSource(source))
		{
			return false;
		}
		Panel panel = FindScrollParent(source);
		if (panel != null)
		{
			SendMessage(panel.Handle, 522, m.WParam, m.LParam);
		}
		return true;
	}

	private static bool ShouldAllowWheelOnSource(Control source)
	{
		if (source.Focused)
		{
			return true;
		}
		if (source is ComboBox comboBox)
		{
			return comboBox.DroppedDown;
		}
		return false;
	}

	private static Panel FindScrollParent(Control source)
	{
		for (Control parent = source.Parent; parent != null; parent = parent.Parent)
		{
			if (parent is Panel { AutoScroll: not false } panel)
			{
				return panel;
			}
		}
		return null;
	}
}
