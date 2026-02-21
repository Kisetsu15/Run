namespace Executable {
    public class ExportCommand : ICommand {
        public void Execute(string[] args) {
            foreach (string arg in args) {
                Terminal.Log($"Argument: {arg}");
            }

            if (Utils.Check(args, 2)) {
                return;
            }
            string filePath = args[1];
            Dictionary<string, string> entries = Utils.LoadJson(Utils.CommandFile);

            try {
                Utils.SaveJson(filePath, entries);
                Terminal.WriteLine($"Commands exported to '{filePath}'.", ConsoleColor.Green);
            } catch (Exception e) {
                Utils.Error($"Failed to export commands: {e.Message}");
            }
        }
    }
}