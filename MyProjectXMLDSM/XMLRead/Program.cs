//#define PLC
//#define DB
//#define SWITCH

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

using CSC;
using readxmlFile;
using Dsmdb;
using s7;
using System.Data.Common;



# region Program

namespace DSMTester
{
    public class DSMTester
    {
        static void Main(string[] args)
        {
            
        #region Introduction


        if (args.Length < 3)
         {
            Console.WriteLine("Please provide command-line arguments Xn Xn Xn");
            Console.WriteLine("[A] -> [A WKOBit CSC 0] and [A 3c90b0d0-09ec-4f9f-a23e-80ab02d8d260 x x]"); // We can check a all information
            Console.WriteLine("[Ad]-> [Ad EFASDatablock TRC 1]");
            Console.WriteLine("[Al]-> [Al TRCDatablock TRC 1]");

            Console.WriteLine("[T] -> [T x CSC x ]");
            Console.WriteLine("[I] -> [I x CSC x ]");

            return;
        }

        string choseOption = args[0];           // Command-line argument, e.g., A - Find Variable for Argument [A WKOBit/up CSC 0]
                                                //                              B - Find Variabel for 1 Level  [Ad EFASDatablock TRC 1] 
        string attributeName = args[1];         // Command-line argument, e.g., WKOBit
        string nameThread = args[2];            // Command-line argument, e.g., CSC CRC TRC OPE
        string stepId = args[3];                // Command-line argument, e.g., 1     // int stepId = int.Parse(args[3]); //
        #endregion
        
        #region INSTANCEs

        #if DB
        // https://learn.microsoft.com/pl-pl/dotnet/csharp/linq/get-started/walkthrough-writing-queries-linq
        using (var context = new DsmDbConntext())
        {
            context.Database.EnsureCreated();

            // var my_m = context.dbModels
            //     .Where(x => x.ModelCode == "2")
            //     .Select(x => x.NumberOfModels)
            //     .ToList();
            
            // var my_c = context.dbComps
            //     .Where(x => x.NumberOfModel == "2")
            //     .Select(x => x.CompCode)
            //     .ToList();

            // string? dmc = "8002";

            // var numberOfmodelFromDB = context.dbModels
            //         .Where(x => x.NumberOfModels == dmc)
            //         .Select(x => x.ModelCode)
            //         .ToList();

        //    foreach (var model in my_m)
        //    {
        //     foreach(var comp in my_c)
        //     {
        //         Console.WriteLine($"COMP: {comp}");
        //     }
        //     Console.WriteLine($"MODEL: {model}");
        //    }

        //    foreach (var numbers in numberOfmodelFromDB){
        //         Console.WriteLine($"#:{numbers}");
        //     }

            //dbContext.SaveChanges();

        }
        #endif
        
        //ReadXML _readXML = new ReadXML("myX.xml");
        //S7con _connect = new S7con("192.168.0.55", 0, 1); // 192.168.1.5   

        XThread myCSC = new XThread("myX.xml", "192.168.1.5", 0, 1);

        while (myCSC.IsAlive())
        {   
           
            var status = myCSC.CSC_thread();
            Console.WriteLine($"General status:{status}");

            Thread.Sleep(2000);
        }

        #endregion

        #if PLC
        var testConnect = _connect.connectPLc();

        if(testConnect == true)
        {
            //float realValue = _connect.ReadRealData(1, 0); // DB1, start at DBD0
            //string text = _connect.ReadString(1001, 6, 50); // DB1, start at DBB0, size 50 bytes

            //Console.WriteLine($"String read from DB1.DBB0: {text}");
            //Console.WriteLine($"Value read from DB1.DBD0: {realValue}");

           
            // int userValue;
            // string input = Console.ReadLine();
            // int.TryParse(input, out userValue);

            // ConsoleKeyInfo keyInfo;
            // do{
            //     keyInfo = Console.ReadKey(intercept: true);
            // }
            // while (keyInfo.Key != ConsoleKey.Enter);
           
            // var key = Console.ReadKey();
            // float keyInt = (float)key.KeyChar;

            bool[] result = new bool[10];
            float test = 241.7f;


            result[0] = _connect.WriteBit(1001, 0, 4, true);
            result[1] = _connect.WriteByte(1001, 1, 255);
            result[2] = _connect.WriteRealData(1001, 2, test);
            result[3] = _connect.WriteString(1001, 6, "#Test03", 50);
            
        }
        _connect.disconnectPLc();
        #endif


        List<string> askThread = new List<string>();                                     // <--- return data from function //DEBUG
        
        #if SWITCH
        #region  SWITCH
        switch (choseOption.ToString())
        {
            case "A":
                // This is public Function from class [ReadXML]
                askThread = _readXML.GetVarInThreadp(attributeName, nameThread);          
                break;

            case "Ad":
                // This is public Function from class [ReadXML]
                askThread = _readXML.GetVar_1LevelInThreadp(stepId, attributeName, nameThread);
                break;

            case "Al":
                // This is public Function from class [ReadXML]
                askThread = _readXML.GetVar_2LevelInThreadp(stepId, attributeName, nameThread);
                break;

            case "T":
                // Test only 
                askThread  = _readXML.StepNUMp(nameThread);
                Console.WriteLine($"Number of all steps: {askThread.Count}");
                break;

            case "I":
                var result = _readXML.AllInfoXMLp(nameThread);
                Console.WriteLine($"INFO:{result.ToString()}");
                break;

            default:
                Console.WriteLine("Not chose ...");
                break;
        }
       
        // I check function [askThread = _readXML.GetVarInThreadp(attributeName, nameThread);]
        if (askThread.Count > 0)                                                    
        {
            //Console.WriteLine($"{askThread[0]}");

            //int numberOfId = askThread.Count;
            //  for(var i = 0; i < numberOfId;)
            //  {
            //     Console.WriteLine($"{i}:{askThread[i]}");
            //     i++;
            //  }

            foreach (string _item in askThread)
            {
                Console.WriteLine(_item);
            }
        }
        #endregion
        #endif

        #region Test Method
        #endregion
        }
    }
}
#endregion




