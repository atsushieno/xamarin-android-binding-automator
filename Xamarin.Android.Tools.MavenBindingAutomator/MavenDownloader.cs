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

		public class Results
		{
			public LocalMavenDownloads Downloads { get; set; } = new LocalMavenDownloads ();
		}

		public Results Process (Options options)
		{
			var results = new Results ();
			var outbase = options.OutputPath ?? Directory.GetCurrentDirectory ();
			foreach (var pom in options.Poms) {
				foreach (var repo in options.Repositories) {
					try {
						var pkgspec = Repository.FromGradleSpecifier (pom);
						if (!repo.CanTryDownloading (pkgspec))
							continue;
						foreach (var kind in new PomComponentKind [] { PomComponentKind.Binary, PomComponentKind.JavadocJar }) {
							var outfile = BuildLocalCachePath (outbase, pkgspec, kind);
							results.Downloads.Entries.Add (new LocalMavenDownloads.Entry (pkgspec, kind, outfile));
							Directory.CreateDirectory (Path.GetDirectoryName (outfile));
							using (var stream = repo.GetStreamAsync (pkgspec, kind, options).Result)
								using (var outfs = File.OpenWrite (outfile))
									stream.CopyTo (outfs);
							break;
						}
						break;
					} catch (RepositoryDownloadException) {
					}
				}
			}
			return results;
		}

		public static string BuildLocalCachePath (string basePath, PackageReference pr, PomComponentKind kind)
		{
			return Path.Combine (basePath, "download_cache", pr.GroupId, pr.ArtifactId, pr.Version, $"{pr.ArtifactId}-{pr.Version}{kind.ToFileSuffix (pr)}");
		}
	}
	
}
