using Microsoft.AspNetCore.Identity;

namespace MainChapar.Models
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public string LName { get; set; }
        
        public ICollection<Comment> Comments { get; set; }
    }
}