#region v01 OK constans
// class Program
// {
//     static void Main(string[] args)
//     {
//         // Get the current directory of the executable
//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

//         // Combine the directory with the XML file name
//         string xmlFilePath = System.IO.Path.Combine(currentDirectory, "my.xml");

//         // Load the XML document
//         XmlDocument doc = new XmlDocument();
//         doc.Load(xmlFilePath);

//         // Select all PlcConsistencyThreadShape and PlcCongruencyThreadShape elements
//         XmlNodeList shapes = doc.SelectNodes("//PlcConsistencyThreadShape | //PlcCongruencyThreadShape");

//         // Loop through each shape element
//         foreach (XmlNode shape in shapes)
//         {
//             // Get the value of WKOBit attribute
//             XmlAttribute wkoBitAttr = shape.Attributes["WKOBit"];
//             if (wkoBitAttr != null)
//             {
//                 string wkoBitValue = wkoBitAttr.Value;
//                 Console.WriteLine($"WKOBit value for shape with Id '{shape.Attributes["Id"].Value}': {wkoBitValue}");
//             }
//         }
//     }
// }

#endregion

#region v02 OK I use DOM API

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a command-line argument (e.g., WKOBit)");
//             return;
//         }

//         string attributeName = args[1]; // Command-line argument, e.g., WKOBit    -->The secound atribut is important only for function GetThreadAll [ivoke: "a"]
//         string choseOption = args[0];   // Command-line Argument e.g. [a] or [g]  --> In this case We use only a one parameter [ivoke: "g"]

//         string shapeId = "9023de9b-953a-4649-95f9-0ae629f6df3b";                // DEBUG, to reject of course --> never use !!!
//         Console.WriteLine("OPTIONS: "  + choseOption + " " + attributeName);    // DEBUG, to reject of course

