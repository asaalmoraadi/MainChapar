using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Collaboration
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public string Major { get; set; }

        [Range(1395, 1404)]
        public int EntryYear { get; set; }

        public string Level { get; set; }

        public string Skills { get; set; }


        //فیلد مورد علاقه برای همکاری:

        public string Interest { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public ICollection<UploadedFile> UploadedFiles { get; set; }
    }
}
