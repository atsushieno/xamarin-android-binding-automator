using System;
using System.IO;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class Logger
	{
		public TextWriter Output { get; set; } = Console.Error;

		public void Log (LogRecord log, params object [] args)
		{
			Output.Write (log.Verbosity);
			Output.Write (' ');
			Output.Write (log.ErrorCode.ToString ("D04"));
			Output.Write (" : ");
			Output.WriteLine (log.Format, args);
		}
	}

	public class LogRecord
	{
		public enum LogVerbosity
		{
			Error,
			Warning,
			Information,
		}

		public LogVerbosity Verbosity { get; set; }
		public string Format { get; set; }
		public int ErrorCode { get; set; }

		public LogRecord (LogVerbosity verbosity, int errorCode, string format)
		{
			Verbosity = verbosity;
			Format = format;
			ErrorCode = errorCode;
		}

		public static readonly LogRecord SpecifiedJavadocJarDoesNotExist = new LogRecord (
			LogVerbosity.Warning, 1001, "Specified Javadoc Jar file '{0}' does not exist");
		public static readonly LogRecord SpecifiedJavadocJarIsOlderThanStamp = new LogRecord (
			LogVerbosity.Information, 1002, "Speciied Javadoc Jar file '{0}' is older than cache timestamp. Skipped unzipping.");
		public static readonly LogRecord CommandFinishedWithinNMilliseconds = new LogRecord (
			LogVerbosity.Information, 1003, "Command tool '{0}' with arguments '{1}' finished within {2} milliseconds.");
	}
}