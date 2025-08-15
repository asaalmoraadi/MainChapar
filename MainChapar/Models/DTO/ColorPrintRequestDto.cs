using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models.DTO
{
    public class ColorPrintRequestDto
    {
        [Required]
        public string PaperType { get; set; }

        [Required]
        public string PaperSize { get; set; }

        [Required]
        public string PrintSide { get; set; }

        [Required]
        [Range(1, 1000)]
        public int CopyCount { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
