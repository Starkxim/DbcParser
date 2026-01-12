# DBC Parser Demo Application

This is a simple console application that demonstrates how to use the DbcParserLib to parse DBC files and display their contents.

## Features

- Parse DBC files and display comprehensive information
- Show Messages, Signals, and Nodes
- Display parsing warnings and errors (including precision loss warnings)
- Colorful console output for better readability

## Usage

### Running the Demo

```bash
dotnet run
```

This will parse the default sample file (`kia_ev6.dbc`).

### Parsing a Custom DBC File

```bash
dotnet run -- <path-to-your-dbc-file>
```

Example:
```bash
dotnet run -- C:\path\to\your\file.dbc
```

## Building for Windows

To build a standalone executable for Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

The executable will be in `bin/Release/net10.0/win-x64/publish/DbcParserDemo.exe`

You can also build for other platforms:
- `win-x86` - 32-bit Windows
- `win-arm64` - ARM64 Windows
- `linux-x64` - 64-bit Linux
- `osx-x64` - macOS Intel
- `osx-arm64` - macOS Apple Silicon

## Understanding the Output

### Parsing Warnings/Errors

The demo uses `SimpleFailureObserver` to capture warnings and errors during parsing. Common warnings include:

1. **Precision Loss Warning** (NEW in this PR):
   ```
   Precision loss for 'GenMsgCycleTime' property: value [100.5] rounded to [100] at line X
   ```
   This warning appears when a float value is provided for an integer-typed property. The parser accepts the value but warns about precision loss.

2. **Syntax Errors**: Issues with DBC file format
3. **Out of Bounds**: Property values outside defined ranges
4. **Missing References**: Referenced elements not found

### DBC File Summary

Shows counts of:
- Messages: CAN message definitions
- Nodes: ECUs or network nodes
- Environment Variables: Global variables

### Messages and Signals

For each message, displays:
- **Message Name and ID**: Both hex and decimal format
- **DLC**: Data Length Code (message size in bytes)
- **Transmitter**: The node that sends this message
- **Cycle Time**: Message transmission interval (if defined)

For each signal, displays:
- **Name**: Signal identifier
- **Start Bit and Length**: Position and size in the message
- **Range**: Minimum and maximum values
- **Factor and Offset**: For scaling raw values to physical values
- **Unit**: Physical unit of measurement
- **Byte Order**: Endianness (0=Big Endian, 1=Little Endian)
- **Value Type**: Signed or Unsigned
- **Initial Value**: Default value

## Code Example

```csharp
using DbcParserLib;
using DbcParserLib.Observers;

// Create an observer to capture warnings
var observer = new SimpleFailureObserver();
Parser.SetParsingFailuresObserver(observer);

// Parse the DBC file
var dbc = Parser.ParseFromPath("your-file.dbc");

// Check for warnings
var errors = observer.GetErrorList();
if (errors.Count > 0)
{
    Console.WriteLine("Warnings/Errors:");
    foreach (var error in errors)
    {
        Console.WriteLine($"  {error}");
    }
}

// Access parsed data
foreach (var message in dbc.Messages)
{
    Console.WriteLine($"Message: {message.Name} (ID: 0x{message.ID:X})");
    
    foreach (var signal in message.Signals)
    {
        Console.WriteLine($"  Signal: {signal.Name}");
        Console.WriteLine($"    Range: [{signal.Minimum}, {signal.Maximum}]");
        Console.WriteLine($"    Unit: {signal.Unit}");
    }
}
```

## Testing Precision Loss Warnings

To see the precision loss warnings in action, create a DBC file with integer properties that have float values:

```dbc
BA_DEF_ BO_ "GenMsgCycleTime" INT 0 10000;
BA_ "GenMsgCycleTime" BO_ 100 100.5;
```

The parser will accept this (loose mode) but display:
```
⚠️  Precision loss for 'GenMsgCycleTime' property: value [100.5] rounded to [100] at line X
```

## Requirements

- .NET 8.0 or later (can be adjusted in the .csproj file)
- DbcParserLib reference

## Related Files

- `Program.cs` - Main application code
- `DbcParserDemo.csproj` - Project configuration
- `../../DbcFiles/` - Sample DBC files for testing
