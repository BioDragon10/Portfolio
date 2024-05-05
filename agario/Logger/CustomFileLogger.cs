using System.Reflection;
using Microsoft.Extensions.Logging;

namespace FileLogger;

/// <summary>
///     Author:    Aidan Spendlove
///     Partner:   Blake Lawlor
///     Date:      4/14/2023
///     Course:    CS 3500, University of Utah, School of Computing
///     Copyright: CS 3500 and Aidan Spendlove and Blake Lawlor- This work may not
///     be copied for use in Academic Coursework.
///     I, Aidan Spendlove and Blake Lawlor, certify that I wrote this code from scratch and
///     did not copy it in part or whole from another source.  All
///     references used in the completion of the assignments are cited
///     in my README file.
///     File Contents
///     This file contains all of the code for the CustomFileLogger, mainly its constructor and
///     Log() method.
/// </summary>
public class CustomFileLogger : ILogger {
    private readonly string _filename;

    /// <summary>
    ///     Sets up the filepath for the Logger file.
    /// </summary>
    /// <inheritdoc cref="ILogger" />
    public CustomFileLogger(string categoryName) {
        //https://stackoverflow.com/questions/19329672/write-file-to-project-folder-on-any-computer

        _filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    Path.DirectorySeparatorChar + "AgarioProgramLogs";
        Directory.CreateDirectory(_filename);
        string timeStamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-d h-mm-ss tt");
        _filename += Path.DirectorySeparatorChar + GetSafeFilename($"{categoryName} {timeStamp}.txt");
    }

    /// <summary>
    ///     Would mark the start of a specific section in your log file, but actually throws an exception
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Logs log messages that need to be logged to a logger file provided specifically to this custom logger implementing
    ///     ILogger
    ///     Logs the thread, log level, date / time with UTC marker, and message
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <param name="formatter"></param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                            Func<TState, Exception?, string> formatter) {
        string thread = Environment.CurrentManagedThreadId.ToString();
        string logLevelTruncated = logLevel.ToString()[..5];
        string timeStamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-d h:mm:ss tt");
        string message = $"{timeStamp} (UTC) ({thread}) - {logLevelTruncated} - {formatter(state, exception)}\n";
        File.AppendAllText(_filename, message);
    }

    /// <summary>
    ///     Code that splits a FilePath around its invalid characters, and replaces them with '_'.
    ///     Source: https://stackoverflow.com/a/12800424
    /// </summary>
    /// <param name="filename">The filename to split.</param>
    /// <returns>The sanitized filename</returns>
    private string GetSafeFilename(string filename) {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
}