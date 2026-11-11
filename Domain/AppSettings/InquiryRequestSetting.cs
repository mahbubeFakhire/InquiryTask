using System;
using System.Collections.Generic;

namespace Domain.AppSettings;

public sealed class InquiryRequestSetting
{
    public const string SectionName = "InquiryRequestSettings";

    public int ProcessingDeadlineMin { get; set; }

    public int EligibleCacheTtlSeconds { get; set; } = 60;

    public int IneligibleCacheTtlSeconds { get; set; } = 10;

    public List<InquiryProviderSetting> Providers { get; set; } = new();
}

public sealed class InquiryProviderSetting
{
    public string Key { get; set; } = string.Empty;

    public TimeSpan Timeout { get; set; }
}