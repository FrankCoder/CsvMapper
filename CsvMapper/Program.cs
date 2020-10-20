using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CsvMapper
{
    class Program
    {
        static int Main(string[] args)
        {
            if(args.Length != 3)
            {
                Console.Error.WriteLine("Incorrect number of arguments.");
                PrintUsage();
#if DEBUG
                Console.ReadKey();
#endif
                return -1;
            }

            try
            {
                Console.Out.WriteLine($"Loading {args[0]} ...");
                CsvDoc source = new CsvDoc(args[0]);
                Console.Out.WriteLine($"Source csv file has {source.NumColumns} columns and {source.Data.Count} rows excluding the first row.");
                Console.Out.WriteLine("Mapping ...");
                CsvDoc target = source.TransformFromMapFile(args[2]);
                Console.Out.WriteLine($"Writing to file: {args[1]} ...");
                target.WriteToFile(args[1]);
                Console.Out.WriteLine($"Target csv file has {target.NumColumns} columns and {target.Data.Count} rows excluding the first row.");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("Exception caught:");
                Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("Terminating");
#if DEBUG
                Console.ReadKey();
#endif
                return -1;
            }
#if DEBUG
                Console.ReadKey();
#endif
            return 0;
        }

        private static void PrintUsage()
        {
            Console.Out.WriteLine("Usage: csvmapper.exe pathToSource.csv pathToOutput.csv pathToMap.xml");
            Console.Out.WriteLine();
            Console.Out.WriteLine("The format for pathToMap.xml file content is:");
            Console.Out.WriteLine();
            Console.Out.WriteLine("<CsvMap merge_on=\"Target column name 2\">");
            Console.Out.WriteLine("  <Target name=\"Target column name\">");
            Console.Out.WriteLine("    <Source>Source column name</Source>");
            Console.Out.WriteLine("  </Target>");
            Console.Out.WriteLine("  <Target name=\"Target column name 2\">");
            Console.Out.WriteLine("    <Source>Source column name first</Source>");
            Console.Out.WriteLine("    <Source>Source column name second</Source>");
            Console.Out.WriteLine("    ...");
            Console.Out.WriteLine("  </Target>");
            Console.Out.WriteLine("  ...");
            Console.Out.WriteLine("</CsvMap>");
            Console.Out.WriteLine();
            Console.Out.WriteLine("You can combine several fields into one by specifying 2 or more sources");
            Console.Out.WriteLine("for a given target.");
            Console.Out.WriteLine();
            Console.Out.WriteLine("You can merge rows if they all share the same value in the specified target column");
            Console.Out.WriteLine("by specifying that target column name as the 'merge_on' attribute of the CsvMap root node.");
            Console.Out.WriteLine();
        }
    }
}
