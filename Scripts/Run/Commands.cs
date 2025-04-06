namespace Run {
    public static class Commands {
        public const string run = "run";
        public const string add = "add";
        public const string _remove = "rm";
        public const string remove = "remove";
        public const string _list = "-l";
        public const string list = "list";
        public const string clear = "clear";
        public const string _help = "-h";
        public const string help = "--help";
        public const string _version = "-v";
        public const string version = "--version";

        public static readonly Dictionary<string, ICommand> commands = new(StringComparer.OrdinalIgnoreCase) {
            { add, new AddCommand() },
            { remove, new RemoveCommand() },
            { list, new ListCommand() },
            { clear, new ClearCommand() },
            { help, new HelpCommand() },
            { version, new VersionCommand() },
            { _remove, new RemoveCommand() },
            { _list, new ListCommand() },
            { _help, new HelpCommand() },
            { _version, new VersionCommand() }
        };
    }
}