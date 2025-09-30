using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static string RgPath =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
        ? "rg.exe"
        : "rg";

    static async Task Main()
    {
        Console.Clear();
        Console.WriteLine("Dynamic Live Fuzzy Finder (ESC to exit)");

        string query = "";
        Process? runningProcess = null;
        CancellationTokenSource? cts = null;

        int inputLine = Console.WindowHeight - 1; // input at bottom
        Console.SetCursorPosition(0, inputLine);
        Console.Write("> ");

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape) break;
                else if (key.Key == ConsoleKey.Backspace && query.Length > 0)
                    query = query.Substring(0, query.Length - 1);
                else if (!char.IsControl(key.KeyChar))
                    query += key.KeyChar;

                // Update input line
                Console.SetCursorPosition(2, inputLine);
                Console.Write(new string(' ', Console.WindowWidth - 2));
                Console.SetCursorPosition(2, inputLine);
                Console.Write(query);

                // Cancel previous rg process
                if (runningProcess != null && !runningProcess.HasExited)
                {
                    try { runningProcess.Kill(true); } catch { }
                }

                // Cancel previous debounce
                cts?.Cancel();
                cts = new CancellationTokenSource();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(150, cts.Token);
                            if (!cts.Token.IsCancellationRequested)
                            {
                                // Clear previous results (everything above input line)
                                for (int i = 1; i < inputLine; i++)
                                {
                                    Console.SetCursorPosition(0, i);
                                    Console.Write(new string(' ', Console.WindowWidth));
                                }
                                Console.SetCursorPosition(0, 1);

                                runningProcess = RunRipGrep(query);
                            }
                        }
                        catch (TaskCanceledException) { }
                    }, cts.Token);
                }
            }

            await Task.Delay(10);
        }
    }

    static Process RunRipGrep(string pattern, string args = "--line-number --hidden --color=always")
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = RgPath,
            Arguments = $"{args} {pattern} .",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (s, e) => {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                // print results above input line
                int top = Console.CursorTop;
                if (top < Console.WindowHeight - 1)
                    Console.WriteLine(e.Data);
            }
        };
        process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) Console.Error.WriteLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }
}
