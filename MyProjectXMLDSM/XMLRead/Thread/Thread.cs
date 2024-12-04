// +------------------------------+
// |   Error List for CSC result  |
// +----------+-------------------+
// | CODE'S   |   DESCRYPTION     |
// +----------+-------------------+
// | 2        | OK                |
// | 3        | Model not correct |
// +----------+-------------------+
//

#define SHOWCSC
#define EFAS_WRITE
#define DBUG_INFO

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;
using System.Threading;
using System.Text;
using Archive;

using readxmlFile;
using s7;
using Dsmdb;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using System.Data;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using System.Xml.Schema;
using ZebraMatrix;
using Microsoft.IdentityModel.Tokens;

namespace CSC 
{
    public class XThread 
    {   
        #region Private and Public Variables
        private int stepsCSC;
        private int stepsCRC;
        private int stepsTRC;
        private int stepsPRT;
       
        private string fileName;
        private string IpAddres;
        private int slot;
        private int rack;

        private bool KeepALive;                     // Keep A Live
        private bool isConnect = false;             // I use regarding semafor which block  PLC connect

        public class EFASdata                       // Not use right now
        {
            public string DB_efas { get; set; }
            public string Byte_efas { get; set; }

            public EFASdata(string db_efas, string byte_efas)
            {
                DB_efas = db_efas;
                Byte_efas = byte_efas;
            }
            public EFASdata() { }

            // How to use it... 

            // List<EFASdata> efasList = new List<EFASdata>
            // {
            //     new EFASdata("DatabaseString1", "ByteString1"),
            //     new EFASdata("DatabaseString2", "ByteString2")
            // };

            // // Access and display the data
            // foreach (EFASdata step in efasList)
            // {
            //     Console.WriteLine("DB_efas: " + step.DB_efas);
            //     Console.WriteLine("Byte_efas: " + step.Byte_efas);
            // }
        }
        // public class ModelInfo                   // ClassData I use to .Sekect(x => new ...)
        // {
        //     public string Model_1 { get; set; }
        //     public string Model_2 { get; set; }
        // }
        #endregion
        #region INSTANCE
        private ReadXML mReadXML;
        private S7con mS7con;
        private MatrixP printerZT411;
        private  ArchiveDbContext mArchiveDB;

        #endregion 
        #region CLASS_Data

        // For raed all Variabels
        internal class VariableData                     // I us it in function [VariableSforStep_Read] to read a all data about one step
        {
            public List<string> VariableDesc { get; set; }
            public List<string> SelectedValueType { get; set; }
            public List<string> TRCDatablock { get; set; }
            public List<string> TRCStartAddress { get; set; }
            public List<string> TRCLength { get; set; }
            public List<string>  MinDatablock {get; set; }
            public List <string> MinStartAddress {get; set; }
            public List<string> MaxDatablock {get; set;}
            public List <string> MaxStartAddress {get; set; }

            public bool IsEmptyAll()
            {
                return (VariableDesc == null || VariableDesc.Count == 0) &&
                    (SelectedValueType == null || SelectedValueType.Count == 0) &&
                    (TRCDatablock == null || TRCDatablock.Count == 0) &&
                    (TRCStartAddress == null || TRCStartAddress.Count == 0) &&
                    (TRCLength == null || TRCLength.Count == 0) &&
                    (MinDatablock == null || MinDatablock.Count == 0) &&
                    (MinStartAddress == null || MinStartAddress.Count == 0) &&
                    (MaxDatablock == null || MaxDatablock.Count == 0) &&
                    (MaxStartAddress == null || MaxStartAddress.Count == 0);
            }

            public bool IsEmptyOnlyVariableDesc()
            {
                return VariableDesc == null || VariableDesc.Count == 0;
            }
        }
        internal class VarSteps
        {
            public  List<VariableData> ListOfVariabelsAllSteps{ get; set; } = new List<VariableData>();

            public bool IsEmptyCheck()
            {
                //return ListOfVariabelsAllSteps == null || ListOfVariabelsAllSteps.Count == 0;
                return ListOfVariabelsAllSteps.All(v => v.IsEmptyOnlyVariableDesc());
            }
        }

        // For read all Componnents
        internal class ComponentData                    // I us it in function [ComponetSforStep_Read] to read a all data about one step
        {
            public List<string> ComponentDesc {get; set;}
            public List<string> TRCDatablock {get; set;}
            public List<string> TRCStartAddress {get; set;}
            public List<string> TRCLenght {get; set;}
        }
        internal class ComSteps
        {
            public List<ComponentData> ListOfComponetsAllSteps {get; set; } = new List<ComponentData>();
        }
        
        // For TRC_archiv dynamic table. "but you must remember, this date are intendet only the one machine !"
        internal class HeaderTable                      // I use this model of data in two cases, for creating headers and creating data that refer to headers
        {
            public List<string> List_nameSteps {get; set;} = new List<string>();
            public List<string> List_statusSteps {get; set;} = new List<string>();
            public List<string> List_nameVariables {get; set;} = new List<string>();
            public List<string> List_variables {get; set;} = new List<string>();
            public List<string> List_varMin {get; set;} = new List<string>();
            public List<string> List_varMax {get; set;} = new List<string>();
            public List<string> List_nameComponent {get; set;} = new List<string>();
            public List<string> List_components {get; set;} = new List<string>();
        } 

        #endregion
        
        public XThread(string fileName, string IpAddres, int slot, int rack, string conectionStringToArchiveDb)
        {
            mReadXML = new ReadXML(fileName);                                   // DataBase to configuration 
            mS7con = new S7con(IpAddres, slot, rack);                           // PLC connect
            mArchiveDB = new ArchiveDbContext(conectionStringToArchiveDb);      // dataBase to archive traceability data

            stepsCSC = 0;
            stepsCSC = 0;
            stepsTRC = 0;
            stepsPRT = 0; 
        }        

