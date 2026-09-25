using System.Collections.Generic;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class MonitorList
{
	public static List<MonitorComboItem> Build(bool usePhysicalPixels)
	{
		Screen[] allScreens = Screen.AllScreens;
		List<MonitorComboItem> list = new List<MonitorComboItem>(allScreens.Length);
		for (int i = 0; i < allScreens.Length; i++)
		{
			MonitorCaptureInfo captureInfo = MonitorGeometry.GetCaptureInfo(allScreens[i], usePhysicalPixels);
			list.Add(new MonitorComboItem
			{
				Index = i,
				DisplayText = $"모니터 {i + 1} — {captureInfo.NativeResolution.Width}×{captureInfo.NativeResolution.Height} (Index {i + 1})"
			});
		}
		return list;
	}

	public static void SelectByIndex(ComboBox combo, int index)
	{
		for (int i = 0; i < combo.Items.Count; i++)
		{
			if (combo.Items[i] is MonitorComboItem monitorComboItem && monitorComboItem.Index == index)
			{
				combo.SelectedIndex = i;
				return;
			}
		}
		if (combo.Items.Count > 0)
		{
			combo.SelectedIndex = 0;
		}
	}

	public static int GetSelectedIndex(ComboBox combo)
	{
		if (combo.SelectedItem is MonitorComboItem monitorComboItem)
		{
			return monitorComboItem.Index;
		}
		return 0;
	}
}
