using MainChapar.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models.DTO
{
    public class BlackWhitePrintRequestDto
    {
        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public int CopyCount { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