//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;        // Get the current directory of the executable
//         string xmlFilePath = Path.Combine(currentDirectory, "myX.xml");         // Combine the directory with the XML file name      
//         if (!File.Exists(xmlFilePath))                                          // Check if the XML file exists
//         {
//             Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//             return;
//         }

        
//         XmlDocument doc = new XmlDocument();                                    // Load the XML document - DOM API
//         doc.Load(xmlFilePath);
  
//         //XDocument doc = XDocument.Load(xmlFilePath);                          // Load the XML document - LinQ

//         // Select all nodes [XmlNodeList] --> I moving it to function !
//         // XmlNodeList shapes = doc.SelectNodes("//OperationShape | //PlcConsistencyThreadShape | //PlcCongruencyThreadShape | //PlcTraceabilityThreadShape");  // DOM API
//         // XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == shapeId);                                                 // LinQ
        
//         List<string> askThread = new List<string>();

//         switch (choseOption.ToString())
//         {
//             case "g":
//                 askThread = GetThread("OPE");
//                 break;

//             case "a":
//                 GetThreadAll();
//                 break;

//             default:
//                 Console.WriteLine("Not chose ...");
//                 break;

//         }     

//         Console.WriteLine($"{askThread[0]}"); 

// #region  Function

//         void GetThreadAll()                 // This function looking for in all threads and return value for a e.g. [WKOBit]                                              
//         {          
//             try
//             {
//                 XmlNodeList shapes = doc.SelectNodes("//OperationShape | //PlcConsistencyThreadShape | //PlcCongruencyThreadShape | //PlcTraceabilityThreadShape");

//                 foreach (XmlNode shape in shapes)
//                 {

//                     // Get the value of the specified attribute [XmlAttribute]
//                     XmlAttribute attr = shape.Attributes[attributeName];

//                     if (attr != null)
//                     {
//                         string attributeValue = attr.Value;
//                         Console.WriteLine($"{attributeName} value for shape with Id '{shape.Attributes["Id"].Value}': {attributeValue}");
                        
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Attribute '{attributeName}' not found for shape with Id '{shape.Attributes["Id"].Value}'.");
//                     }
//                 } 
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error: {ex}");
//             }          
//         }

//         List<string> GetThread(string nameOfShate)                    // This function return [Type] and [Id} for fit threade CSC CRC TRC
//         {
//             string choseThread = "OPE";
            
//             try
//             {
//                 XmlNodeList shapes_All = doc.SelectNodes("//OperationShape | //PlcConsistencyThreadShape | //PlcCongruencyThreadShape | //PlcTraceabilityThreadShape");
//                 XmlNodeList shape_OPE = doc.SelectNodes("//OperationShape");
//                 XmlNodeList shape_CSC = doc.SelectNodes("//PlcConsistencyThreadShape");
//                 XmlNodeList shape_CRCn = doc.SelectNodes("//PlcCongruencyThreadShape");
//                 XmlNodeList shape_TRC = doc.SelectNodes("//PlcTraceabilityThreadShape");

//                 List<string> returnThread = new List<string>();
//                 XmlNodeList shapeCurrent = null;


//                 switch (choseThread)                            // This please we chose threads e.g. CSC CRC TRC or OPE
//                 {
//                     case "OPE":
//                         Console.WriteLine("OPE");               // DEBUG (!)
//                         shapeCurrent = shape_OPE;
//                         break;

//                     case "CSC":
//                         Console.WriteLine("CSC");               // DEBUG (!)
//                         shapeCurrent = shape_CSC;
//                         break;
                    
//                     case "CRC":
//                         Console.WriteLine("CRC");               // DEBUG (!)
//                         shapeCurrent = shape_CRCn;
//                         break;

//                     case "TRC":
//                         Console.WriteLine("TRC");               // DEBUG (!)
//                         shapeCurrent = shape_TRC;
//                         break;
//                 }
                

