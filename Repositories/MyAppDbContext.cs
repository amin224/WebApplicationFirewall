using Microsoft.EntityFrameworkCore;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Repositories;

public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) { }
    
}