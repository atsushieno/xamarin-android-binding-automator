using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xamarin.MavenClient
{

	public enum PomComponentKind
	{
		PomXml,
		Binary,
		SourcesJar,
		JavadocJar,
		MavenMetadataXml,
	}
	
}
