using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{

	public class PackageReference
	{
		public string GroupId { get; set; }
		public string ArtifactId { get; set; }
		public string Version { get; set; }
		public string VersionLong { get; set; }
		public string DeclaredPackaging { get; set; }
		public string Packaging {
			// maven-metadata.xml in Android SDK does not have this, and default should be "aar". Otherwise, "jar".
			get { return DeclaredPackaging ?? (Repository.IsAndroidSdkComponent (GroupId) && ArtifactId != "support-annotations" ? "aar" : "jar"); }
		}
		public string Name { get; set; }
		public string Description { get; set; }
		public string Scope { get; set; }
		public IList<PackageReference> Dependencies { get; private set; } = new List<PackageReference> ();

		public const string MavenPom4Namespace = "http://maven.apache.org/POM/4.0.0";
		internal static readonly XNamespace NS = XNamespace.Get (MavenPom4Namespace);

		public static PackageReference Load (XElement element)
		{
			var old = element.Name.Equals (XName.Get ("metadata"));
			var parent = element.Element (old ? XName.Get ("parent") : NS.GetName ("parent"));
			var pr = parent != null ? Load (parent) : new PackageReference ();
			pr.GroupId = element.Value ("groupId") ?? pr.GroupId;
			pr.ArtifactId = element.Value ("artifactId") ?? pr.ArtifactId;
			pr.Version = (old ? element.Element ("versioning").Element ("versions") : element).Value ("version") ?? pr.Version;
			pr.VersionLong = pr.Version;
			pr.DeclaredPackaging = element.Value ("packaging") ?? pr.DeclaredPackaging;
			pr.Name = element.Value ("name") ?? pr.Name;
			pr.Description = element.Value ("description") ?? pr.Description;
			pr.Scope = element.Value ("scope") ?? pr.Scope;
			var deps = element.Elements (old ? XName.Get ("dependencies") : NS.GetName ("dependencies"));
			pr.Dependencies = deps.SelectMany (p => p.Elements (old ? XName.Get ("dependency") : NS.GetName ("dependency")).Select (d => Load (d))).ToList ();
			return pr;
		}

		public override string ToString ()
		{
			return $"{GroupId}:{ArtifactId}:{Version}";
		}
	}
	
}
