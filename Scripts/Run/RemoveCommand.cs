namespace Run {
    public class RemoveCommand : ICommand {
        public void Execute( string[] args ) {
            if ( Utils.Check(args, 2) )
                return;

            string name = args[1];
            var entries = Utils.LoadJson(Utils.CommandFile);
            if ( !entries.TryGetValue(name, out _) ) {
                Console.WriteLine($"Command '{name}' does not exist.");
                return;
            }
            entries.Remove(name);
            Utils.SaveJson(Utils.CommandFile, entries);
            Console.WriteLine($"Command '{name}' removed.");
        }
    }
}