using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MavenBindingAutomatorOptions
	{
		public MavenDownloader.Options DownloaderOptions { get; set; } = new MavenDownloader.Options ();
		public JavaDocumentImporter.Options JavaDocumentImporterOptions { get; set; } = new JavaDocumentImporter.Options ();
	}

	public class MavenBindingAutomator
	{
		public void Process (MavenBindingAutomatorOptions options)
		{
			// download Java dependencies
			var d = new MavenDownloader ();
			d.Process (options.DownloaderOptions);

			// create project to build
			throw new NotImplementedException ();

			// import javadoc
			var i = new JavaDocumentImporter ();
			i.Process (options.JavaDocumentImporterOptions);
		}
	}
}
