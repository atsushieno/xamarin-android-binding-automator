using System;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			var opts = new BindingAutomatorOptions ();
			foreach (var arg in args) {
				if (arg.StartsWith ("--sdk:", StringComparison.Ordinal))
					opts.Repositories.Add (new LocalAndroidSdkRepository (arg.Substring ("--sdk:".Length)));
				else if (arg.StartsWith ("--out:", StringComparison.Ordinal))
					opts.OutputPath = arg.Substring ("--out:".Length);
				else
					opts.Poms.Add (arg);
			}
			opts.Repositories.Add (new JCenterRepository ());
			new MavenBindingAutomator ().Process (opts);
		}
	}
}
