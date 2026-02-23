using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        public FlottakezeloDbContext _context { get; set; }
        public StatisticsController(FlottakezeloDbContext context)
        {
            _context = context;
        }

        [HttpGet("fuellogs")]
        [Authorize("ADMIN")]
        public
    }
}
