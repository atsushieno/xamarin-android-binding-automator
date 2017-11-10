using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
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

			public void LogMessage (string format, params object [] args)
			{
				LogWriter.WriteLine (format, args);
			}
		}

		public class Results
		{
			public LocalMavenDownloads Downloads { get; set; } = new LocalMavenDownloads ();
		}

		IEnumerable<PackageReference> FlattenDependencies (PackageReference pr, Options options)
		{
			var ret = new List<PackageReference> ();
			foreach (var repo in options.Repositories) {
				try {
					if (!repo.CanTryDownloading (pr))
						continue;
					repo.FixIncompletePackageReference (pr, options);
					var p = repo.RetrievePomContent (pr, options, _pr => BuildLocalCachePath (options.OutputPath, _pr, PomComponentKind.PomXml));
					if (!repo.ShouldSkipDownload (p))
						ret.Add (p);
					if (!options.IgnoreDependencies)
						foreach (var d in p.Dependencies.Where (d => IsScopeCovered (options, d)).SelectMany (d => FlattenDependencies (d, options)))
							ret.Add (d);
					break;
				} catch (RepositoryDownloadException) {
					// try next repo.
				}
			}
			return ret;
		}

		bool IsScopeCovered (Options options, PackageReference p)
		{
			return p.Scope == "compile" || options.ExtraScopes.Contains (p.Scope);
		}

		IEnumerable<PackageReference> FlattenAllPackageReferences (Options options)
		{
			foreach (var pom in options.Poms) {
				foreach (var pkgspec in FlattenDependencies (Repository.FromGradleSpecifier (pom), options))
					yield return pkgspec;
				break;
			}
		}

		public Results Process (Options options)
		{
			var results = new Results ();
			results.Downloads.BaseDirectory = options.OutputPath ?? Directory.GetCurrentDirectory ();
			var pkgspecs = FlattenAllPackageReferences (options).ToArray ();
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
						foreach (var kind in new PomComponentKind [] { PomComponentKind.Binary, PomComponentKind.JavadocJar }) {
							var outfile = BuildLocalCachePath (results.Downloads.BaseDirectory, pkgspec, kind);
							results.Downloads.Entries.Add (new LocalMavenDownloads.Entry (pkgspec, kind, outfile));
							Directory.CreateDirectory (Path.GetDirectoryName (outfile));
							try {
								using (var stream = repo.GetStreamAsync (pkgspec, kind, options, _pr => BuildLocalCachePath (options.OutputPath, _pr, PomComponentKind.PomXml)).Result)
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
			if (string.IsNullOrEmpty (basePath))
				throw new ArgumentException ("basePath is empty.");
			if (string.IsNullOrEmpty (pr.GroupId))
				throw new ArgumentException ("groupId is empty for " + pr);
			if (string.IsNullOrEmpty (pr.ArtifactId))
				throw new ArgumentException ("artifactId is empty for " + pr);
			if (string.IsNullOrEmpty (pr.Version))
				throw new ArgumentException ("version is empty for " + pr);
			return Path.Combine (basePath, "download_cache", pr.GroupId, pr.ArtifactId, pr.Version, $"{pr.ArtifactId}-{pr.Version}{kind.ToFileSuffix (pr)}");
		}
	}
	
}
