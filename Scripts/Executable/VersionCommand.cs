namespace Run.Scripts.Executable {
    public class VersionCommand : ICommand {
        private const string VERSION = "Run v1.0.0";
        public void Execute(string[] args) {
            if (Utils.Check(args, 1)) {
                return;
            }

            Console.WriteLine(VERSION);
        }
    }
}