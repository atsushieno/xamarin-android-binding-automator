using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MavenDownloader
	{
		public class Options
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

		public IDictionary<string,PomComponentKind> SavedFiles { get; private set; } = new Dictionary<string, PomComponentKind> ();

		public void Process (Options options)
		{
			var outbase = options.OutputPath ?? Directory.GetCurrentDirectory ();
			foreach (var pom in options.Poms) {
				foreach (var repo in options.Repositories) {
					try {
						var pkgspec = Repository.FromGradleSpecifier (pom);
						if (!repo.CanTryDownloading (pkgspec))
							continue;
						foreach (var kind in new PomComponentKind [] { PomComponentKind.Binary, PomComponentKind.JavadocJar }) {
							var outfile = BuildLocalCachePath (outbase, pkgspec, kind);
							SavedFiles.Add (outfile, kind);
							Directory.CreateDirectory (Path.GetDirectoryName (outfile));
							using (var stream = repo.GetStreamAsync (pkgspec, kind, options).Result)
								using (var outfs = File.OpenWrite (outfile))
									stream.CopyTo (outfs);
							break;
						}
					} catch (RepositoryDownloadException) {
					}
				}
			}
		}

		public static string BuildLocalCachePath (string basePath, PackageReference pr, PomComponentKind kind)
		{
			return Path.Combine (basePath, "download_cache", pr.GroupId, pr.ArtifactId, pr.Version, $"{pr.ArtifactId}-{pr.Version}{kind.ToFileSuffix (pr)}");
		}
	}
	
}