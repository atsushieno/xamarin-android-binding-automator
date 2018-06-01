using System;
using Xwt;
using System.Linq;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Xamarin.MavenClient;

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

			// package ID entry
			var packageRow = new HBox ();
			layout.PackStart (packageRow);
			packageRow.PackStart (new Label { Text = "POM" });
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

			packageRow.PackStart (packageIdEntry, true);

			// Maven download directory entry
			var downloadDirectoryRow = new HBox ();
			layout.PackStart (downloadDirectoryRow);
			downloadDirectoryRow.PackStart (new Label { Text = "Downloads" });
			var downloadDirectoryEntry = new ComboBoxEntry ();
			using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
				if (store.FileExists ("download_directories.txt"))
					using (var file = store.OpenFile ("download_directories.txt", FileMode.Open, FileAccess.Read))
						foreach (var line in new StreamReader (file).ReadToEnd ().Split ('\n'))
							downloadDirectoryEntry.Items.Add (line.TrimEnd ());
			Action updateDownload = () => {
				State.Options.DownloaderOptions.OutputPath = downloadDirectoryEntry.TextEntry.Text;
			};
			downloadDirectoryEntry.TextInput += (sender, e) => updateDownload ();
			downloadDirectoryEntry.SelectionChanged += (sender, e) => updateDownload ();
			downloadDirectoryRow.PackStart (downloadDirectoryEntry, true);

			var downloadDirectoryPickerButton = new Button () { Label = "Choose..." };
			downloadDirectoryPickerButton.Clicked += delegate {
				var chooser = new SelectFolderDialog ("Choose directory to store downloads.") { CanCreateFolders = true };
				if (chooser.Run ())
					downloadDirectoryEntry.TextEntry.Text = chooser.Folder;
			};
			downloadDirectoryRow.PackStart (downloadDirectoryPickerButton);

			// solution directory entry
			var solutionDirectoryRow = new HBox ();
			layout.PackStart (solutionDirectoryRow);
			solutionDirectoryRow.PackStart (new Label { Text = "Projects" });
			var solutionDirectoryEntry = new ComboBoxEntry ();
			using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ())
				if (store.FileExists ("project_directories.txt"))
					using (var file = store.OpenFile ("project_directories.txt", FileMode.Open, FileAccess.Read))
						foreach (var line in new StreamReader (file).ReadToEnd ().Split ('\n'))
							solutionDirectoryEntry.Items.Add (line.TrimEnd ());
			Action updateSolutionDirectory = () => {
				State.Options.ProjectCreatorOptions.SolutionDirectory = solutionDirectoryEntry.TextEntry.Text;
			};
			solutionDirectoryEntry.TextInput += (sender, e) => updateSolutionDirectory ();
			solutionDirectoryEntry.SelectionChanged += (sender, e) => updateSolutionDirectory ();
			solutionDirectoryRow.PackStart (solutionDirectoryEntry, true);

			var solutionDirectoryPickerButton = new Button () { Label = "Choose..." };
			solutionDirectoryPickerButton.Clicked += delegate {
				var chooser = new SelectFolderDialog ("Choose directory to create projects.") { CanCreateFolders = true };
				if (chooser.Run ())
					solutionDirectoryEntry.TextEntry.Text = chooser.Folder;
			};
			solutionDirectoryRow.PackStart (solutionDirectoryPickerButton);

			// import button
			import_button = new Button { Label = "Import" };
			import_button.Clicked += (sender, e) => {
				// switch to importing state
				import_button.Sensitive = false;
				this.Content.Cursor = CursorType.Wait; // does not seem to make much sense...

				System.Threading.Tasks.Task.Run (() => {
					using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly ()) {
						using (var file = store.OpenFile ("history.txt", FileMode.OpenOrCreate, FileAccess.Write))
						using (var writer = new StreamWriter (file)) {
							writer.WriteLine (packageIdEntry.TextEntry.Text);
							foreach (string item in packageIdEntry.Items)
								if (packageIdEntry.TextEntry.Text != item && !string.IsNullOrWhiteSpace (item))
									writer.WriteLine (item);
						}
						using (var file = store.OpenFile ("download_directories.txt", FileMode.OpenOrCreate, FileAccess.Write))
						using (var writer = new StreamWriter (file)) {
							writer.WriteLine (downloadDirectoryEntry.TextEntry.Text);
							foreach (string item in downloadDirectoryEntry.Items)
								if (downloadDirectoryEntry.TextEntry.Text != item && !string.IsNullOrWhiteSpace (item))
									writer.WriteLine (item);
						}
						using (var file = store.OpenFile ("solution_directories.txt", FileMode.OpenOrCreate, FileAccess.Write))
						using (var writer = new StreamWriter (file)) {
							writer.WriteLine (solutionDirectoryEntry.TextEntry.Text);
							foreach (string item in solutionDirectoryEntry.Items)
								if (solutionDirectoryEntry.TextEntry.Text != item && !string.IsNullOrWhiteSpace (item))
									writer.WriteLine (item);
						}
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

			if (!string.IsNullOrEmpty (State.Options.DownloaderOptions.OutputPath)) {
				var downloaderResults = new MavenDownloader.Results ();
				downloaderResults.Downloads.BaseDirectory = State.Options.DownloaderOptions.OutputPath ?? Directory.GetCurrentDirectory ();
				d.Download (State.Options.DownloaderOptions, downloaderResults, packages);


				// create project to build
				var c = new BindingProjectCreator ();
				var cr = c.Process (State.Options.ProjectCreatorOptions, downloaderResults.Downloads);

				// build project
				var b = new BindingProjectBuilder ();
				b.Process (State.Options.ProjectBuilderOptions, cr.Projects);
			}
		}
	}

	public class MainState
	{
		public MavenBindingAutomatorOptions Options { get; set; }
	}
}
