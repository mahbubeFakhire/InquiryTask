using Domain.Shared.Enums.InquiryRequests;

namespace Application.Contracts.InquiryRequests.Queries.SampleInquiry;

public record SampleInquiryOutputDto
{
    public long InquiryId { get; set; }
    public InquiryOutcome Outcome { get; set; }
    public string ReasonCode { get; set; }
    public ResultSource Source { get; set; }
}
