using System;
using Dsmdb;
using Microsoft.EntityFrameworkCore;

namespace Dsmdb
{
    public class DsmDbConntext:DbContext
    {
        #region Tables from DataBase
        public DbSet<dbModel> dbModels{ get; set; }
        public DbSet<dbComp> dbComps{ get; set; }
        #endregion

        public DsmDbConntext(){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Filename={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "databaseDSM.sqlite")}");
        }

       // public void()

        #region Methods

        #endregion

      
    }
}