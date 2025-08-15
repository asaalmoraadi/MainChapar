namespace MainChapar.Models.DTO
{
    public class LaminatePrintRequestDTO
    {
        public string PaperType { get; set; }
        public string PaperSize { get; set; }
        public string PrintSide { get; set; }
        public int CopyCount { get; set; }
        public string printType { get; set; } // BW , color
        public string LaminateType { get; set; } = "مات"; //مات ، براق
        public string CornerType { get; set; } //گوشه گرد، گوشه تیز
        public List<IFormFile> Files { get; set; }
    }
}
