namespace Executable {
    public class ListCommand : ICommand {
        public void Execute(string[] args) {
            if (Utils.Check(args, 1)) {
                return;
            }

            if (!File.Exists(Utils.CommandFile)) {
                Console.WriteLine(Utils.NO_COMMANDS);
                return;
            }

            Dictionary<string, string> commands = Utils.LoadJson(Utils.CommandFile);
            commands = commands.OrderBy(pair => pair.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (commands.Count == 0) {
                Console.WriteLine(Utils.NO_COMMANDS);
                return;
            }

            const string commandTitle = "Command";
            const string operationTitle = "Operation";

            int longestKey = Math.Max(commandTitle.Length, commands.Keys.Max(k => k.Length));
            int longestValue = Math.Max(operationTitle.Length, commands.Values.Max(v => v.Length));

            string line = $"+{new string('-', longestKey + 2)}+{new string('-', longestValue + 2)}+";

            string centeredCommandTitle = commandTitle.PadLeft((longestKey + commandTitle.Length) / 2).PadRight(longestKey);
            string centeredOperationTitle = operationTitle.PadLeft((longestValue + operationTitle.Length) / 2).PadRight(longestValue);

            Console.WriteLine(line);
            Console.Write($"| ");
            Terminal.Write(centeredCommandTitle, ConsoleColor.Yellow);
            Console.Write($" | ");
            Terminal.Write(centeredOperationTitle, ConsoleColor.Yellow);
            Console.WriteLine($" |");
            Console.WriteLine(line);

            foreach (KeyValuePair<string, string> command in commands) {
                Console.Write($"| ");
                Terminal.Write(command.Key.PadRight(longestKey), ConsoleColor.Cyan);
                Console.WriteLine($" | {command.Value.PadRight(longestValue)} |");
            }

            Console.WriteLine(line);
        }
    }
}