//                 foreach (XmlNode shape in shapeCurrent)
//                 {
//                     #region Descryption
//                     // Get the value of the specified attribute [XmlAttribute]
//                     //XmlAttribute attr = shape.Attributes[attributeName];

//                     //XmlAttribute attrId = shape.Attributes["Id"];
//                     //XmlAttribute attrType = shape.Attributes["Type"];
//                     #endregion

//                     List<XmlAttribute> attrList = new List<XmlAttribute>();
//                     attrList.Add(shape.Attributes["Id"]);
//                     attrList.Add(shape.Attributes["Type"]);

//                     if (attrList != null)
//                     {
//                         string attrID_Value = attrList[0].Value;
//                         string attrTYPE_Value = attrList[1].Value;
                        
//                         //Console.WriteLine($"{attributeName} value for shape with Id '{shape.Attributes["Id"].Value}': {attributeValue}");
//                         //Console.WriteLine($"Type : {SelectedString(attrTYPE_Value)} IndexId: {attrID_Value}");
//                         //Console.WriteLine($"OPE # {attrID_Value}");
//                         returnThread.Add(attrID_Value);
                        
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Attribute '{attributeName}' not found for shape with Id '{shape.Attributes["Id"].Value}'.");
//                     }
//                 }
//                 return returnThread;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error: {ex}");
//                 return new List<string>();
//             }
//         }

//         string SelectedString(string strN)
//         {
//             string fullString = strN;
        
//             string[] parts = fullString.Split('.');

//             if (fullString.Length < 70)
//             {
//                 if (parts.Length > 6)
//                 {
//                     string result = parts[6];
//                     return result;
//                 }
//             }

//             if (fullString.Length > 70)
//             {
//                 if(parts.Length > 7)
//                 {
//                     string result = parts[7];
//                     return result;
//                 }    
//             }

//             return $"Invalid string format. {fullString.Length}";
            
//         }

// #endregion

//     }
// }


#endregion

#region V03 OK I use LinQ [to use...]

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length < 2)
//         {
//             Console.WriteLine("Please provide command-line arguments (e.g., [first] [secound])");
//             Console.WriteLine("[a] | [Id Content ThreadName ACKByte ect.]");
//             Console.WriteLine("[g] | [OPE CSC CRC TRC]");
//             return;
//         }

//         string choseOption = args[0]; // Command-line argument, e.g., g or a
//         string attributeName = args[1]; // Command-line argument, e.g., WKOBit

//         Console.WriteLine("OPTIONS: " + choseOption + " " + attributeName);         // DEBUG, to reject of course

//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;            // Get the current directory of the executable
//         string xmlFilePath = Path.Combine(currentDirectory, "myX.xml");             // Combine the directory with the XML file name

//         if (!File.Exists(xmlFilePath))                                              // Check if the XML file exists
//         {
//             Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//             return;
//         }

//         XDocument doc = XDocument.Load(xmlFilePath);                                // Load the XML document using LINQ

        

//         List<string> askThread = new List<string>();

//         switch (choseOption.ToString())                                             // I use a first argument from command line 
//         {
//             case "g":
//                 askThread = GetThread(doc, attributeName);
//                 break;

//             case "a":
//                 GetThreadAll(doc, attributeName);
//                 break;

//             default:
//                 Console.WriteLine("Not chose ...");
//                 break;
//         }

//         if (askThread.Count > 0)                                                    // I check the value of askThread [when is not  0 I show it ]
//         {
//             //Console.WriteLine($"{askThread[0]}");
//             int numberOfId = askThread.Count;
             
//              for(var i = 0; i < numberOfId;)
//              {
//                 Console.WriteLine($"{i}:{askThread[i]}");
//                 i++;
//              }
//         }
//     }

//     #region  Functions
//     static void GetThreadAll(XDocument doc, string attributeName)                   // This function looks for all threads and returns values for a e.g. [WKOBit]
//     {
//         try
//         {
//             var shapes = doc.Descendants().Where(e => e.Name.LocalName == "OperationShape" ||
//                                                       e.Name.LocalName == "PlcConsistencyThreadShape" ||
//                                                       e.Name.LocalName == "PlcCongruencyThreadShape" ||
//                                                       e.Name.LocalName == "PlcTraceabilityThreadShape");

