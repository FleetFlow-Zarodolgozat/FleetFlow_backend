using backend.Dtos;
using backend.Dtos.FuelLogs;
using backend.Dtos.Trips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly FlottakezeloDbContext _context;
        public TripsController(FlottakezeloDbContext context)
        {
            _context = context;
        }

        [HttpGet("admin/trips")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetFuellogs([FromQuery] Querry query)
        {
            return await this.Run(async () =>
            {
                var tripsQuery = _context.Trips.AsNoTracking().Include(x => x.Driver).Select(v => new TripDto
                {
                    Id = v.Id,
                    UserEmail = v.Driver.User.Email,
                    LicensePlate = v.Vehicle.LicensePlate,
                    IsDeleted = v.IsDeleted,
                    StartTime = v.StartTime,
                    Long = v.EndTime - v.StartTime,
                    StartLocation = v.StartLocation,
                    EndLocation = v.EndLocation,
                    DistanceKm = v.DistanceKm,
                    Notes = v.Notes
                });
                var q = query.StringQ?.Trim();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    tripsQuery = tripsQuery.Where(x =>
                        x.LicensePlate.Contains(q) ||
                        x.UserEmail.Contains(q) ||
                        x.StartLocation.Contains(q) ||
                        x.EndLocation.Contains(q) ||
                        (x.Notes != null && x.Notes.Contains(q)) ||
                        x.DistanceKm.ToString().Contains(q) ||
                        x.Long.ToString().Contains(q) ||
                        x.StartTime.ToString().Contains(q)
                    );
                }
                if (query.IsDeleted == true)
                    tripsQuery = tripsQuery.Where(x => x.IsDeleted == true);
                else
                    tripsQuery = tripsQuery.Where(x => x.IsDeleted == false);
                var totalCount = await tripsQuery.CountAsync();
                tripsQuery = (query.Ordering?.ToLower()) switch
                {
                    "long" => tripsQuery.OrderBy(x => x.Long),
                    "long_desc" => tripsQuery.OrderByDescending(x => x.Long),
                    "distance" => tripsQuery.OrderBy(x => x.DistanceKm),
                    "distance_desc" => tripsQuery.OrderByDescending(x => x.DistanceKm),
                    "starttime" => tripsQuery.OrderBy(x => x.StartTime),
                    "starttime_desc" => tripsQuery.OrderByDescending(x => x.StartTime),
                    _ => tripsQuery.OrderByDescending(x => x.Id)
                };
                var page = query.Page < 1 ? 1 : query.Page;
                var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 200);
                var trips = await tripsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = trips
                });
            });
        }
    }
}
