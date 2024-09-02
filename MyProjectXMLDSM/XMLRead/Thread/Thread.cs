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

namespace CSC 
{
    public class XThread 
    {   
        #region Private and Public Variables

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
        #endregion 

        private int stepsCSC;
        private int stepsCRC;
        private int stepsTRC;
       
        public XThread(string fileName, string IpAddres, int slot, int rack)
        {
            mReadXML = new ReadXML(fileName);
            mS7con = new S7con(IpAddres, slot, rack);
            stepsCSC = 0;
            stepsCSC = 0;
            stepsTRC = 0; 

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
        public bool Print_DataMatric(string NoDmc)
        {
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
            
            string  dmc = string.Empty;         // DMC 
            int model = 0;                      // No model
            bool dmcAndModel_Check = true;      // DMC [correct status]
            bool cycleStatus = false;
            #endregion

            bool REQ = REQ_Read("TRC");         // Read REQ

            #if SHOWCSC
            Console.WriteLine($"--> START THREAD TRC FOR Ip[{IpAddres}] : REQ: {REQ}");
            #endif

            if (REQ != false)                   // Debug
            {
                DateTime _in = InDateTime_Read();
                DateTime _out = OutDateTime_Read();
                string sDTLIn = _in.ToString();
                string sDTLOut = _out.ToString();

                Console.WriteLine($"sDTLIn: {sDTLIn}");
                Console.WriteLine($"sDTLOut: {sDTLOut}");
            }

            if(REQ && (stepsTRC == 0))          // Step [0] thread <-- We are waiting to REQ from PLC
            {
                dmc = DMC_Read("TRC"); // Read DMC
                model = MOD_Read_TRC();
                
                if(string.IsNullOrEmpty(dmc) || (model == 0 ))
                {
                    dmcAndModel_Check = false;
                    //RES_Write(16, "TRC");
                    stepsTRC = 3;
                }

                if(dmcAndModel_Check)
                {
                    // code ... controls
                    stepsTRC++; // 1
                }

                Console.WriteLine($"STEPS :{stepsTRC} DMC: {dmc} NoModel: {model}");

            }

            if(REQ && (stepsTRC == 1))          // Step [1] Stored a data into db.ESTRC_
            {
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

            if(REQ != true && (stepsTRC == 4))  // Steps Finish
            {
                ACK_Write( false, "TRC");
                stepsTRC = 0;
            }
            Console.WriteLine($"STEPS :{stepsTRC}");

            return cycleStatus;
        }
        
        // THE SIGNAL's :bits: ACK|REQ|WOK|WKO|RES and :byte: DMC|MOD
        #region READ
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

        #endregion

        #region WRITE
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
        
        #endregion
    }
}