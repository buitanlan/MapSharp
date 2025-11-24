namespace MapSharp.Tests.Models;

[MapFrom(typeof(UserDto))]
public partial class UserProfile
{
    [MapProperty("Username")]
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    [MapProperty(Ignore = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

