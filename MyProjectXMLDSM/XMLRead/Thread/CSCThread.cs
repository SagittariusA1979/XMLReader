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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;

using readxmlFile;
using s7;
using Dsmdb;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.NetworkInformation;


namespace CSC 
{
    public class XThread 
    {   
        #region Private Variables

        private string _threadName;        // Name Thread
        private int _aCKDatablock;         // e.g. ACK DB1002.DBX0.0
        private int _aCKByte;
        private int _aCKBit;

        private int _rEQDatablock;           // e.g. REQ DB1002.DBX0.1
        private int _rEQByte;           
        private int _rEQBit;

        private int _dMCDatablock;           // e.g. DMC DB1002.DBX1 
        private int _dMCStartByte;
        private int _dMCLenght;

        private int _modelDatablock;         // e.g. ModelDataBlock DB1003
        private int _modelByte;

        private int _outModelDatablock;      // not use 
        private int _outModelByte;

        private int _wOKDatablock;           // e.g. WOK DB1004.DBX0.0
        private int _wOKByte;
        private int _wOKBit;

        private int _wKODatablock;           // e.g. WKO DB1005.DBX0.1
        private int _wKOByte;
        private int _wKOBit;

        private int _workResultDatablock;    // e.g. WR DB1006.DB0
        private int _workResultByte;

        private int _numberOfSteps;          // Number of step per cycle

        private string fileName;
        private string IpAddres;
        private int slot;
        private int rack;

        #endregion
        #region Ret/Set
    //     public string ThreadName
    //     {
    //         get{ return _threadName; }
    //         set{ _threadName = value; }
    //     }

    //     public int ACKDatablock 
    //     {
    //         get{ return _aCKDatablock;}
    //         set{ _aCKDatablock = value;}
    //     }
    //     public int ACKByte
    //     {
    //         get{ return _aCKByte;}
    //         set{ _aCKByte = value;}
    //     }
    //     public int ACKBBit
    //     {
    //         get{ return _aCKBit; }
    //         set{ _aCKBit = value;}
    //     }

    //     public int REQDatablock
    //     {
    //         get{ return _rEQDatablock; } 
    //         set { _rEQDatablock = value;}
    //     }
    //     public int REQByte
    //     {
    //         get{ return _rEQByte; }
    //         set{ _rEQByte = value; }  
    //     }           
    //     public int REQBit
    //     {
    //         get { return _rEQBit; }
    //         set { _rEQBit = value; }
    //     }

    //     public int DMCDatablock
    //     {
    //         get{ return _dMCDatablock;}
    //         set { _dMCDatablock = value;}
    //     }         
    //     public int DMCStartByte
    //     {
    //         get{ return  _dMCStartByte;}
    //         set { _dMCStartByte = value;}
    //     }
    //     public int DMCLenght
    //     {
    //         get{return _dMCLenght; }
    //         set { _dMCLenght = value;}
    //     }

    //     public int ModelDatablock
    //     {
    //         get{ return _modelDatablock; }
    //         set { _modelDatablock = value;}
    //     }         
    //     public int ModelByte
    //     {
    //         get{ return _modelByte; }
    //         set{ _modelByte = value;}
    //     }

    //     public int OutModelDatablock
    //     {
    //         get{ return _outModelDatablock;}
    //         set{ _outModelDatablock = value;}
    //     }     
    //    public int OutModelByte
    //    {
    //         get{ return _outModelByte; }
    //         set{_outModelByte = value;} 
    //    }

    //     public int WOKDatablock
    //     {
    //         get{ return _wOKDatablock;} 
    //         set{_wOKDatablock = value;}
    //     }          
    //     public int WOKByte
    //     {
    //         get{ return _wOKByte;}
    //         set{ _wOKByte = value;}
    //     }
    //     public int WOKBit
    //     {
    //         get{ return _wOKBit;}
    //         set{ _wOKBit = value;}
    //     }

    //     public int WKODatablock
    //     {
    //         get{ return _wKODatablock;}
    //         set {_wKODatablock = value;}
    //     }           
    //     public int WKOByte
    //     {
    //         get{ return _wKOByte;}
    //         set{ _wKOByte = value;}
    //     }
    //     public int WKOBit
    //     {
    //         get { return _wKOBit;}
    //         set{ _wKOBit = value;}
    //     }

