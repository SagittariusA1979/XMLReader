using System;
using Dsmdb;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Dsmdb
{
    public class ExampledbData 
    {
        public ExampledbData(){}

        #region Method
        public void makeTestData()
        {
            Console.WriteLine("Pass data to db ...");
            dbModelEX();
            dbCompEX();
            dbStationEX();
            dbEFAS_PEX();
            dbESTRC_AEX();
        }

        private void dbModelEX()
        {
            using (var context = new DsmDbConntext())
            {
                context.Database.EnsureCreated();

                var ex_ = new List<dbModel> 
                {
                    new dbModel{ModelCode = "2",ModelName = "Model_1",NumberOfModels = "8002"},
                    new dbModel{ModelCode = "3",ModelName = "Model_2",NumberOfModels = "8003"}
                };
               context.AddEntities(ex_);
            }  
        }
        private void dbCompEX()
        {
            using(var context = new DsmDbConntext())
            {
                context.Database.EnsureCreated();

                var ex_ = new List<dbComp>
                {
                    new dbComp{CompCode = "SN002", NumberOfModel = "2"},
                    new dbComp{CompCode = "SN003", NumberOfModel = "2"},
                    new dbComp{CompCode = "SN004", NumberOfModel = "3"},
                    new dbComp{CompCode = "SN005", NumberOfModel = "3"},
                };
                context.AddEntities(ex_);
            }
        }
        private void dbStationEX()
        {
            using(var context = new DsmDbConntext())
            {
                context.Database.EnsureCreated();

                var ex_ = new List<dbStation>
                {
                    new dbStation{OPxxx = "OP700", Model_1 = "2", Model_2 = "0"},
                    new dbStation{OPxxx = "OP705", Model_1 = "1", Model_2 = "0"},
                    new dbStation{OPxxx = "OP710", Model_1 = "3", Model_2 = "1"},
                    new dbStation{OPxxx = "OP715", Model_1 = "0", Model_2 = "2"},
                    new dbStation{OPxxx = "OP720", Model_1 = "4", Model_2 = "3"},
                };
                context.AddEntities(ex_);
            }
        }
        private void dbEFAS_PEX()
        {
            using(var context = new DsmDbConntext())
            {
                context.Database.EnsureCreated();

                var ex_ = new List<dbEFAS_P>
                {
                    new dbEFAS_P{DMC = "#8002002#", EFAS = "1414141", OPxxx ="OP700", NoSeq = 2, Rew = 0},
                    new dbEFAS_P{DMC = "#8002002#", EFAS = "1414141", OPxxx ="OP705", NoSeq = 1, Rew = 0},
                    new dbEFAS_P{DMC = "#8002002#", EFAS = "1414141", OPxxx ="OP710", NoSeq = 3, Rew = 0},
                };

                context.AddEntities(ex_);
            }
        }
        private void dbESTRC_AEX()
        {
            using(var context = new DsmDbConntext())
            {
                context.Database.EnsureCreated();

                var ex_ = new List<dbESTRC_A>
                {
                    new dbESTRC_A{DMC = "#8002666#", ESTRC = "1414141", OPxxx = "OP7xx"}
                };
                context.AddEntities(ex_);
            }
        }
        #endregion
    }
}