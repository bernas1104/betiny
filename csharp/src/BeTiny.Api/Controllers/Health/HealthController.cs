using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BeTiny.Api.Controllers.Health
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : Controller
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Returns the health status of the application and dependencies.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Get()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    exception = e.Value.Exception?.Message,
                    duration = e.Value.Duration.ToString()
                })
            };

            return Ok(result);
        }
    }
}