//             foreach (var shape in shapes)
//             {
//                 var attr = shape.Attribute(attributeName);

//                 if (attr != null)
//                 {
//                     string attributeValue = attr.Value;
//                     Console.WriteLine($"{attributeName} value for shape with Id '{shape.Attribute("Id")?.Value}': {attributeValue}");
//                 }
//                 else
//                 {
//                     Console.WriteLine($"Attribute '{attributeName}' not found for shape with Id '{shape.Attribute("Id")?.Value}'.");
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error: {ex}");
//         }
//     }

//     static List<string> GetThread(XDocument doc, string nameOfShape)                // This function returns [Type] and [Id] for the selected thread
//     {
//         try
//         {
//             var shapes_All = doc.Descendants().Where(e => e.Name.LocalName == "OperationShape" ||
//                                                           e.Name.LocalName == "PlcConsistencyThreadShape" ||
//                                                           e.Name.LocalName == "PlcCongruencyThreadShape" ||
//                                                           e.Name.LocalName == "PlcTraceabilityThreadShape");

//             IEnumerable<XElement> shapeCurrent = null;

//             switch (nameOfShape) // Choose threads based on parameter
//             {
//                 case "OPE":
//                     shapeCurrent = shapes_All.Where(e => e.Name.LocalName == "OperationShape");
//                     break;

//                 case "CSC":
//                     shapeCurrent = shapes_All.Where(e => e.Name.LocalName == "PlcConsistencyThreadShape");
//                     break;

//                 case "CRC":
//                     shapeCurrent = shapes_All.Where(e => e.Name.LocalName == "PlcCongruencyThreadShape");
//                     break;

//                 case "TRC":
//                     shapeCurrent = shapes_All.Where(e => e.Name.LocalName == "PlcTraceabilityThreadShape");
//                     break;

//                 default:
//                     throw new ArgumentException("Invalid thread name");
//             }

//             List<string> returnThread = new List<string>();

//             if (shapeCurrent != null)
//             {
//                 foreach (var shape in shapeCurrent)
//                 {
//                     var attrId = shape.Attribute("Id");
//                     var attrType = shape.Attribute("Type");
//                     var attrContent = shape.Attribute("Content");

//                     if (attrId != null && attrType != null && attrContent != null)
//                     {
//                         string attrID_Value = attrId.Value;
//                         string attrTYPE_Value = attrType.Value;
//                         string attrCONTENT_Value = attrContent.Value;
    
//                         //returnThread.Add($"Type: {attrTYPE_Value}, Id: {attrID_Value}");
                        
//                         if(nameOfShape == "CRC")
//                         {
//                             returnThread.Add($"{attrCONTENT_Value} . {attrID_Value}");

//                         }
//                         else
//                         {
//                             returnThread.Add($"{attrID_Value}");
//                         }
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Shape with missing attributes. Id: {shape.Attribute("Id")?.Value}");
//                     }
//                 }
//             }
//             else
//             {
//                 Console.WriteLine($"No shapes found for thread: {nameOfShape}");
//             }

//             return returnThread;
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error: {ex}");
//             return new List<string>(); // Return an empty list in case of an error
//         }
//     }

//     static string SelectedString(string strN)
//     {
//         string fullString = strN;

//         string[] parts = fullString.Split('.');

//         if (fullString.Length < 70)
//         {
//             if (parts.Length > 6)
//             {
//                 string result = parts[6];
//                 return result;
//             }
//         }

//         if (fullString.Length > 70)
//         {
//             if (parts.Length > 7)
//             {
//                 string result = parts[7];
//                 return result;
//             }
//         }

//         return $"Invalid string format. {fullString.Length}";
//     }
//     #endregion
// }

#endregion

