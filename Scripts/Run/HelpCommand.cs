namespace Run {
    public class HelpCommand : ICommand {
        public void Execute( string[] args ) {
            if ( Utils.Check(args, 1) )
                return;

            Console.WriteLine($"\nUsage: {Commands.run} [command] [option]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine(" <name> [args]               Run the command with the given name.");
            Console.WriteLine(" add <name> <operation>      Add a new command with the given name and operation.");
            Console.WriteLine(" rm <name> | remove <name>   Remove the command with the given name.");
            Console.WriteLine(" clear                       Clear all commands.");
            Console.WriteLine(" -l list                     List all commands.");
            Console.WriteLine(" -v --version                Display run version.");
            Console.WriteLine(" -h --help                   Display this help message.");
        }
    }
}