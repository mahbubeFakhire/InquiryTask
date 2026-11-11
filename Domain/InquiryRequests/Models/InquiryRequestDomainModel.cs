namespace Domain.InquiryRequests;


public sealed record InquiryRequestDomainModel
(
    string IdempotencyKey,
    string RequestHash,
    string ReferenceNumber,
    bool BypassCache 
);
