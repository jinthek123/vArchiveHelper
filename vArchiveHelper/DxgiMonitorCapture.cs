using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice;
using Vortice.DXGI;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace vArchiveHelper;

internal static class DxgiMonitorCapture
{
	private const int AcquireTimeoutMs = 250;

	private const int MaxAcquireAttempts = 8;

	private static string _diagnosticLogPath;

	private static string _lastLoggedExceptionKey;

	public static Bitmap TryCapture(MonitorCaptureInfo monitor, out string error, bool writeDiagnosticLog = false)
	{
		error = null;
		try
		{
			return Capture(monitor);
		}
		catch (Exception ex)
		{
			error = FormatError(ex, writeDiagnosticLog ? WriteDxgiErrorLogOnce(ex) : null);
			return null;
		}
	}

	private static string FormatError(Exception ex, string logPath)
	{
		string text = "DXGI FATAL: " + ex.GetType().Name + ": " + ex.Message;
		if (!string.IsNullOrEmpty(logPath))
		{
			return text + " (log: " + logPath + ")";
		}
		return text;
	}

	private static string WriteDxgiErrorLogOnce(Exception ex)
	{
		try
		{
			string text = ex.GetType().FullName + "|" + ex.Message;
			if (text == _lastLoggedExceptionKey && !string.IsNullOrEmpty(_diagnosticLogPath))
			{
				return _diagnosticLogPath;
			}
			string text2 = Path.Combine(Path.GetTempPath(), "vArchiveHelper-dxgi.log");
			string contents = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
			File.AppendAllText(text2, contents);
			_lastLoggedExceptionKey = text;
			_diagnosticLogPath = text2;
			return text2;
		}
		catch
		{
			return null;
		}
	}

