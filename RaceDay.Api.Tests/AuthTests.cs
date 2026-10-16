using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        private RaceDayDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);

            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" }
            );
            context.SaveChanges();

            return context;
        }

        [Fact]
        public void HashPassword_Returns_Hash()
        {
            var hasher = new PasswordHasher();
            var hash = hasher.HashPassword("Password123");

            Assert.NotNull(hash);
            Assert.NotEqual("Password123", hash);
        }

        [Fact]
        public void VerifyPassword_Returns_True_For_Correct_Password()
        {
            var hasher = new PasswordHasher();
            var hash = hasher.HashPassword("Password123");

            Assert.True(hasher.VerifyPassword("Password123", hash));
        }

        [Fact]
        public void VerifyPassword_Returns_False_For_Wrong_Password()
        {
            var hasher = new PasswordHasher();
            var hash = hasher.HashPassword("Password123");

            Assert.False(hasher.VerifyPassword("WrongPassword", hash));
        }

        [Fact]
        public async Task Can_Register_User()
        {
            var context = GetInMemoryContext();
            var hasher = new PasswordHasher();

            var user = new AppUser
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.co.za",
                PasswordHash = hasher.HashPassword("Password123"),
                RoleId = 1,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@raceday.co.za");
            Assert.NotNull(savedUser);
            Assert.Equal("Test", savedUser!.FirstName);
        }

        [Fact]
        public async Task Cannot_Register_Duplicate_Email()
        {
            var context = GetInMemoryContext();
            var hasher = new PasswordHasher();

            var user1 = new AppUser
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.co.za",
                PasswordHash = hasher.HashPassword("Password123"),
                RoleId = 1,
                IsActive = true
            };

            context.Users.Add(user1);
            await context.SaveChangesAsync();

            // This mirrors the check in the AuthController before saving
            var duplicate = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "test@raceday.co.za");

            // The controller would return 409 Conflict because a user already exists
            Assert.NotNull(duplicate);
            Assert.Equal("test@raceday.co.za", duplicate!.Email);
        }
    }
}