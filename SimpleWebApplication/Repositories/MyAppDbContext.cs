using Microsoft.EntityFrameworkCore;

namespace SimpleWebApplication.Repositories;

public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) { }
    
}