	private static Bitmap Capture(MonitorCaptureInfo monitor)
	{
		using IDXGIFactory1 iDXGIFactory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
		IDXGIAdapter1 iDXGIAdapter = null;
		IDXGIOutput1 iDXGIOutput = null;
		try
		{
			IDXGIAdapter1 adapterOut;
			for (int i = 0; iDXGIFactory.EnumAdapters1(i, out adapterOut).Success; i++)
			{
				if (adapterOut == null)
				{
					continue;
				}
				bool flag = false;
				try
				{
					IDXGIOutput outputOut;
					for (int j = 0; adapterOut.EnumOutputs(j, out outputOut).Success; j++)
					{
						if (outputOut == null)
						{
							continue;
						}
						RawRect desktopCoordinates = outputOut.Description.DesktopCoordinates;
						if (!MonitorGeometry.OutputMatchesMonitor(new Rectangle(desktopCoordinates.Left, desktopCoordinates.Top, desktopCoordinates.Right - desktopCoordinates.Left, desktopCoordinates.Bottom - desktopCoordinates.Top), monitor))
						{
							outputOut.Dispose();
							continue;
						}
						iDXGIAdapter = adapterOut;
						try
						{
							iDXGIOutput = (outputOut as IDXGIOutput1) ?? outputOut.QueryInterface<IDXGIOutput1>();
						}
						catch (Exception innerException)
						{
							outputOut.Dispose();
							throw new InvalidOperationException("STAGE=QueryInterface(Output1)", innerException);
						}
						finally
						{
							if (iDXGIOutput != null && (object)iDXGIOutput != outputOut)
							{
								outputOut.Dispose();
							}
						}
						if (iDXGIOutput == null)
						{
							outputOut.Dispose();
							continue;
						}
						flag = true;
						break;
					}
				}
				finally
				{
					if (!flag)
					{
						adapterOut.Dispose();
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (iDXGIOutput == null)
			{
				throw new InvalidOperationException("DXGI: 해당 모니터 출력을 찾지 못했습니다.");
			}
			using ID3D11Device iD3D11Device = CreateD3D11Device(iDXGIOutput, iDXGIAdapter);
			using ID3D11DeviceContext iD3D11DeviceContext = iD3D11Device.ImmediateContext;
			if (iD3D11DeviceContext == null)
			{
				throw new InvalidOperationException("DXGI: D3D11 컨텍스트 생성 실패");
			}
			Exception ex = null;
			IDXGIOutputDuplication iDXGIOutputDuplication = null;
			try
			{
				try
				{
					iDXGIOutputDuplication = iDXGIOutput.DuplicateOutput(iD3D11Device);
				}
				catch (Exception innerException2)
				{
					throw new InvalidOperationException("STAGE=DuplicateOutput(first)", innerException2);
				}
				if (iDXGIOutputDuplication == null)
				{
					throw new InvalidOperationException("DXGI: DuplicateOutput 실패");
				}
				for (int k = 0; k < 8; k++)
				{
					bool flag2 = false;
					OutduplFrameInfo frameInfo;
					IDXGIResource desktopResource;
					Result result = iDXGIOutputDuplication.AcquireNextFrame(250, out frameInfo, out desktopResource);
					if (result == Vortice.DXGI.ResultCode.WaitTimeout)
					{
						continue;
					}
					if (result == Vortice.DXGI.ResultCode.AccessLost)
					{
						iDXGIOutputDuplication.Dispose();
						try
						{
							iDXGIOutputDuplication = iDXGIOutput.DuplicateOutput(iD3D11Device);
						}
						catch (Exception innerException3)
						{
							throw new InvalidOperationException("STAGE=DuplicateOutput(recreate-after-AccessLost)", innerException3);
						}
						if (iDXGIOutputDuplication == null)
						{
							throw new InvalidOperationException("DXGI: AccessLost 후 DuplicateOutput 실패");
						}
						continue;
					}
					if (result.Failure)
					{
						ex = new InvalidOperationException($"DXGI AcquireNextFrame: {result}");
						continue;
					}
					if (desktopResource == null)
					{
						ex = new InvalidOperationException("DXGI: desktopResource 가 null");
						continue;
					}
					try
					{
						flag2 = true;
						ID3D11Texture2D iD3D11Texture2D;
						try
						{
							iD3D11Texture2D = desktopResource.QueryInterface<ID3D11Texture2D>();
						}
						catch (Exception innerException4)
						{
							throw new InvalidOperationException("STAGE=QueryInterface(Texture2D)", innerException4);
						}
						using (iD3D11Texture2D)
						{
							if (iD3D11Texture2D == null)
							{
								ex = new InvalidOperationException("DXGI: QueryInterface(ID3D11Texture2D) 실패");
								continue;
							}
							Texture2DDescription description = iD3D11Texture2D.Description;
							Texture2DDescription description2 = description;
							description2.BindFlags = BindFlags.None;
							description2.CPUAccessFlags = CpuAccessFlags.Read;
							description2.Usage = ResourceUsage.Staging;
							description2.MiscFlags = ResourceOptionFlags.None;
							ID3D11Texture2D iD3D11Texture2D2;
							try
							{
								iD3D11Texture2D2 = iD3D11Device.CreateTexture2D(description2);
							}
							catch (Exception innerException5)
							{
								throw new InvalidOperationException("STAGE=CreateTexture2D(staging)", innerException5);
							}
							using (iD3D11Texture2D2)
							{
								if (iD3D11Texture2D2 == null)
								{
									ex = new InvalidOperationException("DXGI: CreateTexture2D(staging) 실패");
									continue;
								}
								try
								{
									iD3D11DeviceContext.CopyResource(iD3D11Texture2D2, iD3D11Texture2D);
								}
								catch (Exception innerException6)
								{
									throw new InvalidOperationException("STAGE=CopyResource", innerException6);
								}
								MappedSubresource mappedSubresource = iD3D11DeviceContext.Map(iD3D11Texture2D2, 0);
								try
								{
									return CopyMappedToBitmap(mappedSubresource.DataPointer, mappedSubresource.RowPitch, description.Width, description.Height);
								}
								finally
								{
									iD3D11DeviceContext.Unmap(iD3D11Texture2D2, 0);
								}
							}
						}
					}
					catch (Exception ex2)
					{
						ex = new InvalidOperationException("DXGI: 프레임 처리 중 예외 - " + ex2.GetType().Name + ": " + ex2.Message, ex2);
					}
					finally
					{
						if (flag2)
						{
							try
							{
								iDXGIOutputDuplication.ReleaseFrame();
							}
							catch (Exception innerException7)
							{
								ex = new InvalidOperationException("STAGE=ReleaseFrame", innerException7);
							}
						}
						desktopResource?.Dispose();
					}
				}
				throw ex ?? new InvalidOperationException("DXGI: 프레임 획득 시간 초과");
			}
			finally
			{
				iDXGIOutputDuplication?.Dispose();
			}
		}
		finally
		{
			iDXGIOutput?.Dispose();
			iDXGIAdapter?.Dispose();
		}
	}

	private static ID3D11Device CreateD3D11Device(IDXGIOutput1 output1, IDXGIAdapter1 enumeratedAdapter)
	{
		DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;
		FeatureLevel[] featureLevels = new FeatureLevel[1] { FeatureLevel.Level_11_0 };
		if (TryCreateDevice(null, DriverType.Hardware, flags, featureLevels, out var device))
		{
			return device;
		}
		if (enumeratedAdapter != null && enumeratedAdapter.NativePointer != IntPtr.Zero && TryCreateDevice(enumeratedAdapter, DriverType.Unknown, flags, featureLevels, out device))
		{
			return device;
		}
		using (IDXGIAdapter1 iDXGIAdapter = output1.GetParent<IDXGIAdapter1>())
		{
			if (iDXGIAdapter != null && iDXGIAdapter.NativePointer != IntPtr.Zero && TryCreateDevice(iDXGIAdapter, DriverType.Unknown, flags, featureLevels, out device))
			{
				return device;
			}
		}
		throw new InvalidOperationException("DXGI: D3D11 장치를 만들 수 없습니다.");
	}

	private static bool TryCreateDevice(IDXGIAdapter adapter, DriverType driverType, DeviceCreationFlags flags, FeatureLevel[] featureLevels, out ID3D11Device device)
	{
		device = null;
		if (D3D11.D3D11CreateDevice(adapter, driverType, flags, featureLevels, out device).Success)
		{
			return device != null;
		}
		return false;
	}

	private static Bitmap CopyMappedToBitmap(IntPtr source, int rowPitch, int width, int height)
	{
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		try
		{
			int stride = bitmapData.Stride;
			int count = width * 4;
			for (int i = 0; i < height; i++)
			{
				IntPtr src = IntPtr.Add(source, i * rowPitch);
				CopyMemory(IntPtr.Add(bitmapData.Scan0, i * stride), src, (uint)count);
			}
		}
		finally
		{
			bitmap.UnlockBits(bitmapData);
		}
		return bitmap;
	}

	[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
	private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
}
