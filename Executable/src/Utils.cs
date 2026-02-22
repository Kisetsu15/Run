namespace Executable {
    public class Utils {

        public const string NO_COMMANDS_WARNING = "No commands found.";
        const string FILE_NAME = "commands.json";
        public static string CommandFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Commands.RUN, FILE_NAME);


        public static Dictionary<string, string> LoadJson(string path) {
            return Json.Load<string>(path);
        }

        public static void SaveJson(string path, Dictionary<string, string> data) {
            string? dir = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dir)) {
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            Json.Save(path, data);
        }

        public static bool IsWebUrl(string url) {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool Check(string[] args, int expectedLength) {
            if (args.Length != expectedLength) {
                Error($"Invalid arguments. Use '{Commands.RUN} {Commands.HELP}' for help.");
                return true;
            }
            return false;
        }

        public static void Warning(string message) {
            Terminal.WriteLine("warning: " + message, ConsoleColor.Yellow);
        }

        public static void Error(string message) {
            Terminal.WriteLine("fatal: " + message, ConsoleColor.Red);
        }

        public static bool SupportsArguments(string exePath) {
            string[] testArgs = ["--help", "-h", "/?", "--version"];

            foreach (string arg in testArgs) {
                ProcessStartInfo psi = new() {
                    FileName = exePath,
                    Arguments = arg,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                using Process process = new() { StartInfo = psi };

                try {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output) || !string.IsNullOrWhiteSpace(error)) {
                        return true;
                    }
                } catch {
                    return false;
                }
            }
            return false;
        }
    }
}