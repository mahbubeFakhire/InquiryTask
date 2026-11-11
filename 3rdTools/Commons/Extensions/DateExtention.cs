using System;
using System.Globalization;

namespace Extensions;

public static class DateExtention
{
    private static readonly PersianCalendar PersianCalendar = new();

    public static DateTime? GetMiladiDateFromPersianDate(string date)
    {
        try
        {
            PersianCalendar pc = new();
            var pureDate = date.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] d = pureDate.Split('/');

            var miladiDate = pc.ToDateTime(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]), 0, 0, 0, 0, PersianCalendar.PersianEra);
            return miladiDate;
        }
        catch
        {
            return null;
        }
    }
    public static string GetMiladiStringDateFromPersianDate(string date)
    {
        try
        {
            PersianCalendar pc = new();
            var pureDate = date.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] d = pureDate.Split('/');

            var miladiDate = pc.ToDateTime(int.Parse(d[0]), int.Parse(d[1]), int.Parse(d[2]), 0, 0, 0, 0, PersianCalendar.PersianEra);
            var greDate = miladiDate.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            return greDate;
        }
        catch
        {
            return null;
        }
    }

    public static string GetPersianDate(string dateStr)
    {
        DateTime.TryParse(dateStr, out DateTime date);
        try
        {
            PersianCalendar pc = new();
            string pdate = pc.GetYear(date).ToString().PadLeft(4, '0') + pc.GetMonth(date).ToString().PadLeft(2, '0')
                + pc.GetDayOfMonth(date).ToString().PadLeft(2, '0');
            return pdate;
        }
        catch
        {
            return "";
        }

    }

    public static string GetPersianDate(DateTime date)
    {
        try
        {
            PersianCalendar pc = new();
            string pdate = pc.GetYear(date).ToString().PadLeft(4, '0') + pc.GetMonth(date).ToString().PadLeft(2, '0')
                + pc.GetDayOfMonth(date).ToString().PadLeft(2, '0');
            return pdate;
        }
        catch
        {
            return "";
        }

    }


    public static string GetPersianDateFromDateTimeWithFormat(DateTime date, string format)
    {
        PersianCalendar persianCalendar = new PersianCalendar();

        int year = persianCalendar.GetYear(date);
        int month = persianCalendar.GetMonth(date);
        int day = persianCalendar.GetDayOfMonth(date);
        int hour = persianCalendar.GetHour(date);
        int minute = persianCalendar.GetMinute(date);
        int second = persianCalendar.GetSecond(date);

        format = format.Replace("yyyy", year.ToString());
        format = format.Replace("MM", month.ToString());
        format = format.Replace("dd", day.ToString());
        format = format.Replace("HH", hour.ToString());
        format = format.Replace("mm", minute.ToString());
        format = format.Replace("ss", second.ToString());


        return format;
    }

    public static string GetNowPersianDate()
    {
        PersianCalendar pc = new();
        string pdate = pc.GetYear(DateTime.Now).ToString().PadLeft(4, '0') + pc.GetMonth(DateTime.Now).ToString().PadLeft(2, '0')
            + pc.GetDayOfMonth(DateTime.Now).ToString().PadLeft(2, '0');
        return pdate;
    }

    public static string GetPersianDate(DateTime? date, int format)
    {
        string pdate = string.Empty;

        if (date.HasValue)
        {
            PersianCalendar pc = new();

            switch (format)
            {
                case 1:
                    pdate = $"{pc.GetYear(date.Value).ToString().PadLeft(4, '0')}/{pc.GetMonth(date.Value).ToString().PadLeft(2, '0')}/{pc.GetDayOfMonth(date.Value).ToString().PadLeft(2, '0')}";
                    break;
                case 2:
                    pdate = $"{pc.GetDayOfMonth(date.Value).ToString().PadLeft(2, '0')}/{pc.GetMonth(date.Value).ToString().PadLeft(2, '0')}/{pc.GetYear(date.Value).ToString().PadLeft(4, '0')}";
                    break;

                default:
                    pdate = $"{pc.GetYear(date.Value).ToString().PadLeft(4, '0')}{pc.GetMonth(date.Value).ToString().PadLeft(2, '0')}{pc.GetDayOfMonth(date.Value).ToString().PadLeft(2, '0')}";
                    break;
            }
        }

        return pdate;
    }

    private static readonly TimeZoneInfo IranTimeZone =
    TimeZoneInfo.CreateCustomTimeZone(
        id: "Iran Standard Time Fixed",
        baseUtcOffset: TimeSpan.FromMinutes(210),
        displayName: "(UTC+03:30) Iran Standard Time",
        standardDisplayName: "Iran Standard Time");

    public static (DateTime StartUtc, DateTime EndUtc) GetDailyPeriodUtc(DateTime utcNow)
    {
        utcNow = EnsureUtc(utcNow);

        var iranNow =
            TimeZoneInfo.ConvertTimeFromUtc(
                utcNow,
                IranTimeZone);

        /*
         * شروع روز در ایران.
         *
         * برای Daily مهم timezone ایران است.
         * شماره روز شمسی یا میلادی تفاوتی در مرز نیمه‌شب ندارد.
         */
        var startLocal =
            DateTime.SpecifyKind(
                iranNow.Date,
                DateTimeKind.Unspecified);

        var endLocal =
            startLocal.AddDays(1);

        return (
            StartUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    startLocal,
                    IranTimeZone),

            EndUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    endLocal,
                    IranTimeZone));
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetMonthlyPeriodUtc(DateTime utcNow)
    {
        utcNow = EnsureUtc(utcNow);

        var iranNow =
            TimeZoneInfo.ConvertTimeFromUtc(
                utcNow,
                IranTimeZone);

        var persianYear =
            PersianCalendar.GetYear(iranNow);

        var persianMonth =
            PersianCalendar.GetMonth(iranNow);

        /*
         * روز اول ماه شمسی جاری در ساعت 00:00 ایران
         */
        var startLocal =
            PersianCalendar.ToDateTime(
                persianYear,
                persianMonth,
                1,
                0,
                0,
                0,
                0);

        /*
         * شروع ماه شمسی بعدی.
         */
        var nextYear =
            persianMonth == 12
                ? persianYear + 1
                : persianYear;

        var nextMonth =
            persianMonth == 12
                ? 1
                : persianMonth + 1;

        var endLocal =
            PersianCalendar.ToDateTime(
                nextYear,
                nextMonth,
                1,
                0,
                0,
                0,
                0);

        startLocal =
            DateTime.SpecifyKind(
                startLocal,
                DateTimeKind.Unspecified);

        endLocal =
            DateTime.SpecifyKind(
                endLocal,
                DateTimeKind.Unspecified);

        return (
            StartUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    startLocal,
                    IranTimeZone),

            EndUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    endLocal,
                    IranTimeZone));
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetYearlyPeriodUtc(DateTime utcNow)
    {
        utcNow = EnsureUtc(utcNow);

        var iranNow =
            TimeZoneInfo.ConvertTimeFromUtc(
                utcNow,
                IranTimeZone);

        var persianYear =
            PersianCalendar.GetYear(iranNow);

        var startLocal =
            PersianCalendar.ToDateTime(
                persianYear,
                1,
                1,
                0,
                0,
                0,
                0);

        var endLocal =
            PersianCalendar.ToDateTime(
                persianYear + 1,
                1,
                1,
                0,
                0,
                0,
                0);

        startLocal =
            DateTime.SpecifyKind(
                startLocal,
                DateTimeKind.Unspecified);

        endLocal =
            DateTime.SpecifyKind(
                endLocal,
                DateTimeKind.Unspecified);

        return (
            StartUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    startLocal,
                    IranTimeZone),

            EndUtc:
                TimeZoneInfo.ConvertTimeToUtc(
                    endLocal,
                    IranTimeZone));
    }

    public static DateTime EnsureUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();

        throw new ArgumentException(
            "DateTime must have Utc Kind.",
            nameof(dateTime));
    }

}
