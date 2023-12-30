using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Todo_App.Models;

namespace Todo_App.Data
{
	public class ApiDbContext : IdentityDbContext
	{
		public virtual DbSet<ItemData> Items { get; set; }
		public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
		{ 

		}
	}
}
