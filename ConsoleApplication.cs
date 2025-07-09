using System;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace ShapeReplicator
{
    class ConsoleApplication
    {
        static async Task Main(string[] args)
        {
            var (argsValid, mask, ffeditLocation, count, gauge, skipRoads, setBoundingBox) = ValidateArgs(args);

            if (!argsValid)
                return;

            await BatchConverter.ConvertShape(args[0], args[1], 
                mask, 
                ffeditLocation, 
                count, 
                gauge,
                skipRoads,
                setBoundingBox);
        }

        private static (bool valid, string mask, string ffeditLocation, int count, float gauge, bool skipRoads, bool setBoundingBox) 
            ValidateArgs(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: At least two arguments should be specified");
                PrintHelp();
                return (false, "", "", 0, 0, false, false);
            }

            if (!IsReadableFile(args[0]))
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: Could not open initial shape file");
                PrintHelp();
                return (false, "", "", 0, 0, false, false);
            }

            if (!IsReadableFile(args[1]))
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: Could not open tsection.dat file");
                PrintHelp();
                return (false, "", "", 0, 0, false, false);
            }

            var mask = ""; //m
            var ffeditLocation = ""; //f
            int count = 0; //c
            float gauge = 0; //g
            var skipRoads = true; //r
            var omitBoundingBox = true; //b

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].Length >= 2)
                {
                    switch (args[i].Substring(0, 2))
                    {
                        case "/r":
                            skipRoads = false;
                            break;

                        case "/b":
                            omitBoundingBox = false;
                            break;

                        case "/m":
                            mask = GetMaskValue(args[i]);
                            break;

                        case "/f":
                            ffeditLocation = GetFfeditLocation(args[i]);
                            break;

                        case "/c":
                            count = GetCountValue(args[i]);
                            break;

                        case "/g":
                            gauge = GetGaugeValue(args[i]);
                            break;

                        default:
                            Console.WriteLine($"{DateTime.Now:T} ERROR: Unknown argument provided");
                            PrintHelp();
                            return (false, "", "", 0, 0, false, false);
                    }
                }
            }

            Console.WriteLine($"{DateTime.Now:T} Arguments are correct. Starting conversion");
            return (true, mask, ffeditLocation, count, gauge, skipRoads, omitBoundingBox);
        }

        private static int GetCountValue(string arg)
        {
            if (arg.Length > 3 && int.TryParse(arg[3..], out var count))
                return count;

            Console.WriteLine($"{DateTime.Now:T} WARNING: incorrect shape count value");
            return 1;
        }

        private static float GetGaugeValue(string arg)
        {
            var cultureInfo = new CultureInfo("en-US");

            if (arg.Length > 3 && float.TryParse(arg[3..], NumberStyles.Any, cultureInfo, out var gauge))
                return gauge;

            Console.WriteLine($"{DateTime.Now:T} WARNING: incorrect gauge value, converting all shapes");
            return 0;
        }

        private static string GetFfeditLocation(string arg)
        {
            if (arg.Length > 5 && File.Exists(arg[3..]))
                return RemoveQuotation(arg[3..]);

            Console.WriteLine($"{DateTime.Now:T} WARNING: incorrect ffedit location value");
            return "";
        }

        private static string GetMaskValue(string arg)
        {
            if (arg.Length > 3)
                return RemoveQuotation(arg[3..]);
            
            Console.WriteLine($"{DateTime.Now:T} WARNING: incorrect mask value. No filename mask applied");
            return "";
        }

        private static string RemoveQuotation(string s)
        {
            if (s is null || s.Length < 2)
                return s;

            if (s[0] == '"' && s[s.Length - 1] == '"')
                return s[1..(s.Length - 2)];

            return s;
        }

        private static bool IsReadableFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                using var stream = File.OpenRead(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("This app converts shape described in .csv into many MSTS/OR shapes, replicated along track sections from tsection.dat");
            Console.WriteLine();
            Console.WriteLine("Usage: <input shape.csv> <tsection.dat> [/m:<mask>] [/f:<ffedit>] [/c:<count>] [/g:<gauge>] [/r] [/b]");
            Console.WriteLine();
            Console.WriteLine("1. Path and filename of .csv with the description of shape structure to create.");
            Console.WriteLine("2. Path and filename of tsection.dat, i.e. \"C:\\Train\\Global\\tsection.dat\".");
            Console.WriteLine("Paths may be omitted when files are in the current folder.");
            Console.WriteLine("/m: Create only shapes corresponding to the mask. * and ? symbols may be used, eg. /m:\"A?t*.s\"");
            Console.WriteLine("/f: Full path of ffeditc_unicode.exe if you want to compress created shapes immediately");
            Console.WriteLine("/c: Limit the number of created shapes, eg. /c:20");
            Console.WriteLine("/g: Convert only shapes with given track gauge. Use . as delimiter, eg. /f:1.5");
            Console.WriteLine("/r  Don't skip road shapes. Otherwise track shapes processed only. No value needed, eg. just /r");
            Console.WriteLine("/b  Limit shape visibility angles by setting bounding box in .sd. No value needed, eg. just /b");
            Console.WriteLine();
            Console.WriteLine("After conversion .ref entries are created.");
            Console.WriteLine("After conversion a .bat to compress shapes later with ffeditc_unicode.exe is also created.");
            Console.WriteLine();
            Console.WriteLine("Get further help and copyright info at\r\nhttps://github.com/Myaroslavtsev/ShapeReplicator");
        }
    }
}
