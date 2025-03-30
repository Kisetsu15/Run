namespace Run {
    public static class Commands {
        public const string run = "run";
        public const string add = "add";
        public const string remove = "rm";
        public const string list = "-l";
        public const string clear = "clear";
        public const string help = "-h";
        public const string administrator = "-a";
        public const string shell = "-sh";
        public const string version = "-v";
        public static string[] Strings { get; private set; } = [run, add, remove, list, clear, help, administrator, shell];
    }
}