        // THE THREADS: CSC|CRC|TRC - PRT|KA
        public bool IsAlive(string _db, string _dbb, string _dbx)
        {
            try
            {
                int DB_ka = int.Parse(_db);
                int Byte_ka = int.Parse(_dbb);
                int Bite_ka = int.Parse(_dbx);

                //KeepALive = KeepALive == false ? true : false;
                KeepALive = !KeepALive;

                #if SHOWCSC
                Console.WriteLine($"{KeepALive}");
                #endif

                if(mS7con.connectPLc() && !isConnect){
                    isConnect = true;

                    mS7con.WriteBit(DB_ka, Byte_ka, Bite_ka, KeepALive);

                    isConnect = false;
                    mS7con.disconnectPLc();
                }           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error [IsAlive] --> isConnect: {isConnect} | {ex.Message}");
                return false;
            }
            return KeepALive;
        }
        public bool Print_DataMatric(List<int> REQPrint, List<int> MODPrint, List<int> ACKPrint)
        {
            try
            {
                printerZT411 = new MatrixP("192.168.0.22", 6101); // I have to move it to constructor but not now !!! [_disposed]
            }
            catch (Exception ex)
            {
                throw new Exception("Error during made a MatrixP's instance " + ex.Message);
            }

            if((REQPrint.Count > 0) && (MODPrint.Count > 0) && (ACKPrint.Count > 0)){

                var REQ = REQprt_Read(REQPrint);
                var mod = MODprt_Read(MODPrint);

                if(REQ && (stepsPRT == 0))
                {
                    Console.WriteLine($"model:{mod} | check a model and get corect DMC code for it ...");
                    var statusPrin = printerZT411.printDMCcodeforModel("#6988#", 1);
                    if(statusPrin){
                       Console.WriteLine("Printed OK"); 
                    }else{
                       Console.WriteLine("Printed NOK"); 
                    }

                    stepsPRT++;
                    Console.ReadKey();
                }

                if(REQ && (stepsPRT == 1))
                {
                    Console.WriteLine($"PRINTING & set ACK");
                    ACKprt_Write(true, ACKPrint);  
                    stepsPRT++;
                }

                if(!REQ && (stepsPRT == 2))
                {
                    Console.WriteLine("finish");
                    ACKprt_Write(false, ACKPrint);
                    stepsPRT = 0;
                }

            }else{
                Console.WriteLine($"Error into: [Try to Print DMC] isConnect: {isConnect}");
            }
            return true;
    }
        public bool CSC_thread()
        {
            #region DESCRYYPTION
            // This function will be to menage a CSC thread.
            // --------------------------------------------
            // PLC      : set a DMC and MOD after set REQ  
            // Server   : if(REQ is 1) read a DMC and MOD
            // DB:      :  find a number of MOD refer to DMC and after compare whit MOD from PLC
            //            : if result is OK and rest control also OK
            // Server   : set a RES|WOK|WKO| and ACK
            #endregion

            #region VARIABELS
            List<string>? numberOfModelFromDB = new List<string>(); //List<string?>? numberOfModelFromDB = new List<string?>();
            List<string>? nameOfModelFromDB = new List<string>();

            bool status_csc = false;    // Basicly ststus for function
            bool resultDB = false;      // Result from check 1 (DMC=OK, mdel=OK ) and 2 (OPxx is exist curent Model)
            int model = 0;              // Model    refer DSM           r:327   up date after all change !!!
            string dmc  = string.Empty; // dmc      refer DSM           r:326   up date after all change !!!
            bool dmc_Check = true;      // convert dms                  r:318   up date after all change !!!
            string currentOPName = "OPxxx"; // Get OP name e.g. OP700   r:172   up date after all change !!!
            string NoSeq = string.Empty;    // Number in sequence [if 0 that mean not exist] r:369  up date after all change !!!
            List<string>? curentOpEFAS = new List<string>(); // EFAS for current OPxxx r:489     up date after all change !!!

            //int steps = 0;
            #endregion

            currentOPName = OpName_Read();   // read OP name for CSC
            bool REQ = REQ_Read("CSC");      // read request CSC
            
            #if SHOWCSC
            Console.WriteLine($"--> START THREAD CSC FOR stNo:[{currentOPName}] : REQ: {REQ} S:{stepsCSC}");
            #endif

            if(REQ && (stepsCSC == 0))      // Step [0] Start thread <-- We are waiting to REQ from PLC
            {     
                dmc = DMC_Read("CSC"); 
                model = MOD_Read("CSC");
                string paternDMC = dmc.Length >= 4 ? dmc.Substring(0, 9) : dmc;

                // #if SHOWCSC
                // plcString(dmc);
                // #endif

                if(paternDMC.Contains('\0') && dmc[0] == '#') // string.IsNullOrEmpty(dmc) && dmc[0] == '0'
                {
                    dmc_Check = false;
                    stepsCSC = 4;
                }
                else
                {
                    stepsCSC++;
                }
                
                #if SHOWCSC
                Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->>  DMC:{dmc} MODEL:{model} dmc_Check:{dmc_Check}");
                #endif
            }

            if(REQ && (stepsCSC == 1))      // Step [1] thread <-- We looking for a DMC and refer it to number of model
            {
                // DB context operation [We looking for a DMC and refer it to number of model] <--------
                using (var context = new DsmDbConntext())
                {
                    try
                    {
                        context.Database.EnsureCreated();

                        string c_dmc = NoPart(dmc, 1, 4); // Convert NoDMC to model patern e.g. 8002
                        

                        #if SHOWCSC
                        Console.WriteLine($"Before convert:{dmc} | Afeter convert: {c_dmc}");
                        #endif

                        var numberOfModelFromDB_ = context.dbModels // e.g [2]
                            .Where(x => x.NumberOfModels == c_dmc)
                            .Select(x => x.ModelCode)
                            .Where(x => x != null)      // Filter out nulls
                            .Cast<string>()             // Safely cast from string? to string
                            .ToList();

                        var nameOfModelFrmaDB_ = context.dbModels // e.g [Model_1]
                            .Where(x => x.NumberOfModels == c_dmc)
                            .Select(x => x.ModelName)
                            .Where(x => x != null)      // Filter out nulls
                            .Cast<string>()             // Safely cast from string? to string
                            .ToList();

                        if(nameOfModelFrmaDB_ != null && nameOfModelFrmaDB_.Count > 0 && nameOfModelFrmaDB_[0] != null)
                        {
                            var OpExistInThisModel = context.dbStations // e.g [2 number in sequence]
                            .Where(x => x.OPxxx == currentOPName)
                            .Select(x => EF.Property<string>(x, nameOfModelFrmaDB_[0]!)) // Null-forgiving Operator [!] you must apsolut ensuree
                            .ToList();

                            NoSeq = OpExistInThisModel[0];
                        }
                        else
                        {
                            throw new ArgumentNullException(nameof(nameOfModelFrmaDB_), "The property name list is null or empty.");
                        }

                        numberOfModelFromDB = numberOfModelFromDB_;
                        nameOfModelFromDB = nameOfModelFrmaDB_;
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Error during [looking for a DMC and refer it to number of model] Step 1A :{ex.Message}");
                    }
                    
                    #if SHOWCSC
                    
                    Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->>  DMC:{dmc} MODEL:{model} dmc_Check:{dmc_Check}");
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------");
                    Console.WriteLine($"Correcy of numberOfModelFromDB          :{numberOfModelFromDB[0]}   -> Instance: {numberOfModelFromDB.Count}");
                    Console.WriteLine($"nameOfModelFromDB:                      :{nameOfModelFromDB[0]}     -> Instance: {nameOfModelFromDB.Count}");
                    Console.WriteLine($"OpExistInThisModel:                     :{NoSeq}");
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------");
                    #endif

                    try
                    {
                        // Assuming you want to check if the model exists in the results
                        bool result_firstCheck = numberOfModelFromDB.Any(num => int.TryParse(num, out int numAsInt) && numAsInt == model);
                        // Assuming you want to check if the Opxx exists in the current Model
                        bool result_secondCheck = (int.Parse(NoSeq) != 0) ? true : false;
                        // Invalid component Code
                        bool resuly_thirdCheck =  dmc_Check;

                        if(result_firstCheck)
                        {
                            if(result_secondCheck)
                            {
                                if(resuly_thirdCheck)
                                {
                                    resultDB = true; 
                                }
                                else
                                {
                                    Console.WriteLine($"OP not exist in this Model !");
                                    RES_Write(13, "CSC"); 
                                }
                            }
                            else
                            {
                                Console.WriteLine($"OP not exist in this Model !");
                                RES_Write(2, "CSC"); 
                            }    
                        }
                        else
                        {
                                Console.WriteLine($"Model Mishmash !");
                                RES_Write(3, "CSC");
                        }
                        
                    // Assuming you want to check if the model exists in the results
                    // resultDB = numberOfModelFromDB.Contains(c_dmc.ToString());
    
                    #if SHOWCSC
                    Console.WriteLine("Result from quality [three controls] (DMC=OK, mdel=OK ) and (OPxx is exist curent Model) and (empty or null the DMC)");
                    Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} -> resultDB: {resultDB}");
                    #endif

                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Error during [Three controls] 1B:{ex.Message}");

                    }  
                }
                stepsCSC++;
            }

            if(REQ && (stepsCSC == 2))      // Step [2] thread <-- We have a result [true] from compare No model bettwen DMC code & Opxx exists in the current Model
            {

                #if SHOWCSC
                Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->> START");
                #endif

                if(resultDB == true){  // Step [2] INTYRODUCTION CONTROLS RESULT ARE -> [true] <--

                    using(var context = new DsmDbConntext())
                    {
                        context.Database.EnsureCreated();

                        List<string> step = STEp_Read("CSC");  // Return list of quantity Steps [List<string> step]
                        string? NameOfModel = context.dbModels // Get Name of model e.g Model_1 
                            .Where(x => x.ModelCode == model.ToString())
                            .Select(x => x.ModelName)
                            .FirstOrDefault();  
                       
                        
                        string all_dmc = NoPart(dmc, 0, 9);                         // Patern for DataMatrix Code
                        bool exists = context.dbEFAS_Ps.Any(x => x.DMC == all_dmc); // I looking for into db.EFAS if dmc is exist or not exist [BASIC] 

                        #if SHOWCSC
                        Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->> DMC:{dmc} exist:{exists}");
                        #endif

                        if(exists) // Exist DMS and OPxxx
                        {  
                            bool existsSpecificOP = context.dbEFAS_Ps.Any(x => x.DMC == all_dmc && x.OPxxx == currentOPName); // DMC for OP : Exist or not [SPECIFIC]

                            if(existsSpecificOP)
                            { 
                                // last variables it determines is Exsit for specific (#8002002# and OPxxx) or Basic (#8002002# )
                                bool Rules_whit_Op = RulsControl(dmc, NoSeq, true); 

                                if(Rules_whit_Op)
                                {
                                    var efasCurrentOPxxx = context.dbEFAS_Ps // We get EFAS value for current OP
                                    .Where(x => x.DMC == all_dmc && x.OPxxx == currentOPName)
                                    .Select(x => x.EFAS).ToList();
                                    curentOpEFAS = efasCurrentOPxxx;

                                    if(step.Count > 0){
                                        var result = ESFAS_Write(step, "CSC", curentOpEFAS); // This function writes EFAS for each steps 
                                    }

                                    WOK_Write(true, "CSC");
                                    RES_Write(0, "CSC");
                                        
                                    #if SHOWCSC
                                    Console.WriteLine($"Exist DMS and OPxxx EFAS: current WOK: 1 RES: 1");

                                    foreach(var item in curentOpEFAS){
                                        Console.WriteLine($"currentOPName: {currentOPName} EFAS: {item}");
                                    }
                                    #endif 
                                }
                                else // I wondered if removed belowe code regarding -> else{} <-
                                {
                                    WKO_Write(true, "CSC");
                                    RES_Write(6, "CSC");

                                    #if SHOWCSC
                                    Console.WriteLine($"EFAS: not change WKO: 1 RES: 6");
                                    #endif
                                }   
                            }
                            else   // Exist DMS
                            {
                                // last variables it determines is Exsit for specific (#8002002# and OPxxx) or Basic (#8002002# )
                                bool Rules_whitout_Op = RulsControl(dmc, NoSeq, false); 

                                if (Rules_whitout_Op)
                                {
                                    if(step.Count > 0){
                                    ESFAS_WriteForStep(step, "CSC", 2);
                                    }

                                    WOK_Write(true, "CSC");
                                    RES_Write(0, "CSC");

                                    #if SHOWCSC
                                    Console.WriteLine($"EFAS: 2 WOK: 1 RES: 0");
                                    #endif
                                }
                                else
                                {
                                    WKO_Write(true, "CSC");
                                    RES_Write(6, "CSC");

                                    #if SHOWCSC
                                    Console.WriteLine($"EFAS: not chnge WKO: 1 RES: 6");
                                    #endif
                                }
                            }
                        }
                        else       // DMC NOT NEVER EXIST (FIRST STATION OPxxx) 
                        {  

                            if(int.Parse(NoSeq) == 1) // OPxx is first OP becouse NoSeq is equal to 1 [1A]
                            {
                                if(step.Count > 0){
                                var result = ESFAS_WriteForStep(step, "CSC", 2);
                                }

                                WOK_Write(true, "CSC");
                                RES_Write(0, "CSC");

                                #if SHOWCSC
                                Console.WriteLine($"EFAS: 2 WOK: 1 RES: 0");
                                #endif
                            }
                            else
                            {
                                WKO_Write(true, "CSC");
                                RES_Write(5, "CSC");

                                #if SHOWCSC
                                Console.WriteLine($"EFAS: not change WKO: 1 RES: 5");
                                #endif
                            }                      
                        }
                    }

                    ACK_Write(true, "CSC");  // and Error handling (!)

                    #if SHOWCSC
                    Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->> ResultDB - > TRUE <- after execute");
                    #endif
                }
                    
                if(resultDB == false){ // Step [2] thread | RESULT : [false] <--
                    WKO_Write(true, "CSC");
                    RES_Write(6, "CSC");

                    ACK_Write(true, "CSC");
                   
                     #if SHOWCSC
                    Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->> ResultDB - > FALSE <- after execute");
                    #endif
                }
                stepsCSC++;
            }

