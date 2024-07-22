using System;
using System.Text;

namespace QuickLook.Plugin.BinaryViewer;

internal static class BinaryConverter
{
    public static string ToHexString(byte[] value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        return ToHexString(value, 0, value.Length);
    }

    public static string ToHexString(byte[] value, int startIndex, int length, char sep = '\0', int? lineLength = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (startIndex < 0 || (startIndex >= value.Length && startIndex > 0))
        {
            throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_GenericPositive");
        }

        if (startIndex > value.Length - length)
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        if (length == 0)
        {
            return string.Empty;
        }

        if (length > 715827882)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_LengthTooLarge");
        }

        StringBuilder result = new(length * 3);
        int bytesProcessed = default;

        for (int i = startIndex; i < startIndex + length; i++)
        {
            byte b = value[i];
            result.Append(GetHexValue(b / 16));
            result.Append(GetHexValue(b % 16));
            if (sep != '\0')
            {
                result.Append(sep);
            }

            bytesProcessed++;

            if (lineLength.HasValue && bytesProcessed >= lineLength.Value)
            {
                result.Append(Environment.NewLine);
                bytesProcessed = 0;
            }
        }

        return result.ToString();
    }

    private static char GetHexValue(int i)
    {
        if (i < 10)
        {
            return (char)(i + 48);
        }

        return (char)(i - 10 + 65);
    }

    public static string ToByteString(byte[] value, int startIndex, int length, char sep = '\0', int? lineLength = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (startIndex < 0 || (startIndex >= value.Length && startIndex > 0))
        {
            throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_GenericPositive");
        }

        if (startIndex > value.Length - length)
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        if (length == 0)
        {
            return string.Empty;
        }

        if (length > 715827882)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_LengthTooLarge");
        }

        StringBuilder result = new(length * 3);
        int bytesProcessed = default;

        for (int i = startIndex; i < startIndex + length; i++)
        {
            byte b = value[i];
            result.Append(b.ToString().PadRight(3, ' '));
            if (sep != '\0')
            {
                result.Append(sep);
            }

            bytesProcessed++;

            if (lineLength.HasValue && bytesProcessed >= lineLength.Value)
            {
                result.Append(Environment.NewLine);
                bytesProcessed = 0;
            }
        }

        return result.ToString();
    }

    public static string ToCharString(byte[] value, int startIndex, int length, char sep = '\0', int? lineLength = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (startIndex < 0 || (startIndex >= value.Length && startIndex > 0))
        {
            throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_StartIndex");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_GenericPositive");
        }

        if (startIndex > value.Length - length)
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        if (length == 0)
        {
            return string.Empty;
        }

        if (length > 715827882)
        {
            throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_LengthTooLarge");
        }

        StringBuilder result = new(length * 3);
        int bytesProcessed = default;

        for (int i = startIndex; i < startIndex + length; i++)
        {
            byte b = value[i];
            result.Append((char)b);
            if (sep != '\0')
            {
                result.Append(sep);
            }

            bytesProcessed++;

            if (lineLength.HasValue && bytesProcessed >= lineLength.Value)
            {
                result.Append(Environment.NewLine);
                bytesProcessed = 0;
            }
        }

        return result.ToString();
    }
}
