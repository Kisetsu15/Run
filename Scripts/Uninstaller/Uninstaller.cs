using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace Run_Uninstaller {
    public class Uninstaller {
        private static string InstallPath = IsRunningAsAdmin()
            ? "C:\\Program Files\\Run"
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        private static readonly string Executable = "run.exe";
        private static readonly string LogFile = "install.log";

        public static void Main( string[] args ) {
            Console.WriteLine($"Uninstalling Run from '{InstallPath}'...");

            // Check if file exists
            string exePath = Path.Combine(InstallPath, Executable);
            if ( !File.Exists(exePath) ) {
                Console.WriteLine("Run is not installed.");
                return;
            }

            try {
                RemoveFromPath(InstallPath);
                Console.WriteLine("Removed from PATH.");

                // Create a temporary batch script to delete itself
                string batchFile = Path.Combine(Path.GetTempPath(), "uninstall_run.bat");
                File.WriteAllText(batchFile, $@"
@echo off
timeout /t 2 /nobreak >nul
rmdir /s /q ""{InstallPath}""
del /f /q ""{batchFile}""
");

                // Run batch script and close application
                ProcessStartInfo psi = new ProcessStartInfo {
                    FileName = "cmd.exe",
                    Arguments = $"/c start /min \"\" \"{batchFile}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);

                Console.WriteLine("Uninstalling...");
                Environment.Exit(0);
            } catch ( Exception ex ) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void RemoveFromPath( string pathToRemove ) {
            EnvironmentVariableTarget target = IsRunningAsAdmin()
                ? EnvironmentVariableTarget.Machine
                : EnvironmentVariableTarget.User;

            string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";
            var paths = currentPath.Split(';').ToList();

            if ( paths.Contains(pathToRemove) ) {
                paths.Remove(pathToRemove);
                Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), target);
            }
        }

        private static bool IsRunningAsAdmin() {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}



