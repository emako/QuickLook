using System.Security.Cryptography;
using System.Text;

namespace QuickLook.Plugin.BinaryViewer;

internal static class HashCalculator
{
    public static string ComputeHashAll(byte[] buffer)
    {
        StringBuilder sb = new();

        sb.AppendLine("MD5:");
        sb.AppendLine(ComputeMD5Hash(buffer));
        sb.AppendLine();
        sb.AppendLine("SHA1:");
        sb.AppendLine(ComputeSHA1Hash(buffer));
        sb.AppendLine();
        sb.AppendLine("SHA256:");
        sb.AppendLine(ComputeSHA256Hash(buffer));
        sb.AppendLine();
        sb.AppendLine("SHA384:");
        sb.AppendLine(ComputeSHA384Hash(buffer));
        sb.AppendLine();
        sb.AppendLine("SHA512:");
        sb.AppendLine(ComputeSHA512Hash(buffer));
        sb.AppendLine();
        return sb.ToString();
    }

    public static string ComputeMD5Hash(byte[] input)
    {
        using MD5 md5 = MD5.Create();
        return ComputeHash(md5, input);
    }

    public static string ComputeSHA1Hash(byte[] input)
    {
        using SHA1 sha1 = SHA1.Create();
        return ComputeHash(sha1, input);
    }

    public static string ComputeSHA256Hash(byte[] input)
    {
        using SHA256 sha256 = SHA256.Create();
        return ComputeHash(sha256, input);
    }

    public static string ComputeSHA384Hash(byte[] input)
    {
        using SHA384 sha384 = SHA384.Create();
        return ComputeHash(sha384, input);
    }

    public static string ComputeSHA512Hash(byte[] input)
    {
        using SHA512 sha512 = SHA512.Create();
        return ComputeHash(sha512, input);
    }

    private static string ComputeHash(HashAlgorithm algorithm, byte[] input)
    {
        byte[] hashBytes = algorithm.ComputeHash(input);
        StringBuilder sb = new();

        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
