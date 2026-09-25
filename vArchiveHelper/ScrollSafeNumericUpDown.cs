using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class ScrollSafeNumericUpDown : NumericUpDown
{
	protected override void WndProc(ref Message m)
	{
		if (!UiScrollGuard.TryInterceptWheel(this, ref m))
		{
			base.WndProc(ref m);
		}
	}
}
