using System.ComponentModel.DataAnnotations;

namespace MainChapar.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        public string? MenuTitle { get; set; }

        public string? Link { get; set; }

        public string? Type { get; set; }
    }
}
