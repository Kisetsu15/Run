namespace Executable {
    public class RemoveCommand : ICommand {
        public void Execute(string[] args) {
            if (Utils.Check(args, 2)) {
                return;
            }

            string name = args[1];
            Dictionary<string, string> entries = Utils.LoadJson(Utils.CommandFile);

            if (!entries.TryGetValue(name, out _)) {
                Terminal.WriteLine($"warning: Command '{name}' does not exist.", ConsoleColor.Yellow);
                return;
            }

            entries.Remove(name);
            Utils.SaveJson(Utils.CommandFile, entries);
            Terminal.WriteLine($"Command '{name}' removed.", ConsoleColor.Green);
        }
    }
}