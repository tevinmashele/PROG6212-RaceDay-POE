using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public Enrolment? Enrolment { get; set; }
        public string FinishTime { get; set; } = string.Empty;
        public int FinishPosition { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}