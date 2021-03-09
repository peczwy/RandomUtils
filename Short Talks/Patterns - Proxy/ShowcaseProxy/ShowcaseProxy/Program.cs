using ShowcaseProxy.Core;
using ShowcaseProxy.Core.Impl;
using ShowcaseProxy.FileSystem;
using ShowcaseProxy.FileSystem.Impl;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShowcaseProxy
{
    class Program
    {
        static ICalculation<double> Calculation;

        static IFileOperation FS, FSProxy;

        static void CalculationExample()
        {
            /*
             * Auxiliary field for measuring time and output
             */
            DateTime total, temp;
            double output;

            /*
             * Let's do it hard way first.
             * We instantiate the HardCalculation directly. 
             */
            Console.WriteLine(@"Let's do it hard way first. We instantiate the HardCalculation directly:");
            Calculation = new HardCalculation();
            total = DateTime.Now;
            for (int i = 1; i < 10; ++i)
            {
                temp = DateTime.Now;
                output = Calculation.Calculate(i % 2);
                Console.WriteLine("\tNo. {0}, input: {1}, output: {2}, time: {3} second(s)", i, i % 2, output, (DateTime.Now - temp).TotalSeconds);
            }
            Console.WriteLine("\t\tTotal Time: {0} second(s)", (DateTime.Now - total).TotalSeconds);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();

            /*
             * Now we're proxing the calculations using MemoryProxy class
             */
            Console.WriteLine(@"Now we're proxing the calculations using MemoryProxy class:");
            Calculation = new MemoryProxy<double>(new HardCalculation());
            total = DateTime.Now;
            for (int i = 1; i < 10; ++i)
            {
                temp = DateTime.Now;
                output = Calculation.Calculate(i % 2);
                Console.WriteLine("\tNo. {0}, input: {1}, output: {2}, time: {3} second(s)", i, i % 2, output, (DateTime.Now - temp).TotalSeconds);
            }
            Console.WriteLine("\t\tTotal Time: {0} second(s)", (DateTime.Now - total).TotalSeconds);
        }

        static void FileSystemExample()
        {
            /*
             * Let's assume some path and file:
             */
            var path = "C:/Temp/FSTest";
            var file = "test.txt";

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }

            /*
             * Let's check that FileOperation works properly
             */
            Console.WriteLine("Firstly we check the FileOperation class");
            FS = new FileOperation();

            Console.WriteLine("\tIs File Removed: {0}", FS.IsRemoved(path, file));
            FS.Create(path, file);
            FS.Append(path, file, new List<string>() { "Top-Level: Foo", "Top-Level: Bar" });
            Console.WriteLine("\tContents of FileOperation FS file:");
            foreach (var line in FS.Read(path, file))
            {
                Console.WriteLine("\t\t{0}", line);
            }
            Console.WriteLine("Press enter to continue:");
            Console.ReadLine();

            /*
             * Now let's check the Proxy on the layer some_layer
             */

            Console.WriteLine("Now let's check the Proxy on the layer some_layer");
            FSProxy = new ProxyFileOperation(new FileOperation(), "some_layer");
            FSProxy.Create(path, file);
            FSProxy.Append(path, file, new List<string>() { "SOME_LAYER - Yup"});
            Console.WriteLine("\tContents of ProxyOperation FS file (some_layer):");
            foreach (var line in FSProxy.Read(path, file))
            {
                Console.WriteLine("\t\t{0}", line);
            }
            Console.WriteLine("Press enter to continue:");
            Console.ReadLine();
            /*
             * Now let's check the Proxy on the layer layer
             */
            Console.WriteLine("Now let's check the Proxy on the layer mindfcuk");
            FSProxy = new ProxyFileOperation(new FileOperation(), "mindfcuk");
            FSProxy.Create(path, file);
            FSProxy.Append(path, file, new List<string>() { "mindfcuk - ORly?" });
            Console.WriteLine("\tContents of ProxyOperation FS file (mindfcuk):");
            foreach (var line in FSProxy.Read(path, file))
            {
                Console.WriteLine("\t\t{0}", line);
            }
            Console.WriteLine("Should we delete the file (1) or not (0)");
            var input = Console.ReadLine();
            if ("1".Equals(input))
            {
                FSProxy.Delete(path, file);
            }

            Console.WriteLine("And now let's delete the contents of file. Is it removed: {0}", FSProxy.IsRemoved(path, file));

            Console.WriteLine("Press enter to continue:");
            Console.ReadLine();

            /*
             * Absurd level
             */
            Console.WriteLine("Absurd level: Proxy of Proxy:");
            FSProxy = new ProxyFileOperation(new ProxyFileOperation(new FileOperation(), "mindfcuk"), "inception");
            FSProxy.Append(path, file, new List<string>() { "Inception - How is it possible", "Inception - Seriously?" });
            Console.WriteLine("\tContents of ProxyProxy FS file (inception)");
            foreach (var line in FSProxy.Read(path, file))
            {
                Console.WriteLine("\t\t{0}", line);
            }
        }

        static void Main(string[] args)
        {
            /*
             * Declare some helper variables
             */
            var input = "";

            /*
             * Flow
             */
            Console.WriteLine("Hello!!");
            do
            {
                Console.WriteLine(@"Please enter the following:
                exit (default) - to exit
                1 - to show the calculation proxy example
                2 - to show the filesystem proxy example
                ");
                input = Console.ReadLine().ToUpper();
                switch (input)
                {
                    case "1":
                        {
                            CalculationExample();
                            break;
                        }
                    case "2":
                        {
                            FileSystemExample();
                            break;
                        }
                    default:
                        {
                            input = "EXIT";
                            break;
                        }
                }

            } while (!input.Equals("EXIT"));

            Console.WriteLine("Goodbye!");
            Console.ReadLine();
        }
    }
}
