using System.Diagnostics;

namespace Uninstaller {
    public class Uninstaller {
        static readonly string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        static readonly string executable = "run.exe";

        static bool isSilent = false;

        public static void Main(string[] args) {
            isSilent = args.Contains("--silent") || args.Contains("/S") || args.Contains("/silent");
            Whisper($"Uninstalling Run from '{installPath}'...");

            string exePath = Path.Combine(installPath, executable);
            if (!File.Exists(exePath) && !Directory.Exists(installPath)) {
                Whisper("Run is not installed.");
                return;
            }

            try {
                RemoveFromPath(installPath);
                Whisper("Removed from PATH.");

                string batchFile = Path.Combine(Path.GetTempPath(), "uninstall_run.bat");
                File.WriteAllText(batchFile, CreateBatchScript(installPath));

                string installerFile = Path.Combine(GetInstallerDir(), "Run-Installer.exe");
                string logFile = Path.Combine(GetInstallerDir(), "install.log");

                if (File.Exists(logFile)) {
                    File.Delete(logFile);
                }

                if (File.Exists(installerFile)) {
                    File.Delete(installerFile);
                }

                ProcessStartInfo psi = new() {
                    FileName = "cmd.exe",
                    Arguments = $"/c start /min \"\" \"{batchFile}\"",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);

                Whisper("Uninstalling...");
            } catch (Exception ex) {
                Whisper($"Error: {ex.Message}");
            }
        }

        static string GetInstallerDir() {
            try {
                string path = "installerLocation.txt";
                return File.Exists(path) ? File.ReadAllText(path) : "";
            } catch {
                return "";
            }
        }

        static void Whisper(string message) {
            if (!isSilent) {
                Console.WriteLine(message);
            }
        }

        static void RemoveFromPath(string pathToRemove) {
            EnvironmentVariableTarget target = EnvironmentVariableTarget.User;

            string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";
            List<string> paths = [.. currentPath.Split(';')];

            if (!paths.Contains(pathToRemove)) {
                return;
            }

            paths.Remove(pathToRemove);
            Environment.SetEnvironmentVariable("PATH", string.Join(";", paths), target);
        }

        static string CreateBatchScript(string installPath) {
            return $@"
@echo off
taskkill /IM run.exe /F >nul 2>&1
timeout /t 2 /nobreak >nul

if exist ""{installPath}"" (
    rmdir /s /q ""{installPath}""
)

(
    echo @echo off
    echo timeout /t 2 /nobreak ^>nul
    echo del ""%%~f0""
) > ""%temp%\cleanup.bat""
start /min """" ""%temp%\cleanup.bat""
exit
";
        }
    }
}
