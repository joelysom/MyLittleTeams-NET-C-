/*
using System;
using System.IO;

class ErrorCapture
{
    static void Main(string[] args)
    {
        string logFile = "error_captured.log";
        
        try
        {
            // Redirect console output
            using (StreamWriter writer = new StreamWriter(logFile, false))
            {
                Console.SetOut(writer);
                Console.SetError(writer);
                
                writer.WriteLine("=== APPLICATION START ===");
                writer.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                writer.WriteLine("Attempting to start Choas...\n");
                writer.Flush();
                
                // Try to start the app
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "Choas.exe";
                    process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    
                    process.OutputDataReceived += (s, e) => 
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            writer.WriteLine($"OUT: {e.Data}");
                        writer.Flush();
                    };
                    
                    process.ErrorDataReceived += (s, e) => 
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            writer.WriteLine($"ERR: {e.Data}");
                        writer.Flush();
                    };
                    
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    
                    writer.WriteLine($"\nProcess exited with code: {process.ExitCode}");
                    writer.WriteLine("=== APPLICATION END ===");
                }
            }
            
            // Print completed
            Console.Out.WriteLine("\nLog saved to error_captured.log");
        }
        catch (Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(logFile, true))
            {
                writer.WriteLine($"EXCEPTION: {ex.GetType().Name}");
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}
*/
