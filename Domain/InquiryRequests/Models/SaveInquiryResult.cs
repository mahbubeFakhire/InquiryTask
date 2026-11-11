using Domain.Enums.InquiryRequests;

namespace Domain.InquiryRequests.Models;

public sealed record SaveInquiryResult(bool IsChanged, InquiryRequest Request = null);
