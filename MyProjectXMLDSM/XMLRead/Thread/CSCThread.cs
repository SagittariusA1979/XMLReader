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


namespace CSC 
{
    public class CSCThread
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
        
        private ReadXML mReadXML; 

        public CSCThread()
        {
            
        
        }
        
        #region Methods 
        public bool CSC_cycle()
        {
            bool cycleOK = false;

            //  PLC -> | DMC | Model |           set:REQ
            //  DSM <- | WKO or WOK | WR error | set:ACK 
            //

            //if (REQ == 1){} // start
                // check Model and DMC from SQLite [Model][DMC] [2][824501]


            if(cycleOK == true){
                return true;
            }
            else{
                return false;
            }
        }
        #endregion
    }
}