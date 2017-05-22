using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.ProjectTools;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class BindingProjectBuilder
	{
		public class Options
		{
			public string SolutionDirectory { get; set; }
		}

		public class Result
		{
		}

		public Result Process (Options options, IList<XamarinAndroidBindingProject> projects)
		{
			var result = new Result ();

			if (options.SolutionDirectory == null)
				throw new ArgumentException ("Project generation target directory is not set in the project creator options.");
			if (!Directory.Exists (options.SolutionDirectory))
				throw new ArgumentException (string.Format ("Project generation target directory '{0}' specified in the project creator options does not exist.", options.SolutionDirectory));

			foreach (var p in projects) {
				var builder = new ProjectBuilder (Path.Combine (options.SolutionDirectory, p.ProjectName));
				// We'd like to investigate the outcomes, so leave them there.
				builder.CleanupOnDispose = false;
				builder.Verbosity = Microsoft.Build.Framework.LoggerVerbosity.Diagnostic;
				builder.Build (p);
				builder.Dispose ();
			}
			return result;
		}
	}
}