    //     public int WorkResultDatablock
    //     {
    //         get{ return _workResultDatablock;}
    //         set{ _workResultDatablock = value;}
    //     }    
    //     public int WorkResultByte
    //     {
    //         get{return _workResultByte;}
    //         set{_workResultByte = value;}
    //     }

    //     public int NumberOfSteps
    //     {
    //         get{ return _numberOfSteps;}
    //         set{ _numberOfSteps = value;}
    //     }  

        #endregion
        #region INSTANCE
        private ReadXML mReadXML;
        private S7con mS7con;
        #endregion 

        private int steps;
       
        public XThread(string fileName, string IpAddres, int slot, int rack)
        {
            mReadXML = new ReadXML(fileName);
            mS7con = new S7con(IpAddres, slot, rack);
            steps = 0; 
        }

       
        // THE THREADS: CSC|CRC|TRC
        public bool IsAlive()
        {
            bool statusConnect = false;

            if(true){
                return statusConnect = true;

            }else{
                return statusConnect = false;
            }
        }

        public bool CSC_thread()
        {
            #region DESCRYYPTION
            // This function will be to menage a CSC thread.
            // --------------------------------------------
            // PLC      : set a DMC and MOD after set REQ  
            // Server   : if(REQ is 1) read a DMC and MOD
            // DB:      :  find a number of MOD refer to DMC and after compare whit MOD from PLC
            //            : if result is OK
            // Server   : set a RES|WOK|WKO| and ACK
            #endregion
                          
            List<string>? numberOfModelFromDB = new List<string>();

            bool status_csc = false;    // Basicly ststus for function
            bool resultDB = false;      // Result from search 
            int model = 0;              // Model    refer DSM
            string dmc = "#null#";      // dmc      refer DSM
            //int steps = 0;


            bool REQ = REQ_Read(); // Start csc thread <--
            
            #if SHOWCSC
            Console.WriteLine($"--> Status of REQ:{REQ}");
            #endif

            if (REQ && (steps == 0)){
                dmc = DMC_Read();
                model = MOD_Read();
                steps++;
                
                #if SHOWCSC
                Console.WriteLine($"REQ is True -> dms:{dmc} model:{model} S:{steps}");
                #endif
            }

            if(REQ && (steps == 1))
            {
                // DB context operation [We looking for a DMC and refer it to number of model] <--------
                using (var context = new DsmDbConntext())
                {
                    context.Database.EnsureCreated();
                    string t_dmc = dmc.Replace("\0", "").Trim();
                    Console.WriteLine($"#:{t_dmc}");
                    string c_dmc = ConvertCut(t_dmc, 1, 4);

                    #if SHOWCSC
                    Console.WriteLine($"Before convert:{t_dmc} | Afeter convert: {c_dmc}");
                    #endif

                    numberOfModelFromDB = context.dbModels
                        .Where(x => x.NumberOfModels == c_dmc)
                        .Select(x => x.ModelCode)
                        .ToList();

                    #if SHOWCSC
                    Console.WriteLine($"Query result from search :{dmc}: Number of models returned from DB:{numberOfModelFromDB.Count}");
                    #endif

                    // Assuming you want to check if the model exists in the results
                    resultDB = numberOfModelFromDB.Any(num => int.TryParse(num, out int numAsInt) && numAsInt == model);

                    // Assuming you want to check if the model exists in the results
                    //resultDB = numberOfModelFromDB.Contains(c_dmc.ToString());
                    /// <--------------------------------
                    
                    #if SHOWCSC
                    Console.WriteLine($"RESELT FOR SCHEAR:{resultDB} S:{steps}");
                    #endif
                    steps++;
                }
            }

            if(REQ && (steps == 2))
            {

                #if SHOWCSC
                Console.WriteLine($"REQ:{REQ} S:{steps}");
                #endif

                if(resultDB == true){
                    RES_Write(2,"CSC"); 
                    // EFAS write 
                    var step = STEp_Read("CSC");

                    if(step.Count > 0){
                        var quantity = step.Count;
                        for(int i=0; i < quantity; i++){
                            Console.WriteLine($"steps:{i}");
                            //EFAS read value e.g. [2]
                        }
                    }

                    WOK_Write(true);
                    ACK_Write(true);  // and Error handling (!)
                    //status_csc = true;

                    #if SHOWCSC
                    Console.WriteLine($"IF [resultDB == true]:|RES|WOK|ACK set 1 S:{steps}");
                    #endif
                }
                    
                if(resultDB == false){
                    RES_Write(3,"CSC");
                    // EFAS write
                    WKO_Write(true);
                    ACK_Write(true);
                    //status_csc = false;
                   
                    #if SHOWCSC
                    Console.WriteLine($"IF [resultDB == false]: RES|WKO|WKO| ACK set 1 S:{steps}");
                    #endif
                }
                steps++;
            }

            if (REQ != true && (steps == 3))
            {
                    ACK_Write(false); // and Error handling (!)
                    status_csc = true;
                    steps = 0;

                    #if SHOWCSC
                    Console.WriteLine($"ACK set 0| S:{steps} <--(FINISH CSC)");
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
            bool cycleStatus = false;

            return cycleStatus;
        }
        
        // THE SIGNAL's :bits: ACK|REQ|WOK|WKO|RES and :byte: DMC|MOD
        #region READ
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
        private bool REQ_Read()
        {
            bool? RqeStatus = null;

            var DB_req = mReadXML.GetVarInThreadp("REQDatablock","CSC");
            var Byte_req = mReadXML.GetVarInThreadp("REQByte", "CSC");
            var Bite_req = mReadXML.GetVarInThreadp("REQBit", "CSC");

            if((DB_req.Count > 0) && (Byte_req.Count > 0) && (Bite_req.Count > 0)){

                int _reqDatablock = int.Parse(DB_req[0]);
                int _reqByte = int.Parse(Byte_req[0]);
                int _reqbite = int.Parse(Bite_req[0]);

                if(mS7con.connectPLc()){
                    bool REQ_signal = mS7con.ReadBit(_reqDatablock, _reqByte, _reqbite);
                    RqeStatus = REQ_signal;
                }else{
                    throw new Exception("Not connect to PLC or something alse !");
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
        private int MOD_Read()
            {
                int returnModel = 0;

                var DB_model = mReadXML.GetVarInThreadp("ModelDatablock", "CSC");
                var Byte_model = mReadXML.GetVarInThreadp("ModelByte", "CSC");

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
        private List<string> STEp_Read(string _thread)                  // This function return a quantity of steps and number of steps.
        {
            var list = mReadXML.StepNUMp(_thread);
        
            if(list.Count > 0){
               return list; 
            }else{
                throw new Exception("No to read a steps !");
            }
        }
        #endregion

        #region WRITE
        private bool ACK_Write(bool _val)
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
                    AckStatus = mS7con.WriteBit(_ackDatablock, _ackbyte, _ackbite, _val);
                }else{
                    throw new Exception("Not connect to PLC or something alse !");
                }
            }
            return AckStatus ?? false;
        }
        private bool WOK_Write(bool _val)
        {
            bool? WokStatus = null;

            var DB_wok = mReadXML.GetVarInThreadp("WOKDatablock", "CSC");
            var Byte_wok = mReadXML.GetVarInThreadp("WOKByte", "CSC");
            var Bite_wok = mReadXML.GetVarInThreadp("WOKBit", "CSC");

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
        private bool WKO_Write(bool _val)
        {
             bool? WokStatus = null;

            var DB_wko = mReadXML.GetVarInThreadp("WKODatablock", "CSC");
            var Byte_wko = mReadXML.GetVarInThreadp("WKOByte", "CSC");
            var Bite_wko = mReadXML.GetVarInThreadp("WKOBit", "CSC");

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
        private bool RES_Write(int errorInfo, string _val)
        {
            bool? ResStatus = null;
            byte errorCode = Convert.ToByte(errorInfo);
            
            var DB_res = mReadXML.GetVarInThreadp("WorkResultDatablock", _val);
            var Byte_res = mReadXML.GetVarInThreadp("WorkResultByte", _val);

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
        private bool ESFAS_WriteForStep(string _thread)
        {
            bool result = false;
            return result;
        }
        #endregion

        #region SUPPORTS
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
        #endregion

    }
}