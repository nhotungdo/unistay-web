namespace Unistay_Web.Models.Connection
{
    public enum GroupRole { Member = 0, Admin = 1 }

    public class ChatGroupMember
    {
        public int Id { get; set; }
        public int ChatGroupId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public GroupRole Role { get; set; }
    }
}
