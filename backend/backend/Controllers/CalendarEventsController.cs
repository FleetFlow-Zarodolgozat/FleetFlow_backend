using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using backend.Dtos.CalendarEvents;

namespace backend.Controllers
{
    [Route("api/calendarevents")]
    [ApiController]
    public class CalendarEventsController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        public CalendarEventsController(FlottakezeloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,DRIVER")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            return await this.Run(async () =>
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized();
                ulong userId = ulong.Parse(userIdClaim);
                var calendarEvents = await _context.CalendarEvents.AsNoTracking().Where(x => x.OwnerUserId == userId).Select(x => new CalendarEventDto
                {
                    EventType = x.EventType,
                    Title = x.Title,
                    Description = x.Description,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    RelatedServiceRequestId = x.RelatedServiceRequestId
                }).ToListAsync();
                return Ok(calendarEvents);
            });
        }
    }
}
