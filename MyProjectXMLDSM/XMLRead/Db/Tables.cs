using System;
using Dsmdb;

namespace Dsmdb
{
    public class dbModel
    {
        public int Id { get; set; }
        public string? ModelCode { get; set; }       // DMC code
        public string? NumberOfModels { get; set; }  // number of mode
    }

    public class dbComp
    {
        public int Id { get; set;}
        public string? CompCode { get; set;}         // COMP code
        public string? NumberOfModel { get; set; }   // compare of model
    }
}