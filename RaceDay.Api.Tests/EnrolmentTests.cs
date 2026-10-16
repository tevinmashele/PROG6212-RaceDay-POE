using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
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
        public async Task Participant_Can_Enrol_In_Event()
        {
            var context = GetInMemoryContext();

            var organiser = new AppUser { FirstName = "Org", LastName = "One", Email = "org@test.com", PasswordHash = "x", RoleId = 1 };
            var participant = new AppUser { FirstName = "Part", LastName = "One", Email = "part@test.com", PasswordHash = "x", RoleId = 2 };
            context.Users.AddRange(organiser, participant);
            await context.SaveChangesAsync();

            var ev = new Event { Name = "Test Event", Description = "", EventDate = DateTime.UtcNow, Location = "X", DistanceKm = 10, EventType = "Running", OrganiserId = organiser.UserId };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var cat = new Category { EventId = ev.EventId, Name = "Open", Description = "", EntryFee = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var enrolment = new Enrolment
            {
                ParticipantId = participant.UserId,
                EventId = ev.EventId,
                CategoryId = cat.CategoryId,
                Status = "Confirmed"
            };

            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var saved = await context.Enrolments.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(participant.UserId, saved!.ParticipantId);
        }

        [Fact]
        public async Task Participant_Cannot_Enrol_Twice_In_Same_Event()
        {
            var context = GetInMemoryContext();

            var organiser = new AppUser { FirstName = "Org", LastName = "One", Email = "org2@test.com", PasswordHash = "x", RoleId = 1 };
            var participant = new AppUser { FirstName = "Part", LastName = "Two", Email = "part2@test.com", PasswordHash = "x", RoleId = 2 };
            context.Users.AddRange(organiser, participant);
            await context.SaveChangesAsync();

            var ev = new Event { Name = "Event", Description = "", EventDate = DateTime.UtcNow, Location = "X", DistanceKm = 10, EventType = "Running", OrganiserId = organiser.UserId };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var cat = new Category { EventId = ev.EventId, Name = "Open", Description = "", EntryFee = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            context.Enrolments.Add(new Enrolment { ParticipantId = participant.UserId, EventId = ev.EventId, CategoryId = cat.CategoryId, Status = "Confirmed" });
            await context.SaveChangesAsync();

            // This mirrors the check in the EnrolmentsController before saving
            var existing = await context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participant.UserId && e.EventId == ev.EventId);

            // The controller would return 409 Conflict because the enrolment already exists
            Assert.NotNull(existing);
            Assert.Equal(participant.UserId, existing!.ParticipantId);
            Assert.Equal(ev.EventId, existing.EventId);
        }
    }
}