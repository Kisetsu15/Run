# Run - Command Execution Utility

## Overview

Run is a simple yet powerful command execution utility that allows you to define and manage custom commands for executing files, scripts, or URLs. It provides support for running commands as an administrator, managing command entries, and executing shell commands with ease.

## Features

- Run predefined commands with arguments.
- Add and remove custom command entries.
- List and clear all saved commands.
- Execute commands in a shell.
- Run commands as an administrator.

## Installation

To install, download and execute the installer. Run the installer as an administrator to make it globally accessible.

## Usage

Run commands using the following syntax:

```
run [command] [option]
```

### Commands:

| Command                  | Description                                          |
| ------------------------ | ---------------------------------------------------- |
| `<name> [args]`          | Run the command with the given name.                 |
| `-a <name> [args]`       | Run the command as an administrator.                 |
| `add <name> <operation>` | Add a new command with the given name and operation. |
| `rm <name>`              | Remove the command with the given name.              |
| `-l`                     | List all saved commands.                             |
| `clear`                  | Clear all stored commands.                           |
| `-v`                     | Display version information.                         |
| `-h`                     | Display help message.                                |

### Options:

| Option | Description                 |
| ------ | --------------------------- |
| `-sh`  | Run the command in a shell. |

## Examples

1. **Adding a command**

   ```sh
   run add myscript "C:\Scripts\myscript.bat"
   ```

2. **Running a command**

   ```sh
   run myscript
   ```

3. **Running a command in a shell**

   ```sh
   run -sh myscript
   ```

4. **Running as Administrator**

   ```sh
   run -a myscript
   ```

5. **Listing all commands**

   ```sh
   run -l
   ```

6. **Removing a command**

   ```sh
   run rm myscript
   ```

7. **Clearing all commands**

   ```sh
   run clear
   ```

## License

This project is licensed under the MIT License.

## Contributing

Feel free to submit issues and pull requests for improvements.

