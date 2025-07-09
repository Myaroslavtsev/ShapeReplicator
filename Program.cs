using System;
using System.IO;
using System.Threading.Tasks;
using ShapeData.Kuju_shape;

namespace ShapeReplicator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!ValidateArgs(args))
                return;

            int count = 0;
            if (args.Length >= 3) 
                int.TryParse(args[3], out count);

            await BatchConverter.ConvertShape(args[0], args[1], args[2], count);
        }

        private static bool ValidateArgs(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: At least two arguments should be specified");
                PrintHelp();
                return false;
            }

            if (!IsValidAndReadableFile(args[0]))
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: Could not open initial shape file");
                PrintHelp();
                return false;
            }

            if (!IsValidAndReadableFile(args[1]))
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: Could not open tsection.dat file");
                PrintHelp();
                return false;
            }

            if (!int.TryParse(args[3], out var count) || count <= 0)
            {
                Console.WriteLine($"{DateTime.Now:T} ERROR: Shape quantity is not a positive integer");
                PrintHelp();
                return false;
            }

            Console.WriteLine($"{DateTime.Now:T} Arguments are correct. Starting conversion");
            return true;
        }


        public static bool IsValidAndReadableFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                using (var stream = File.OpenRead(path))
                {
                    return true;
                }
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
            Console.WriteLine("Usage: <input shape.csv> <tsection.dat> [output shape file mask] [limit shapes count]");
            Console.WriteLine();
            Console.WriteLine("1. Path and filename of .csv with the description of shape structure to create.");
            Console.WriteLine("2. Path and filename of tsection.dat, i.e. \"C:\\Train\\Global\\tsection.dat\".");
            Console.WriteLine("Paths may be omitted when files are in the current folder.");
            Console.WriteLine("3. Create only shapes corresponding to the mask. * and ? symbols may be used, i.e. A?t*.s");
            Console.WriteLine("4. Limit the number of created shapes.");
            Console.WriteLine("After conversion a .ref entry is created.");
            Console.WriteLine("After conversion a .bat to compress shapes with ffeditc_unicode.exe is also created.");
            Console.WriteLine();
            Console.WriteLine("Get further help and copyright info at\r\nhttps://github.com/Myaroslavtsev/ShapeReplicator");
        }
    }
}
