using Microsoft.EntityFrameworkCore;

namespace ChatServer;

public class ChatDbContext : DbContext
{
    
    public DbSet<User> Users { get; set; } = null!;

}

public class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}