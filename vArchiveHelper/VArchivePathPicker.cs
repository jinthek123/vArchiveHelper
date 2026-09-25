using System.IO;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class VArchivePathPicker
{
	public static bool TryBrowse(IWin32Window owner, string currentPath, out string selectedPath)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = "v-archive|v-archive.exe|실행 파일|*.exe|모든 파일|*.*",
			Title = "v-archive.exe 선택"
		};
		if (!string.IsNullOrWhiteSpace(currentPath))
		{
			try
			{
				openFileDialog.InitialDirectory = Path.GetDirectoryName(currentPath);
				openFileDialog.FileName = Path.GetFileName(currentPath);
			}
			catch
			{
			}
		}
		if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
		{
			selectedPath = null;
			return false;
		}
		selectedPath = openFileDialog.FileName;
		return true;
	}
}
