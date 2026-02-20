using System.Diagnostics;

namespace Run.Scripts.Executable {
    public class RunCommand : ICommand {
        public void Execute(string[] args) {

            string name = args[0];
            args = args[1..];

            if (string.IsNullOrWhiteSpace(Utils.CommandFile) || !File.Exists(Utils.CommandFile)) {
                Console.WriteLine(Utils.NO_COMMANDS);
                return;
            }

            Dictionary<string, string> entries = Utils.LoadJson(Utils.CommandFile) ?? [];
            if (entries.Count == 0) {
                Console.WriteLine(Utils.NO_COMMANDS);
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

            if (!Utils.SupportsArguments(value) && args.Length > 0) {
                Console.WriteLine("fatal: Arguments are not supported for this command.");
                return;
            }

            try {
                var startInfo = new ProcessStartInfo {
                    FileName = value,
                    Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
                    UseShellExecute = Path.GetExtension(value) is not ".exe"
                };

                if (Utils.IsWebUrl(value)) {
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
    }
}