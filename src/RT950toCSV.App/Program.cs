using System;
using System.Windows.Forms;

namespace RT950toCSV.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }

        var command = args[0].Trim().ToLowerInvariant();
        try
        {
            return command switch
            {
                "export" => RunExport(args),
                "import" => RunImport(args),
                _        => HandleUnknown()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 2;
        }
    }

    private static int RunExport(string[] args)
    {
        if (args.Length < 3) { PrintUsage(); return 1; }
        var count = ConverterCore.ExportToChirpCsv(args[1], args[2]);
        Console.WriteLine($"Exported {count} channels to {args[2]}.");
        return 0;
    }

    private static int RunImport(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Import requires: <channels.csv> <output.dat> <template.dat>");
            PrintUsage();
            return 1;
        }
        var (imported, skipped) = ConverterCore.ImportFromChirpCsv(args[1], args[2], args[3]);
        Console.WriteLine($"Imported {imported} channels ({skipped} skipped) into {args[2]}.");
        return 0;
    }

    private static int HandleUnknown() { PrintUsage(); return 1; }

    private static void PrintUsage()
    {
        Console.WriteLine("RT950toCSV — Radtel RT950 Pro channel converter");
        Console.WriteLine();
        Console.WriteLine("Launch without arguments to open the GUI, or use CLI:");
        Console.WriteLine("  rt950 export  <Radio.dat> <channels.csv>");
        Console.WriteLine("  rt950 import  <channels.csv> <output.dat> <template.dat>");
    }
}