#region Id version for DOM 
// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a shape Id as a command-line argument.");
//             return;
//         }

//         string shapeId = args[0]; // Command-line argument, e.g., f4b9a125-a10b-4e9b-bc92-7ce5ce29c2ca

//         // Get the current directory of the executable
//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

//         // Combine the directory with the XML file name
//         string xmlFilePath = Path.Combine(currentDirectory, "myX.xml");

//         // Check if the XML file exists
//         if (!File.Exists(xmlFilePath))
//         {
//             Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//             return;
//         }

//         // Load the XML document
//         XmlDocument doc = new XmlDocument();
//         doc.Load(xmlFilePath);

//         // Select the shape element with the specified Id
//         // XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == shapeId);
//          XmlNode shapeNode = doc.SelectSingleNode($"//*[(@Id = '{shapeId}')]");

//         // Check if shape with given Id exists
//         if (shapeNode == null)
//         {
//             Console.WriteLine($"Shape with Id '{shapeId}' not found in the XML file.");
//             return;
//         }

//         // Print all attributes and their values for the shape
//         Console.WriteLine($"Data for shape with Id '{shapeId}':");

//         foreach (XmlAttribute attr in shapeNode.Attributes)
//         {
//             Console.WriteLine($"{attr.Name}: {attr.Value}");
//         }
//     }
// }
#endregion

#region Id version for LinQ whit make a files for Thread SCS CRC TRC OPE [to use...]

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a shape Id as a command-line argument e.g:[3c90b0d0-09ec-4f9f-a23e-80ab02d8d260] [name for files *.xml].");
//             return;
//         }

//         string shapeId = args[0];                                           // Command-line argument, e.g., f4b9a125-a10b-4e9b-bc92-7ce5ce29c2ca
//         string nameFileThread = args[1];                                    // Name of files for Thread e.g. CSC

//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
//         string xmlFilePath = Path.Combine(currentDirectory, "myX.xml");

//         if (!File.Exists(xmlFilePath))                                      // Check if the XML file exists
//         {
//             Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//             return;
//         }

//         XDocument doc = XDocument.Load(xmlFilePath);

//         // Select the shape element with the specified Id
//         XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == shapeId);

//         if (shapeElement == null)                                           // Check if shape with given Id exists
//         {
//             Console.WriteLine($"Shape with Id '{shapeId}' not found in the XML file.");
//             return;
//         }

//         // Print all attributes and their values for the shape
//         Console.WriteLine($"Data for shape with Id '{shapeId}':");

//         WriteDataToFile(shapeElement, nameFileThread);

//         foreach (XAttribute attr in shapeElement.Attributes())
//         {
//             Console.WriteLine($"{attr.Name}: {attr.Value}");
            
//         }
//     }

//     #region  Function
//     static void WriteDataToFile(XElement shapeElement, string NameOfFiles) // This function saves data into separate files 
//     {
//             // Get the directory where the executable is located
//             string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

//             // Combine the directory with the file name
//             string filePath = Path.Combine(exeDirectory, NameOfFiles);
            
//             // Open a StreamWriter to append or create the file
//             using (StreamWriter writer = new StreamWriter(filePath, true))
//             {
//                 foreach (var attr in shapeElement.Attributes())
//                 {
//                     writer.WriteLine($"{attr.Name}:{attr.Value}");
//                 }
//             }
//     }
    
//     #endregion
// }

#endregion

#region LinQ --> 
// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a shape Id as a command-line argument.");
//             return;
//         }

//         string shapeId = args[0];       // Command-line argument, e.g., f4b9a125-a10b-4e9b-bc92-7ce5ce29c2ca
//         string childTree = args[1];     // Secound argument e.g. OpName but when you use a AllSteps you get a all childs


//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
//         string xmlFilePath = Path.Combine(currentDirectory, "myX.xml");       
//         if (!File.Exists(xmlFilePath))
//         {
//             Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//             return;
//         }

//         XDocument doc = XDocument.Load(xmlFilePath);                                                                // Load the XML document


