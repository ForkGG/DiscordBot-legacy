using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class DatabaseContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($@"Data source= {AppDomain.CurrentDomain.BaseDirectory + "Fork.db"}");
        }
        public DbSet<Auth> Auth { get; set; }
        public DbSet<Notify> Notify { get; set; }
        public DbSet<OnJoin> OnJoin { get; set; }
        public DbSet<OnHold> OnHold { get; set; }
    }
}
