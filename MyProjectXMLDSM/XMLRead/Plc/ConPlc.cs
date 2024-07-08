// EXAMPLE
// class Program
// {
//     static void Main(string[] args)
//     {
 
//         S7Client client = new S7Client();
//         string ipAddress = "192.168.1.5"; // Replace with your PLC's IP address

      
//         int result = client.ConnectTo(ipAddress, 0, 1); // Rack and Slot: 0, 1 for S7-1200/1500

//         if (result == 0){
//             Console.WriteLine("Successfully connected to the PLC.");
//         }
//         else{
//             Console.WriteLine($"Error connecting to the PLC: {client.ErrorText(result)}");
//         }
        
//         client.Disconnect();
//     }
// }

#define DEBUG

using System;
using System.Text;
using Sharp7;


namespace s7
{
    class S7con
    {
         private S7Client _client;
        private string _ipAddress;
        private int _slot;
        private int _rack;
       

        public S7con(string ipAddress, int rack, int slot)
        {
            _client = new S7Client();
            _ipAddress = ipAddress;
            _slot = slot;
            _rack = rack;
        }

        #region Method's       
        public bool connectPLc()
        {
            var result = _client.ConnectTo(_ipAddress, _rack, _slot);
            return result == 0;
        }

        public int disconnectPLc()
        {
            return _client.Disconnect();
        }

        public float[] ReadRealDataV01(int dbNumber, int start, int count)
        {
            int size = count * 4; // Each REAL (float) is 4 bytes
            byte[] buffer = new byte[size];
            int result = _client.DBRead(dbNumber, start, size, buffer);

            if (result == 0)
            {
                float[] values = new float[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = BitConverter.ToSingle(buffer, i * 4);
                }
                return values;
            }
            else
            {
                throw new Exception($"Error reading from the PLC: {_client.ErrorText(result)}");
            }
        }

        public float ReadRealDataV02(int dbNumber, int start)
        {
            byte[] buffer = new byte[4]; // A REAL is 4 bytes
            int result = _client.DBRead(dbNumber, start, 4, buffer);

            // if (result == 0)
            // {
            //     return BitConverter.ToSingle(buffer, 0);
            // }
            // else
            // {
            //     throw new Exception($"Error reading from the PLC: {_client.ErrorText(result)}");
            // }

            if (result == 0)
            {
                // Reverse bytes if the PLC is using big-endian format
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                return BitConverter.ToSingle(buffer, 0);
            }
            else
            {
                throw new Exception($"Error reading from the PLC: {_client.ErrorText(result)}");
            }
        }

        public string ReadString(int dbNumber, int start, int size)
        {
            byte[] buffer = new byte[size];
            int result = _client.DBRead(dbNumber, start, size, buffer);

            if (result == 0)
            {
                // Convert the byte array to a string
                return Encoding.Default.GetString(buffer);
            }
            else
            {
                throw new Exception($"Error reading from the PLC: {_client.ErrorText(result)}");
            }
        }

        public short[] ReadXData(int dbNumber, int start, int size)
        {

            byte[] buffer = new byte[size];
            int result = _client.DBRead(dbNumber, start, size, buffer);

            if (result == 0)
            {
                
                short[] values = new short[size / 2];               // 16-bit
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = BitConverter.ToInt16(buffer, i * 2);
                }
                return values;
            }
            else
            {
                throw new Exception($"Error reading from the PLC: {_client.ErrorText(result)}");
            }
        }

        public void WriteXData(int dbNumber, int start, short[] values)
        {
            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                BitConverter.GetBytes(values[i]).CopyTo(buffer, i * 2);
            }

            int result = _client.DBWrite(dbNumber, start, buffer.Length, buffer);

            if (result != 0)
            {
                throw new Exception($"Error writing to the PLC: {_client.ErrorText(result)}");
            }
        }


        #endregion
    } 
}
