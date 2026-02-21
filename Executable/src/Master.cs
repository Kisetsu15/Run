namespace Executable {
    public class Master {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                new HelpCommand().Execute(args);
                return;
            }

            string command = args[0];

            if (Commands.commands.TryGetValue(command, out ICommand? commandInstance)) {
                commandInstance.Execute(args);
            } else {
                new RunCommand().Execute(args);
            }
        }
    }
}