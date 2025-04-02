# Run - Command Execution Utility

## Overview

Run is a simple yet powerful command execution utility that allows you to define and manage custom commands for executing files, scripts, or URLs. It provides support for managing command entries with ease.

## Features

- Run predefined commands with arguments.
- Add and remove custom command entries.
- List and clear all saved commands.

## Installation

To install, download and execute the installer. Run the installer to make it globally accessible.

## Usage

Run commands using the following syntax:

```
run [command] [option]
```

### Commands:

| Command                     | Description                                          |
| --------------------------- | ---------------------------------------------------- |
| `<name> [args]`             | Run the command with the given name.                 |
| `add <name> <operation>`    | Add a new command with the given name and operation. |
| `rm <name>` `remove <name>` | Remove the command with the given name.              |
| `-l` `list`                 | List all saved commands.                             |
| `clear`                     | Clear all stored commands.                           |
| `-v` `--version`            | Display version information.                         |
| `-h` `--help`               | Display help message.                                |


## Examples

1. **Adding a command**

   ```sh
   run add myscript "C:\Scripts\myscript.bat"
   ```
   ```sh
   run add youtube "https://www.youtube.com"
   ```
   ```sh
   run add vs "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
   ```

2. **Running a command**

   ```sh
   run myscript
   ```
   ```sh
   run youtube
   ```
   ```sh
   run vs
   ```

3. **Listing all commands**

   ```sh
   run -l
   ```

4. **Removing a command**

   ```sh
   run rm myscript
   ```

5. **Clearing all commands**

   ```sh
   run clear
   ```

## License

This project is licensed under the MIT License.

## Contributing

Feel free to submit issues and pull requests for improvements.

