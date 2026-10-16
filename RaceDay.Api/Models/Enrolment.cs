using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public AppUser? Participant { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Confirmed";
        public Result? Result { get; set; }
    }
}