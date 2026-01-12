using System;
using System.IO;
using System.Linq;
using DbcParserLib;
using DbcParserLib.Observers;

namespace DbcParserDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== DBC File Parser Demo ===");
            Console.WriteLine();

            // Get DBC file path from command line or use default
            string dbcFilePath;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                dbcFilePath = args[0];
            }
            else
            {
                // Default to a sample file
                dbcFilePath = Path.Combine("..", "..", "..", "..", "DbcFiles", "kia_ev6.dbc");
                Console.WriteLine($"Usage: DbcParserDemo <path-to-dbc-file>");
                Console.WriteLine($"Using default file: {dbcFilePath}");
                Console.WriteLine();
            }

            if (!File.Exists(dbcFilePath))
            {
                Console.WriteLine($"Error: File not found: {dbcFilePath}");
                return;
            }

            try
            {
                // Create an observer to capture warnings and errors
                var observer = new SimpleFailureObserver();
                Parser.SetParsingFailuresObserver(observer);

                Console.WriteLine($"Parsing file: {Path.GetFileName(dbcFilePath)}");
                Console.WriteLine(new string('-', 60));

                // Parse the DBC file
                var dbc = Parser.ParseFromPath(dbcFilePath);

                // Display parsing warnings/errors
                var errors = observer.GetErrorList();
                if (errors.Count > 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠️  Parsing Warnings/Errors ({errors.Count}):");
                    Console.ResetColor();
                    foreach (var error in errors.Take(10)) // Show first 10 errors
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    if (errors.Count > 10)
                    {
                        Console.WriteLine($"  ... and {errors.Count - 10} more");
                    }
                    Console.WriteLine();
                }

                // Display DBC file information
                Console.WriteLine();
                Console.WriteLine("=== DBC File Summary ===");
                Console.WriteLine($"Total Messages: {dbc.Messages.Count()}");
                Console.WriteLine($"Total Nodes: {dbc.Nodes.Count()}");
                Console.WriteLine($"Total Environment Variables: {dbc.EnvironmentVariables.Count()}");
                Console.WriteLine();

                // Display Messages and their Signals
                Console.WriteLine("=== Messages and Signals ===");
                Console.WriteLine();

                foreach (var message in dbc.Messages.Take(5)) // Show first 5 messages
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Message: {message.Name} (ID: 0x{message.ID:X}, {message.ID})");
                    Console.ResetColor();
                    Console.WriteLine($"  DLC: {message.DLC} bytes");
                    Console.WriteLine($"  Transmitter: {message.Transmitter}");
                    
                    // Display cycle time if available
                    if (message.CycleTime(out var cycleTime))
                    {
                        Console.WriteLine($"  Cycle Time: {cycleTime} ms");
                    }

                    if (message.Signals.Any())
                    {
                        Console.WriteLine($"  Signals ({message.Signals.Count()}):");
                        foreach (var signal in message.Signals.Take(3)) // Show first 3 signals
                        {
                            Console.WriteLine($"    - {signal.Name}");
                            Console.WriteLine($"      Start Bit: {signal.StartBit}, Length: {signal.Length} bits");
                            Console.WriteLine($"      Range: [{signal.Minimum}, {signal.Maximum}]");
                            Console.WriteLine($"      Factor: {signal.Factor}, Offset: {signal.Offset}");
                            Console.WriteLine($"      Unit: {(string.IsNullOrEmpty(signal.Unit) ? "(none)" : signal.Unit)}");
                            Console.WriteLine($"      Byte Order: {signal.ByteOrder}, Value Type: {signal.ValueType}");
                            Console.WriteLine($"      Initial Value: {signal.InitialValue}");
                        }
                        if (message.Signals.Count() > 3)
                        {
                            Console.WriteLine($"    ... and {message.Signals.Count() - 3} more signals");
                        }
                    }
                    Console.WriteLine();
                }

                if (dbc.Messages.Count() > 5)
                {
                    Console.WriteLine($"... and {dbc.Messages.Count() - 5} more messages");
                    Console.WriteLine();
                }

                // Display Nodes
                if (dbc.Nodes.Any())
                {
                    Console.WriteLine("=== Nodes ===");
                    foreach (var node in dbc.Nodes.Take(5))
                    {
                        Console.WriteLine($"  - {node.Name}");
                        if (node.CustomProperties.Any())
                        {
                            Console.WriteLine($"    Custom Properties: {node.CustomProperties.Count}");
                        }
                    }
                    if (dbc.Nodes.Count() > 5)
                    {
                        Console.WriteLine($"  ... and {dbc.Nodes.Count() - 5} more nodes");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine(new string('-', 60));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Parsing completed successfully!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
