namespace Run {
    public class ClearCommand : ICommand {
        public void Execute( string[] args ) {
            if ( Utils.Check(args, 1) )
                return;

            Utils.SaveJson(Utils.CommandFile, []);
            Console.WriteLine("All commands cleared.");
        }
    }
}