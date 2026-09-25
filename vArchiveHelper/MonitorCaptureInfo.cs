using System.Drawing;

namespace vArchiveHelper;

internal sealed class MonitorCaptureInfo
{
	public Rectangle LogicalBounds { get; }

	public Rectangle CaptureBounds { get; }

	public Rectangle PhysicalBounds { get; }

	public Size NativeResolution { get; }

	public double ScaleX { get; }

	public double ScaleY { get; }

	public MonitorCaptureInfo(Rectangle logicalBounds, Rectangle captureBounds, Size nativeResolution, double scaleX, double scaleY)
	{
		LogicalBounds = logicalBounds;
		CaptureBounds = captureBounds;
		PhysicalBounds = captureBounds;
		NativeResolution = nativeResolution;
		ScaleX = scaleX;
		ScaleY = scaleY;
	}
}
