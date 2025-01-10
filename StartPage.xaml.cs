using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;


namespace ImageAnnotation
{
    public sealed partial class StartPage : UserControl
    {
        public StartPage()
        {
            this.InitializeComponent();
            Config config = App.LoadConfig();
            projectView.ItemsSource = App.config.Projects;
        }

        private async Task<StorageFolder> SelectFolder()
        {
            FolderPicker picker = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                ViewMode = PickerViewMode.List
            };
            picker.FileTypeFilter.Add("*");
            IntPtr hwnd = Win32Interop.GetWindowFromWindowId(App.Window.AppWindow.Id);

            InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSingleFolderAsync();
        }

        private async void CreateNewProject(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await SelectFolder();
            if (folder != null)
            {
                string[] files = Directory.GetFiles(folder.Path);
                if (files.Length > 0)
                {
                    await dialog.ShowAsync();
                    string name = projectBox.Text;
                    if (name == string.Empty)
                    {
                        name = "未命名项目";
                    }
                    ObservableCollection<string> tokens = [];
                    foreach (var item in tagTokenBox.Items)
                    {
                        if (item is string token)
                        {
                            tokens.Add(token);
                        }
                    }

                    Project project = new()
                    {
                        folder = folder.Path,
                        name = name,
                        tokens = tokens
                    };
                    OpenProject(project, true);
                }
            }
        }

        private void ProjectItemClick(object sender, ItemClickEventArgs e)
        {
            Project selectedProject = e.ClickedItem as Project;
            OpenProject(selectedProject, false);
        }

        private void OpenProject(Project project, bool isNewProject)
        {
            MainWindow window = App.CreateWindow();
            MainPage mainPage = new(project, isNewProject);
            window.SetContent(mainPage);
            window.Activate();
            App.Window.Close();
        }

        private void RemoveProject(object sender, RoutedEventArgs e)
        {
            Project project = (sender as Button).DataContext as Project;
            App.config.Projects.Remove(project);
        }
    }
}
