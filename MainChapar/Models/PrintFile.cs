namespace MainChapar.Models
{
    public class PrintFile
    {
        public int Id { get; set; }

        public int PrintRequestId { get; set; }
        public PrintRequest PrintRequest { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int PageCount { get; set; }
    }
}
