using Newtonsoft.Json;
using System.Diagnostics;

namespace Run.Scripts.Executable {
    public class Utils {

        public const string NO_COMMANDS = "warning: No commands found.";
        private const string FILE_NAME = "commands.json";
        public static string CommandFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Commands.RUN, FILE_NAME);


        public static Dictionary<string, string> LoadJson(string path) {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path)) ?? [] : [];
        }

        public static void SaveJson(string path, Dictionary<string, string> data) {
            if (!Directory.Exists(Path.GetDirectoryName(path)!)) {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static bool IsWebUrl(string url) {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static void Execute(string name, string[] args) {
            if (string.IsNullOrWhiteSpace(CommandFile) || !File.Exists(CommandFile)) {
                Console.WriteLine(NO_COMMANDS);
                return;
            }

            Dictionary<string, string> entries = LoadJson(CommandFile) ?? [];
            if (entries.Count == 0) {
                Console.WriteLine(NO_COMMANDS);
                return;
            }

            if (!entries.TryGetValue(name, out string? value) || string.IsNullOrWhiteSpace(value)) {
                if (Commands.commands.TryGetValue(name, out _)) {
                    Console.WriteLine($"fatal: Command '{name}' is a built-in command. See '{Commands.RUN} {Commands.HELP}' or '{Commands.RUN} {Commands.LIST}'.");
                    return;
                }
                Console.WriteLine($"fatal: Command '{name}' does not exist. See '{Commands.RUN} {Commands.HELP}' or '{Commands.RUN} {Commands.LIST}'.");
                return;
            }

            if (!SupportsArguments(value) && args.Length > 0) {
                Console.WriteLine("fatal: Arguments are not supported for this command.");
                return;
            }

            try {
                var startInfo = new ProcessStartInfo {
                    FileName = value,
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = Path.GetExtension(value) is not ".exe"
                };

                if (IsWebUrl(value)) {
                    startInfo.UseShellExecute = true;
                    Console.WriteLine($"Opening site '{value}'...");
                }

                using var process = Process.Start(startInfo);
                if (process == null) {
                    Console.WriteLine($"fatal: Failed to start process '{name}'.");
                }
                Console.WriteLine($"Running '{name}'...");
            } catch (Exception e) {
                Console.WriteLine($"fatal: {e.Message}");
            }
        }

        public static bool Check(string[] args, int expectedLength) {
            if (args.Length != expectedLength) {
                Console.WriteLine($"Invalid arguments. Use '{Commands.RUN} {Commands.HELP}' for help.");
                return true;
            }
            return false;
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