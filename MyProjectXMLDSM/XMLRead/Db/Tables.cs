using System;
using Dsmdb;

namespace Dsmdb
{
    public class dbModel
    {
        public int Id { get; set; }
        public string? ModelCode { get; set; }          // DMC code
        public string? ModelName { get; set; }          // Model name 
        public string? NumberOfModels { get; set; }     // number of mode
    }

    public class dbComp
    {
        public int Id { get; set;}
        public string? CompCode { get; set;}         // COMP code
        public string? NumberOfModel { get; set; }   // compare of model
    }

    public class dbStation
    {
        public int Id { get; set;}
        public string? OPxxx { get; set; }
        public string? Model_1 { get; set; }
        public string? Model_2 {get; set;}
    }

    public class dbEFAS_P
    {
        public int Id { get; set;}
        public string? DMC { get; set;}
        public string? EFAS { get; set; }
        public string? OPxxx {get; set; }
        public int NoSeq { get; set; }
        public int Rew {get; set;}
    }

    public class dbESTRC_A
    {
        public int Id { get; set;}
        public string? DMC {get; set;}
        public string? ESTRC {get; set;}
        public string? OPxxx {get; set; }
    }
}