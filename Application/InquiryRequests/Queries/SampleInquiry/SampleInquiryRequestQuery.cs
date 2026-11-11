using MediatR;

namespace Application.Contracts.InquiryRequests.Queries.SampleInquiry;

public record SampleInquiryRequestQuery(string ReferenceNumber, bool BypassCache) : IRequest<SampleInquiryOutputDto>;