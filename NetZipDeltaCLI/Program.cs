using NetZipDelta;
using System;

namespace NetZipDeltaCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ZipDelta.GetDiffFile("","", "Merged");
            Console.WriteLine("Hello World!");
        }
    }
}
