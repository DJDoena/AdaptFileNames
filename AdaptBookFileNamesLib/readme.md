# DoenaSoft.AdaptBookFileNames

A .NET library for organizing and standardizing audiobook and e-book file names with consistent naming conventions.

## Overview

DoenaSoft.AdaptBookFileNames is a reusable library that provides processors for batch renaming audiobook files (MP3, M4A, MP4) and e-book files (EPUB, MOBI). It handles sequential numbering, chapter information extraction, and associated file renaming (covers, PDFs, XMLs).

## Features

- Audiobook Processing: Renames MP3, M4A, and MP4 files with sequential numbering and chapter support
- E-Book Processing: Renames EPUB and MOBI files to match book names
- Chapter Management: Intelligently extracts and appends chapter information from file names
- Sequential Numbering: Automatically numbers files with appropriate zero-padding
- Cover Image Standardization: Renames cover images (JPG/JPEG) to cover.jpg
- Associated Files: Renames related PDF, EPUB, MOBI, and XML files alongside audiobooks
- Abstraction Layer: Built on DoenaSoft.AbstractionLayer.IO for testability and flexibility
- Interactive Interface: Supports custom user interaction implementations

## Installation

Install via NuGet Package Manager:

```powershell
Install-Package DoenaSoft.AdaptBookFileNames
```

Or via .NET CLI:

```bash
dotnet add package DoenaSoft.AdaptBookFileNames
```

## Usage

### Basic Audiobook Processing

```csharp
using DoenaSoft.AdaptBookFileNames;
using DoenaSoft.AbstractionLayer.IOServices;

var ioServices = new IOServices();
var interaction = new YourInteractionImplementation();
var renameQueue = new RenameQueue(ioServices);

renameQueue.Initialize();

var processor = new AudioBookProcessor(
    renameQueue, 
    ioServices.Path, 
    interaction
);

var folder = ioServices.GetFolder(@"C:\Audiobooks\BookFolder");
processor.Process(folder);

var renamedCount = renameQueue.Commit();
Console.WriteLine($"{renamedCount} files renamed.");
```

### Basic E-Book Processing

```csharp
using DoenaSoft.AdaptBookFileNames;
using DoenaSoft.AbstractionLayer.IOServices;

var ioServices = new IOServices();
var interaction = new YourInteractionImplementation();
var renameQueue = new RenameQueue(ioServices);

renameQueue.Initialize();

var processor = new EBookProcessor(
    renameQueue, 
    ioServices.Path, 
    interaction
);

var folder = ioServices.GetFolder(@"C:\EBooks\BookFolder");
processor.Process(folder);

var renamedCount = renameQueue.Commit();
```

### Implementing IInteraction

You need to provide an implementation of IInteraction for user input/output:

```csharp
public class ConsoleInteraction : IInteraction
{
    public string ReadLine() => Console.ReadLine();
    
    public void WriteLine(string message = null) => Console.WriteLine(message);
}
```

## API Reference

### IBookProcessor

Interface for book file processors.

- Process(IFolderInfo folder): Processes all book files in the specified folder.

### AudioBookProcessor

Processes audiobook files with sequential numbering and chapter support.

Constructor Parameters:
- IRenameQueue renameQueue: Queue for file rename operations
- IPath path: Path service for file operations
- IInteraction interaction: User interaction service

Behavior:
- Finds all MP3, M4A, and MP4 files in the folder
- Prompts user for book name (defaults to folder name)
- Numbers multiple files sequentially (e.g., 01 Book Name.mp3, 02 Book Name.mp3)
- Extracts chapter information from file names containing dashes
- Renames associated cover images, e-books, and XML files

### EBookProcessor

Processes e-book files (EPUB, MOBI).

Constructor Parameters:
- IRenameQueue renameQueue: Queue for file rename operations
- IPath path: Path service for file operations
- IInteraction interaction: User interaction service

Behavior:
- Finds all EPUB and MOBI files in the folder
- Prompts user for book name (defaults to folder name)
- Renames files to match the book name while preserving extensions
- Alerts if unexpected number of files found (expects 0 or 2)
- Renames associated cover images

### IInteraction

Interface for user input/output operations.

- string ReadLine(): Reads a line of text from the input stream
- void WriteLine(string message): Writes a message to the output stream

## Example Scenarios

### Audiobook with Chapters

Input files:
```
01 - Introduction.mp3
02 - Chapter One - The Beginning.mp3
03 - Chapter Two - The Journey.mp3
```

After processing with book name "My Audiobook":
```
01 My Audiobook - Introduction.mp3
02 My Audiobook - Chapter One - The Beginning.mp3
03 My Audiobook - Chapter Two - The Journey.mp3
```

### E-Book Files

Input files:
```
some_ebook_file.epub
some_ebook_file.mobi
cover_image.jpg
```

After processing with book name "The Great Novel":
```
The Great Novel.epub
The Great Novel.mobi
cover.jpg
```

## Dependencies

- DoenaSoft.AbstractionLayer.IO (>= 5.0.1): Provides file system abstraction layer

## Contributing

Contributions are welcome! Please submit pull requests to the GitHub repository.

## License

This project is licensed under the MIT License.

## Author

DJ Doena - https://github.com/DJDoena

## Links

- Repository: https://github.com/DJDoena/AdaptFileNames
- Issues: https://github.com/DJDoena/AdaptFileNames/issues
- NuGet Package: https://www.nuget.org/packages/DoenaSoft.AdaptBookFileNames