//         #region  First Request ---[ I get you down specific data  ]-------->
                                                                      
//         XElement? shapeElement = doc.Descendants().FirstOrDefault(e => (string?)e.Attribute("Id") == shapeId);    // Select the shape element with the specified Id
        
//         string? result = doc.Descendants()
//                         .Where(e => (string?)e.Attribute("Id") == shapeId)
//                         .Select(e => (string?)e.Attribute(childTree))
//                         .FirstOrDefault();

//         Console.WriteLine($"{result}");
//         #endregion

//         #region Secound Request ----------->
//         XDocument currentResult = XDocument.Parse(result);
        
//         var stepModel = currentResult.Descendants("StepModel") // OPE
//                            .Where(e => (int?)e.Element("StepId") == 1)
//                            .Select(e => new
//                            {
//                                StepId = (int)e.Element("StepId"),
//                                Name = (string)e.Element("Name"),
//                                EFASDatablock = (int)e.Element("EFASDatablock"),
//                                EFASByte = (int)e.Element("EFASByte"),
//                                ESTRCDatablock = (int?)e.Element("ESTRCDatablock"),
//                                ESTRCByte = (int?)e.Element("ESTRCByte"),
//                                DateTimeDatablock = (int?)e.Element("DateTimeDatablock"),
//                                DateTimeStartByte = (int?)e.Element("DateTimeStartByte"),
//                                SelectedDateTimeType = (string?)e.Element("SelectedDateTimeType")
//                                // StepRefs
//                                // CompRefs
//                            })
//                            .FirstOrDefault();


//             if (stepModel != null)
//             {
//                 Console.WriteLine($"StepId: {stepModel.StepId}");
//                 Console.WriteLine($"Name: {stepModel.Name}");
//                 Console.WriteLine($"EFASDatablock: {stepModel.EFASDatablock}");
//                 Console.WriteLine($"EFASByte: {stepModel.EFASByte}");
//                 Console.WriteLine($"ESTRCDatablock: {stepModel.ESTRCDatablock}");
//                 Console.WriteLine($"ESTRCByte: {stepModel.ESTRCByte}");
//                 Console.WriteLine($"DateTimeDatablock: {stepModel.DateTimeDatablock}");
//                 Console.WriteLine($"DateTimeStartByte: {stepModel.DateTimeStartByte}");
//                 Console.WriteLine($"SelectedDateTimeType: {stepModel.SelectedDateTimeType}");
//                 Console.WriteLine($"ESTRCByte: {stepModel.ESTRCByte}");
//             }
//             else
//             {
//                 Console.WriteLine("Step with StepId = 12 not found.");
//             }
//             #endregion
    
//         #region  Convert [I try to show other data fro thread]
//         // request for converter files
//         //XElement shapeElement = doc.Descendants("StepModel").FirstOrDefault(e => (string)e.Element("StepId") == shapeId);

//         // if (shapeElement != null)
//         // {
//         //     // Access EFASDatablock value
//         //     string efasDatablockValue = (string)shapeElement.Element("EFASDatablock");

//         //     // Print the value
//         //     Console.WriteLine($"EFASDatablock value: {efasDatablockValue}");
//         // }
//         // else
//         // {
//         //     Console.WriteLine("Shape element not found.");
//         // }
//         #endregion

//         if (shapeElement == null)                                                                                  // Check if shape with given Id exists
//         {
//             Console.WriteLine($"Shape with Id '{shapeId}' not found in the XML file.");
//             return;
//         }

//         //Console.WriteLine($"Data for shape with Id '{shapeId}':");                                               // Print all attributes and their values for the shape
//         //WriteDataToFile(shapeElement, "0ae629f6df3b.xml");
//         //Console.WriteLine("Data has been written to the file.");

//         foreach (var attr in shapeElement.Attributes())
//         {
//             // if (attr.Name == "OpName") // I can show all data or specific data. How this case
//             // { 
//             //      Console.WriteLine($"{attr.Name}:{attr.Value}");
//             // }
//             Console.WriteLine($"{attr.Name}:{attr.Value}");
//         }

