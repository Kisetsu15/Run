using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Run_Installer {
    public class Installer {

        private const string Executable = "run.exe";
        private const string UninstallExecutable = "uninstall.exe";
        private const string LogFile = "install.log";
        private const string InstallerLocationFile = "installerLocation.txt";

        private static readonly HttpClient client = new();
        private static bool silent = false;
        private static readonly string InstallPath = IsRunningAsAdmin()
            ? "C:\\Program Files\\Run"
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        public static async Task Main( string[] args ) {     

            silent = args.Contains("--silent") || args.Contains("/S") || args.Contains("/silent");
            if( args.Contains("--uninstall") ) {

                if ( IsRunningAsAdmin() ) {
                    Uninstall(true);
                    return;
                }
                Uninstall(false);
                return;
            }
            

            if ( !IsRunningAsAdmin() && InstallPath.StartsWith("C:\\Program Files") ) {
                Log("Requesting administrative privileges...");
                RestartAsAdmin(args);
                return;
            }

            string url = "https://github.com/Kisetsu15/Run/releases/latest/download/run.exe";
            string uninstallUrl = "https://github.com/Kisetsu15/Run/releases/latest/download/uninstall.exe";
            string runDestination = Path.Combine(Path.GetTempPath(), Executable);
            string uninstallDestination = Path.Combine(Path.GetTempPath(), UninstallExecutable);

            Log($"Downloading '{Executable}' from '{url}'...");
            await DownloadFileAsync(url, runDestination);
            if ( !File.Exists(runDestination) ) {
                Log("Download failed: Run.exe not found.");
                return;
            }
            Log("Downloading 'uninstall.exe'...");
            await DownloadFileAsync(uninstallUrl, uninstallDestination);
            Log("Downloads complete.");
            File.WriteAllText(InstallerLocationFile, Directory.GetCurrentDirectory());
            try {
                Log($"Creating installation directory at {InstallPath}...");
                Directory.CreateDirectory(InstallPath);

                string runDestFile = Path.Combine(InstallPath, Executable);
                string unistallDestFile = Path.Combine(InstallPath, UninstallExecutable);

                Log($"Moving Files to '{InstallPath}'...");
                Move(runDestination, runDestFile);
                Move(uninstallDestination, unistallDestFile);
                Move(InstallerLocationFile, Path.Combine(InstallPath, InstallerLocationFile));
                Log("Move complete.");
                AddToPath(InstallPath);
                Log("Installation complete.");

                static void Move( string source, string destination ) {
                    try {
                        if ( File.Exists(destination) ) {   
                            Log($"'{Path.GetFileName(destination)}' already exists in '{InstallPath}'. Overwriting...");
                            File.Delete(destination);
                        }
                        File.Move(source, destination);
                    } catch ( Exception e ) {
                        Log($"fatal: {e.Message}");
                    }
                }

            } catch ( Exception ex ) {
                Log($"fatal: {ex.Message}");
            }
        }

        private static void Uninstall( bool runas ) {
            try {
                string uninstallPath = Path.Combine(InstallPath, UninstallExecutable);
                if ( !File.Exists(uninstallPath) ) {
                    Log("Uninstall executable not found.");
                    return;
                }

                ProcessStartInfo startInfo = new() {
                    FileName = uninstallPath,
                    Arguments = silent ? "--silent" : "",
                    Verb = runas ? "runas" : "",
                    UseShellExecute = true
                };
                Process.Start(startInfo);

            } catch ( Exception e ) {
                Log($"fatal: {e.Message}");
            }
        }

        private static void RestartAsAdmin( string[] args ) {
            try {
                var startInfo = new ProcessStartInfo {
                    FileName = Environment.ProcessPath,
                    Arguments = string.Join(" ", args),
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process elevatedProcess = Process.Start(startInfo)!;
                elevatedProcess?.WaitForExit();
                Environment.Exit(0);
            } catch ( Exception e ) {
                Log($"fatal: Unable to elevate privileges: {e.Message}");
            }
        }

        private static void AddToPath( string newPath ) {
            try {
                EnvironmentVariableTarget target = IsRunningAsAdmin() ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
                string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";

                if ( currentPath.Split(';').Contains(newPath) ) {
                    Log($"'{newPath}' is already in PATH.");
                    return;
                }

                string updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable("PATH", updatedPath, target);
                Log($"Successfully added '{newPath}' to PATH.");

                RefreshEnvironment();
            } catch ( Exception ex ) {
                Log($"Failed to update PATH\n fatal:{ex.Message}");
            }
        }

        static bool IsRunningAsAdmin() {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RefreshEnvironment() {
            try {
                Log("Refreshing environment variables...");
                const int HWND_BROADCAST = 0xFFFF;
                const int WM_SETTINGCHANGE = 0x1A;
                const int SMTO_ABORTIFHUNG = 0x0002;

                SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 5000, out nint result);
            } catch ( Exception ex ) {
                Log($"Failed to refresh environment variables: {ex.Message}");
            }
        }
        static async Task DownloadFileAsync( string url, string destination ) {
            try {
                using HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                await using FileStream fs = new(destination, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
            } catch ( Exception e ) {
                Log($"Error: {e.Message}");
            }
        }

        private static void Log( string message ) {
            if ( !silent )
                Console.WriteLine(message);

            File.AppendAllText(LogFile, message + Environment.NewLine);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout( IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult );
    }
}

