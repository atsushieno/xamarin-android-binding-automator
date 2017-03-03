using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MavenBindingAutomator
	{
		public void Process (BindingAutomatorOptions options)
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
