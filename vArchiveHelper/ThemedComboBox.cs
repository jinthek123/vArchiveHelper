using System.Windows.Forms;

namespace vArchiveHelper;

internal sealed class ThemedComboBox : ComboBox
{
	public ThemedComboBox()
	{
		base.FlatStyle = FlatStyle.Flat;
	}

	protected override void WndProc(ref Message m)
	{
		if (!UiScrollGuard.TryInterceptWheel(this, ref m))
		{
			base.WndProc(ref m);
			if (m.Msg == 15)
			{
				UiThemedInputs.PaintComboDropdownButton(this);
			}
		}
	}
}
