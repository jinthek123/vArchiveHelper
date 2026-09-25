using System.Drawing;
using System.Drawing.Imaging;

namespace vArchiveHelper;

internal static class BitmapCrop
{
	public static Bitmap Crop(Bitmap source, Rectangle region)
	{
		if (region.X == 0 && region.Y == 0 && region.Width == source.Width && region.Height == source.Height)
		{
			return new Bitmap(source);
		}
		Bitmap bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(source, new Rectangle(0, 0, region.Width, region.Height), region, GraphicsUnit.Pixel);
		return bitmap;
	}
}
