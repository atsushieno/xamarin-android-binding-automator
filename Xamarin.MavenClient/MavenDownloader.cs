using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Xamarin.MavenClient
{
	public class MavenDownloader
	{
		public class Options
		{
			public bool IgnoreDependencies { get; set; }
			public string OutputPath { get; set; }
			public IList<string> Poms { get; private set; } = new List<string> ();
			public TextWriter LogWriter { get; set; } = Console.Out;
			public IList<Repository> Repositories { get; private set; } = new List<Repository> ();
			public IList<string> ExtraScopes { get; private set; } = new List<string> ();
			public bool SkipExisting { get; set; } = true;

			public void LogMessage (string format, params object [] args)
			{
				LogWriter.WriteLine (format, args);
			}
		}

		public class Results
		{
			public LocalMavenDownloads Downloads { get; set; } = new LocalMavenDownloads ();
		}

		void FlattenDependencies (IList<PackageReference> results, PackageReference pr, Options options)
		{
			foreach (var repo in options.Repositories) {
				try {
					if (!repo.CanTryDownloading (pr))
						continue;
					repo.FixIncompletePackageReference (pr, options);
					var p = repo.RetrievePomContent (pr, options, null);
					if (!repo.ShouldSkipDownload (p))
						results.Add (p);
					if (!options.IgnoreDependencies)
						foreach (var dep in p.Dependencies.Where (d => results.All (r => r.ToString () != d.ToString ()) && IsScopeCovered (options, d)))
							FlattenDependencies (results, dep, options);
					break;
				} catch (RepositoryDownloadException) {
					// try next repo.
				}
			}
		}

		bool IsScopeCovered (Options options, PackageReference p)
		{
			return p.Scope == "compile" || options.ExtraScopes.Contains (p.Scope);
		}

		public Results Process (Options options)
		{
			var pkgspecs = FlattenAllPackageReferences (options).ToArray ();
			var results = new Results ();
			results.Downloads.BaseDirectory = options.OutputPath ?? Directory.GetCurrentDirectory ();

			return Download (options, results, pkgspecs);
		}

		public IEnumerable<PackageReference> FlattenAllPackageReferences (Options options)
		{
			var list = new List<PackageReference> ();
			foreach (var pom in options.Poms)
				FlattenDependencies (list, Repository.FromGradleSpecifier (pom), options);
			return list;
		}

		public Results Download (Options options, Results results, IEnumerable<PackageReference> pkgspecs)
		{
			var processed = new List<PackageReference> ();
			foreach (var pkgspec in pkgspecs) {
				if (processed.Any (p => p.ToString () == pkgspec.ToString ()))
					continue;
				processed.Add (pkgspec);
				bool done = false;
				foreach (var repo in options.Repositories) {
					try {
						if (!repo.CanTryDownloading (pkgspec))
							continue;
						repo.FixIncompletePackageReference (pkgspec, options);
						foreach (var kind in new PomComponentKind [] { PomComponentKind.PomXml, PomComponentKind.Binary, PomComponentKind.JavadocJar, PomComponentKind.SourcesJar }) {
							var outfile = BuildLocalCachePath (results.Downloads.BaseDirectory, pkgspec, kind);
							results.Downloads.Entries.Add (new LocalMavenDownloads.Entry (pkgspec, kind, outfile));
							if (options.SkipExisting && File.Exists (outfile))
								continue;
							Directory.CreateDirectory (Path.GetDirectoryName (outfile));
							try {
								using (var stream = repo.GetStreamAsync (pkgspec, kind, options, null).Result)
								using (var outfs = File.OpenWrite (outfile))
									stream.CopyTo (outfs);
							} catch {
								options.LogMessage ($"could not download {outfile}");
								continue;
							}
							options.LogMessage ($"saved at {outfile}");
							done = true;
						}
						break;
					} catch (RepositoryDownloadException) {
					}
				}
				if (!done)
					options.LogMessage ($"WARNING: package {pkgspec} was not downloaded.");
			}
			return results;
		}

		public static string BuildLocalCachePath (string basePath, PackageReference pr, PomComponentKind kind)
		{
			if (string.IsNullOrEmpty (pr.GroupId))
				throw new ArgumentException ("groupId is empty for " + pr);
			if (string.IsNullOrEmpty (pr.ArtifactId))
				throw new ArgumentException ("artifactId is empty for " + pr);
			if (string.IsNullOrEmpty (pr.Version))
				throw new ArgumentException ("version is empty for " + pr);
			return Path.Combine (basePath ?? "", pr.GroupId, pr.ArtifactId, pr.Version, $"{pr.ArtifactId}-{pr.Version}{kind.ToFileSuffix (pr)}");
		}
	}

}
