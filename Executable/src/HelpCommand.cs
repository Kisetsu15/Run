namespace Executable {
    public class HelpCommand : ICommand {
        public void Execute(string[] args) {
            Console.WriteLine($"\nUsage: {Commands.RUN} [command] [option]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine(" <name> [args]               Run the command with the given name.");
            Console.WriteLine(" add <name> <operation>      Add a new command with the given name and operation.");
            Console.WriteLine(" rm <name> | remove <name>   Remove the command with the given name.");
            Console.WriteLine(" import <file>               Import commands from specified file");
            Console.WriteLine(" export <file>               Export commands to specified file");
            Console.WriteLine(" clear                       Clear all commands.");
            Console.WriteLine(" -l list                     List all commands.");
            Console.WriteLine(" -v --version                Display run version.");
            Console.WriteLine(" -h --help                   Display this help message.\n");
        }
    }
}