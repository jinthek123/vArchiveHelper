using System;
using System.Text;
using System.Windows.Forms;

namespace vArchiveHelper;

internal static class CaptureHotkey
{
	public const int ModAlt = 1;

	public const int ModControl = 16;

	public const int ModShift = 4;

	public const int ModWin = 8;

	public const int ModNoRepeat = 16384;

	public static int ModifiersFromKeyData(Keys keyData)
	{
		int num = 0;
		Keys keys = keyData & Keys.Modifiers;
		if (keys.HasFlag(Keys.Alt))
		{
			num |= 1;
		}
		if (keys.HasFlag(Keys.Control))
		{
			num |= 0x10;
		}
		if (keys.HasFlag(Keys.Shift))
		{
			num |= 4;
		}
		return num;
	}

	public static Keys KeyFromKeyData(Keys keyData)
	{
		return keyData & Keys.KeyCode;
	}

	public static bool TryParseKeyData(Keys keyData, out int modifiers, out int virtualKey, out string error)
	{
		modifiers = ModifiersFromKeyData(keyData);
		Keys keys = KeyFromKeyData(keyData);
		if (keys == Keys.None)
		{
			error = "유효한 키가 아닙니다.";
			virtualKey = 0;
			return false;
		}
		if (IsModifierOnlyKey(keys))
		{
			error = "Ctrl/Alt/Shift만으로는 등록할 수 없습니다. 다른 키와 함께 누르세요.";
			virtualKey = 0;
			return false;
		}
		if ((uint)(keys - 91) <= 1u)
		{
			error = "Windows 키는 사용할 수 없습니다.";
			virtualKey = 0;
			return false;
		}
		virtualKey = (int)keys;
		error = null;
		return true;
	}

	public static bool IsModifierOnlyKey(Keys key)
	{
		if ((uint)(key - 16) <= 2u || (uint)(key - 91) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static string FormatDisplay(int modifiers, int virtualKey)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((modifiers & 0x10) != 0)
		{
			stringBuilder.Append("Ctrl+");
		}
		if ((modifiers & 4) != 0)
		{
			stringBuilder.Append("Shift+");
		}
		if ((modifiers & 1) != 0)
		{
			stringBuilder.Append("Alt+");
		}
		Keys keys = (Keys)virtualKey;
		stringBuilder.Append(keys.ToString());
		return stringBuilder.ToString();
	}

	public static bool IsValidForRegistration(int modifiers, int virtualKey, out string error)
	{
		if ((modifiers & 8) != 0)
		{
			error = "Windows 키 조합은 사용할 수 없습니다.";
			return false;
		}
		if (virtualKey <= 0 || !Enum.IsDefined(typeof(Keys), virtualKey))
		{
			error = "등록할 수 없는 키입니다.";
			return false;
		}
		if (IsModifierOnlyKey((Keys)virtualKey))
		{
			error = "단독 수정자 키는 사용할 수 없습니다.";
			return false;
		}
		error = null;
		return true;
	}
}
