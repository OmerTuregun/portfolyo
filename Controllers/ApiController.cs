using Microsoft.AspNetCore.Mvc;
using My_Portfolyo.Services;

namespace My_Portfolyo.Controllers
{
    [ApiController]
    [Route("api/github/contributions")]
    public class ApiController : ControllerBase
    {
        private readonly GitHubContributionsService _contributions;

        public ApiController(GitHubContributionsService contributions)
        {
            _contributions = contributions;
        }

        [HttpGet]
        public async Task<IActionResult> GetContributions(CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "public, max-age=3600";

            var data = await _contributions.GetAsync(cancellationToken);
            if (data == null)
            {
                return NoContent();
            }

            return Ok(data);
        }
    }
}
