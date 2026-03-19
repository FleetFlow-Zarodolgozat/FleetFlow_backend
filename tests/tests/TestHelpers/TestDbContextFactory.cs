using backend;
using Microsoft.EntityFrameworkCore;

namespace tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static FlottakezeloDbContext Create()
        {
            var options = new DbContextOptionsBuilder<FlottakezeloDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new FlottakezeloDbContext(options);
        }
    }
}
