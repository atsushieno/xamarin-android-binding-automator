using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{

	public class PackageReference
	{
		public string GroupId { get; set; }
		public string ArtifactId { get; set; }
		public string Version { get; set; }
		public string VersionLong { get; set; }
		string packaging;
		public string Packaging {
			// maven-metadata.xml in Android SDK does not have this, and default should be "aar". Otherwise, "jar".
			get { return packaging ?? (Repository.IsAndroidSdkComponent (GroupId) ? "aar" : "jar"); }
			set { packaging = value; }
		}
		public string Name { get; set; }
		public string Description { get; set; }

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
			pr.Packaging = element.Value ("packaging") ?? pr.Packaging;
			pr.Name = element.Value ("name") ?? pr.Name;
			pr.Description = element.Value ("description") ?? pr.Description;
			return pr;
		}
	}
	
}
