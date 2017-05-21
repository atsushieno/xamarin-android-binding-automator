using System;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			var automatorOptions = new MavenBindingAutomatorOptions ();
			var dlOpts = automatorOptions.DownloaderOptions;
			var creatorOpts = automatorOptions.ProjectCreatorOptions;
			var builderOpts = automatorOptions.ProjectBuilderOptions;
			var javadocOpts = automatorOptions.JavaDocumentImporterOptions;

			dlOpts.Repositories.Add (new GoogleRepository ());
			foreach (var arg in args) {
				if (arg == "--help") {
					ShowHelp ();
					return;
				}
				if (arg.StartsWith ("--android-sdk:", StringComparison.Ordinal))
					dlOpts.Repositories.Add (new LocalAndroidSdkRepository (arg.Substring ("--android-sdk:".Length)));
				else if (arg.StartsWith ("--xamarin-sdk:", StringComparison.Ordinal))
					javadocOpts.XamarinSdk = arg.Substring ("--xamarin-sdk:".Length);
				else if (arg.StartsWith ("--projects:", StringComparison.Ordinal)) {
					creatorOpts.SolutionDirectory = arg.Substring ("--projects:".Length);
					builderOpts.SolutionDirectory = arg.Substring ("--projects:".Length);
				}
				else if (arg.StartsWith ("--out:", StringComparison.Ordinal))
					dlOpts.OutputPath = arg.Substring ("--out:".Length);
				else
					dlOpts.Poms.Add (arg);
			}
			dlOpts.Repositories.Add (new JCenterRepository ());

			new MavenBindingAutomator ().Process (automatorOptions);
		}

		static void ShowHelp ()
		{
			Console.WriteLine (@"
Arguments:
	--help: show help.
	--xamarin-sdk:[specify path-to Xamarin.Android SDK prefix]
	--projects:[maven package ID]
	--out:[output path]
"); 
		}
	}
}
