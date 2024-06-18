using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

// Atributes OpName
// List of nodes [shapes]

#region  v01 OK constans
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

#region  v02 OK

// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Please provide a command-line argument (e.g., WKOBit)");
//             return;
//         }

//         string attributeName = args[1]; // Command-line argument, e.g., WKOBit
//         string choseOption = args[0];   // Command-line Argument e.g. [a] or [g]

//         // CSC -> 9b8e4076-1a9d-4d27-83a5-5c4cac85e048
//         // TRC -> 2e455e7c-7dcc-4d02-8294-921226e19e22

//         string shapeId = "9023de9b-953a-4649-95f9-0ae629f6df3b";                // DEBUG, to reject of course

//         Console.WriteLine("OPTIONS: "  + choseOption + " " + attributeName);    // DEBUG, to reject of course
//         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;        // Get the current directory of the executable
//         string xmlFilePath = Path.Combine(currentDirectory, "my.xml");          // Combine the directory with the XML file name      
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
        

//         switch (choseOption.ToString())
//         {
//             case "g":
//                 GetThread();
//                 break;

//             case "a":
//                 GetThreadAll();
//                 break;

//             default:
//                 Console.WriteLine("Not chose ...");
//                 break;

//         }      

// #region  Function

//         void GetThreadAll()                                               
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

//         void GetThread()
//         {
//             try
//             {
//                 XmlNodeList shapes = doc.SelectNodes("//OperationShape | //PlcConsistencyThreadShape | //PlcCongruencyThreadShape | //PlcTraceabilityThreadShape");

//                 foreach (XmlNode shape in shapes)
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
//                         Console.WriteLine($"Type : {SelectedString(attrTYPE_Value)} IndexId: {attrID_Value}");
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

#region v04 Id
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
//         string xmlFilePath = Path.Combine(currentDirectory, "my.xml");

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

#region LinQ
public class StepModel              // OpInfo OPxxx <Struct of XML for StepModel>
{
    public int StepId { get; set; }
    public string Name { get; set; }
    public int EFASDatablock { get; set; }
    public int EFASByte { get; set; }
    public int ESTRCDatablock { get; set; }
    public int ESTRCByte { get; set; }
    public int DateTimeDatablock { get; set; }
    public int DateTimeStartByte { get; set; }
    public string SelectedDateTimeType { get; set; }
    // Add other properties as needed
}

public class VariableModel          // OpInfo OPxxx <Struct of XML for StepModel>
{
    public int VariableId { get; set; }
    public string Name { get; set; }
}
class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a shape Id as a command-line argument.");
            return;
        }

        string shapeId = args[0]; // Command-line argument, e.g., f4b9a125-a10b-4e9b-bc92-7ce5ce29c2ca
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string xmlFilePath = Path.Combine(currentDirectory, "my.xml");       
        if (!File.Exists(xmlFilePath))
        {
            Console.WriteLine($"XML file '{xmlFilePath}' not found.");
            return;
        }

        
        XDocument doc = XDocument.Load(xmlFilePath);                                                            // Load the XML document
        XElement shapeElement = doc.Descendants().FirstOrDefault(e => (string)e.Attribute("Id") == shapeId);    // Select the shape element with the specified Id

        if (shapeElement == null)                                                                                // Check if shape with given Id exists
        {
            Console.WriteLine($"Shape with Id '{shapeId}' not found in the XML file.");
            return;
        }

        // Print all attributes and their values for the shape
        Console.WriteLine($"Data for shape with Id '{shapeId}':");

        foreach (var attr in shapeElement.Attributes())
        {
            // Console.WriteLine($"{attr.Name}: {attr.Value}");
            Console.WriteLine($"{attr.Name}:");


            // if (attr.Name == "Type")
            // {
            //     Console.WriteLine($"TYPE: {attr.Value}");
            //     break;
            // }

            // if(attr.Name == "EFASDatablock")
            // {
            //     Console.WriteLine($"#{attr.Name}: {attr.Value}");
            // }
            
        }
    }
}

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