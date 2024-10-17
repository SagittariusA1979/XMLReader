
using System;
using System.Text;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;


#region EXAMPLES_ZPL
// CT~~CD,~CC^~CT~
// ^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,45^JMA^PR2,2~SD24^JUS^LRN^CI0^XZ
// ~DG000.GRF,00512,016,
// ,::::J070761C19F0707C03FC01FF04,J030661E3BF0F87E03FE00FF8E,J03067143311DC46010700181C,J038E23230380880030200183C,J0184776707078DC0307001844,K08C2I207038FE03FE00180E,J01DC37370181DC601FF001804,K0H8323201808C2030300180C,K0D8363671D8CC60303001C04,K0F81E1E3188CE6030300180E,K0701C1C1F1FC7C01FF001804,K02008080E0703803FE00180C,,::::::::::::::^XA
// ^MMP
// ^PW189
// ^LL0165
// ^LS0
// ^BY101,100^FT51,116^BXN,5,200,0,0,1,_
// ^FH\^FD#K0B67651215100{{EventTime.Year|Cut(2,2)}}{{EventTime.Month|Left(2,'0')}}{{EventTime.Day|Left(2,'0')}}{{Product.ProductCode|Cut(8,6)}}#^FS
// ^FT15,80^A0N,17,21^FH\^FDBE^FS
// ^PQ1,0,1,Y^XZ


// string zplCommand = 
//             "^XA" +
//             "^FO50,50" +                    // Field origin for DataMatrix (X, Y)
//             "^BXN,5,200,0,0,1" +            // DataMatrix barcode format (N = Normal, Magnification = 5, Height = 200)
//             "^FD" + codeToPrint + "^FS" +   // Field data for the barcode
//             "^FO50,300" +                   // Field origin for description text (X, Y)
//             "^A0N,50,50" +                  // Font size for the description (A0 = font, N = normal orientation)
//             "^FD" + description + "^FS" +   // Field data for the description
//             "^XZ";                          // End the ZPL command
#endregion

namespace ZebraMatrix 
{
    public class MatrixP
    {
        #region VARIABLES
        private string ipAddress; 
        private int port; 
        private Connection connectionPrinter;
        private DateTime currentDateTime = DateTime.Now;
        
        #endregion

        public MatrixP(string ipPrinter, int portToConnect)
        {
            this.ipAddress = ipPrinter;
            this.port = portToConnect;
            connectionPrinter = new TcpConnection(ipAddress, portToConnect);
        }

        # region MAIN_METHODS
        public bool printDMCcodeforModel( string dmsCode, int counter)
        {
            try
            {
                connectionPrinter.Open();

                string formattedDateTime = getTime();
                string paddedNumber = IntToZeroPaddedString(counter);
                string codeToPrint = $"#{dmsCode}{formattedDateTime}{paddedNumber}#";

                string zplCommand = 
                    "^XA" +
                    "^FO50,50" +                    // Field origin (X, Y)
                    "^BXN,5,200,0,0,1" +            // DataMatrix barcode format; N = Normal, 2 = Magnification, 200 = Height 2,200 BXN,5,200,0,0,1,
                    "^FD" + codeToPrint + "^FS" +   // Field data for the barcode
                    "^XZ";

                byte[] zplBytes = Encoding.UTF8.GetBytes(zplCommand);       // Convert the ZPL command from string to byte[]
                connectionPrinter.Write(zplBytes);                          // Send the byte array to the printer
                connectionPrinter.Close();

                Console.WriteLine($"Printed DataMatrix: {codeToPrint}");

            }
            catch (ConnectionException e)
            {
                Console.WriteLine("Error connecting to printer: " + e.Message);
                return false;
            }
        
            return true;
        }
        
        #endregion
        
        #region METHODS
        private string getTime()                                    // Get current Time string
        {
            string formattedDateTime = currentDateTime.ToString("ddMMyyyy"); // Change format as needed HHmmss
            return formattedDateTime;
        }
        private string IntToZeroPaddedString(int number)            // exchange 6 to 000006
        {
            return number.ToString("D6");
        }
        public int ZeroPaddedStringToInt(string paddedNumber)       // exchange 000006 to 6
        {
            return int.Parse(paddedNumber);
        }

        #endregion

    }
      
}

