using Newtonsoft.Json;
using System.Diagnostics;

namespace Run {
    public class Utils {
        public static string CommandFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Commands.run, fileName);
        public const string noCommands = "warning: No commands found.";
        private const string fileName = "commands.json";

        public static Dictionary<string, string> LoadJson( string path ) {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path)) ?? [] : [];
        }

        public static void SaveJson( string path, Dictionary<string, string> data ) {
            if ( !Directory.Exists(Path.GetDirectoryName(path)!) ) {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        public static bool IsWebUrl( string url ) {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                   && ( uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps );
        }

        public static void Execute( string name, string[] args, bool shell = false, bool runas = false ) {
            if ( string.IsNullOrWhiteSpace(CommandFile) || !File.Exists(CommandFile) ) {
                Console.WriteLine(noCommands);
                return;
            }

            var entries = Utils.LoadJson(CommandFile) ?? [];
            if ( entries.Count == 0 ) {
                Console.WriteLine(noCommands);
                return;
            }

            if ( !entries.TryGetValue(name, out string? value) || string.IsNullOrWhiteSpace(value) ) {
                if ( Commands.Strings.Contains(name) ) {
                    Console.WriteLine($"fatal: Command '{name}' is a built-in command. See '{Commands.run} {Commands.help}' or '{Commands.run} {Commands.list}'.");
                    return;
                }
                Console.WriteLine($"fatal: Command '{name}' does not exist. See '{Commands.run} {Commands.help}' or '{Commands.run} {Commands.list}'.");
                return;
            }

            if ( shell && !SupportsArguments(value) && args.Length > 0 ) {
                Console.WriteLine("fatal: Arguments are not supported for this command.");
                return;
            }

            try {
                var startInfo = new ProcessStartInfo {
                    FileName = value,
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = shell || ( Path.GetExtension(value) is not ".exe" )
                };

                if ( IsWebUrl(value) ) {
                    startInfo.UseShellExecute = true;
                    Console.WriteLine($"Opening site '{value}'...");
                } else if ( runas ) {
                    if ( Path.GetExtension(value) != ".exe" ) {
                        Console.WriteLine("fatal: Cannot run a non-executable file as an administrator.");
                        return;
                    }
                    startInfo.Verb = "runas";
                    Console.WriteLine($"Running '{name}' as administrator...");
                } else {
                    Console.WriteLine($"Running '{name}'...");
                }

                using var process = Process.Start(startInfo);
                if ( process == null ) {
                    Console.WriteLine($"fatal: Failed to start process '{name}'.");
                }
            } catch ( Exception e ) {
                Console.WriteLine($"fatal: {e.Message}");
            }
        }

        private static bool SupportsArguments( string exePath ) {
            string[] testArgs = ["--help", "-h", "/?", "--version"];

            foreach ( string arg in testArgs ) {
                ProcessStartInfo psi = new() {
                    FileName = exePath,
                    Arguments = arg,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new() { StartInfo = psi };

                try {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if ( !string.IsNullOrWhiteSpace(output) || !string.IsNullOrWhiteSpace(error) ) {
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
