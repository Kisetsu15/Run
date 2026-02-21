namespace Executable {
    public static class Commands {
        public const string RUN = "run";
        public const string ADD = "add";
        public const string REMOVE_SHORT = "rm";
        public const string REMOVE = "remove";
        public const string LIST_SHORT = "-l";
        public const string LIST = "list";
        public const string CLEAR = "clear";
        public const string HELP_SHORT = "-h";
        public const string HELP = "--help";
        public const string VERSION_SHORT = "-v";
        public const string VERSION = "--version";
        public const string IMPORT = "import";
        public const string EXPORT = "export";

        public static readonly Dictionary<string, ICommand> commands = new(StringComparer.OrdinalIgnoreCase) {
            { ADD, new AddCommand() },
            { REMOVE, new RemoveCommand() },
            { LIST, new ListCommand() },
            { CLEAR, new ClearCommand() },
            { HELP, new HelpCommand() },
            { VERSION, new VersionCommand() },
            { REMOVE_SHORT, new RemoveCommand() },
            { LIST_SHORT, new ListCommand() },
            { HELP_SHORT, new HelpCommand() },
            { VERSION_SHORT, new VersionCommand() },
            { IMPORT, new ImportCommand()  },
            { EXPORT, new ExportCommand() }
        };
    }
}