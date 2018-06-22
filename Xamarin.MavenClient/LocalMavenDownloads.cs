using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace Xamarin.MavenClient
{
	public class LocalMavenDownloads
	{
		public class Entry
		{
			public Entry (PackageReference package, PomComponentKind kind, string localFile)
			{
				Package = package;
				ComponentKind = kind;
				LocalFile = localFile;
			}
			public PackageReference Package { get; set; }
			public PomComponentKind ComponentKind { get; set; }
			public string LocalFile { get; set; }
		}

		public string BaseDirectory { get; set; }
		
		public IList<Entry> Entries { get; private set; } = new List<Entry> ();

		public bool Exists (PackageReference package, PomComponentKind kind)
		{
			return Find (package, kind) != null;
		}

		public Entry Find (PackageReference package, PomComponentKind kind)
		{
			return Entries.FirstOrDefault (e => e.ComponentKind == kind && e.Package.Equals (package));
		}

		public static LocalMavenDownloads PopulateFromDirectory (string directory)
		{
			var ret = new LocalMavenDownloads ();
			foreach (var groupDir in new DirectoryInfo (directory).GetDirectories ()) {
				foreach (var artifactDir in groupDir.GetDirectories ()) {
					foreach (var versionDir in artifactDir.GetDirectories ()) {
						var pomFile = Path.Combine (versionDir.FullName, $"{artifactDir.Name}-{versionDir.Name}.pom");
						var pr = PackageReference.Load (XElement.Load (pomFile));
						ret.Entries.Add (new Entry (pr, PomComponentKind.PomXml, pomFile));
						foreach (var jar in versionDir.GetFiles ("*.jar")) {
							if (jar.Name.EndsWith ("-sources.jar", StringComparison.OrdinalIgnoreCase))
								ret.Entries.Add (new Entry (pr, PomComponentKind.SourcesJar, jar.FullName));
							if (jar.Name.EndsWith ("-javadoc.jar", StringComparison.OrdinalIgnoreCase))
								ret.Entries.Add (new Entry (pr, PomComponentKind.JavadocJar, jar.FullName));
							else
								ret.Entries.Add (new Entry (pr, PomComponentKind.Binary, jar.FullName));
						}
					}
				}
			}
			return ret;
		}
	}
}
