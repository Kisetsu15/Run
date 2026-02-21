namespace Executable {
    public class VersionCommand : ICommand {
        const string VERSION = "Run v1.0.1";

        public void Execute(string[] args) {
            if (Utils.Check(args, 1)) {
                return;
            }

            Console.WriteLine(VERSION);
        }
    }
}