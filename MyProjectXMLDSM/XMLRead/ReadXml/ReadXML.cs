// --------------------------------------------------------------------------
            // List<string> list_data = new List<string>();
            // try
            // {
            //     // [*]
            //     XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == "3c90b0d0-09ec-4f9f-a23e-80ab02d8d260");

            //     foreach (XAttribute attr in shapeElement.Attributes())
            //     {    
            //         if(attr.Name == "AllSteps")
            //         {
            //             Console.WriteLine($"{attr.Name}: {attr.Value}"); // All <> nested
            //         }         
            //     }  
            // } 
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"Error: {ex.Message}");
            // }
            // return list_data;
// -----------------------------------------------------------------------------
//                   throw new ArgumentException($"Unknown signal name: {NameSignal}");
// -----------------------------------------------------------------------------
//                  _data = data ?? throw new ArgumentNullException(nameof(data));
// -----------------------------------------------------------------------------
//                  if(shape.Name.LocalName == "PlcConsistencyThreadShape")
// -----------------------------------------------------------------------------
// var thread = StrThr(_thread);
//             var result = _doc.Descendants(_thread)
//             .Where(x => x.Attribute("ACKDatablock") != null)
//                          .Select(x => new
//                          {
//                              Id = x.Attribute("Id")?.Value,
//                              ThreadName = x.Attribute("ThreadName")?.Value
//                          });
//-------------------------------------------------------------------------------

