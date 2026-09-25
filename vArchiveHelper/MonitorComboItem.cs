namespace vArchiveHelper;

internal sealed class MonitorComboItem
{
	public int Index { get; set; }

	public string DisplayText { get; set; }

	public override string ToString()
	{
		return DisplayText;
	}
}
