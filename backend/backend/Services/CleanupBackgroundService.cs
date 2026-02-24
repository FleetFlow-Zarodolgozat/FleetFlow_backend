using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanupBackgroundService> _logger;

        public CleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup service started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, "Cleanup failed");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task RunCleanupAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FlottakezeloDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var limitDate = DateTime.UtcNow.AddDays(-90);
            var deletedUsers = await context.Users.Where(u => u.UpdatedAt < limitDate && u.IsActive == false).ToListAsync();
            if (deletedUsers.Count > 0)
            {
                var driverIds = deletedUsers.Select(u => u.Driver!.Id).ToList();
                var usersIds = deletedUsers.Select(u => u.Id).ToList();
                var notifications = await context.Notifications.Where(n => usersIds.Contains(n.UserId)).ToListAsync();
                context.Notifications.RemoveRange(notifications);
                var usersFuelLogs = await context.FuelLogs.Where(f => driverIds.Contains(f.DriverId)).ToListAsync();
                context.FuelLogs.RemoveRange(usersFuelLogs);
                var usersTrips = await context.Trips.Where(t => driverIds.Contains(t.DriverId)).ToListAsync();
                context.Trips.RemoveRange(usersTrips);
                var usersServiceRequests = await context.ServiceRequests.Where(s => usersIds.Contains(s.CreatedByDriverUserId)).ToListAsync();
                context.ServiceRequests.RemoveRange(usersServiceRequests);
                var calendarEvents = await context.CalendarEvents.Where(c => usersIds.Contains(c.OwnerUserId) || usersIds.Contains(c.CreatedByUserId)).ToListAsync();
                context.CalendarEvents.RemoveRange(calendarEvents);
                var drivers = await context.Drivers.Where(d => driverIds.Contains(d.Id)).ToListAsync();
                context.Drivers.RemoveRange(drivers);
                var files = await context.Files.Where(f => usersIds.Contains(f.UploadedByUserId)).ToListAsync();
                foreach (var f in files)
                {
                    var file = await context.Files.FindAsync(f.Id);
                    if (file != null)
                    {
                        var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                        var path = Directory.GetFiles(storageRoot, file.StoredName, SearchOption.AllDirectories).FirstOrDefault();
                        if (path != null)
                            File.Delete(path);
                        context.Files.Remove(file);
                    }
                }
            }
            context.Users.RemoveRange(deletedUsers!);
            var deletedVehicles = await context.Vehicles.Where(v => v.UpdatedAt < limitDate && v.Status == "RETIRED").ToListAsync();
            if (deletedVehicles.Count > 0)            {
                var vehicleIds = deletedVehicles.Select(v => v.Id).ToList();
                var vehicleServiceRequests = await context.ServiceRequests.Where(s => vehicleIds.Contains(s.VehicleId)).ToListAsync();
                context.ServiceRequests.RemoveRange(vehicleServiceRequests);
                var vehicleServiceRequestNotifications = await context.Notifications.Where(n => n.RelatedServiceRequestId != null && vehicleServiceRequests.Select(s => s.Id).Contains(n.RelatedServiceRequestId.Value)).ToListAsync();
                context.Notifications.RemoveRange(vehicleServiceRequestNotifications);
                var vehicleTrips = await context.Trips.Where(t => vehicleIds.Contains(t.VehicleId)).ToListAsync();
                context.Trips.RemoveRange(vehicleTrips);
                var vehicleFuelLogs = await context.FuelLogs.Where(f => vehicleIds.Contains(f.VehicleId)).ToListAsync();
                context.FuelLogs.RemoveRange(vehicleFuelLogs);
                var calendarEvents = await context.CalendarEvents.Where(c => c.RelatedServiceRequestId != null && vehicleServiceRequests.Select(s => s.Id).Contains(c.RelatedServiceRequestId.Value)).ToListAsync();
                context.CalendarEvents.RemoveRange(calendarEvents);
                foreach (var f in vehicleFuelLogs)
                {
                    var file = await context.Files.FindAsync(f.ReceiptFileId);
                    if (file != null)
                    {
                        var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                        var path = Directory.GetFiles(storageRoot, file.StoredName, SearchOption.AllDirectories).FirstOrDefault();
                        if (path != null)
                            File.Delete(path);
                        context.Files.Remove(file);
                    }
                }
                foreach (var s in vehicleServiceRequests)
                {
                    var file = await context.Files.FindAsync(s.InvoiceFileId);
                    if (file != null)
                    {
                        var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                        var path = Directory.GetFiles(storageRoot, file.StoredName, SearchOption.AllDirectories).FirstOrDefault();
                        if (path != null)
                            File.Delete(path);
                        context.Files.Remove(file);
                    }
                }
            }
            context.Vehicles.RemoveRange(deletedVehicles);
            var deletedServiceRequests = await context.ServiceRequests.Where(s => s.ClosedAt < limitDate).ToListAsync();
            context.ServiceRequests.RemoveRange(deletedServiceRequests);
            var serviceRequestNotifications = await context.Notifications.Where(n => n.RelatedServiceRequestId != null && deletedServiceRequests.Select(s => s.Id).Contains(n.RelatedServiceRequestId.Value)).ToListAsync();
            context.Notifications.RemoveRange(serviceRequestNotifications);
            var serviceRequestCalendarEvents = await context.CalendarEvents.Where(c => c.RelatedServiceRequestId != null && deletedServiceRequests.Select(s => s.Id).Contains(c.RelatedServiceRequestId.Value)).ToListAsync();
            context.CalendarEvents.RemoveRange(serviceRequestCalendarEvents);
            foreach (var s in deletedServiceRequests)
            {
                var file = await context.Files.FindAsync(s.InvoiceFileId);
                if (file != null)
                {
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    var path = Directory.GetFiles(storageRoot, file.StoredName, SearchOption.AllDirectories).FirstOrDefault();
                    if (path != null)
                        File.Delete(path);
                    context.Files.Remove(file);
                }
            }
            var deltedFuelLogs = await context.FuelLogs.Where(f => f.UpdatedAt < limitDate && f.IsDeleted == true).ToListAsync();
            context.FuelLogs.RemoveRange(deltedFuelLogs);
            foreach (var f in deltedFuelLogs)
            {
                var file = await context.Files.FindAsync(f.ReceiptFileId);
                if (file != null)
                {
                    var storageRoot = Path.Combine(env.ContentRootPath, "storage");
                    var path = Directory.GetFiles(storageRoot, file.StoredName, SearchOption.AllDirectories).FirstOrDefault();
                    if (path != null)
                        File.Delete(path);
                    context.Files.Remove(file);
                }
            }
            var deletedTrips = await context.Trips.Where(t => t.UpdatedAt < limitDate && t.IsDeleted == true).ToListAsync();
            context.Trips.RemoveRange(deletedTrips);
            var oldNotifications = await context.Notifications.Where(n => n.CreatedAt < limitDate).ToListAsync();
            context.Notifications.RemoveRange(oldNotifications);
            await context.SaveChangesAsync();
            _logger.LogInformation("Cleanup executed at {time}", DateTime.UtcNow);
        }
    }
}
