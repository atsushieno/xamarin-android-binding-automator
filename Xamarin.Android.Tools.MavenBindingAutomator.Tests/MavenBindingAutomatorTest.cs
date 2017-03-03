using NUnit.Framework;
using System;
namespace Xamarin.Android.Tools.MavenBindingAutomator.Tests
{
	[TestFixture]
	public class MavenBindingAutomatorTest
	{
		[Test]
		public void BuildLocalCachePath ()
		{
			var pr = new PackageReference () {
				GroupId = "com.xamarin.example",
				ArtifactId = "testproject",
				Version = "1.0.0",
			};
			var path1 = MavenBindingAutomator.BuildLocalCachePath ("/foo/bar/baz", pr, PomComponentKind.Binary);
			Assert.AreEqual ("/foo/bar/baz/download_cache/com.xamarin.example/testproject/1.0.0/testproject-1.0.0.jar", path1, "#1");
			var path2 = MavenBindingAutomator.BuildLocalCachePath ("/foo/bar/baz", pr, PomComponentKind.JavadocJar);
			Assert.AreEqual ("/foo/bar/baz/download_cache/com.xamarin.example/testproject/1.0.0/testproject-1.0.0-javadoc.jar", path2, "#2");
		}
	}
}
