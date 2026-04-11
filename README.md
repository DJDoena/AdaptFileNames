# AdaptFileNames

A .NET command-line utility for batch renaming audiobook and e-book files with a consistent naming convention.

## Overview

AdaptFileNames is a tool designed to help organize and standardize file names for audiobooks (MP3, M4A, MP4) and e-books (EPUB, MOBI) in your media library. It recursively processes folders, allowing you to rename files with consistent patterns, including chapter information and proper numbering.

## Features

- **Audiobook Support**: Processes MP3, M4A, and MP4 audiobook files
- **E-Book Support**: Processes EPUB and MOBI e-book files
- **Batch Processing**: Recursively processes all subfolders in a given directory
- **Chapter Management**: Intelligently handles chapter information for audiobooks
- **Sequential Numbering**: Automatically numbers audiobook files with appropriate padding
- **Safe Renaming**: Queues all renames before execution to prevent conflicts
- **Cover Image Standardization**: Renames cover images to a standard `cover.jpg` filename
- **Associated Files**: Also renames related PDF, XML, and other media files in audiobook folders
- **Interactive Mode**: Prompts for book names and chapter selection when needed
- **Command-Line Support**: Can be run in batch mode with command-line arguments

## Requirements

- .NET 10.0 or higher
- Windows, macOS, or Linux

## Installation

### From Source

1. Clone the repository:
   ```bash
   git clone https://github.com/DJDoena/AdaptFileNames.git
   ```

2. Navigate to the project directory:
   ```bash
   cd AdaptFileNames/AdaptFileNames
   ```

3. Build the project:
   ```bash
   dotnet build -c Release
   ```

4. The executable will be in `bin/Release/net10.0/`

## Usage

### Interactive Mode

Run the executable without arguments for interactive mode:

```bash
AdaptFileNames.exe
```

You'll be prompted to:
1. Select file type (mp3 or epub)
2. Enter the folder path to process

### Command-Line Mode

```bash
AdaptFileNames.exe <filetype> <folderpath>
```

**Parameters:**
- `<filetype>`: Either `mp3` (for audiobooks) or `epub` (for e-books)
- `<folderpath>`: The path to the folder containing your files

**Examples:**

Process audiobooks:
```bash
AdaptFileNames.exe mp3 "C:\My Audiobooks\Book Series"
```

Process e-books:
```bash
AdaptFileNames.exe epub "C:\My E-Books\Fiction"
```

## How It Works

### Audiobook Processing (MP3 Mode)

1. **Scans folders** recursively for MP3, M4A, and MP4 files
2. **Prompts for book name** (defaults to folder name if you press Enter)
3. **Handles single vs. multiple files**:
   - Single file: Renames to book name
   - Multiple files: Numbers files sequentially (e.g., `01 Book Name`, `02 Book Name`)
4. **Chapter detection**: If files contain dashes (`-`), you can select which part represents chapter information
5. **Renames associated files**: Covers, PDFs, EPUBs, MOBIs, and XML files in the same folder
6. **Standardizes cover images** to `cover.jpg`

**Example Output:**
```
Original: Track1.mp3, Track2.mp3, Track3.mp3
Becomes:  01 Harry Potter - Chapter One.mp3
          02 Harry Potter - Chapter Two.mp3
          03 Harry Potter - Chapter Three.mp3
```

### E-Book Processing (EPUB Mode)

1. **Scans folders** recursively for EPUB and MOBI files
2. **Prompts for book name** (defaults to folder name if you press Enter)
3. **Renames files** to match the book name with original extension
4. **Alerts** if unexpected number of files found (expects 0 or 2 files - typically one EPUB and one MOBI)
5. **Standardizes cover images** to `cover.jpg`

**Example Output:**
```
Original: some_book.epub, some_book.mobi
Becomes:  The Great Gatsby.epub, The Great Gatsby.mobi
```

## Safety Features

- All rename operations are queued before execution
- Files are validated before renaming
- The program shows the total number of files renamed
- No files are deleted, only renamed

## Error Handling

The program validates:
- File type is either `mp3` or `epub`
- Folder path exists
- Handles exceptions gracefully with detailed error messages
- Waits for user confirmation before exiting (useful when double-clicking the executable)

## Exit Codes

- `0`: Success
- `-1`: Invalid file type specified
- `-2`: Specified folder does not exist

## Dependencies

- [DoenaSoft.AbstractionLayer.IO.Default](https://www.nuget.org/packages/DoenaSoft.AbstractionLayer.IO.Default/) (v1.0.2) - Provides abstraction layer for file system operations

## Building

```bash
dotnet build
```

For release builds:
```bash
dotnet build -c Release
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

DJ Doena - [GitHub Profile](https://github.com/DJDoena)

## Project Links

- Repository: https://github.com/DJDoena/AdaptFileNames
- Issue Tracker: https://github.com/DJDoena/AdaptFileNames/issues

## Version Information

The assembly version is automatically generated based on the build time in the format `yyyy.MM.dd.HHmm`.

---

*Made with ❤️ for better media library organization*
