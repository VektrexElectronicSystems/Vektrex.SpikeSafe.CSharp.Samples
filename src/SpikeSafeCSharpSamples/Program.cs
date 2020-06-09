using System;
using System.Text;
using SpikeSafeCSharpSamples.GettingStarted;

namespace SpikeSafeCSharpSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            int lastTestNumber = 0;

            while (true)
            {
                StringBuilder menu = new StringBuilder();
                
                // TODO. Improve to go into sub-directories first, then list tests, and add way back to main menu?
                menu.AppendLine("Samples menu:");
                menu.AppendLine("1) TcpSample");
                menu.AppendLine("r) Rerun previous test");
                menu.Append("q) Quit");
                Console.WriteLine(menu);
                Console.Write("Enter test # to run, or r to re-run previous test, or q to quit. And then press Enter: ");

                // Read user input
                string input = Console.ReadLine();
                Console.WriteLine("Option entered: {0}", input);

                int testNumber = 0;
                bool isInteger = int.TryParse(input, out testNumber);

                // Exit on quit
                if (isInteger == false && input == "q")
                    break;
                // Run test on test number or re-run
                else if ((isInteger == false && input == "r") || isInteger)
                {
                    // Reset last test for re-run
                    if (isInteger)
                        lastTestNumber = testNumber;
                    
                    // Run test based on selection
                    switch(lastTestNumber)
                    {
                        // TODO. May be better way to run selection than hard-coded options in two places (here and menu)
                        case 1:
                            new TcpSample().Run();
                            break;
                        default:
                            Console.WriteLine("Invalid test # entered, please try again.");
                            break;
                    }
                }
            }
        }
    }
}