//         #region Function

//         static void WriteDataToFile(XElement shapeElement, string NameOfFiles) // This function saves data into separate files 
//         {
//             // Get the directory where the executable is located
//             string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

//             // Combine the directory with the file name
//             string filePath = Path.Combine(exeDirectory, NameOfFiles);
            
//             // Open a StreamWriter to append or create the file
//             using (StreamWriter writer = new StreamWriter(filePath, true))
//             {
//                 foreach (var attr in shapeElement.Attributes())
//                 {
//                     writer.WriteLine($"{attr.Name}:{attr.Value}");
//                 }
//             }
//         }

//         static void DivideFiles(string xmlFile) // This function divide files *.xml to separately threads SCS CRC TRC
//         {
//             XDocument FilesToDivide = XDocument.Load(xmlFile);

//             //XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == shapeId);


//         }
        
//         #endregion
        
//     }
// }

#endregion

#region gread Linq --> to delete, the same sytuation as below

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a StepId as a command-line argument.");
//             return;
//         }

//         string stepId = args[0];

//         try
//         {
//             string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
//             string xmlFilePath = Path.Combine(currentDirectory, "my.xml");

//             if (!File.Exists(xmlFilePath))
//             {
//                 Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//                 return;
//             }

//             XDocument doc = XDocument.Load(xmlFilePath);

//             XElement stepElement = doc.Descendants("StepModel").FirstOrDefault(e => (string)e.Element("StepId") == stepId);

//             if (stepElement == null)
//             {
//                 Console.WriteLine($"Step with StepId '{stepId}' not found in the XML file.");
//                 return;
//             }

//             XElement efasDatablockElement = stepElement.Element("EFASDatablock");

//             if (efasDatablockElement != null)
//             {
//                 Console.WriteLine($"EFASDatablock for StepId '{stepId}': {efasDatablockElement.Value}");
//             }
//             else
//             {
//                 Console.WriteLine($"EFASDatablock element not found for StepId '{stepId}'.");
//             }
//         }
//         catch (FileNotFoundException ex)
//         {
//             Console.WriteLine($"File not found: {ex.Message}");
//         }
//         catch (XmlException ex)
//         {
//             Console.WriteLine($"XML error: {ex.Message}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"An error occurred: {ex.Message}");
//         }
//     }
// }

#endregion

#region gread Linq --> to delete becouse this approach is wrong

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a StepId as a command-line argument.");
//             return;
//         }

//         string stepId = args[0];

//         try
//         {
//             string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
//             string xmlFilePath = Path.Combine(currentDirectory, "my.xml");

//             if (!File.Exists(xmlFilePath))
//             {
//                 Console.WriteLine($"XML file '{xmlFilePath}' not found.");
//                 return;
//             }

//             XDocument doc = XDocument.Load(xmlFilePath);

//             // Ensure StepId is checked as a string, as XML values are often strings
//             XElement stepElement = doc.Descendants("StepModel").FirstOrDefault(e => (string)e.Element("StepId") == stepId);

//             if (stepElement == null)
//             {
//                 Console.WriteLine($"Step with StepId '{stepId}' not found in the XML file.");
//                 return;
//             }

//             XElement efasDatablockElement = stepElement.Element("EFASDatablock");

//             if (efasDatablockElement != null)
//             {
//                 Console.WriteLine($"EFASDatablock for StepId '{stepId}': {efasDatablockElement.Value}");
//             }
//             else
//             {
//                 Console.WriteLine($"EFASDatablock element not found for StepId '{stepId}'.");
//             }
//         }
//         catch (FileNotFoundException ex)
//         {
//             Console.WriteLine($"File not found: {ex.Message}");
//         }
//         catch (XmlException ex)
//         {
//             Console.WriteLine($"XML error: {ex.Message}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"An error occurred: {ex.Message}");
//         }
//     }
// }

#endregion