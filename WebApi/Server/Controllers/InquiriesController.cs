using Application.Contracts.InquiryRequests.Queries.SampleInquiry;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Server.Controllers;

[Route("api/inquiries")]
public class InquiriesController : ApiController
{
    [HttpPost("sample")]
    public async Task<ActionResult<SampleInquiryOutputDto>> Sample([FromBody] SampleInquiryRequestQuery command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }

}
