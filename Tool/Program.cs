using System;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			var automatorOptions = new MavenBindingAutomatorOptions ();
			var dlOpts = automatorOptions.DownloaderOptions;
			var javadocOpts = automatorOptions.JavaDocumentImporterOptions;
			foreach (var arg in args) {
				if (arg.StartsWith ("--android-sdk:", StringComparison.Ordinal))
					dlOpts.Repositories.Add (new LocalAndroidSdkRepository (arg.Substring ("--android-sdk:".Length)));
				else if (arg.StartsWith ("--xamarin-sdk:", StringComparison.Ordinal))
					javadocOpts.XamarinSdk = arg.Substring ("--xamarin-sdk:".Length);
				else if (arg.StartsWith ("--out:", StringComparison.Ordinal))
					dlOpts.OutputPath = arg.Substring ("--out:".Length);
				else
					dlOpts.Poms.Add (arg);
			}
			dlOpts.Repositories.Add (new JCenterRepository ());
			new MavenBindingAutomator ().Process (automatorOptions);
		}
	}
}
