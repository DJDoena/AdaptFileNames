namespace DoenaSoft.AdaptFileNames;

/// <summary>
/// Defines interaction methods for user input and output operations.
/// </summary>
public interface IInteraction
{
    /// <summary>
    /// Reads a line of text from the input stream.
    /// </summary>
    /// <returns>The line read from the input stream.</returns>
    string ReadLine();

    /// <summary>
    /// Writes a message followed by a line terminator to the output stream.
    /// </summary>
    /// <param name="message">The message to write. Can be null to write an empty line.</param>
    void WriteLine(string message = null);
}