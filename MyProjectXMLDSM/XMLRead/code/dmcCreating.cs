using System;
using ZXing;
using ZXing.Datamatrix;
using ZXing.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Text;

namespace dmc
{
    public class dmcCreate
    {
        #region Variables
        private string pdataToDms;
        private string pidNumber;
        private string preturnDMc;
        public string returnDmc { get; set; }
        #endregion

        public dmcCreate(){}

        #region Methods
        public void pngDMc(string date_0, string date_1)
        {
            string combinedData = date_0 + "|" + date_1;

            // Create DataMatrixWriter to generate DataMatrix code
            DataMatrixWriter writer = new DataMatrixWriter();
            var matrix = writer.encode(combinedData, BarcodeFormat.DATA_MATRIX, 200, 200);

            // Create BarcodeWriter with PixelDataRenderer
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new ZXing.Common.EncodingOptions
                {
                    //Width = 200,
                    //Height = 200
                }
            };

            // Generate the pixel data for the barcode
            var pixelData = barcodeWriter.Write(combinedData);

            // Create an ImageSharp image from the pixel data
            var dataMatrixImage = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

            // Get the current directory where the application is running
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataMatrixCode.png");

            // Save the image in the same directory where the software is running
            dataMatrixImage.Save(outputPath);

            Console.WriteLine($"DataMatrix code generated and saved at {outputPath}.");
        }
        public string GenerateZPL(string date_0, string date_1)
        {
            string combinedData = date_0 + "|" + date_1;
            
            // // Create DataMatrixWriter to generate DataMatrix code
            // DataMatrixWriter writer = new DataMatrixWriter();
            // var matrix = writer.encode(combinedData, BarcodeFormat.DATA_MATRIX, 200, 200);

            // // Create BarcodeWriter with PixelDataRenderer
            // var barcodeWriter = new BarcodeWriterPixelData
            // {
            //     Format = BarcodeFormat.DATA_MATRIX,
            //     Options = new ZXing.Common.EncodingOptions
            //     {
            //         Width = 200,
            //         Height = 200
            //     }
            // };

            // // Generate the pixel data for the barcode
            // var pixelData = barcodeWriter.Write(combinedData);

            // Generate ZPL code
            var zpl = new StringBuilder();
            zpl.AppendLine("^XA"); // Start label
            zpl.AppendLine("^FO50,50"); // Field Origin
            zpl.AppendLine("^BXN,10,200,200,200,2,6,30"); // DataMatrix barcode command
            zpl.AppendLine($"^FD{combinedData}^FS"); // Field Data
            zpl.AppendLine("^XZ"); // End label

            return zpl.ToString();
        }
        #endregion
    }
}
