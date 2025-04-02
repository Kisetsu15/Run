namespace Run {
    public class AddCommand : ICommand {
        public void Execute( string[] args ) {
            if ( Utils.Check(args, 3) )
                return;

            string name = args[1];
            string operation = args[2];

            if ( !File.Exists(operation) && !Utils.IsWebUrl(operation) ) {
                Console.WriteLine($"File '{operation}' does not exist and is not a valid URL.");
                return;
            }

            if ( !File.Exists(Utils.CommandFile) )
                Utils.SaveJson(Utils.CommandFile, []);

            var entries = Utils.LoadJson(Utils.CommandFile);
            bool exists = entries.TryGetValue(name, out string? old);
            entries[name] = operation;
            Utils.SaveJson(Utils.CommandFile, entries);

            if ( exists ) {
                Console.WriteLine($"Command '{name}' overwritten. '{old}' -> '{operation}'");
            } else {
                Console.WriteLine($"Command '{name}' added.");
            }
        }
    }
}