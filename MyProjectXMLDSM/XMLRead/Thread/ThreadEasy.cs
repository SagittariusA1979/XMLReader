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
using sqleasy;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Net;
using System.Data;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using System.Xml.Schema;


namespace support 
{
    public class EThread 
    {   
        #region Private and Public Variables

        private string fileName;
        private string IpAddres;
        private int slot;
        private int rack;

        private bool KeepALive;                     // Keep A Live
        private bool isConnect = false;             // I use regarding semafor which block  PLC connect

        #endregion
 
        #region INSTANCE
        private ReadXML mReadXML;
        private S7con mS7con;
        #endregion 

        private int stepsCSC;
       
        public EThread(string fileName, string IpAddres, int slot, int rack)
        {
            mReadXML = new ReadXML(fileName);
            mS7con = new S7con(IpAddres, slot, rack);
            stepsCSC = 0; 

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
            List<string>? numberOfModelFromDB = new List<string>();
            List<string>? nameOfModelFromDB = new List<string>();

            bool status_csc = false;    
            bool resultDB = false;      
            int model = 0;              
            string dmc = string.Empty;                 
            bool dmc_Check = true;      
            string currentOPName = "OPxxx"; 
            string NoSeq = string.Empty;    
            List<string>? curentOpEFAS = new List<string>(); 
            #endregion

            currentOPName = OpName_Read();
            bool REQ = REQ_Read("CSC"); 
            
            #if SHOWCSC
            Console.WriteLine($"--> START THREAD CSC FOR stNo:[{currentOPName}] : REQ: {REQ}");
            #endif

            if (REQ && (stepsCSC == 0))            // Step [0] Start thread <-- We are waiting to REQ from PLC
            {
                dmc = DMC_Read();
                model = MOD_Read("CSC");
                
                if(string.IsNullOrEmpty(dmc))
                {
                    dmc_Check = false;
                    RES_Write(16, "CSC"); 
                    stepsCSC = 4;
                }
                else
                {
                    stepsCSC++;
                }
                
                #if SHOWCSC
                Console.WriteLine($"REQ is True -> dms:{dmc} dmc_Check:{dmc_Check} model:{model} S:{stepsCSC}");
                #endif
            }

            if(REQ && (stepsCSC == 1))             // Step [1] thread <-- We looking for a DMC and refer it to number of model
            {
                using (var context = new DsmDbConntext())
                {
                    try
                    {
                        context.Database.EnsureCreated();

                        string c_dmc = NoPart(dmc, 1, 4); // Convert NoDMC to model patern e.g. 8002 this place I define a paterm for DMC

                        #if SHOWCSC
                        Console.WriteLine($"Before convert:{dmc} | Afeter convert: {c_dmc}");
                        #endif

                        var numberOfModelFromDB_ = context.dbModels // e.g [2]
                            .Where(x => x.NumberOfModels == c_dmc)
                            .Select(x => x.ModelCode)
                            .ToList();

                        var nameOfModelFrmaDB_ = context.dbModels // e.g [Model_1]
                            .Where(x => x.NumberOfModels == c_dmc)
                            .Select(x => x.ModelName)
                            .ToList();

                        var OpExistInThisModel = context.dbStations // e.g [2 number in sequence]
                            .Where(x => x.OPxxx == currentOPName)
                            .Select(x => EF.Property<string>(x, nameOfModelFrmaDB_[0]))
                            .ToList();

                        numberOfModelFromDB = numberOfModelFromDB_;
                        nameOfModelFromDB = nameOfModelFrmaDB_;
                        NoSeq = OpExistInThisModel[0];

                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Error during Step 1 :{ex.Message}");
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
                        throw new Exception($"Error during Step 1:{ex.Message}");

                    }  
                }
                stepsCSC++;
            }

            if(REQ && (stepsCSC == 2))     // Step [2] thread <-- We have a result [true] from compare No model bettwen DMC code & Opxx exists in the current Model
            {

                #if SHOWCSC
                Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->>");
                #endif

                if(resultDB == true){  // Step [2] thread | RESULT : [true] <--

                    // resultFromSQLdb = .... // check sqlServer regarding TM for the case a NOK & OK parts into previouse station

                    // if(resultFromSQLdb)
                    // {
                    //     // code ...
                    // }
                    // else
                    // {
                    //     WKO_Write(true, "CSC");
                    //     RES_Write(5, "CSC");

                    //     ACK_Write(true, "CSC");  // and Error handling (!)
                    // }

                    // #if SHOWCSC
                    // Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->> DMC:{dmc} resultFromSQLdb:{resultFromSQLdb}");
                    // #endif
                }

                if(resultDB == false){ // Step [2] thread | RESULT : [false] <--
                    WKO_Write(true, "CSC");
                    RES_Write(6, "CSC");

                    ACK_Write(true, "CSC");
                   
                    #if SHOWCSC
                    Console.WriteLine($"stNo:[{currentOPName}] :REQ: {REQ} :S:{stepsCSC} ->>");
                    #endif
                }
                stepsCSC++;
            }

            if(REQ && (stepsCSC == 4)) // When We have a error from quality check DMC and NoModel
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

            if (REQ != true && (stepsCSC == 3))
            {
                    ACK_Write(false, "CSC"); // and Error handling (!)
                    status_csc = true;
                    stepsCSC = 0;

                    #if SHOWCSC
                    Console.WriteLine($"STEP :{stepsCSC} <--(FINISH CSC)");
                    #endif
            } 
            return status_csc;
        }
        public bool TRC_thread()
        {
            #region DESCRYPTION

            // ...
            #endregion

            #region VARIABELS

            bool cycleStatus = false;
            #endregion

            bool REQ = REQ_Read("TRC");

            if (REQ != false)
            {
                DateTime _in = InDateTime_Read();
                DateTime _out = OutDateTime_Read();
                string sDTLIn = _in.ToString();
                string sDTLOut = _out.ToString();
 
                Console.WriteLine($"sDTLIn: {sDTLIn}");
                Console.WriteLine($"sDTLOut: {sDTLOut}");
            }

            #if SHOWCSC
            Console.WriteLine($"--> START THREAD TRC : REQ: {REQ}");
            #endif


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
        private string DMC_Read()
        {
            string returnDmc = "No Value";

            var DB_dmc = mReadXML.GetVarInThreadp("DMCDatablock", "CSC");
            var Byte_dmc = mReadXML.GetVarInThreadp("DMCStartByte", "CSC");
            var Lenght_dmc = mReadXML.GetVarInThreadp("DMCLenght", "CSC");           

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
        
        #endregion
    }
}
    