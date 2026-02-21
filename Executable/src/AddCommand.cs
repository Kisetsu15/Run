namespace Executable {
    public class AddCommand : ICommand {
        public void Execute(string[] args) {
            if (Utils.Check(args, 3)) {
                return;
            }

            string name = args[1], operation = args[2];

            if (!File.Exists(operation) && !Utils.IsWebUrl(operation)) {
                Terminal.WriteLine($"File '{operation}' does not exist and is not a valid URL.", ConsoleColor.Red);
                return;
            }

            if (!File.Exists(Utils.CommandFile)) {
                Utils.SaveJson(Utils.CommandFile, []);
            }

            Dictionary<string, string> entries = Utils.LoadJson(Utils.CommandFile);
            bool exists = entries.TryGetValue(name, out string? old);
            entries[name] = operation;
            Utils.SaveJson(Utils.CommandFile, entries);

            if (exists) {
                Terminal.Write($"Command '{name}' overwritten.");
                Terminal.Write($"'{old}'", ConsoleColor.Red);
                Terminal.Write("->");
                Terminal.WriteLine($"'{operation}'", ConsoleColor.Green);
            } else {
                Terminal.WriteLine($"Command '{name}' added.", ConsoleColor.Green);
            }
        }
    }
}