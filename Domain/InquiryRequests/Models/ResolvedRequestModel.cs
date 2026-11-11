using Domain.Enums.InquiryRequests;

namespace Domain.InquiryRequests.Models;

public record ResolvedRequestModel(InquiryRequestResolvationType ResolvationType, InquiryRequest ExistingRequest);

