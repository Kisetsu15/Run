namespace Run {
    public class Master {
        public static void Main( string[] args ) {
            if ( args.Length == 0 ) {
                HelpCommand();
                return;
            }

            switch ( args[0] ) {
                case Commands.add:
                    CheckAndExecute(3, () => AddCommand(args[1], args[2]));
                    break;
                case Commands.remove:
                    CheckAndExecute(2, () => RemoveCommand(args[1]));
                    break;
                case Commands.list:
                    CheckAndExecute(1, ListCommand);
                    break;
                case Commands.clear:
                    CheckAndExecute(1, ClearCommand);
                    break;
                case Commands.help:
                    CheckAndExecute(1, HelpCommand);
                    break;
                case Commands.administrator:
                    RunAsAdministratorCommand(args);
                    break;
                default:
                    RunCommand(args);
                    return;
            }

            void CheckAndExecute( int expectedLength, Action action ) {
                if ( args.Length != expectedLength ) {
                    Console.WriteLine($"Invalid arguments. Use '{Commands.run} {Commands.help}' for help.");
                    return;
                }
                action.Invoke();
            }
        }

        private static void RunCommand( string[] args ) {
            if ( args[0] == Commands.shell ) {
                if ( args.Length < 2 ) {
                    Console.WriteLine($"Invalid arguments. Use '{Commands.run} {Commands.help}' for help.");
                    return;
                }
                Utils.Execute(args[1], [.. args.Skip(2)], true);
                return;
            }

            Utils.Execute(args[0], [.. args.Skip(1)]);
        }

        private static void RunAsAdministratorCommand( string[] args ) {
            if ( args.Length < 2 ) {
                Console.WriteLine($"Invalid arguments. Use '{Commands.run} {Commands.help}' for help.");
                return;
            }

            if ( args[1] == Commands.shell ) {
                if ( args.Length < 3 ) {
                    Console.WriteLine($"Invalid arguments. Use '{Commands.run} {Commands.help}' for help.");
                    return;
                }
                Utils.Execute(args[2], [.. args.Skip(3)], true, true);
                return;
            }

            Utils.Execute(args[1], [.. args.Skip(2)],false ,true);
        }

        private static void HelpCommand() {
            Console.WriteLine($"\nUsage: {Commands.run} [command] [option]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine(" <name> [args]           Run the command with the given name.");
            Console.WriteLine(" -a <name> [args]        Run the command with the given name as an administrator.");
            Console.WriteLine(" add <name> <operation>  Add a new command with the given name and operation.");
            Console.WriteLine(" rm <name>               Remove the command with the given name.");
            Console.WriteLine(" -l                      List all commands.");
            Console.WriteLine(" clear                   Clear all commands.");
            Console.WriteLine(" -v                      Display version.");
            Console.WriteLine(" -h                      Display this help message.");
            Console.WriteLine("\nOptions:");
            Console.WriteLine(" -sh                     Run the command in a shell.");
        }

        private static void ClearCommand() {
            Utils.SaveJson(Utils.CommandFile, []);
            Console.WriteLine("All commands cleared.");
        }

        private static void AddCommand( string name, string operation ) {
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

        private static void RemoveCommand( string name ) {
            var entries = Utils.LoadJson(Utils.CommandFile);
            if ( !entries.TryGetValue(name, out _) ) {
                Console.WriteLine($"Command '{name}' does not exist.");
                return;
            }
            entries.Remove(name);
            Utils.SaveJson(Utils.CommandFile, entries);
            Console.WriteLine($"Command '{name}' removed.");
        }

        private static void ListCommand() {

            if ( !File.Exists(Utils.CommandFile) ) {
                Console.WriteLine(Utils.noCommands);
                return;
            }

            var commands = Utils.LoadJson(Utils.CommandFile);
            commands = commands.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if ( commands.Count == 0 ) {
                Console.WriteLine(Utils.noCommands);
                return;
            }

            const string commandTitle = "Command";
            const string operationTitle = "Operation";

            int maxKey = Math.Max(commandTitle.Length, commands.Keys.Max(k => k.Length));
            int maxValue = Math.Max(operationTitle.Length, commands.Values.Max(v => v.Length));

            string line = $"+{new string('-', maxKey + 2)}+{new string('-', maxValue + 2)}+";

            string centeredCommandTitle = commandTitle.PadLeft(( maxKey + commandTitle.Length ) / 2).PadRight(maxKey);
            string centeredOperationTitle = operationTitle.PadLeft(( maxValue + operationTitle.Length ) / 2).PadRight(maxValue);

            Console.WriteLine(line);
            Console.WriteLine($"| {centeredCommandTitle} | {centeredOperationTitle} |");
            Console.WriteLine(line);

            foreach ( var command in commands ) {
                Console.WriteLine($"| {command.Key.PadRight(maxKey)} | {command.Value.PadRight(maxValue)} |");
            }
            Console.WriteLine(line);
        }
    }
}