            if(REQ && (stepsCSC == 4))      // Step [4] When We have a error from quality check DMC and Number of Model. This steps is very iomportant
            {
                WKO_Write(true, "CSC");
                RES_Write(16, "CSC");

                ACK_Write(true, "CSC");
                stepsCSC = 3;

                #if SHOWCSC
                Console.WriteLine($"We have a error from quality check DMC and Number of Model.");
                Console.WriteLine($" WKO: 1 RES: 16 ACK: 1");
                #endif
            }

            if (REQ != true && (stepsCSC == 3))  // Step [3] Finish thread 
            {
                    ACK_Write(false, "CSC"); // and Error handling (!)
                    status_csc = true;
                    stepsCSC = 0;

                    #if SHOWCSC
                    Console.WriteLine($"--> STOP THREAD CSC FOR stNo:[{currentOPName}] : REQ: {REQ}");
                    #endif
            } 
            return status_csc;
        }
        public bool CRC_thread()
        {
            bool cycleStatus = false;

            return cycleStatus;
        }
        public bool TRC_thread()
        {
            #region DESCRYPTION

            // ...
            #endregion

            #region VARIABELS
            string nameOp = string.Empty;                            // Name OP
            string  dmc = string.Empty;                              // DMC 
            int model = 0;                                           // No model
            bool dmcAndModel_Check = true;                           // DMC [correct status]
            List<int> eSTRC_OP;                                      // ESTRC's 
            List<string> NameOfSteps = new List<string>();           // Name of Steps
            //HeaderTable resultHeaderForArchivtable = new HeaderTable();
            bool cycleStatus = false;

            #endregion

            bool REQ = REQ_Read("TRC");         // Read REQ

            #if SHOWCSC
            Console.WriteLine($"--> START THREAD TRC FOR Ip[{IpAddres}] : REQ: {REQ}");
            #endif

            if (REQ != false)                   // Debug
            {
                // DateTime _in = InDateTime_Read();
                // DateTime _out = OutDateTime_Read();
                // string sDTLIn = _in.ToString();
                // string sDTLOut = _out.ToString();

                // Console.WriteLine($"sDTLIn: {sDTLIn}");
                // Console.WriteLine($"sDTLOut: {sDTLOut}");
            }

            if(REQ && (stepsTRC == 0))          // Step [0] thread <-- We are waiting to REQ from PLC
            {
                dmc = DMC_Read("TRC"); // Read DMC
                model = MOD_Read_TRC();
                
                if(string.IsNullOrEmpty(dmc) || (model == 0 ))  // In this place I implemented ease controls : [I only check if dms isn't null and model diffrent to 0]
                {
                    dmcAndModel_Check = false;                 
                    RES_Write(30, "TRC");                       // Invalid DMC or Model code, the passed code is null or empty
                    stepsTRC = 3;
                }

                if(dmcAndModel_Check)
                {
                    // code ... controls or direct next step
                    stepsTRC++; // 1
                }

                Console.WriteLine($"STEPS :{stepsTRC} DMC: {dmc} NoModel: {model}");

            }

            if(REQ && (stepsTRC == 1))          // Step [1] Stored a data into db.ESTRC_ and db.Archive for specific OPxxx
            {
                // --- Preparing a Data to write SQL DataBase ---

                nameOp = OpName_Read();                                                                                             // Name of curent machines
                var quantityOfSteps = mReadXML.StepNUMp("CSC");                                                                     // <-- CSC !!! (it correct). [List<string>] Read a Quantity of Steps from XML files
                                                                                                                                    // the [StepNUMp()] rteturn a styep's number form XML becouse are specific, 
                                                                                                                                    // They are not in order in the queue !!! e.g steps 1 would be a number 7
                
                for(int i = 0; i < quantityOfSteps.Count; i++){
                    var NameSteps_tem = mReadXML.GetVar_1LevelInThreadp(quantityOfSteps[i], "Name", "TRC");                         // <-- Name of Step [List<string>]
                    NameOfSteps.Add(NameSteps_tem[0]);
                }
                
                int quantityOfStepsForSpecificOPxxx = quantityOfSteps.Count;                                                        // <-- quqntity of Steps [int]
                eSTRC_OP = ESTRC_Read(quantityOfSteps);                                                                             // Read directly value ESTRC from PLC
                
                var resultVarSANDComS = CompSANDVarSforStep_Read();                                                                         // Read diectly value of Variabels and Components for specific step 
                var resultHeaderForArchivtable = CreatingHeader(NameOfSteps, resultVarSANDComS.Item1, resultVarSANDComS.Item2);             // It return a headers for a archive table
                var resultVarandCommForArchivetable = CreatingVarAndComm(NameOfSteps, eSTRC_OP, resultVarSANDComS.Item1, resultVarSANDComS.Item2);    // ....

                // In this feild I have a all data regarding PLC's DB and next step I have to use this data and read directly data from PLC 
                // and writ are to sqlDb, [ but after write, I have to check if tables which I want to write exist or not !!! ]

                // --- These directly data ! I have to change are to header 
                 // string NmaeOP               - string [nameOP]
                 // list <string> NameOfSteps   - list [step's name]
                 // list <string> eSTRC_OP      - list [ESTRC's No for each Steps] // eSTRC_OP.Select(x => x.ToString()).ToList(),
                 
                
                
                MakeTableAndHeaderArchiveTable(nameOp, "OP", resultHeaderForArchivtable);                     // This place i check (if exist or not, table in SQL database) and creating it if not exist

                AddDataToArchiveTable("OP700", resultVarandCommForArchivetable, resultHeaderForArchivtable);
               

                // ---

                #if DBUG_INFO // DEBUG INFORMATIONS
                
                Console.WriteLine($"{nameOp}");                         // Name intendent to specific OP

                foreach(var item in NameOfSteps){
                    Console.WriteLine($"Neme: {item}");                 // all step's name [Neme: Step03]
                }

                foreach(var item in quantityOfSteps){                   // all No. of steps [Step: 12]
                    Console.WriteLine($"Step: {item}");
                }

                foreach(var item in eSTRC_OP){
                    Console.WriteLine($"ESTRC: {item.ToString()}");     // all data's ESTRC for each step in specificity Machines [ESTRC: 3]
                }

                
                Console.WriteLine("--------------- Headers  -------------------------");
                Console.WriteLine();
                Console.WriteLine("Steps: " + string.Join(", ", resultHeaderForArchivtable.List_nameSteps));
                Console.WriteLine("Status: " + string.Join(", ", resultHeaderForArchivtable.List_statusSteps));
                Console.WriteLine("NameVariables: " + string.Join(", ", resultHeaderForArchivtable.List_nameVariables));
                Console.WriteLine("Variables: " + string.Join(", ", resultHeaderForArchivtable.List_variables));
                Console.WriteLine("varMin: " + string.Join(", ", resultHeaderForArchivtable.List_varMin));
                Console.WriteLine("VarMax: " + string.Join(", ", resultHeaderForArchivtable.List_varMax));
                Console.WriteLine("NameComponents: " + string.Join(", ", resultHeaderForArchivtable.List_nameComponent));
                Console.WriteLine("CodeComponents: " + string.Join(", ", resultHeaderForArchivtable.List_components));
                Console.WriteLine();

                Console.WriteLine("--------------- Archve Date -------------------------");
                 int mark = 1;
                foreach(var item in resultVarandCommForArchivetable)
                {
                    Console.WriteLine("NameSteps_" + mark + " : " + string.Join("  ", item.List_nameSteps));
                    Console.WriteLine("StatusSteps_" + mark + " : " + string.Join("  ", item.List_statusSteps));
                    Console.WriteLine("NameVariabel_" + mark + " : " + string.Join("  ", item.List_nameVariables));
                    Console.WriteLine("Variabel_" + mark + " : " + string.Join("  ", item.List_variables));
                    Console.WriteLine("VarMin_" + mark + " : " + string.Join("  ", item.List_varMin));
                    Console.WriteLine("VarMax_" + mark + " : " + string.Join("  ", item.List_varMax));
                    Console.WriteLine("NameComponent_" + mark + " : " + string.Join("  ", item.List_nameComponent));
                    Console.WriteLine("Serial_" + mark + " : " + string.Join("  ",  item.List_components));
                    Console.WriteLine();
                    mark++;
                }
            
                Console.WriteLine("--------------- Var -------------------------");
                DysplayAllVarForAllSteps(resultVarSANDComS.Item1);

                Console.WriteLine("--------------- Comp ------------------------");
                DysplayAllCompForAllSteps(resultVarSANDComS.Item2);
                
                #endif

                Console.ReadKey();

                stepsTRC++; // 2
            }

            if(REQ && (stepsTRC == 2))          // When We have succes
            {
                //WOK_Write(true, "TRC");
                ACK_Write(true, "TRC");
                stepsTRC = 4;
            }

            if(REQ && (stepsTRC == 3))          // When We have a error 
            {
               // WKO_Write(true, "TRC");
                ACK_Write(true, "TRC");
                stepsTRC = 4;
            }

            if(!REQ && (stepsTRC == 4))  // Steps Finish
            {
                ACK_Write( false, "TRC");
                stepsTRC = 0;
            }

            Console.WriteLine($"DEBUG: {stepsTRC} !");

            return cycleStatus;
        }
        
