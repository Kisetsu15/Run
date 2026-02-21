using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Installer {
    public class Installer {

        const string EXECUTABLE = "run.exe";
        const string UNINSTALL_EXECUTABLE = "uninstall.exe";
        const string LOG_FILE = "install.log";
        const string INSTALLER_LOCATION_FILE = "installerLocation.txt";
        const string EXECUTABLE_URL = "https://github.com/Kisetsu15/Run/releases/latest/download/run.exe";
        const string UNINSTALL_URL = "https://github.com/Kisetsu15/Run/releases/latest/download/uninstall.exe";
        static readonly HttpClient client = new();
        static bool isSilent = false;
        static readonly string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Run");

        public static async Task Main(string[] args) {
            isSilent = args.Contains("--silent") || args.Contains("/S") || args.Contains("/silent");

            if (args.Contains("--uninstall")) {
                Uninstall();
                return;
            }

            string runDestination = Path.Combine(Path.GetTempPath(), EXECUTABLE);
            string uninstallDestination = Path.Combine(Path.GetTempPath(), UNINSTALL_EXECUTABLE);

            Log($"Downloading '{EXECUTABLE}' from '{EXECUTABLE_URL}'...");
            if (!await DownloadFileWithRetryAsync(EXECUTABLE_URL, runDestination)) {
                Log("Download failed: Run.exe not found.");
                return;
            }

            Log("Downloading 'uninstall.exe'...");
            if (!await DownloadFileWithRetryAsync(UNINSTALL_URL, uninstallDestination)) {
                Log("Download failed: Uninstall.exe not found.");
                return;
            }

            Log("Downloads complete.");
            File.WriteAllText(INSTALLER_LOCATION_FILE, Directory.GetCurrentDirectory());
            try {
                Log($"Creating installation directory at {installPath}...");
                Directory.CreateDirectory(installPath);
                Log("Moving files to install directory...");
                MoveFile(runDestination, Path.Combine(installPath, EXECUTABLE));
                MoveFile(uninstallDestination, Path.Combine(installPath, UNINSTALL_EXECUTABLE));
                MoveFile(INSTALLER_LOCATION_FILE, Path.Combine(installPath, INSTALLER_LOCATION_FILE));
                Log("Move complete.");
                AddToPath(installPath);
                Log("Installation complete.");
            } catch (Exception ex) {
                Log($"Fatal error: {ex.Message}");
            }
        }

        static void Uninstall() {
            try {
                string uninstallPath = Path.Combine(installPath, UNINSTALL_EXECUTABLE);
                if (!File.Exists(uninstallPath)) {
                    Log("Uninstall executable not found.");
                    return;
                }

                var startInfo = new ProcessStartInfo {
                    FileName = uninstallPath,
                    Arguments = isSilent ? "--silent" : "",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            } catch (Exception e) {
                Log($"Fatal error during uninstallation: {e.Message}");
            }
        }

        static async Task<bool> DownloadFileWithRetryAsync(string url, string destination, int maxRetries = 3) {
            for (int attempt = 1; attempt <= maxRetries; attempt++) {
                try {
                    using HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fs);
                    return true;
                } catch (Exception e) {
                    Log($"Attempt {attempt}: Download failed - {e.Message}");
                    if (attempt == maxRetries) {
                        return false;
                    }
                }
            }
            return false;
        }

        static void MoveFile(string source, string destination) {
            try {
                if (File.Exists(destination)) {
                    Log($"'{Path.GetFileName(destination)}' already exists in '{installPath}'. Overwriting...");
                    File.Delete(destination);
                }
                File.Move(source, destination);
            } catch (Exception e) {
                Log($"Fatal error while moving file: {e.Message}");
            }
        }

        static void AddToPath(string newPath) {
            try {
                EnvironmentVariableTarget target = EnvironmentVariableTarget.User;
                string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? "";

                if (currentPath.Split(';').Contains(newPath)) {
                    Log($"'{newPath}' is already in PATH.");
                    return;
                }

                string updatedPath = currentPath + ";" + newPath;
                Environment.SetEnvironmentVariable("PATH", updatedPath, target);
                Log($"Successfully added '{newPath}' to PATH.");
                RefreshEnvironment();
            } catch (Exception ex) {
                Log($"Failed to update PATH\nFatal error: {ex.Message}");
            }
        }

        static void RefreshEnvironment() {
            try {
                Log("Refreshing environment variables...");
                const int hWND_BROADCAST = 0xFFFF;
                const int wM_SETTINGCHANGE = 0x1A;
                const int sMTO_ABORTIFHUNG = 0x0002;

                SendMessageTimeout((IntPtr) hWND_BROADCAST, wM_SETTINGCHANGE, IntPtr.Zero, "Environment", sMTO_ABORTIFHUNG, 5000, out _);
            } catch (Exception ex) {
                Log($"Failed to refresh environment variables: {ex.Message}");
            }
        }

        static void Log(string message) {
            if (!isSilent) {
                Console.WriteLine(message);
            }

            File.AppendAllText(LOG_FILE, message + Environment.NewLine);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
    }
}
