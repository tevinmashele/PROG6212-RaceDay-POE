using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventTests
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
        public async Task Organiser_Can_Create_Event()
        {
            var context = GetInMemoryContext();

            var organiser = new AppUser
            {
                FirstName = "Thabo",
                LastName = "Nkosi",
                Email = "thabo@raceday.co.za",
                PasswordHash = "hash",
                RoleId = 1,
                IsActive = true
            };

            context.Users.Add(organiser);
            await context.SaveChangesAsync();

            var newEvent = new Event
            {
                Name = "Soweto Marathon",
                Description = "Annual marathon",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Soweto",
                DistanceKm = 42.2,
                EventType = "Running",
                OrganiserId = organiser.UserId
            };

            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            var events = await context.Events.ToListAsync();
            Assert.Single(events);
            Assert.Equal("Soweto Marathon", events[0].Name);
        }

        [Fact]
        public async Task Only_Own_Events_Are_Returned_To_Organiser()
        {
            var context = GetInMemoryContext();

            var organiser1 = new AppUser { FirstName = "A", LastName = "One", Email = "a@test.com", PasswordHash = "x", RoleId = 1 };
            var organiser2 = new AppUser { FirstName = "B", LastName = "Two", Email = "b@test.com", PasswordHash = "x", RoleId = 1 };
            context.Users.AddRange(organiser1, organiser2);
            await context.SaveChangesAsync();

            context.Events.AddRange(
                new Event { Name = "Event A", Description = "", EventDate = DateTime.UtcNow, Location = "X", DistanceKm = 10, EventType = "Running", OrganiserId = organiser1.UserId },
                new Event { Name = "Event B", Description = "", EventDate = DateTime.UtcNow, Location = "Y", DistanceKm = 20, EventType = "Running", OrganiserId = organiser2.UserId }
            );
            await context.SaveChangesAsync();

            var myEvents = await context.Events
                .Where(e => e.OrganiserId == organiser1.UserId)
                .ToListAsync();

            Assert.Single(myEvents);
            Assert.Equal("Event A", myEvents[0].Name);
        }
    }
}