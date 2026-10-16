using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class ResultTests
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
        public async Task Organiser_Can_Capture_Result()
        {
            var context = GetInMemoryContext();

            var organiser = new AppUser { FirstName = "Org", LastName = "One", Email = "org3@test.com", PasswordHash = "x", RoleId = 1 };
            var participant = new AppUser { FirstName = "Part", LastName = "Three", Email = "part3@test.com", PasswordHash = "x", RoleId = 2 };
            context.Users.AddRange(organiser, participant);
            await context.SaveChangesAsync();

            var ev = new Event { Name = "Event", Description = "", EventDate = DateTime.UtcNow, Location = "X", DistanceKm = 10, EventType = "Running", OrganiserId = organiser.UserId };
            context.Events.Add(ev);
            await context.SaveChangesAsync();

            var cat = new Category { EventId = ev.EventId, Name = "Open", Description = "", EntryFee = 100 };
            context.Categories.Add(cat);
            await context.SaveChangesAsync();

            var enrolment = new Enrolment { ParticipantId = participant.UserId, EventId = ev.EventId, CategoryId = cat.CategoryId, Status = "Confirmed" };
            context.Enrolments.Add(enrolment);
            await context.SaveChangesAsync();

            var result = new Result
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTime = "01:23:45",
                FinishPosition = 5
            };

            context.Results.Add(result);
            await context.SaveChangesAsync();

            var saved = await context.Results.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("01:23:45", saved!.FinishTime);
            Assert.Equal(5, saved.FinishPosition);
        }
    }
}