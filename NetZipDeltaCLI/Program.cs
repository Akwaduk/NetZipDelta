using NetZipDelta;
using System;

namespace NetZipDeltaCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example usage
            var AllBasePaths = "C:\\Users\\username\\Desktop\\";
            var FileOneZip = AllBasePaths + "Old.zip";
            var FileTwoZip = AllBasePaths + "New.zip";
            var OutputDir = AllBasePaths + "NewZipTemp";
            var ZipFileName = AllBasePaths + "NewZipWithUpdatedFiles.zip";

            ZipDelta.GetDeltaFile(FileOneZip, FileTwoZip, OutputDir, ZipFileName);
            Console.WriteLine("Press any key to close the application");
        }
    }
}
