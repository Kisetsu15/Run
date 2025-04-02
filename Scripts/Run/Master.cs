namespace Run {
    public class Master {
        public static void Main( string[] args ) {
            if ( args.Length == 0 ) {
                new HelpCommand().Execute(args);
                return;
            }

            if ( Commands.commands.TryGetValue(args[0], out ICommand? command) ) {
                command.Execute(args);
            } else {
                new RunCommand().Execute(args);
            }
        }
    }
}