namespace MainChapar.Models.DTO
{
    public class PlanPrintRequestDto
    {
        public int CopyCount { get; set; }

        public string SizeOrScaleDescription { get; set; }

        public string PaperType { get; set; }

        public string printType { get; set; } // BW , color

        public string? AdditionalDescription { get; set; }

        public string BindingType { get; set; } = "هیچ‌کدام";

        public List<IFormFile> Files { get; set; }

    }
}
