using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extensions;

public static class StringExtension
{
    private static readonly IReadOnlyDictionary<string, bool> hostBooleanDictionary = new Dictionary<string, bool> { { "N", false }, { "Y", true } };

    public static bool ToHostBoolean(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var upperInput = input.ToUpper();
        if (hostBooleanDictionary.ContainsKey(upperInput))
        {
            return hostBooleanDictionary[upperInput];
        }
        throw new InvalidCastException("Message Is Not Valid Boolean");
    }

    public static int ToIntWithDefault(this string stringNo, int defaultValue = -1)
    {
        return int.TryParse(stringNo, out int value) ? value : defaultValue;
    }

    public static long ToLong(this string stringNo, int defaultValue = 0)
    {
        return long.TryParse(stringNo, out long value) ? value : defaultValue;
    }


    public static string SubStringFromEnd(this string str, int index)
    {
        return str.Length > index ? str.Substring(str.Length - index) : str;
    }

    private static readonly Regex DateRegex = new(@"^[1-9][0-9][0-9][0-9][/][0-9][0-9][/][0-9][0-9]$", RegexOptions.Compiled);
    private static readonly Regex DateTimeRegex = new(@"^[1-9][0-9][0-9][0-9][/][0-9][0-9][/][0-9][0-9]\s([0-1][0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled);
    private static readonly Regex DateTimeToSecondsRegex = new(@"^[1-9][0-9][0-9][0-9][/][0-9][0-9][/][0-9][0-9]\s([0-1][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", RegexOptions.Compiled);
    private static readonly Regex TimeToMinsRegex = new(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled);
    private static readonly Regex TimeToSecondsRegex = new(@"^([0-1][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", RegexOptions.Compiled);

    public static bool IsCorrectDateAgreedFormat(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            return DateRegex.IsMatch(str) || DateTimeRegex.IsMatch(str) || DateTimeToSecondsRegex.IsMatch(str);
        }
        return false;
    }

    public static bool IsCorrectTimeAgreedFormat(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            return TimeToMinsRegex.IsMatch(str) || TimeToSecondsRegex.IsMatch(str);
        }
        return false;
    }

    public static string ToDateFormat(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str) && str.Length == 8)
        {
            return string.Format("{0:####/##/##}", Convert.ToInt32(str));
        }
        else if (!string.IsNullOrWhiteSpace(str) && str.Length == 14)
        {
            return string.Format("{0:####/##/## ##:##:##}", Convert.ToInt64(str));
        }
        return str;
    }

    public static string ToCurrencyFormat(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            return string.Format("{0:N0}", Convert.ToDecimal(str));
        }
        return str;
    }

    public static string To98FirstMobileNo(this string str)
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            var last10number = str.Length > 10 ? str.Substring(str.Length - 10) : str;
            return string.Concat("98", last10number);
        }
        return str;
    }

    public static string RemoveDecimalPointAndPrecisions(this string str)
    {
        if (str == null)
            return null;

        int idexOfPoint = str.IndexOf(".", StringComparison.OrdinalIgnoreCase);

        if (idexOfPoint > 0)
        {
            return str.Substring(0, idexOfPoint);
        }
        return str;
    }


    public static string MaskCardOrAccountOrIban(this string source)
    {
        return source.Length >= 16 ? $"{source[..4]}{new string('*', source.Length - 8)}{source[^4..]}" : $"{source[..2]}{new string('*', source.Length - 4)}{source[^2..]}";
    }

    public static List<String> ToSeperateInput(this string input, string seprator = ",")
    {
        var listString = input.Split(seprator).ToList();
        return listString;
    }
    public static bool IsNullOrEmpty(this string source)
    {
        return String.IsNullOrEmpty(source);
    }
    public static bool IsNullOrWhiteSpace(this string source)
    {
        return String.IsNullOrWhiteSpace(source);
    }
    public static decimal ToDecimal(this string stringNo)
    {
        return decimal.TryParse(stringNo, out decimal value) ? value : default;
    }
    public static int ToInt(this string stringNo)
    {
        return int.TryParse(stringNo, out int value) ? value : default;
    }

    public static int ToIntTime(this string stringNo)
    {
        return int.TryParse(stringNo.Replace(":", "").PadRight(8, '0'), out int value) ? value : default;
    }
    public static string NormalizeToPersionString(this string source)
    {
        if (source is null)
        {
            return null;
        }

        return source
                     .Replace('ئ', 'ی')
                     .Replace('ي', 'ی')
                     .Replace('ك', 'ک')
                     .Replace('ؤ', 'و')
                     .Replace('آ', 'ا')
                     .Replace('أ', 'ا');
    }

    public static string TrimToMaxLength(this string source, int maxLength)
    {
        if (source is null)
        {
            return null;
        }

        if (source.Length > maxLength)
        {
            return source[..maxLength];
        }
        else
        {
            return source;
        }             
    }

    public static Guid ToGuid(this string valueForConvet)
    {
        return Guid.TryParse(valueForConvet, out Guid value) ? value : default;
    }

    public static ulong ToULong(this string stringNo, ulong defaultValue = 0)
    {
        return ulong.TryParse(stringNo, out ulong value) ? value : defaultValue;
    }
    public static string NormalizeIranMobile(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0098")) return "0" + digits[4..];
        if (digits.StartsWith("98")) return "0" + digits[2..];
        if (!digits.StartsWith("0") && digits.Length == 10) return "0" + digits;
        return digits;
    }

    public static string MaskIranMobile(this string msisdn)
    {
        return string.IsNullOrEmpty(msisdn) || msisdn.Length < 6
            ? "****"
            : $"{msisdn[..2]}****{msisdn[^4..]}";
    }
}
