using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.MavenClient
{
	internal static class Extensions
	{
		public static string ToFileSuffix (this PomComponentKind kind, PackageReference pr)
		{
			switch (kind) {
			case PomComponentKind.PomXml: return ".pom";
			case PomComponentKind.Binary: return "." + pr.Packaging;
			case PomComponentKind.JavadocJar: return "-javadoc.jar";
			case PomComponentKind.SourcesJar: return "-sources.jar";
			}
			throw new NotSupportedException ();
		}

		static XElement GetRoot (this XElement e)
		{
			return e.Document != null ? e.Document.Root : e.Parent != null ? e.Parent.GetRoot () : e;
		}

		internal static string Value (this XElement el, string name)
		{
			var isEmptyNS = el.GetRoot ().Name.Namespace.NamespaceName.Length == 0;
			return el.Element (isEmptyNS ? XName.Get (name) : PackageReference.NS.GetName (name))?.Value;
		}
	}
}
