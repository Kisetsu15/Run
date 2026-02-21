namespace Executable {
    public class ClearCommand : ICommand {
        public void Execute(string[] args) {
            if (Utils.Check(args, 1)) {
                return;
            }

            string choice = Terminal.Input(ConsoleColor.Yellow, "Are you sure you want to clear all commands? (y/n): ");

            if (!choice.Equals("y", StringComparison.CurrentCultureIgnoreCase)) {
                Terminal.WriteLine("Clear command cancelled.", ConsoleColor.Red);
                return;
            }

            Utils.SaveJson(Utils.CommandFile, []);
            Terminal.WriteLine("All commands cleared.", ConsoleColor.Yellow);
        }
    }
}