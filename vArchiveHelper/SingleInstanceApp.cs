using System;
using System.Threading;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class SingleInstanceApp
{
	private const string MutexName = "Local\\vArchiveHelper.SingleInstance";

	private const string ShowEventName = "Local\\vArchiveHelper.ShowWindow";

	private static Mutex _mutex;

	private static EventWaitHandle _showEvent;

	private static Thread _listener;

	private static volatile bool _listen;

	public static bool TryAcquire()
	{
		_mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
		if (createdNew)
		{
			return true;
		}
		if (!RequestShow())
		{
			MessageBox.Show(
				"vArchiveHelper가 이미 실행 중입니다.\n작업 표시줄이나 트레이 아이콘을 확인하세요.",
				Application.ProductName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Asterisk);
		}
		return false;
	}

	public static void StartShowListener(Action onShowRequested)
	{
		if (onShowRequested == null)
		{
			return;
		}
		_showEvent = new EventWaitHandle(initialState: false, EventResetMode.AutoReset, ShowEventName);
		_listen = true;
		_listener = new Thread(() =>
		{
			while (_listen)
			{
				if (_showEvent != null && _showEvent.WaitOne(500) && _listen)
				{
					try
					{
						onShowRequested();
					}
					catch
					{
					}
				}
			}
		})
		{
			IsBackground = true,
			Name = "vArchiveHelper.ShowListener"
		};
		_listener.Start();
	}

	public static bool RequestShow()
	{
		try
		{
			using EventWaitHandle eventWaitHandle = EventWaitHandle.OpenExisting(ShowEventName);
			eventWaitHandle.Set();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void StopShowListener()
	{
		_listen = false;
		try
		{
			_showEvent?.Set();
		}
		catch
		{
		}
		if (_listener != null && _listener.IsAlive)
		{
			_listener.Join(1000);
		}
		_listener = null;
		_showEvent?.Dispose();
		_showEvent = null;
	}

	public static void Release()
	{
		StopShowListener();
		if (_mutex != null)
		{
			try
			{
				_mutex.ReleaseMutex();
			}
			catch
			{
			}
			_mutex.Dispose();
			_mutex = null;
		}
	}
}
