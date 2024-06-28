
//                 default:
//                     throw new ArgumentException($"Unknown signal name: {NameSignal}");

//                  _data = data ?? throw new ArgumentNullException(nameof(data));

//                  if(shape.Name.LocalName == "PlcConsistencyThreadShape")


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;

namespace readxmlFile 
{
    public class ReadXML
    {   
        #region Private Variables

        readonly private string _threadName;        // Name Thread
        readonly private int _aCKDatablock;         // e.g. ACK DB1002.DBX0.0
        readonly private int _aCKByte;
        private int _aCKBit;

        readonly private int _rEQDatablock;           // e.g. REQ DB1002.DBX0.1
        readonly private int _rEQByte;           
        readonly private int _rEQBit;

        readonly private int _dMCDatablock;           // e.g. DMC DB1002.DBX1 
        readonly private int _dMCStartByte;
        readonly private int _dMCLenght;

        readonly private int _modelDatablock;         // e.g. ModelDataBlock DB1003
        readonly private int _modelByte;

        readonly private int _outModelDatablock;      // not use 
        readonly private int _outModelByte;

        readonly private int _wOKDatablock;           // e.g. WOK DB1004.DBX0.0
        readonly private int _wOKByte;
        readonly private int _wOKBit;

        readonly private int _wKODatablock;           // e.g. WKO DB1005.DBX0.1
        readonly private int _wKOByte;
        readonly private int _wKOBit;

        readonly private int _workResultDatablock;    // e.g. WR DB1006.DB0
        readonly private int _workResultByte;

        readonly private int _numberOfSteps;          // Number of step per cycle

        readonly private string _currentDirectory;    // make path for files.xml 
        readonly private string _currentFilePath;
        private XDocument _data;                      // _data stored value from XML file


        private static  Dictionary<string, string> convertString = new Dictionary<string, string>()
        {  
            { "OPE", "OperationShape" },
            { "CSC", "PlcConsistencyThreadShape" },
            { "CRC", "PlcCongruencyThreadShape" },
            { "TRC", "PlcTraceabilityThreadShape" }
        };
        #endregion

        public ReadXML(string filename)
        {
            _currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _currentFilePath = Path.Combine(_currentDirectory, filename);

            if(!File.Exists(_currentFilePath))
            {
              Console.WriteLine($"XML file '{_currentFilePath}' not found.");
              return;   
            }

            _data = XDocument.Load(_currentFilePath);
        }

        #region Methods

        // PRIVATE INTERFACE
        private static List<string> GetVarInThread(XDocument doc, string attributeName, string threadName) // This function looks for all threads and returns values for a e.g. [WKOBit]                  
        {
            List<string> list_data = new List<string>();
            try
            {
                var convString = StrThr(threadName);
                var shapes = doc.Descendants().Where(e => e.Name.LocalName == convString);              // CSC CRC TRC OPE

                foreach (var shape in shapes)
                {
                    var attr = shape.Attribute(attributeName);                                          // This is a realy nessesery point, becouse when you use a Variabel = [AllSteps] this code return [XML structur]
                    
                    if (attr != null)
                    {
                        string attributeValue = attr.Value;
                        list_data.Add(attributeValue);

                        
                        //------------------------------------->>
                        if(attributeName == "AllSteps")
                        {
                            XDocument xdoc = XDocument.Parse(attributeValue);                           // In this plase I start a new serch for variabels e.g. EFASDatablock
                            var stepModel = xdoc.Descendants("StepModel").Where(x => (int)x.Element("StepId") == 1).FirstOrDefault();

                            if (stepModel != null)
                            {
                                var efasDatablock = (int)stepModel.Element("EFASDatablock");
                                
                                Console.WriteLine($"EFASDatablock: {efasDatablock}");
                                Console.WriteLine(stepModel);
                                
                            }
                        }
                        //-------------------------------------<<

                        //Console.WriteLine($"{attributeName} value for shape with Id '{shape.Attribute("Id")?.Value}': {attributeValue}");
                    }
                    else
                    {
                        Console.WriteLine($"Attribute {attributeName} not exist.");
                        return list_data;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return list_data;
        }

        public string GetStepElementValueById(int stepId, string elementName)
        {
            if (_data == null)
            {
                Console.WriteLine("XML data not loaded.");
                return null;
            }

            var step = _data.Descendants("StepModel").FirstOrDefault(s => (int)s.Element("StepId") == stepId);

            if (step != null)
            {
                var element = step.Element(elementName);
                if (element != null)
                {
                    return element.Value;
                }
                else
                {
                    Console.WriteLine($"{elementName} not found for StepId {stepId}.");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"StepModel with StepId {stepId} not found.");
                return null;
            }
        }

        // PUBLIC INTERFACE
        public List<string> GetVarInThreadp(string attributeName, string threadName)
        {
            List<string> list_data = new  List<string>();

            if (_data != null){
                 list_data = GetVarInThread(_data, attributeName, threadName);
            }
            else{
                Console.WriteLine("XML data not loaded.");
            }
            return list_data;
        } 

        private static string StrThr(string input)
        {
            if (convertString.ContainsKey(input)){
                return convertString[input];
            }
            else{
                return "Unknown";
            }
        }
        #endregion
    }
}