using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MavenBindingAutomatorOptions
	{
		public MavenDownloader.Options DownloaderOptions { get; set; } = new MavenDownloader.Options ();
		public BindingProjectCreator.Options ProjectCreatorOptions { get; set; } = new BindingProjectCreator.Options ();
		public BindingProjectBuilder.Options ProjectBuilderOptions { get; set; } = new BindingProjectBuilder.Options ();
		public JavaDocumentImporter.Options JavaDocumentImporterOptions { get; set; } = new JavaDocumentImporter.Options ();
	}

	public class MavenBindingAutomator
	{
		public void Process (MavenBindingAutomatorOptions options)
		{
			// download Java dependencies
			var d = new MavenDownloader ();
			var dr = d.Process (options.DownloaderOptions);

			if (options.ProjectCreatorOptions.SolutionDirectory == null) {
				options.DownloaderOptions.LogMessage ("Required option for projects directory is missing. No further action is taken. Done.");
				return;
			}

			// create project to build
			var c = new BindingProjectCreator ();
			var cr = c.Process (options.ProjectCreatorOptions, dr.Downloads);

			// build project
			var b = new BindingProjectBuilder ();
			b.Process (options.ProjectBuilderOptions, cr.Projects);

			// import javadoc ... so far it's done within binding builds.
			//var i = new JavaDocumentImporter ();
			//i.Process (options.JavaDocumentImporterOptions);
		}
	}
}
