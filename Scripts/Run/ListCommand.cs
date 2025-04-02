namespace Run {
    public class ListCommand : ICommand {
        public void Execute( string[] args ) {
            if ( Utils.Check(args, 1) )
                return;

            if ( !File.Exists(Utils.CommandFile) ) {
                Console.WriteLine(Utils.NoCommands);
                return;
            }

            var commands = Utils.LoadJson(Utils.CommandFile);
            commands = commands.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if ( commands.Count == 0 ) {
                Console.WriteLine(Utils.NoCommands);
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