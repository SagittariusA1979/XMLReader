using System;
using Dsmdb;
using Microsoft.EntityFrameworkCore;

namespace Dsmdb
{
    public class DsmDbConntext:DbContext
    {
        #region Tables from DataBase
        public DbSet<dbModel> dbModels{ get; set; }             // Table of Models
        public DbSet<dbComp> dbComps{ get; set; }               // Tabels of Components
        public DbSet<dbStation> dbStations{ get; set; }         // Tabels of Stations
        public DbSet<dbEFAS_P> dbEFAS_Ps { get; set; }          // Tabels of EFAS's
        public DbSet<dbESTRC_A> dbESTRC_As {get; set; }         // Tabelss of ESTRC's
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