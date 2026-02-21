namespace Executable {
    public class RunCommand : ICommand {
        public void Execute(string[] args) {

            string name = args[0];
            args = args[1..];

            if (string.IsNullOrWhiteSpace(Utils.CommandFile) || !File.Exists(Utils.CommandFile)) {
                Utils.Warning(Utils.NO_COMMANDS);
                return;
            }

            Dictionary<string, string> entries = Utils.LoadJson(Utils.CommandFile) ?? [];
            if (entries.Count == 0) {
                Utils.Warning(Utils.NO_COMMANDS);
                return;
            }

            if (!entries.TryGetValue(name, out string? value) || string.IsNullOrWhiteSpace(value)) {
                if (Commands.commands.TryGetValue(name, out _)) {
                    Utils.Error($"Command '{name}' is a built-in command. See '{Commands.RUN} {Commands.HELP}' or '{Commands.RUN} {Commands.LIST}'.");
                    return;
                }
                Utils.Error($"Command '{name}' does not exist. See '{Commands.RUN} {Commands.HELP}' or '{Commands.RUN} {Commands.LIST}'.");
                return;
            }

            if (!Utils.SupportsArguments(value) && args.Length > 0) {
                Utils.Error($"Arguments are not supported for this command.");
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
                } else {
                    if (!File.Exists(value) && !Uri.IsWellFormedUriString(value, UriKind.Absolute)) {
                        Utils.Error($"File not found '{value}'.");
                        string choice = Terminal.Input($"Do you want to remove the command '{name}'? (y/n): ").Trim().ToLower();

                        if (choice == "y" || choice == "yes") {
                            entries.Remove(name);
                            Utils.SaveJson(Utils.CommandFile, entries);
                            Console.WriteLine($"Command '{name}' removed.");
                        }

                        return;
                    }
                }

                using var process = Process.Start(startInfo);
                if (process == null) {
                    Utils.Error($"Failed to start process '{name}'.");
                }

                Terminal.WriteLine($"Running '{name}'...", ConsoleColor.Green);
            } catch (Exception e) {
                Utils.Error($"{e.Message}");
            }
        }
    }
}