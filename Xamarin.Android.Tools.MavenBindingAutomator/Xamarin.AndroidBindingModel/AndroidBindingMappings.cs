using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class AndroidBindingMappings
	{
		public string MavenId { get; set; }
		public string NuGetId { get; set; }
		public string BindingProjectUrl { get; set; }
	}

	public class AndroidBindingProjectPart
	{
		public IList<string> MetadataXmlFiles { get; } = new List<string> ();
		public IList<string> Additions { get; } = new List<string> ();
	}
}