        // THE SIGNAL's :bits: ACK|REQ|WOK|WKO|RES and :byte: DMC|MOD
        #region READ
        private bool REQprt_Read(List<int> REQPrint)        // PRT Manage
        {
             bool? RqeStatus = null;

            if(REQPrint.Count > 0){

             var _reqDatablock = REQPrint[0];
             var _reqByte = REQPrint[1];
             var _reqbite = REQPrint[2];

                if(mS7con.connectPLc() && !isConnect){
                    isConnect = true;

                    bool REQ_print = mS7con.ReadBit(_reqDatablock, _reqByte, _reqbite);
                    RqeStatus = REQ_print;

                    isConnect = false;
                    mS7con.disconnectPLc();
                }else{
                    Console.WriteLine($"Error into: [Try to Print DMC] isConnect: {isConnect}");
                }
            }
             return RqeStatus ?? false;
        }
        private int MODprt_Read(List<int> MODPrint)         // PRT Manage
        {
            int returnModel = 0;

            if(MODPrint.Count > 0){
                var _modelDatablock = MODPrint[0];
                var _modelByte = MODPrint[1];

                if(mS7con.connectPLc() && !isConnect){
                    isConnect = true;

                    int NModel = mS7con.ReadByte(_modelDatablock, _modelByte);
                    returnModel = NModel;

                    isConnect = false;
                    mS7con.disconnectPLc();
                }else{
                    Console.WriteLine($"Error into: [Try to Print DMC] isConnect: {isConnect}");
                }
                return returnModel;
            }
            return returnModel;
        }
        private string OpName_Read()
        {
            var OpName = mReadXML.GetVarInThreadp("OpName", "OPE");
            return OpName[0];
        }
        private bool ACK_Read()
        {
            bool? AckStatus = null;

            var DB_ack = mReadXML.GetVarInThreadp("ACKDatablock", "CSC");
            var Byte_ack = mReadXML.GetVarInThreadp("ACKByte", "CSC");
            var Bite_ack = mReadXML.GetVarInThreadp("ACKBit", "CSC");

            if((DB_ack.Count > 0) && (Byte_ack.Count > 0) && (Bite_ack.Count > 0))
            {
                int _ackDatablock = int.Parse(DB_ack[0]);
                int _ackbyte = int.Parse(Byte_ack[0]);
                int _ackbite = int.Parse(Bite_ack[0]);

                if(mS7con.connectPLc()){
                    bool ACK_signal = mS7con.ReadBit(_ackDatablock, _ackbyte, _ackbite);
                    AckStatus = ACK_signal;
                }else{
                    throw new Exception("Not connect to PLC or something alse !");
                }
            }
            return AckStatus ?? false;
        }
        private bool REQ_Read(string _thread)
        {
            bool? RqeStatus = null;

            var DB_req = mReadXML.GetVarInThreadp("REQDatablock", _thread);
            var Byte_req = mReadXML.GetVarInThreadp("REQByte", _thread);
            var Bite_req = mReadXML.GetVarInThreadp("REQBit", _thread);

            if((DB_req.Count > 0) && (Byte_req.Count > 0) && (Bite_req.Count > 0)){

                int _reqDatablock = int.Parse(DB_req[0]);
                int _reqByte = int.Parse(Byte_req[0]);
                int _reqbite = int.Parse(Bite_req[0]);

                if(mS7con.connectPLc() && !isConnect){
                    isConnect = true;

                    bool REQ_signal = mS7con.ReadBit(_reqDatablock, _reqByte, _reqbite);
                    RqeStatus = REQ_signal;

                    isConnect = false;
                    mS7con.disconnectPLc();
                }else{
                    Console.WriteLine($"Error into: [REQ_Read] isConnect: {isConnect}");
                }
            }
            return RqeStatus ?? false;
        }
        private string DMC_Read(string _thread)
        {
            string returnDmc = "No Value";

            var DB_dmc = mReadXML.GetVarInThreadp("DMCDatablock", _thread);
            var Byte_dmc = mReadXML.GetVarInThreadp("DMCStartByte", _thread);
            var Lenght_dmc = mReadXML.GetVarInThreadp("DMCLenght", _thread);           

            if((DB_dmc.Count > 0) && (Byte_dmc.Count > 0) && (Lenght_dmc.Count > 0))
            {
                int _dMCDatablock = int.Parse(DB_dmc[0]);
                int _dMCStartByte = int.Parse(Byte_dmc[0]);
                int _dMCLenght = int.Parse(Lenght_dmc[0]);

                if (mS7con.connectPLc())
                {
                    string DMC = mS7con.ReadString(_dMCDatablock, _dMCStartByte, _dMCLenght);
                    returnDmc = DMC;
                }
                else
                {
                    throw new Exception("Not connect to PLC or somthing alse !");
                }
                return returnDmc;
            }
            return returnDmc;
        }
        private int MOD_Read(string _thread)
        {
            int returnModel = 0;

            var DB_model = mReadXML.GetVarInThreadp("ModelDatablock", _thread);
            var Byte_model = mReadXML.GetVarInThreadp("ModelByte", _thread );

            if((DB_model.Count > 0) && (Byte_model.Count > 0))
            {
                int _modelDatablock = int.Parse(DB_model[0]);
                int _modelByte = int.Parse(Byte_model[0]);

                if(mS7con.connectPLc())
                {
                    int NModel = mS7con.ReadByte(_modelDatablock, _modelByte);
                    returnModel = NModel;
                }
                else
                {
                    throw new Exception("Not connect to PLC or something alse !");
                }
                return returnModel;
            }
            return returnModel;
        }
        private int MOD_Read_TRC()
        {
            int returnModel = 0;

            var DB_model = mReadXML.GetVarInThreadp("ModelCodeDatablock", "TRC");
            var Byte_model = mReadXML.GetVarInThreadp("ModelCodeByte", "TRC" );

            if((DB_model.Count > 0) && (Byte_model.Count > 0))
            {
                int _modelDatablock = int.Parse(DB_model[0]);
                int _modelByte = int.Parse(Byte_model[0]);

                if(mS7con.connectPLc())
                {
                    int NModel = mS7con.ReadByte(_modelDatablock, _modelByte);
                    returnModel = NModel;
                }
                else
                {
                       throw new Exception("Not connect to PLC or something alse !");
                }
                return returnModel;
            }
            return returnModel;
        }
        private List<string> STEp_Read(string _thread)                  // This function return a quantity of steps
        {
            var list = mReadXML.StepNUMp(_thread);
        
            if(list.Count > 0){
               return list; 
            }else{
                throw new Exception("No to read a steps !");
            }
        }
        private DateTime InDateTime_Read()                              // Only for TRC Thread
        {
            DateTime dt = DateTime.MinValue;

            var DB_inDTL = mReadXML.GetVarInThreadp("InDateTimeDatablock", "TRC");
            var Byte_inDTL = mReadXML.GetVarInThreadp("InDateTimeStartByte", "TRC");

            if((DB_inDTL.Count > 0) && (Byte_inDTL.Count > 0))
            {
                int _inDateTimeDatablock = int.Parse(DB_inDTL[0]);
                int _inDateTimeStartByte = int.Parse(Byte_inDTL[0]);

                if(mS7con.connectPLc())
                {
                    var inDLT = mS7con.DBReadDTL(_inDateTimeDatablock, _inDateTimeStartByte);
                    dt = inDLT;   //.ToLocalTime();
                }
                else
                {
                    throw new Exception("Not connect to PLC or somthing alse ! during In Date time read TRC");
                }
            }
            return dt;
        }
        private DateTime OutDateTime_Read()                             // Only for TRC Thread
        {
            DateTime dt = DateTime.MinValue;

            var DB_outDTL = mReadXML.GetVarInThreadp("OutDateTimeDatablock", "TRC");
            var Byte_outDTL = mReadXML.GetVarInThreadp("OutDateTimeStartByte", "TRC");

            if((DB_outDTL.Count > 0) && (Byte_outDTL.Count > 0))
            {
                int _outDateTimeDatablock = int.Parse(DB_outDTL[0]);
                int _outDateTimeStartByte = int.Parse(Byte_outDTL[0]);

                if(mS7con.connectPLc())
                {
                    var inDLT = mS7con.DBReadDTL(_outDateTimeDatablock, _outDateTimeStartByte);
                    dt = inDLT;   //.ToLocalTime();
                }
                else
                {
                    throw new Exception("Not connect to PLC or somthing alse ! during In Date time read TRC");
                }
            }
            return dt;
        }
        private List<int> ESTRC_Read(List<string> quantityOfSteps)      // Only for TRC Thread
        {
             #region VARIABELS

            List<string> DB_ESTRC_tem = new List<string>();
            List<string> Byte_ESTRC_tem = new List<string>();

            List<int> DB_ESTRC_int = new List<int>();
            List<int> Byte_ESTRC_int = new List<int>();

            List<int> ESTRS_forSpecificOPxxx = new List<int>();
            #endregion
                
            try // ESTRC for each specific steps
            {
                if(quantityOfSteps.Count > 0) // I read from XML data regarding a ESTRC DB & Bytes
                {
                    foreach(var item in quantityOfSteps){
                        var temDBStep = mReadXML.GetVar_1LevelInThreadp(item, "ESTRCDatablock", "TRC");
                        var temByteStep = mReadXML.GetVar_1LevelInThreadp(item, "ESTRCByte", "TRC");
                        DB_ESTRC_tem.Add(temDBStep[0]);
                        Byte_ESTRC_tem.Add(temByteStep[0]);    
                   }
                }

                if((DB_ESTRC_tem.Count > 0) && (Byte_ESTRC_tem.Count > 0)) // I convert List<string> to List<int>. I use Linq
                {
                    DB_ESTRC_int = DB_ESTRC_tem.Select(int.Parse).ToList();
                    Byte_ESTRC_int = Byte_ESTRC_tem.Select(int.Parse).ToList();

                    if(mS7con.connectPLc())
                    {
                        if(DB_ESTRC_int.Count == Byte_ESTRC_int.Count){
                            for(int i = 0; i < DB_ESTRC_int.Count; i++){
                                ESTRS_forSpecificOPxxx.Add(mS7con.ReadByte(DB_ESTRC_int[i], Byte_ESTRC_int[i]));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Error during: Tray to read ESTRC of each steps :{ex.Message}\n{ex.StackTrace}");
            }
            
            return ESTRS_forSpecificOPxxx;
        }
        private VariableData VariableSforStep_Read(string NoStep)       // Only for TRC Thread
        {
            VariableData dataStepVar;
    
            if(!int.TryParse(NoStep, out int NoStepCheck) || NoStepCheck <= 0)
            {
                throw new ArgumentException("Invalid steo number");
            }

            // helper
            List<string> GetXmlData_helperVar(string noStep, string tag)
            {
                return mReadXML.GetVar_2LevelInThreadpSRM(noStep, tag, "TRC");
            }

            dataStepVar = new VariableData{
                VariableDesc = GetXmlData_helperVar(NoStep, "VariableDesc"),
                SelectedValueType = GetXmlData_helperVar(NoStep, "SelectedValueType"),
                TRCDatablock = GetXmlData_helperVar(NoStep, "TRCDatablock"),
                TRCStartAddress = GetXmlData_helperVar(NoStep, "TRCStartAddress"),
                TRCLength = GetXmlData_helperVar(NoStep, "TRCLenght"),
                MinDatablock = GetXmlData_helperVar(NoStep, "MinDatablock"),
                MinStartAddress = GetXmlData_helperVar(NoStep, "MinStartAddress"),
                MaxDatablock = GetXmlData_helperVar(NoStep, "MaxDatablock"),
                MaxStartAddress = GetXmlData_helperVar(NoStep, "MaxStartAddress")
            };
 
            return dataStepVar;
        }
        private ComponentData ComponetSforStep_Read(string NoStep)      // Only for TRC Thread
        {
            ComponentData dataStepComp;

            if(!int.TryParse(NoStep, out int NoStepCheck) || NoStepCheck <= 0)
            {
                throw new ArgumentException("Invalid step number");
            }

            // helper
            List<string> GetXmlData_helperComp(string noStep, string tag)
            {
                return mReadXML.GetVar_2LevelInThreadpCRM(noStep, tag, "TRC");
            }

            dataStepComp = new ComponentData{
                ComponentDesc = GetXmlData_helperComp(NoStep, "ComponentDesc"),
                TRCDatablock = GetXmlData_helperComp(NoStep, "TRCDatablock"),
                TRCStartAddress = GetXmlData_helperComp(NoStep, "TRCStartAddress"),
                TRCLenght = GetXmlData_helperComp(NoStep, "TRCLenght")
            };

            return dataStepComp;
        }
        private (VarSteps, ComSteps) CompSANDVarSforStep_Read()         // [Only for TRC Thread] --> Those Data I use to refer dairectly data in to PLC [Components & Variabels] 
        {
            // This function is Realy Important.
            // She use a two functio: 
            //            [VariableSforStep_Read(string NoStep)]
            //            [ComponetSforStep_Read(string NoStep)]

            // and return DB's addres for a Process Archive data
            // and alsow I use it to creatin Tables Headers in SQL Database    

            #region Variables
            VarSteps sTepsDataVar = new VarSteps();
            ComSteps sTepsDataComp =new ComSteps();

            #endregion
            
            List<string> quantityOfSteps = mReadXML.StepNUMp("CSC");
                  
            // The function VariableSforStep_Read return instans for VariableData class which conntent all Variabels for specific Step
            for(int i = 0; i < quantityOfSteps.Count; i++){
                sTepsDataVar.ListOfVariabelsAllSteps.Add(VariableSforStep_Read(quantityOfSteps[i]));
                sTepsDataComp.ListOfComponetsAllSteps.Add(ComponetSforStep_Read(quantityOfSteps[i]));
            }

            return (sTepsDataVar, sTepsDataComp);
        }
        #endregion

        #region WRITE
        private bool ACKprt_Write(bool _val, List<int> ACKPrint)        // PRT Manage
        {
             bool? AckStatus = null;

            if(ACKPrint.Count > 0){

             var _ackDatablock = ACKPrint[0];
             var _ackbyte = ACKPrint[1];
             var _ackbite = ACKPrint[2];

                if(mS7con.connectPLc() && !isConnect){
                    isConnect = true;

                    AckStatus = mS7con.WriteBit(_ackDatablock, _ackbyte, _ackbite, _val);

                    isConnect = false;
                    mS7con.disconnectPLc();
                }else{
                    Console.WriteLine($"Error into: [Try to Print DMC] isConnect: {isConnect}");
                }
            }
             return AckStatus ?? false;
        }
        private bool ACK_Write(bool _val, string _thread)
        {
            bool? AckStatus = null;

            var DB_ack = mReadXML.GetVarInThreadp("ACKDatablock", _thread);
            var Byte_ack = mReadXML.GetVarInThreadp("ACKByte", _thread);
            var Bite_ack = mReadXML.GetVarInThreadp("ACKBit", _thread);

            if((DB_ack.Count > 0) && (Byte_ack.Count > 0) && (Bite_ack.Count > 0))
            {
                int _ackDatablock = int.Parse(DB_ack[0]);
                int _ackbyte = int.Parse(Byte_ack[0]);
                int _ackbite = int.Parse(Bite_ack[0]);

                if(mS7con.connectPLc()){
                    AckStatus = mS7con.WriteBit(_ackDatablock, _ackbyte, _ackbite, _val);
                }else{
                    throw new Exception("Not connect to PLC or something alse !");
                }
            }
            return AckStatus ?? false;
        }
        private bool WOK_Write(bool _val, string _thread)
        {
            bool? WokStatus = null;

            var DB_wok = mReadXML.GetVarInThreadp("WOKDatablock", _thread);
            var Byte_wok = mReadXML.GetVarInThreadp("WOKByte", _thread);
            var Bite_wok = mReadXML.GetVarInThreadp("WOKBit", _thread);

            if((DB_wok.Count > 0) && (Byte_wok.Count > 0) && (Bite_wok.Count > 0))
            {
                int _wokDatablock = int.Parse(DB_wok[0]);
                int _wokByte = int.Parse(Byte_wok[0]);
                int _wokBite = int.Parse(Bite_wok[0]);

                if(mS7con.connectPLc()){
                    WokStatus = mS7con.WriteBit(_wokDatablock, _wokByte, _wokBite, _val);
                }
            }
            return WokStatus ?? false;
        }
        private bool WKO_Write(bool _val, string _thread)
        {
             bool? WokStatus = null;

            var DB_wko = mReadXML.GetVarInThreadp("WKODatablock", _thread);
            var Byte_wko = mReadXML.GetVarInThreadp("WKOByte", _thread);
            var Bite_wko = mReadXML.GetVarInThreadp("WKOBit", _thread);

            if((DB_wko.Count > 0) && (Byte_wko.Count > 0) && (Bite_wko.Count > 0))
            {
                int _wkoDatablock = int.Parse(DB_wko[0]);
                int _wkoByte = int.Parse(Byte_wko[0]);
                int _wkoBite = int.Parse(Bite_wko[0]);

                if(mS7con.connectPLc()){
                    WokStatus = mS7con.WriteBit(_wkoDatablock, _wkoByte, _wkoBite, _val);
                }
            }
            return WokStatus ?? false;
        }
        private bool RES_Write(int errorInfo, string _thread)
        {
            bool? ResStatus = null;
            byte errorCode = Convert.ToByte(errorInfo);
            
            var DB_res = mReadXML.GetVarInThreadp("WorkResultDatablock", _thread);
            var Byte_res = mReadXML.GetVarInThreadp("WorkResultByte", _thread);

            if((DB_res.Count > 0) && (Byte_res.Count > 0))
            {
                int _res_Datablock = int.Parse(DB_res[0]);
                int _res_Byte = int.Parse(Byte_res[0]);

                if(mS7con.connectPLc()){
                    var result = mS7con.WriteByte(_res_Datablock, _res_Byte, errorCode);
                    ResStatus = result;
                }
            }
            return ResStatus ?? false;
        }
        private bool ESFAS_WriteForStep(List <string> listStep, string _thread, int efasVal)
        {
            bool? EfasStatus = null;
            byte _efasVal = Convert.ToByte(efasVal);

            for (int i = 0; i < listStep.Count; i++)
            {   
                var DB_efas = mReadXML.GetVar_1LevelInThreadp(listStep[i],"EFASDatablock", _thread);
                var Byte_efas = mReadXML.GetVar_1LevelInThreadp(listStep[i], "EFASByte", _thread);

                #if EFAS_WRITE
                Console.WriteLine($"[debug!]NoStep: {listStep[i]} DB: {DB_efas[0]} Byte: {Byte_efas[0]}");
                #endif

                if((DB_efas.Count > 0) && (Byte_efas.Count > 0)){
                    int _efas_Datablock = int.Parse(DB_efas[0]);
                    int _efas_Byte = int.Parse(Byte_efas[0]);

                    if(mS7con.connectPLc() && !isConnect){
                        isConnect = true;
                        var result = mS7con.WriteByte(_efas_Datablock, _efas_Byte, _efasVal);
                        isConnect = false;
                    }
                }

            }
            return EfasStatus ?? false;
        }
        private bool ESFAS_Write(List <string> listStep, string _thread, List<string> efasVal)
        {
            bool? EfasStatus = null;
            
            List<int> efasInt = efasVal[0]
                .Select(c => int.Parse(c.ToString()))
                .ToList();

            #if SHOWCSC
            foreach(var item in efasInt){
                Console.WriteLine($"[debug!] LIST INT: {item}");
            }
            #endif

            //List<byte> byteList = efasInt.ConvertAll<int, byte>(i => (byte)i);
            List<byte> byteListEFAS = efasInt.Select(i => (byte)i).ToList();


            for (int i = 0; i < listStep.Count; i++)
            {   
                var DB_efas = mReadXML.GetVar_1LevelInThreadp(listStep[i],"EFASDatablock", _thread);
                var Byte_efas = mReadXML.GetVar_1LevelInThreadp(listStep[i], "EFASByte", _thread);

                #if EFAS_WRITE
                Console.WriteLine($"[debug!]NoStep: {listStep[i]} DB: {DB_efas[0]} Byte: {Byte_efas[0]}");
                #endif

                if((DB_efas.Count > 0) && (Byte_efas.Count > 0)){
                    int _efas_Datablock = int.Parse(DB_efas[0]);
                    int _efas_Byte = int.Parse(Byte_efas[0]);

                    if(mS7con.connectPLc() && !isConnect){
                        isConnect = true;
                        var result = mS7con.WriteByte(_efas_Datablock, _efas_Byte, byteListEFAS[i]);
                        isConnect = false;
                    }
                }

            }
            return EfasStatus ?? false;
        }
        #endregion

        #region SUPPORTS
        private string NoPart(string _dmc, int statIndex, int offset)
        {
            var t_dmc = _dmc.Replace("\0", "").Trim();  // remove all [white chars] from the string
            var c_dmc = ConvertCut(t_dmc, statIndex, offset);  // this examples -> startIndex: 1  offset: 4

            return c_dmc;
        }
        private string ConvertCut(string inputString, int startIndex, int offset)
        {
            // Check if the startIndex and offset are within the bounds of the inputString
            if (startIndex < 0 || startIndex >= inputString.Length){
                Console.WriteLine($"{inputString.Length}");
                throw new Exception("Error: startIndex is out of bounds.");
            }
            if (offset < 0 || startIndex + offset > inputString.Length){
                Console.WriteLine($"{inputString.Length}");
                throw new Exception($"Error: offset goes out of bounds of the inputString Start:{startIndex} Offset:{offset}.");
            }
            return inputString.Substring(startIndex, offset);
        }
        bool RulsControl(string whole_dmc, string NoSeq_, bool SpecificExist) // This function check two rules: Sequences && EFAS for previouse station.
        {
            List<int> NoSeqForAllExecuteOp = new List<int>();
            List<string?> EFASforLastRunOpInSeq = new List<string?>();
            List<string?> EFASforPreviousOpInSeq = new List<string?>();
            string all_dmc = NoPart(whole_dmc, 0, 9);
            int LastRuningOPmax = 0;

            bool rules_1;
            bool rules_2;
            bool rules_3;

            try
            {
                using(var context = new DsmDbConntext())
                {
                    NoSeqForAllExecuteOp = context.dbEFAS_Ps // for basic and specifci
                        .Where(x => x.DMC == all_dmc)
                        .Select(x => x.NoSeq)
                        .ToList();
                    
                    LastRuningOPmax = NoSeqForAllExecuteOp.DefaultIfEmpty(0).Max(); // This Ensure that if thelist is empty, default value assing will by [0]
                    LastRuningOPmax = NoSeqForAllExecuteOp.Max();
                    
                    EFASforLastRunOpInSeq = context.dbEFAS_Ps   // for basic and specifci
                        .Where(x => x.NoSeq == LastRuningOPmax)
                        .Select(x => x.EFAS).ToList();

                    // I need to know NoSek and subtract 1 from it, it's probably a good way to win :)
                    var previouseNoSeq = int.Parse(NoSeq_) - 1;

                    if(previouseNoSeq > 0){

                        EFASforPreviousOpInSeq = context.dbEFAS_Ps
                        .Where(x => x.NoSeq == previouseNoSeq)
                        .Select(x => x.EFAS).ToList();
                    }

                    rules_1 = ((int.Parse(NoSeq_) - LastRuningOPmax) == 1 || (int.Parse(NoSeq_) - LastRuningOPmax) == 0) ? true : false; // Work only on the same or next POxxx
                    rules_2 = EFASforLastRunOpInSeq.Any(item => item == null || !item.Contains("0")); // EFASforLastRunOpInSeq.Contains("0"); 
                    rules_3 = EFASforPreviousOpInSeq.Any(item => item == null || !item.Contains("0")); // EFASforPreviousOpInSeq.Contains("0");

                    if(SpecificExist)
                    {
                        var ReworkByOperatorSequenceOff = context.dbEFAS_Ps // only specific !!!
                            .Where(x => x.NoSeq == int.Parse(NoSeq_))
                            .Select(x => x.Rew)
                            .FirstOrDefault(); 
                        
                        if(ReworkByOperatorSequenceOff == 1)
                        {
                            rules_1 = true;
                            rules_2 = true;

                            var result = context.dbEFAS_Ps.FirstOrDefault(x => x.NoSeq == int.Parse(NoSeq_) && x.Rew == 1);
                            if(result != null){
                                 result.Rew = 0;
                                 context.SaveChanges();
                            }
                        
                            #if SHOWCSC
                            Console.WriteLine($"ReworkByOperatorSequenceOff: {ReworkByOperatorSequenceOff}");
                            #endif
                        }     
                    }
                    
                    #if SHOWCSC
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine("Rules Check ... (Exist only DMC and OP)");
                    Console.WriteLine($"LastRuningOPmax: {LastRuningOPmax}");
                     
                    foreach(var item in NoSeqForAllExecuteOp){
                        Console.WriteLine($"No Seq For All Execute Op: {item}");
                    }
                    Console.WriteLine("------------------------------------------");
                    foreach(var item in EFASforLastRunOpInSeq){
                        Console.WriteLine($"EFAS for Last Run Op In Seq: {item}");
                    }
                    foreach(var item in EFASforPreviousOpInSeq){
                        Console.WriteLine($"EFAS for Previous Op In Seq: {item} RULES 3: {rules_3}");
                    }
                    Console.WriteLine("------------------------------------------");
                    #endif
                   
                    if(rules_1 && rules_2 && rules_3){
                        Console.WriteLine($"Result [OK] from check R1: SEQ: {rules_1} R2: EFAS last OP: {rules_2} EFAS previousOP: {rules_3}");
                        return true;
                    }
                    else{
                        Console.WriteLine($"Result [NOK] from check R1: SEQ :{rules_1} R2: EFAS last OP :{rules_2} EFAS previous OP: {rules_3}");
                        return false;
                    }
                    
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Error in RulsControl:{ex.Message}");   
            }
        }
        private void plcString(string dmc_)
        {
            int counter = 0;

            foreach(var x in dmc_)
            {
                counter++;
                Console.WriteLine($"index:{counter} --> [{x}]");
            }
        }
        private void DysplayAllVarForStep(VariableData dataAll)                 // This function dysplay data from model type of VariableData which I us to read a data for all steps
        {
            //VariableData variableData = dataAll;

            Console.WriteLine("VariableDesc: " + string.Join(", ", dataAll.VariableDesc));
            Console.WriteLine("SelectedValueType: " + string.Join(", ", dataAll.SelectedValueType));
            Console.WriteLine("TRCDatablock: " + string.Join(", ", dataAll.TRCDatablock));
            Console.WriteLine("TRCStartAddress: " + string.Join(", ", dataAll.TRCStartAddress));
            Console.WriteLine("TRCLength: " + string.Join(", ", dataAll.TRCLength));
            Console.WriteLine("MinDatablock: " + string.Join(", ", dataAll.MinDatablock));
            Console.WriteLine("MinStartAddress: " + string.Join(", ", dataAll.MinStartAddress));
            Console.WriteLine("MaxDatablock: " + string.Join(", ", dataAll.MaxDatablock));
            Console.WriteLine("MaxStartAddress: " + string.Join(", ", dataAll.MaxStartAddress)); 
          
        }
        private void DysplayAllVarForAllSteps(VarSteps dataAllSteps)            // ...
        {
            //VarSteps variablesData = dataAllSteps;

            for (int i = 0; i < dataAllSteps.ListOfVariabelsAllSteps.Count; i++)
            {
                //Console.WriteLine($"Step {i + 1}:");
                Console.WriteLine($"Step {i + 1} [VariableDesc]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].VariableDesc));
                Console.WriteLine($"Step {i + 1} [TRCDatablock]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].TRCDatablock));
                Console.WriteLine($"Step {i + 1} [TRCStartAddress]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].TRCStartAddress));
                Console.WriteLine($"Step {i + 1} [TRCLength]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].TRCLength));
                Console.WriteLine($"Step {i + 1} [MinDatablock]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].MinDatablock));
                Console.WriteLine($"Step {i + 1} [MinStartAddress]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].MinStartAddress));
                Console.WriteLine($"Step {i + 1} [MaxDatablock]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].MaxDatablock));
                Console.WriteLine($"Step {i + 1} [MaxStartAddress]: " + string.Join(", ", dataAllSteps.ListOfVariabelsAllSteps[i].MaxStartAddress));
            }
        }
        private void DysplayAllCompForAllSteps(ComSteps dataAllSteps)           // I'll want to description
        {
            //ComSteps componentData = dataAllSteps;
            
            for(int i = 0; i < dataAllSteps.ListOfComponetsAllSteps.Count; i++)
            {
                Console.WriteLine($"{i +1} [ComponentDesc]: " + string.Join(", ", dataAllSteps.ListOfComponetsAllSteps[i].ComponentDesc));
                Console.WriteLine($"{i +1} [TRCDatablock]: " + string.Join(", ", dataAllSteps.ListOfComponetsAllSteps[i].TRCDatablock));
                Console.WriteLine($"{i +1} [TRCStartAddres]: " + string.Join(", ", dataAllSteps.ListOfComponetsAllSteps[i].TRCStartAddress));
                Console.WriteLine($"{i +1} [TRCLenght]: " + string.Join(", ", dataAllSteps.ListOfComponetsAllSteps[i].TRCLenght));
            }
        }
        private void MakeTableAndHeaderArchiveTable(string tableName, string nameOP, HeaderTable newArchiveTable_trc)                                        // To male a Heders of dynamich archve tables --> External class [ ArchiveDbContext]
        {
            // This function is really important. She will make an Archive table in the SQL database 
            // for a data archive which will be processed during the TRC thread.
            // Before making a table check if it exists in the database if it exists. 
            // She breaks a work and then does not create a new Archive table. 
            // This decision is processed on the name of tables 
            try
            {
                 if(newArchiveTable_trc != null){   
                    var nameSteps = newArchiveTable_trc.List_nameSteps;
                    var statusSteps = newArchiveTable_trc.List_statusSteps;

                    var nameVariables = newArchiveTable_trc.List_nameVariables;
                    var variables = newArchiveTable_trc.List_variables;
                    var varMin = newArchiveTable_trc.List_varMin;
                    var varMax = newArchiveTable_trc.List_varMax;

                    var nameComponent = newArchiveTable_trc.List_nameComponent;
                    var components = newArchiveTable_trc.List_components;

                    var (isConnected, errorMessage) = mArchiveDB.CheckDatabaseConnection_nextGen();

                    if (isConnected){
                        Console.WriteLine("Database connected successfully.");
                    }
                    else{
                        Console.WriteLine($"Failed to connect to the database. Error: {errorMessage}");
                    }

                    mArchiveDB.MakeTable(tableName, nameOP, nameSteps, statusSteps, nameVariables, variables, varMin, varMax, nameComponent, components);
                }
                else{
                    Console.WriteLine("The list of header is empty...");
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during make a archive tavle for TRC thread" + ex.Message.Select(x => x.ToString()));
            }
        }
        private HeaderTable CreatingHeader(List<string> NameOfSteps, VarSteps item1, ComSteps item2) // I have to add a controls of flowe TRY/CATCH
        {
            HeaderTable headerTable = new HeaderTable();
            
            // Creating list for Name and Status. The loop run exacli so many time as we have a Steps
            for(int i = 0; i < NameOfSteps.Count; i++)
            {
                headerTable.List_nameSteps.Add($"Step{i + 1}");
                headerTable.List_statusSteps.Add($"Status{i + 1}");
            }
            // Creating list for Name/Var/Min/Max Variabels    
            for (int i = 0; i < item1.ListOfVariabelsAllSteps.Count; i++)
            {
                for(int vd = 0; vd < item1.ListOfVariabelsAllSteps[i].VariableDesc.Count; vd++)
                {
                    headerTable.List_nameVariables.Add($"S{1 + i}_VariablesName{1 + vd}");
                    headerTable.List_variables.Add($"S{1 + i}_Value{1 + vd}");
                    headerTable.List_varMax.Add($"S{1 + i}_ValMax_{1 + vd}");
                    headerTable.List_varMin.Add($"S{1 + i}_ValMin{1 + vd}");
                }
            }
            // Creating list for Name/NoSerial Commponent
            for(int i = 0; i < item2.ListOfComponetsAllSteps.Count; i++)
            {
                for(int cd = 0; cd < item2.ListOfComponetsAllSteps[i].ComponentDesc.Count; cd++)
                {
                    headerTable.List_nameComponent.Add($"S{1 + i}_ComponentsName{1 + cd}");
                    headerTable.List_components.Add($"S{1 + i}_ComponentsCode{1 + cd}");
                }
            }

            return headerTable;
        }
        private List<HeaderTable> CreatingVarAndComm(List<string> NameOfSteps, List<int> eSTRC_OP, VarSteps itemVar, ComSteps itemStep) // This function reads directly  the data from PLC 
        {
            List<HeaderTable> dataToHedesrForSpecificStepAll = new List<HeaderTable>();
           // HeaderTable dataToHedesrForSpecificStep = new HeaderTable();

            try
            {
                // Variables for Steps (#)
                if(itemVar.ListOfVariabelsAllSteps.Count != 0){
                    
                        
                    for (int i = 0; i < itemVar.ListOfVariabelsAllSteps.Count; i++) // This loop reads a directly data from PLC and writes source data to the SQL database
                    {
                        HeaderTable dataToHedesrForSpecificStep = new HeaderTable();

                        dataToHedesrForSpecificStep.List_nameSteps.Add($"{NameOfSteps[i]}"); 
                        dataToHedesrForSpecificStep.List_statusSteps.Add($"{eSTRC_OP[i]}");

                        dataToHedesrForSpecificStep.List_nameVariables.AddRange(itemVar.ListOfVariabelsAllSteps[i].VariableDesc);             // name of variabels 
                        
                        var DB_TRCDatablock = itemVar.ListOfVariabelsAllSteps[i].TRCDatablock;              // DB for value refer variabel
                        var Byte_TRCStartAddress = itemVar.ListOfVariabelsAllSteps[i].TRCStartAddress;      // Byte for value refer variable 
                        var amount_TRCLength = itemVar.ListOfVariabelsAllSteps[i].TRCLength;                // Amount for value refer variable

                        var DB_MinDatablock = itemVar.ListOfVariabelsAllSteps[i].MinDatablock;
                        var Byte_MinStartAddress = itemVar.ListOfVariabelsAllSteps[i].MinStartAddress;
                        
                        var BD_MaxDatablock = itemVar.ListOfVariabelsAllSteps[i].MaxDatablock;
                        var Byte_MaxStartAddress = itemVar.ListOfVariabelsAllSteps[i].MaxStartAddress;



                        Console.WriteLine($"Debug information {i + 1} <<<<<<<< VARIABEL >>>>>>>");
                        Console.WriteLine($"DB_TRCDatablock: " + string.Join("| ", DB_TRCDatablock));
                        Console.WriteLine("Byte_TRCStartAddress: " + string.Join("| ", Byte_TRCStartAddress));
                        Console.WriteLine("amount_TRCLength: " + string.Join("| ", amount_TRCLength));

                        if((DB_TRCDatablock.Count > 0) && (Byte_TRCStartAddress.Count > 0) && (amount_TRCLength.Count > 0))
                        {
                            for(int x = 0;  x < DB_TRCDatablock.Count; x++)
                            {
                                int DB_TRC = int.Parse(DB_TRCDatablock[x]);
                                int Byte_TRC = int.Parse(Byte_TRCStartAddress[x]);
                                int len_TRC = int.Parse(amount_TRCLength[x]);

                                int DB_MAX = int.Parse(BD_MaxDatablock[x]);
                                int Byte_MAX = int.Parse(Byte_MaxStartAddress[x]);

                                int DB_MIN = int.Parse(DB_MinDatablock[x]);
                                int Byte_MIN = int.Parse(Byte_MinStartAddress[x]);

                                if(mS7con.connectPLc())
                                {

                                    int val = mS7con.ReadIntegerData(DB_TRC, Byte_TRC, len_TRC);   // Real or Integer !!! TO CHECK
                                    int max = mS7con.ReadIntegerData(DB_MAX, Byte_MAX, 1);
                                    int min = mS7con.ReadIntegerData(DB_MIN, Byte_MIN, 1);
                                    
                                    Console.WriteLine($"DB:{DB_TRC} Byte: {Byte_TRC} Len: {len_TRC}");
                                    Console.WriteLine($"value: {val}");
                                    
                                    Console.WriteLine($"DBmax:{DB_MAX} Bytemax: {Byte_MAX} Lenmax: {1}");
                                    Console.WriteLine($"value: {max}");

                                    Console.WriteLine($"DB:{DB_MIN} Byte: {Byte_MIN} Len: {1}");
                                    Console.WriteLine($"value: {min}");


                                    dataToHedesrForSpecificStep.List_variables.Add(Convert.ToString(val));
                                    dataToHedesrForSpecificStep.List_varMax.Add(Convert.ToString(max));
                                    dataToHedesrForSpecificStep.List_varMin.Add(Convert.ToString(min));
                                }
                                else
                                {
                                    Console.WriteLine("[CreatingVarAndComm()] We have issue whit the connection to PLC...");
                                }
                            }
                        }
                        dataToHedesrForSpecificStepAll.Add(dataToHedesrForSpecificStep);
                    }
                    
                }else{
                    Console.WriteLine("The item Var is empty !!!");
                    dataToHedesrForSpecificStepAll.Add(new HeaderTable());
                    return dataToHedesrForSpecificStepAll;
                }

                // Commponents for Step (#)     
                if(itemStep.ListOfComponetsAllSteps.Count !=0)
                {
                    // Creating list for Name/serial Component
                    for(int i = 0; i < itemStep.ListOfComponetsAllSteps.Count; i++) 
                    {

                        dataToHedesrForSpecificStepAll[i].List_nameComponent.AddRange(itemStep.ListOfComponetsAllSteps[i].ComponentDesc);

                        var DB_TRCDatablock = itemStep.ListOfComponetsAllSteps[i].TRCDatablock;               // DB for value refer serial
                        var Byte_TRCStartAddress = itemStep.ListOfComponetsAllSteps[i].TRCStartAddress;       // Byte for value refer serial 
                        var amount_TRCLength = itemStep.ListOfComponetsAllSteps[i].TRCLenght;                 // Amount for value refer serial

                        Console.WriteLine($"Debug information {i + 1} <<<<<<<< COMPONENT >>>>>>>");
                        Console.WriteLine($"DB_TRCDatablock: " + string.Join("| ", DB_TRCDatablock));
                        Console.WriteLine("Byte_TRCStartAddress: " + string.Join("| ", Byte_TRCStartAddress));
                        Console.WriteLine("amount_TRCLength: " + string.Join("| ", amount_TRCLength));

                        if((DB_TRCDatablock.Count > 0) && (Byte_TRCStartAddress.Count > 0) && (amount_TRCLength.Count > 0))
                        {
                            for(int x = 0;  x < DB_TRCDatablock.Count; x++)
                            {
                                int DB_TRC = int.Parse(DB_TRCDatablock[x]);
                                int Byte_TRC = int.Parse(Byte_TRCStartAddress[x]);
                                int len_TRC = int.Parse(amount_TRCLength[x]);

                                if(mS7con.connectPLc())
                                {
                                    var serial = mS7con.ReadString(DB_TRC, Byte_TRC, len_TRC);  // string
                                    
                                    Console.WriteLine($"DB:{DB_TRC} Byte: {Byte_TRC} Len: {len_TRC}");

                                    dataToHedesrForSpecificStepAll[i].List_components.Add(serial);
                                }
                                else
                                {
                                    Console.WriteLine("[CreatingVarAndComm()] We have issue whit the connection to PLC...");
                                }
                            }
                        }
                        // In this field We not return and add because everything is done in the above code  rows:[1683] & [1708]
                    }

                }
                else{
                    Console.WriteLine("The item Commponents are empty");
                    dataToHedesrForSpecificStepAll.Add(new HeaderTable());
                    return dataToHedesrForSpecificStepAll;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error, during prepare a data [dataToArchive()] : {ex.Message}");
            }
            return dataToHedesrForSpecificStepAll;
        }

        //----
        // private void AddDataToArchiveTable(string tableName, List<HeaderTable> dataToStoreInDatabase, HeaderTable columnName)
        // {
        //     try
        //     {
        //         // Check database connection
        //         var (isConnected, errorMessage) = mArchiveDB.CheckDatabaseConnection_nextGen();

        //         if (!isConnected)
        //         {
        //             Console.WriteLine($"Failed to connect to the database. Error: {errorMessage}");
        //             return;
        //         }

        //         foreach (var dataToArchive in dataToStoreInDatabase)
        //         {
        //             // SQL Insert Command Construction
        //             var insertDataQuery = new StringBuilder();
        //             insertDataQuery.AppendLine($"INSERT INTO {tableName} (");

        //             // Adding the column names from HeaderTable (in a specific order)
        //             var columnNames = new List<string>();
        //             columnNames.Add("OP");
        //             columnNames.AddRange(columnName.List_nameSteps);
        //             columnNames.AddRange(columnName.List_statusSteps);
        //             columnNames.AddRange(columnName.List_nameVariables);
        //             columnNames.AddRange(columnName.List_variables);
        //             columnNames.AddRange(columnName.List_varMin);
        //             columnNames.AddRange(columnName.List_varMax);
        //             columnNames.AddRange(columnName.List_nameComponent);
        //             columnNames.AddRange(columnName.List_components);
        //             columnNames.Add("DateAndTime");

        //             insertDataQuery.AppendLine(string.Join(", ", columnNames));
        //             insertDataQuery.AppendLine(") VALUES (");

        //             // Adding the values for each of the corresponding columns
        //             var values = new List<string>();
        //             values.Add("'Checked'"); // Assuming "OP" column value is constant

        //             // Match values from dataToArchive to columns, ensuring correct alignment
        //             int variableIndex = 0;
        //             foreach (var step in columnName.List_nameSteps)
        //             {
        //                 values.Add(dataToArchive.List_nameSteps.Count > variableIndex ? $"'{dataToArchive.List_nameSteps[variableIndex]}'" : "NULL");
        //                 variableIndex++;
        //             }

        //             variableIndex = 0;
        //             foreach (var status in columnName.List_statusSteps)
        //             {
        //                 values.Add(dataToArchive.List_statusSteps.Count > variableIndex ? $"'{dataToArchive.List_statusSteps[variableIndex]}'" : "NULL");
        //                 variableIndex++;
        //             }

        //             variableIndex = 0;
        //             foreach (var variableName in columnName.List_nameVariables)
        //             {
        //                 if (variableIndex < dataToArchive.List_nameVariables.Count)
        //                 {
        //                     values.Add($"'{dataToArchive.List_nameVariables[variableIndex]}'");
        //                     values.Add($"'{dataToArchive.List_variables[variableIndex]}'");
        //                     values.Add($"'{dataToArchive.List_varMin[variableIndex]}'");
        //                     values.Add($"'{dataToArchive.List_varMax[variableIndex]}'");
        //                 }
        //                 else
        //                 {
        //                     values.Add("NULL");
        //                     values.Add("NULL");
        //                     values.Add("NULL");
        //                     values.Add("NULL");
        //                 }
        //                 variableIndex++;
        //             }

        //             variableIndex = 0;
        //             foreach (var componentName in columnName.List_nameComponent)
        //             {
        //                 if (variableIndex < dataToArchive.List_nameComponent.Count)
        //                 {
        //                     values.Add($"'{dataToArchive.List_nameComponent[variableIndex]}'");
        //                     values.Add($"'{dataToArchive.List_components[variableIndex]}'");
        //                 }
        //                 else
        //                 {
        //                     values.Add("NULL");
        //                     values.Add("NULL");
        //                 }
        //                 variableIndex++;
        //             }

        //             values.Add($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");

        //             insertDataQuery.AppendLine(string.Join(", ", values));
        //             insertDataQuery.AppendLine(");");

        //             // Execute the SQL Insert command
        //             mArchiveDB.Database.ExecuteSqlRaw(insertDataQuery.ToString());
        //         }
        //         Console.WriteLine("Data added to archive table successfully.");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error adding data to archive table: {ex.Message}");
        //     }
        // }

        private void AddDataToArchiveTable(string tableName, List<HeaderTable> dataToStoreInDatabase, HeaderTable columnName)
        {

            // Usage Example:
            // var columnName = new HeaderTable
            // {
            //     List_nameSteps = new List<string> { "Step1", "Step2", "Step3", "Step4", "Step5", "Step6", "Step7" },
            //     List_statusSteps = new List<string> { "Status1", "Status2", "Status3", "Status4", "Status5", "Status6", "Status7" },
            //     List_nameVariables = new List<string> { "S1_VariablesName1", "S1_VariablesName2", "S2_VariablesName1", "S3_VariablesName1", "S4_VariablesName1", "S5_VariablesName1", "S6_VariablesName1", "S7_VariablesName1" },
            //     List_variables = new List<string> { "S1_Value1", "S1_Value2", "S2_Value1", "S3_Value1", "S4_Value1", "S5_Value1", "S6_Value1", "S7_Value1" },
            //     List_varMin = new List<string> { "S1_ValMin1", "S1_ValMin2", "S2_ValMin1", "S3_ValMin1", "S4_ValMin1", "S5_ValMin1", "S6_ValMin1", "S7_ValMin1" },
            //     List_varMax = new List<string> { "S1_ValMax_1", "S1_ValMax_2", "S2_ValMax_1", "S3_ValMax_1", "S4_ValMax_1", "S5_ValMax_1", "S6_ValMax_1", "S7_ValMax_1" },
            //     List_nameComponent = new List<string> { "S6_ComponentsName1", "S7_ComponentsName1" },
            //     List_components = new List<string> { "S6_ComponentsCode1", "S7_ComponentsCode1" }
            // };
            // var dataToStoreInDatabase = new List<HeaderTable>
            // {
            //     new HeaderTable
            //     {
            //         List_nameSteps = new List<string> { "Step1" },
            //         List_statusSteps = new List<string> { "Completed", "Pending" },
            //         List_nameVariables = new List<string> { "S1_VariablesName1", "S1_VariablesName2" },
            //         List_variables = new List<string> { "Val1", "Val2" },
            //         List_varMin = new List<string> { "S1_ValMin1", "S1_ValMin2" },
            //         List_varMax = new List<string> { "S1_ValMax_1", "S1_ValMax_2" },
            //         List_nameComponent = new List<string> {},
            //         List_components = new List<string> {}
            //     }
            // };

            try
            {
                // Check database connection
                var (isConnected, errorMessage) = mArchiveDB.CheckDatabaseConnection_nextGen();

                if (!isConnected)
                {
                    Console.WriteLine($"Failed to connect to the database. Error: {errorMessage}");
                    return;
                }

                // SQL Insert Command Construction
                var insertDataQuery = new StringBuilder();
                insertDataQuery.AppendLine($"INSERT INTO {tableName} (");

                // Adding the column names from HeaderTable (in a specific order)
                var columnNames = new List<string>();
                columnNames.Add("OP");
                columnNames.AddRange(columnName.List_nameSteps);
                columnNames.AddRange(columnName.List_statusSteps);
                columnNames.AddRange(columnName.List_nameVariables);
                columnNames.AddRange(columnName.List_variables);
                columnNames.AddRange(columnName.List_varMin);
                columnNames.AddRange(columnName.List_varMax);
                columnNames.AddRange(columnName.List_nameComponent);
                columnNames.AddRange(columnName.List_components);
                columnNames.Add("DateAndTime");

                insertDataQuery.AppendLine(string.Join(", ", columnNames));
                insertDataQuery.AppendLine(") VALUES (");

                // Adding the values for each of the corresponding columns
                var values = new List<string>();
                values.Add("'Checked'"); // Assuming "OP" column value is constant

                // Extract and add values from dataToStoreInDatabase for each column
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_nameSteps));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_statusSteps));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_nameVariables));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_variables));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_varMin));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_varMax));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_nameComponent));
                values.AddRange(ExtractValues(dataToStoreInDatabase, d => d.List_components));

                // Adding DateAndTime
                values.Add($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'");

                insertDataQuery.AppendLine(string.Join(", ", values));
                insertDataQuery.AppendLine(");");

                // Execute the SQL Insert command
                mArchiveDB.Database.ExecuteSqlRaw(insertDataQuery.ToString());

                Console.WriteLine("Data added to archive table successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding data to archive table: {ex.Message}");
            }
        }

        private List<string> ExtractValues(List<HeaderTable> dataToStoreInDatabase, Func<HeaderTable, List<string>> selector)       // Helper function to extract values from a list of HeaderTable objects
        {
            var values = new List<string>();

            foreach (var data in dataToStoreInDatabase)
            {
                var selectedList = selector(data);
                foreach (var value in selectedList)
                {
                    values.Add(!string.IsNullOrEmpty(value) ? $"'{value}'" : "NULL");
                }
            }

            return values;
        }
    

        //----

        #endregion

        #region PUBLIC_METHOD
        #endregion
    }
}