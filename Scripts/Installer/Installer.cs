using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Run_Installer {
    public class Installer {
        private const string Executable = "run.exe";
        private static readonly HttpClient client = new();
        private static bool silent = false;
        private static readonly string InstallPath = IsRunningAsAdmin()
            ? "C:\\Program Files\\Run"
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        public static async Task Main( string[] args ) {
            silent = args.Contains("--silent") || args.Contains("/S") || args.Contains("/silent");

            if ( !IsRunningAsAdmin() && InstallPath.StartsWith("C:\\Program Files") ) {
                Log("Requesting administrative privileges...");
                RestartAsAdmin(args);
                return;
            }

            string url = "https://github.com/Kisetsu15/Run/releases/latest/download/run.exe";
            string destination = Path.Combine(Path.GetTempPath(), Executable);

            Log($"Downloading '{Executable}' from '{url}'...");
            await DownloadFileAsync(url, destination);
            Log("Download complete.");

            try {
                Log($"Creating installation directory at {InstallPath}...");
                Directory.CreateDirectory(InstallPath);

                string destFile = Path.Combine(InstallPath, Executable);
                if ( File.Exists(destFile) ) {
                    Log($"'{Executable}' already exists in '{InstallPath}'. Overwriting...");
                    File.Delete(destFile);
                }

                Log($"Copying '{Executable}' to '{InstallPath}'...");
                File.Copy(destination, destFile);
                Log("Copy complete.");

                AddToPath(InstallPath);
                Log("Installation complete.");
            } catch ( Exception ex ) {
                Log($"fatal: {ex.Message}");
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
                Process.Start(startInfo);
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

            File.AppendAllText("install.log", message + Environment.NewLine);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout( IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult );
    }
}

