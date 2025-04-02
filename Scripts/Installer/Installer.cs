using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Run_Installer {
    public class Installer {

        private const string Executable = "run.exe";
        private const string UninstallExecutable = "uninstall.exe";
        private const string LogFile = "install.log";
        private const string InstallerLocationFile = "installerLocation.txt";
        private const string ExecutableUrl = "https://github.com/Kisetsu15/Run/releases/latest/download/run.exe";
        private const string UninstallUrl = "https://github.com/Kisetsu15/Run/releases/latest/download/uninstall.exe";
        private static readonly HttpClient client = new();
        private static bool isSilent = false;
        private static readonly string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        public static async Task Main( string[] args ) {
            isSilent = args.Contains("--silent") || args.Contains("/S") || args.Contains("/silent");

            if ( args.Contains("--uninstall") ) {
                Uninstall();
                return;
            }

            string runDestination = Path.Combine(Path.GetTempPath(), Executable);
            string uninstallDestination = Path.Combine(Path.GetTempPath(), UninstallExecutable);

            Log($"Downloading '{Executable}' from '{ExecutableUrl}'...");
            if ( !await DownloadFileWithRetryAsync(ExecutableUrl, runDestination) ) {
                Log("Download failed: Run.exe not found.");
                return;
            }

            Log("Downloading 'uninstall.exe'...");
            if ( !await DownloadFileWithRetryAsync(UninstallUrl, uninstallDestination) ) {
                Log("Download failed: Uninstall.exe not found.");
                return;
            }

            Log("Downloads complete.");
            File.WriteAllText(InstallerLocationFile, Directory.GetCurrentDirectory());
            try {
                Log($"Creating installation directory at {installPath}...");
                Directory.CreateDirectory(installPath);
                Log("Moving files to install directory...");
                MoveFile(runDestination, Path.Combine(installPath, Executable));
                MoveFile(uninstallDestination, Path.Combine(installPath, UninstallExecutable));
                MoveFile(InstallerLocationFile, Path.Combine(installPath, InstallerLocationFile));
                Log("Move complete.");
                AddToPath(installPath);
                Log("Installation complete.");
            } catch ( Exception ex ) {
                Log($"Fatal error: {ex.Message}");
            }
        }

        private static void Uninstall() {
            try {
                string uninstallPath = Path.Combine(installPath, UninstallExecutable);
                if ( !File.Exists(uninstallPath) ) {
                    Log("Uninstall executable not found.");
                    return;
                }

                var startInfo = new ProcessStartInfo {
                    FileName = uninstallPath,
                    Arguments = isSilent ? "--silent" : "",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            } catch ( Exception e ) {
                Log($"Fatal error during uninstallation: {e.Message}");
            }
        }

        private static async Task<bool> DownloadFileWithRetryAsync( string url, string destination, int maxRetries = 3 ) {
            for ( int attempt = 1; attempt <= maxRetries; attempt++ ) {
                try {
                    using var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fs);
                    return true;
                } catch ( Exception e ) {
                    Log($"Attempt {attempt}: Download failed - {e.Message}");
                    if ( attempt == maxRetries )
                        return false;
                }
            }
            return false;
        }

        private static void MoveFile( string source, string destination ) {
            try {
                if ( File.Exists(destination) ) {
                    Log($"'{Path.GetFileName(destination)}' already exists in '{installPath}'. Overwriting...");
                    File.Delete(destination);
                }
                File.Move(source, destination);
            } catch ( Exception e ) {
                Log($"Fatal error while moving file: {e.Message}");
            }
        }

        private static void AddToPath( string newPath ) {
            try {
                var target = EnvironmentVariableTarget.User;
                var currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";

                if ( currentPath.Split(';').Contains(newPath) ) {
                    Log($"'{newPath}' is already in PATH.");
                    return;
                }

                var updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable("PATH", updatedPath, target);
                Log($"Successfully added '{newPath}' to PATH.");
                RefreshEnvironment();
            } catch ( Exception ex ) {
                Log($"Failed to update PATH\nFatal error: {ex.Message}");
            }
        }

        private static void RefreshEnvironment() {
            try {
                Log("Refreshing environment variables...");
                const int HWND_BROADCAST = 0xFFFF;
                const int WM_SETTINGCHANGE = 0x1A;
                const int SMTO_ABORTIFHUNG = 0x0002;

                SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out _);
            } catch ( Exception ex ) {
                Log($"Failed to refresh environment variables: {ex.Message}");
            }
        }

        private static void Log( string message ) {
            if ( !isSilent )
                Console.WriteLine(message);

            File.AppendAllText(LogFile, message + Environment.NewLine);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout( IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult );
    }
}
