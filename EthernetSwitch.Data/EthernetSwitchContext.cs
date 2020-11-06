using System;
using System.Linq;
using EthernetSwitch.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

namespace EthernetSwitch.Data
{
    public class EthernetSwitchContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SNMPTrapUser> TrapUsers { get; set; }
        public DbSet<SNMPMessage> TrapMessages { get; set; }

        public EthernetSwitchContext()
        {
            if (Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder {DataSource = "sqlitedemo.db"};
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            options.UseSqlite(connection);
        }
    }
}