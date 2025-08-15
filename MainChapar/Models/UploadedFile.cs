namespace MainChapar.Models
{
    public class UploadedFile
    {
      
            public int Id { get; set; }
            public string FilePath { get; set; }


            public int CollaborationId { get; set; }
            public Collaboration Collaboration { get; set; }
       
    }
}
