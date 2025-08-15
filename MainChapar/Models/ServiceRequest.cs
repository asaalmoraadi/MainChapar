using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Family { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public string Description { get; set; }

        public string? UploadedFilePath { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public ServiceType Type { get; set; }  // Enum: Typing یا PowerPoint

        public string? ProjectTitle { get; set; }  // فقط برای پاورپوینت استفاده میشه
    }

    public enum ServiceType
    {
        Typing,
        PowerPoint
    }

}
