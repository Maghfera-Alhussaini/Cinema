using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Apriori_Algo.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
          : base(options)
        {

        }
       protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
        }
        public DbSet<Movie> Movies { set; get; }
        public DbSet<Views> Views { set; get; }
    }
}
