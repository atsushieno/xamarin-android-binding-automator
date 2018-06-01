using System;
using NUnit.Framework;
using Xamarin.MavenClient;

namespace Xamarin.Android.Tools.MavenBindingAutomator.Tests
{
	[TestFixture]
	public class PackageReferenceTest
	{
		[Test]
		public void Packaging ()
		{
			var pr1 = new PackageReference { GroupId = "com.android.support" };
			Assert.AreEqual ("aar", pr1.Packaging, "#1");
			var pr2 = new PackageReference { GroupId = "foo.bar.baz" };
			Assert.AreEqual ("jar", pr2.Packaging, "#2");
		}
	}
}
