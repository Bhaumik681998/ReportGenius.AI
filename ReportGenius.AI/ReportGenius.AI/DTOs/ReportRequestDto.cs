using System.ComponentModel.DataAnnotations;

namespace ReportGenius.AI.DTOs
{
    public class ReportRequestDto
    {
        //public string? ConnectionString { get; set; }
        [Required]
        public string? Prompt { get; set; }

        //public DateTime From { get; set; }
        //public DateTime To { get; set; }
        //public string? ReportType { get; set; }
    }
}
