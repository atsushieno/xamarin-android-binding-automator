using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class BindingAutomatorOptions
	{
		public string OutputPath { get; set; }
		public IList<string> Poms { get; private set; } = new List<string> ();
		public TextWriter LogWriter { get; set; } = Console.Out;
		public IList<Repository> Repositories { get; private set; } = new List<Repository> ();

		public void LogMessage (string format, params object [] args)
		{
			LogWriter.WriteLine (format, args);
		}
	}
}