namespace Executable {
    public class ImportCommand : ICommand {
        public void Execute(string[] args) {
            if (Utils.Check(args, 2)) {
                return;
            }
            string filePath = args[1];
            if (!File.Exists(filePath)) {
                Utils.Error($"File '{filePath}' does not exist.");
                return;
            }
            try {
                Dictionary<string, string> importedEntries = Utils.LoadJson(filePath);
                Dictionary<string, string> existingEntries = Utils.LoadJson(Utils.CommandFile);

                foreach (KeyValuePair<string, string> entry in importedEntries) {
                    existingEntries[entry.Key] = entry.Value;
                }

                Utils.SaveJson(Utils.CommandFile, existingEntries);
                Terminal.WriteLine($"Commands imported from '{filePath}'.", ConsoleColor.Green);
            } catch (Exception e) {
                Utils.Error($"Failed to import commands: {e.Message}");
            }
        }
    }
}