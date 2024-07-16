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
using System.Net.Http.Headers;


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

        public XThread(string fileName, string IpAddres, int slot, int rack)
        {
            mReadXML = new ReadXML(fileName);
            mS7con = new S7con(IpAddres, slot, rack); 
        }
        
        public bool CSC_cycle()
        {
            bool cycleStatus = false;

            #region DESCRYYPTION
            
            //  PLC -> | DMC | Model |           set:REQ
            //  DSM <- | WKO or WOK | WR error | set:ACK

            // Check: refer to DMC and Model in DataBase     
            #endregion

            var dmc = dmcRead();
            var model = modelRead();

            //DEBUG only
            Console.WriteLine($"DMC:{dmc} Model:{model}");

            return cycleStatus;
        }
        public bool TRC_cycle()
        {
            bool cycleStatus = false;

            return cycleStatus;
        }
        public bool CRC_cycle()
        {
            bool cycleStatus = false;

            return cycleStatus;
        }

  

        private bool ACK_Check()
        {
            bool AckStatus = false;
            // code ...
            return AckStatus;
        }
        private bool REQ_Check()
        {
            bool RqeStatus = false;
            // code ...
            return RqeStatus;
        }

        private string dmcRead()
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
                    throw new Exception("Not connect to PLC !");
                }
                return returnDmc;
            }
            return returnDmc;
        }
        private int modelRead()
            {
                int returnModel = 0;

                var DB_model = mReadXML.GetVarInThreadp("ModelDatablock", "CSC");
                var Byte_model = mReadXML.GetVarInThreadp("ModelByte", "CSC");

                if((DB_model.Count > 0) && (Byte_model.Count > 0))
                {
                    int _modelDatablock = int.Parse(DB_model[0]);
                    int _modelByte = int.Parse(DB_model[0]);

                    if(mS7con.connectPLc())
                    {
                        int NModel = mS7con.ReadByte(_modelDatablock, _modelByte);
                        returnModel = NModel;
                    }
                    else
                    {
                        throw new Exception("Not connect to PLC !");
                    }
                    return returnModel;
                }
                return returnModel;
            }

    }
}