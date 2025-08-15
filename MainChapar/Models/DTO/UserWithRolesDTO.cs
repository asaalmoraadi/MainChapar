namespace MainChapar.Models.DTO
{
    public class UserWithRolesDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentRole { get; set; } // "user" یا "admin"
        public List<string> AllRoles { get; set; } // برای dropdown تغییر نقش
    }
}
