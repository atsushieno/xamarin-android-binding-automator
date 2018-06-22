using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xamarin.MavenClient;

namespace Xamarin.Android.Tools.MavenBindingAutomator.BuildTasks
{
	public class DownloadMavenDependencies : Task
	{
		public string [] MavenPackageIds { get; set; }

		public string AndroidSdkDirectory { get; set; }

		public string XamarinSdkDirectory { get; set; }

		public string OutputDirectory { get; set; }

		[Output]
		public string [] OutputJars { get; set; }

		public bool EnableGoogleRepository { get; set; }

		public bool EnableJCenterRepository { get; set; }

		public override bool Execute ()
		{
			var automatorOptions = new MavenBindingAutomatorOptions ();
			var dlOpts = automatorOptions.DownloaderOptions;
			var creatorOpts = automatorOptions.ProjectCreatorOptions;
			var builderOpts = automatorOptions.ProjectBuilderOptions;

			if (EnableGoogleRepository)
				dlOpts.Repositories.Add (new GoogleRepository ());

			if (Directory.Exists (AndroidSdkDirectory))
				dlOpts.Repositories.Add (new LocalAndroidSdkRepository (AndroidSdkDirectory));

			dlOpts.OutputPath = OutputDirectory;
			foreach (var pkg in MavenPackageIds)
				dlOpts.Poms.Add (pkg);

			if (EnableJCenterRepository)
				dlOpts.Repositories.Add (new JCenterRepository ());

			new MavenBindingAutomator ().Process (automatorOptions);

			return true;
		}
	}
}
