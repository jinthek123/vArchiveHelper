using System.Drawing;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class AppIcon
{
	private static Icon _cached;

	public static Icon Get()
	{
		if (_cached == null)
		{
			_cached = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		return _cached;
	}
}
