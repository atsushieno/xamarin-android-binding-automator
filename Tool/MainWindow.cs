using System;
using Xwt;
using System.Linq;
using System.IO;

namespace Xamarin.Android.Tools.MavenBindingAutomator
{
	public class MainWindow : Window
	{
		public MainWindow ()
		{
			Width = 800;
			Height = 600;

			MainMenu = BuildMainMenu ();

			var layout = new VBox ();

			var packageIdEntry = new ComboBoxEntry ();
			using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
				if (store.FileExists ("history.txt"))
					using (var file = store.OpenFile ("history.txt", FileMode.Open, FileAccess.Read))
						foreach (var line in new StreamReader (file).ReadToEnd ().Split ('\n'))
							packageIdEntry.Items.Add (line.TrimEnd ());
			Action updatePoms = () => {
				State.Options.DownloaderOptions.Poms.Clear ();
				State.Options.DownloaderOptions.Poms.Add (packageIdEntry.TextEntry.Text);
			};
			packageIdEntry.TextInput += (sender, e) => updatePoms ();
			packageIdEntry.SelectionChanged += (sender, e) => updatePoms ();

			layout.PackStart (packageIdEntry);

			import_button = new Button { Label = "Import" };
			import_button.Clicked += (sender, e) => {
				// switch to importing state
				import_button.Sensitive = false;
				this.Content.Cursor = CursorType.Wait; // does not seem to make much sense...

				System.Threading.Tasks.Task.Run (() => {
					using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
					using (var file = store.OpenFile ("history.txt", FileMode.OpenOrCreate, FileAccess.Write))
					using (var writer = new StreamWriter (file)) {
						writer.WriteLine (packageIdEntry.TextEntry.Text);
						foreach (string item in packageIdEntry.Items)
							if (packageIdEntry.TextEntry.Text != item && !string.IsNullOrWhiteSpace (item))
								writer.WriteLine (item);
					}
					PerformImport ();
				});
			};
			layout.PackStart (import_button);

			var tree = new TreeView ();
			package_field = new DataField<string> ();
			tree_store = new TreeStore (package_field);
			tree.DataSource = tree_store;
			tree.Columns.Add ("Packages", package_field);
			layout.PackStart (tree, true);

			Content = layout;
		}

		Button import_button;
		TreeStore tree_store;
		DataField<string> package_field;

		public MainState State { get; } = new MainState ();

		Menu BuildMainMenu ()
		{
			var menu = new Menu ();

			var file = new MenuItem { Label = "_File" };
			file.SubMenu = new Menu ();
			menu.Items.Add (file);

			var fileExit = new MenuItem { Label = "E_xit" };
			fileExit.Clicked += (sender, e) => Application.Exit ();
			file.SubMenu.Items.Add (fileExit);

			return menu;
		}

		void PerformImport ()
		{
			var d = new MavenDownloader ();
			var packages = d.FlattenAllPackageReferences (State.Options.DownloaderOptions);
			Application.Invoke (() => {
				var nav = tree_store.AddNode ();
				foreach (var package in packages)
					nav.AddChild ().SetValue (package_field, package.ToString ()).MoveToParent ();

				// restore default state
				Content.Cursor = CursorType.Arrow;
				import_button.Sensitive = true;
			});
		}
	}

	public class MainState
	{
		public MavenBindingAutomatorOptions Options { get; set; }
	}
}
