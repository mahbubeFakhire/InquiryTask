using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Contracts.InquiryRequests.Queries.SampleInquiry;

public class SampleInquiryRequestQueryValidator : AbstractValidator<SampleInquiryRequestQuery>
{
    private const string InvalidReferenceNumber = "invalid_reference_number";

    private static readonly Regex ReferenceNumberPattern =
        new(@"\A[A-Za-z0-9][A-Za-z0-9_-]{0,63}\z");

    public SampleInquiryRequestQueryValidator()
    {
        RuleFor(query => query.ReferenceNumber)
        .Cascade(CascadeMode.Stop)
        .NotEmpty()
            .WithErrorCode(InvalidReferenceNumber)
            .WithMessage("Reference number is required.")

        .Must(referenceNumber => ReferenceNumberPattern.IsMatch(referenceNumber.Trim()))
            .WithErrorCode(InvalidReferenceNumber)
            .WithMessage(
                "Reference number must be 1 to 64 characters long, start with an ASCII letter or digit, " +
                "and contain only ASCII letters, digits, hyphens, or underscores.");
    }
}
