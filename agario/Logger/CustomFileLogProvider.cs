using Microsoft.Extensions.Logging;

namespace FileLogger
{
    /// <summary>
    /// Author:    Aidan Spendlove
    /// Partner:   Blake Lawlor
    /// Date:      4/2/2023
    /// Course:    CS 3500, University of Utah, School of Computing
    /// Copyright: CS 3500 and Aidan Spendlove and Blake Lawlor- This work may not 
    ///            be copied for use in Academic Coursework.
    ///
    /// I, Aidan Spendlove and Blake Lawlor, certify that I wrote this code from scratch and
    /// did not copy it in part or whole from another source.  All 
    /// references used in the completion of the assignments are cited 
    /// in my README file.
    ///
    /// File Contents
    /// 
    /// This file provides a CustomFileLogger
    /// 
    /// </summary>
    public class CustomFileLogProvider : ILoggerProvider
    {
        private ILogger? _fileLogger;

        /// <inheritdoc cref="ILoggerProvider"/>
        public ILogger CreateLogger(string categoryName)
        {
            _fileLogger = new CustomFileLogger(categoryName);
            return _fileLogger;
        }

        /// <inheritdoc cref="ILoggerProvider"/>
        public void Dispose()
        {
            _fileLogger = null;
        }
    }
}