#define DEBUG                   // function -> private static List<string> GetVarInThread(XDocument doc, string attributeName, string threadName) 
//#define STEPS_WHIT_NAME         // function -> private static List<string> StepQuestion(XDocument doc, string threadName). This is switch [on/off]


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

        // readonly private string _threadName;        // Name Thread
        // readonly private int _aCKDatablock;         // e.g. ACK DB1002.DBX0.0
        // readonly private int _aCKByte;
        // private int _aCKBit;

        // readonly private int _rEQDatablock;           // e.g. REQ DB1002.DBX0.1
        // readonly private int _rEQByte;           
        // readonly private int _rEQBit;

        // readonly private int _dMCDatablock;           // e.g. DMC DB1002.DBX1 
        // readonly private int _dMCStartByte;
        // readonly private int _dMCLenght;

        // readonly private int _modelDatablock;         // e.g. ModelDataBlock DB1003
        // readonly private int _modelByte;

        // readonly private int _outModelDatablock;      // not use 
        // readonly private int _outModelByte;

        // readonly private int _wOKDatablock;           // e.g. WOK DB1004.DBX0.0
        // readonly private int _wOKByte;
        // readonly private int _wOKBit;

        // readonly private int _wKODatablock;           // e.g. WKO DB1005.DBX0.1
        // readonly private int _wKOByte;
        // readonly private int _wKOBit;

        // readonly private int _workResultDatablock;    // e.g. WR DB1006.DB0
        // readonly private int _workResultByte;

        // readonly private int _numberOfSteps;          // Number of step per cycle

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

        private class AllStepsData // Data for function -> private static List<string> StepQuestion(XDocument doc, string threadName)
            {
                public string Name { get; set; }
                public string StepId { get; set; }
            }
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
        private static List<string> GetVarInThread(XDocument doc, string attributeName, string threadName)                              // This function looks for specific thread and returns values for a e.g. [WKOBit]                  
        {
            List<string> list_data = new List<string>();
            try
            {
                var convString = StrThr(threadName);
                var shapes = doc.Descendants().Where(e => e.Name.LocalName == convString);              // CSC CRC TRC OPE

                #if DEBUG // DEBUG to manage this solution -> [A 3c90b0d0-09ec-4f9f-a23e-80ab02d8d260 x x]
                if(attributeName.Count() > 20)
                {
                    XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == attributeName);

                    foreach (XAttribute attr in shapeElement.Attributes())
                    {                      
                        Console.WriteLine($"{attr.Name}: {attr.Value}");
                    }   
                }
                #endif // <--(end)
                
                foreach (var shape in shapes)
                {
                    // This is a realy nessesery point, becouse when you use a Variabel = [AllSteps] this code return [XML structur]
                    var attr = shape.Attribute(attributeName);                                          
                    
                    if (attr != null)
                    {
                        string attributeValue = attr.Value;
                        list_data.Add(attributeValue);

                        #region  This is an example of deep search (Those functions are realized in the next two functions)
                        //------------------------------------->>
                        // if(attributeName == "AllSteps")
                        // {
                        //     XDocument xdoc = XDocument.Parse(attributeValue);                           // In this plase I start a new serch for variabels e.g. EFASDatablock
                        //     var stepModel = xdoc.Descendants("StepModel").Where(x => (int)x.Element("StepId") == 1).FirstOrDefault();

                        //     if (stepModel != null)
                        //     {
                        //         var efasDatablock = (int)stepModel.Element("EFASDatablock");

                        //         // var variableId = stepModel.Descendants("StepReferenceModel")        // for a TRC
                        //         //       .Select(x => (int)x.Element("VariableId"))
                        //         //       .FirstOrDefault();
                                
                        //         Console.WriteLine($"EFASDatablock: {efasDatablock}");
                        //         Console.WriteLine(stepModel);
                                
                        //     }
                        // }
                        //-------------------------------------<<
                        #endregion
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
                Console.WriteLine($"[GetVarInThread] Error: {ex.Message}");
            }
            return list_data;
        }

        private static List<string> GetVar_1LevelInThread(XDocument doc, string stepId, string attributesName, string  threadName)      // This finction looks for specific thread and StepsId after return values e.g. [EFASDatablock]
        {
            List<string> list_data = new List<string>();
            try
            {
                var convString = StrThr(threadName);
                var shapes = doc.Descendants().Where(e => e.Name.LocalName == convString);  // first search for a Thraed

                foreach (var shape in shapes)
                {
                    var attr = shape.Attribute("AllSteps");                                 // asigne to [attr] variabels for AllSteps 

                    if (attr != null)
                    {
                        string attributeValue = attr.Value;

                        XDocument dipdata = XDocument.Parse(attributeValue);                // second dip search in  variables of AllSteps
                        var stepModel = dipdata.Descendants("StepModel").Where(e => (string)e.Element("StepId") == stepId).FirstOrDefault();

                        if(stepModel != null)
                            {
                                string dipElement = (string)stepModel.Element(attributesName);
                                list_data.Add(dipElement);
                                //Console.WriteLine(dipElement); 
                            }
                        else
                            {
                                Console.WriteLine($"Atributes: {stepModel} not exist... !");
                                return list_data;
                            }
                    }
                    else
                    {
                        Console.WriteLine($"Atributes: {attr} not exist... !");
                        return list_data;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return list_data;
        }

        private static List<string> GetVar_2LevelInThread(XDocument _doc, string stepId, string attributesName, string  threadName)
        {
            List<string> list_data = new List<string>();
            
            try
            {
                var convString = StrThr(threadName);
                var shape = _doc.Descendants().Where(e => e.Name.LocalName == convString);

                foreach(var shapes in shape)
                {
                    var attr = shapes.Attribute("AllSteps");
                    if(attr != null)
                    {
                        string attributeValue = attr.Value;
                        XDocument dipdata = XDocument.Parse(attributeValue);
                        var stepModel = dipdata.Descendants("StepModel").Where(e => (string)e.Element("StepId") == stepId).FirstOrDefault();

                         if(stepModel != null)
                         {
                            var variableId = stepModel.Descendants("StepReferenceModel")
                            .Select(x => (string)x.Element(attributesName))
                            .FirstOrDefault();

                            list_data.Add(variableId);
                        }
                        else
                        {
                            Console.WriteLine($"Atribut : {stepModel} not exist");
                            return list_data;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Atribut: {attr} not exist");
                        return list_data;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            return list_data;
        }

        private static List<string> StepNUM(XDocument doc, string threadName)
        {
            List<string> list_data = new List<string>();
            #if STEPS_WHIT_NAME 
            List<AllStepsData> tuple_data = new List<AllStepsData>();
            #endif
            string attributeValue = string.Empty;
        
            try
            { // --> (start)
                var convString = StrThr(threadName);
                var shapes = doc.Descendants().Where(e => e.Name.LocalName == convString);

                foreach (var shape in shapes)
                {
                    var attr = shape.Attribute("Id");
                        
                    if (attr != null)
                    {
                        attributeValue = attr.Value;
                        //list_data.Add(attributeValue);
                    }
                    else
                    {
                        Console.WriteLine("not exist.");
                        return list_data;
                    }
                } // This plase I finsih a first search (end!) <--

                XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == attributeValue);

                if (shapeElement != null)
                {
                    foreach (XAttribute attr in shapeElement.Attributes())
                    {    
                        if (attr.Name == "AllSteps")
                        {
                            // Load the nested XML from the AllSteps attribute
                            XDocument allStepsDoc = XDocument.Parse(attr.Value);
                            var stepIds = allStepsDoc.Descendants("StepId")
                                                    .Select(step => step.Value)
                                                    .ToList();
                            list_data.AddRange(stepIds);

                            #if STEPS_WHIT_NAME // Only debug (when I commpare StepId whit Step's Name)
                            var steps = allStepsDoc.Descendants()   
                               .Where(step => step.Name == "Name" || step.Name == "StepId")
                               .GroupBy(step => step.Parent)
                               .Select(group => new AllStepsData
                               {
                                   Name = group.FirstOrDefault(x => x.Name == "Name")?.Value,
                                   StepId = group.FirstOrDefault(x => x.Name == "StepId")?.Value
                               })
                               .ToList();

                            tuple_data.AddRange(steps);

                            foreach (var step in tuple_data)
                            {
                                // The result of search
                                Console.WriteLine($"Name: {step.Name}, StepId: {step.StepId}");
                            }
                            #endif
                        }         
                    }
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return list_data;         
        }

        private static bool AllInfoXML(XDocument doc, string threadName)
        {
            List<AllStepsData> allSteps_data = new List<AllStepsData>();
            string attributeValue = string.Empty;
        
            try
            {   
                // Description, how it to work: Conversion e.g. CSC to description string
                // OPE        --> OperationShape    --> [Id: 98dc2b41-4f29-4bb6-a065-7dae31a1f7f8]
                // threadName --> convString        --> attributeValue = attr.Value; 

                var convString = StrThr(threadName);
                var shapes = doc.Descendants().Where(e => e.Name.LocalName == convString);

                // This is first search
                foreach (var shape in shapes){
                    var attr = shape.Attribute("Id");
                        
                    if (attr != null){
                        attributeValue = attr.Value;
                    }
                    else{
                        Console.WriteLine($"Issue into: Convert Thread to code e.g [Id: 98dc2b41-4f29-4bb6-a065-7dae31a1f7f8]:{attr}");
                        return false;
                    }
                } 
                
                // Start to looking for into Attribute
                XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == attributeValue);
                if (shapeElement != null)
                {
                    foreach (XAttribute attr in shapeElement.Attributes())
                    {   
                        // // Looking for variables for a AllSteps
                        if (attr.Name == "AllSteps"){

                            // Load the nested XML from the AllSteps attribute
                            XDocument allStepsDoc = XDocument.Parse(attr.Value);
                            
                            var steps = allStepsDoc.Descendants()
                               .Where(step => step.Name == "Name" || step.Name == "StepId")
                               .GroupBy(step => step.Parent)
                               .Select(group => new AllStepsData
                               {
                                   Name = group.FirstOrDefault(x => x.Name == "Name")?.Value,
                                   StepId = group.FirstOrDefault(x => x.Name == "StepId")?.Value,
                               })
                               .ToList();

                            allSteps_data.AddRange(steps);

                            foreach (var step in allSteps_data){
                                Console.WriteLine($"Name:{step.Name}, StepId:{step.StepId}");
                            }

                        }else{
                            Console.WriteLine($"Other attributes of AllSteps: :{attr.Name}");
                        }   
                    }
                }else{
                    throw new Exception($":{shapeElement}:is null !");
                }
            } 
            catch (Exception ex){
                Console.WriteLine($"Error: {ex.Message}");
            }
            return true;       
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

        public List<string> GetVar_1LevelInThreadp(string stepId, string attributesName, string  threadName)
        {
            List<string> list_data = new List<string>();

            if(_data != null){
                list_data = GetVar_1LevelInThread(_data, stepId, attributesName, threadName);
            }
            else{
                Console.WriteLine("XML data not load.");
            }
            return list_data;
        }

        public List<string>GetVar_2LevelInThreadp(string stepId, string attributesName, string  threadName)
        {
            List<string> list_data = new List<string>();

            if(_data != null){
                list_data = GetVar_2LevelInThread(_data, stepId, attributesName, threadName);
            }else{
                Console.WriteLine("Error");
                return list_data;
            }
            return list_data;

        }

        public List<string> StepNUMp(string threadName)
        {
            var list_data = new List<string>();

            if(_data != null)
            {
                list_data = StepNUM(_data, threadName);
            }else{
                Console.WriteLine("Error");
                return list_data;
            }
            return list_data;
        }

        public bool AllInfoXMLp(string threadName)
        {
            if(_data != null){
                return AllInfoXML(_data, threadName);
            }else{
                Console.WriteLine("Error in function AllInformatinTRCp()");
                return false;
            }
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