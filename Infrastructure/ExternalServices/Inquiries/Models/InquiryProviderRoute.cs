using System;

namespace Infrastructure.ExternalServices.Inquiries.Models;

public sealed record InquiryProviderRoute(IInquiryProvider Provider, TimeSpan Timeout);