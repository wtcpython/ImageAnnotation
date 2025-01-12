using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Windows.Foundation;


namespace ImageAnnotation
{
    public sealed partial class MainPage : UserControl
    {
        private List<Annotation> annotations = [];
        private ObservableCollection<BitmapImage> images = [];
        private List<FileInfo> fileInfos = [];

        private bool isDrawing = false;

        private Project currentProject;
        private Annotation anno;

        public MainPage(Project project, bool isNewProject)
        {
            this.InitializeComponent();
            currentProject = project;
            projLabel.Text = project.Name;
            Gallery.SelectedIndex = 0;
            this.Loaded += (s, e) => InitProject(project, isNewProject);
        }

        public void InitProject(Project project, bool isNewProject)
        {
            if (isNewProject)
            {
                App.config.Projects.Add(project);
            }
            string[] files = Directory.GetFiles(project.Folder);

            foreach (string file in files)
            {
                FileInfo fileInfo = new(file);
                string extensions = fileInfo.Extension;
                if (App.imageTypes.Contains(extensions))
                {
                    BitmapImage image = new(new Uri(file));
                    images.Add(image);
                    annotations.Add(new Annotation());
                    fileInfos.Add(fileInfo);
                }
            }
            Gallery.ItemsSource = images;
            tagBox.ItemsSource = project.Tokens;

            CheckAndSetState();
        }

        private void ImagePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (annoBox.IsChecked == true)
            {
                isDrawing = !isDrawing;
                anno.StartGlobalPoint = e.GetCurrentPoint(background).Position;
                anno.StartLocalPoint = e.GetCurrentPoint(Gallery).Position;

                annotation.Visibility = Visibility.Visible;
                annotation.Width = 0;
                annotation.Height = 0;

                Canvas.SetLeft(annotation, anno.StartGlobalPoint.X);
                Canvas.SetTop(annotation, anno.StartGlobalPoint.Y);
            }
        }

        private void ImagePointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isDrawing)
            {
                Point currentPoint = e.GetCurrentPoint(background).Position;
                double width = currentPoint.X - anno.StartGlobalPoint.X;
                double height = currentPoint.Y - anno.StartGlobalPoint.Y;

                annotation.Width = Math.Abs(width);
                annotation.Height = Math.Abs(height);

                if (width < 0)
                    Canvas.SetLeft(annotation, currentPoint.X);
                else
                    Canvas.SetLeft(annotation, anno.StartGlobalPoint.X);

                if (height < 0)
                    Canvas.SetTop(annotation, currentPoint.Y);
                else
                    Canvas.SetTop(annotation, anno.StartGlobalPoint.Y);
            }
        }

        private void ImagePointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (annoBox.IsChecked == true && isDrawing)
            {
                isDrawing = false;

                anno.EndGlobalPoint = e.GetCurrentPoint(background).Position;
                anno.EndLocalPoint = e.GetCurrentPoint(Gallery).Position;
                double startPercentX = anno.StartLocalPoint.X / Gallery.ActualWidth;
                double startPercentY = anno.StartLocalPoint.Y / Gallery.ActualHeight;
                double endPercentX = anno.EndLocalPoint.X / Gallery.ActualWidth;
                double endPercentY = anno.EndLocalPoint.Y / Gallery.ActualHeight;

                anno.CenterX = (endPercentX + startPercentX) / 2;
                anno.CenterY = (endPercentY + startPercentY) / 2;
                anno.Width = endPercentX - startPercentX;
                anno.Height = endPercentY - startPercentY;

                if (CheckAndSetState())
                {
                    anno.ImageTagIndex = tagBox.SelectedIndex;
                    annotations[Gallery.SelectedIndex] = anno;
                }
            }
        }

        private void ChangeAnnoStatus(object sender, RoutedEventArgs e)
        {
            if (annoBox.IsChecked == false)
            {
                isDrawing = false;
                annotation.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveAnnotation(object sender, RoutedEventArgs e)
        {
            string tagsDirectory = Path.Combine(fileInfos[0].DirectoryName, "tags");
            if (!Directory.Exists(tagsDirectory))
            {
                Directory.CreateDirectory(tagsDirectory);
            }

            for (int i = 0; i < fileInfos.Count; i++)
            {
                FileInfo fileInfo = fileInfos[i];
                string name = fileInfo.Name;

                string txtFileName = name + ".txt";
                string filePath = Path.Combine(tagsDirectory, txtFileName);

                Annotation anno = annotations[i];
                string data = $"{anno.ImageTagIndex} {anno.CenterX} {anno.CenterY} {anno.Width} {anno.Height}\n";

                File.WriteAllText(filePath, data);
            }

            Process.Start("explorer.exe", $"/select,\"{tagsDirectory}\"");
        }

        private void ImageChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Gallery.SelectedIndex;
            MainWindow window = App.GetWindowForElement(this);
            window.SetTitleFileName(fileInfos[index].FullName);
            annotation.Visibility = Visibility.Collapsed;
            anno = annotations[index];
            CheckAndSetState();
        }

        private bool CheckAndSetState()
        {
            int index = tagBox.SelectedIndex;

            if (anno.StartLocalPoint.X > 0 && anno.StartLocalPoint.Y > 0 && index >= 0)
            {
                statusText.Text = "标注成功";
                statusText.Foreground = new SolidColorBrush(Colors.Green);
                anno.IsValid = true;
                return true;
            }
            else
            {
                statusText.Text = "内容未标注";
                statusText.Foreground = new SolidColorBrush(Colors.Red);
                anno.IsValid = false;
                return false;
            }
        }

        private void SelectedTagChanged(object sender, SelectionChangedEventArgs e)
        {
            SetImageTag(tagBox.SelectedIndex, tagBox.SelectedItem.ToString());
        }

        private void AddNewImageTag(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            string text = args.Text;
            if (!currentProject.Tokens.Contains(text))
            {
                currentProject.Tokens.Add(text);

                int index = currentProject.Tokens.IndexOf(text);
                SetImageTag(index, text);
            }
        }

        private void SetImageTag(int index, string text)
        {
            int gindex = Gallery.SelectedIndex;
            anno.ImageTag = text;
            anno.ImageTagIndex = index;
            CheckAndSetState();

            annotations[gindex] = anno;
        }

        private async void DeleteImage(object sender, RoutedEventArgs e)
        {
            int index = Gallery.SelectedIndex;
            FileInfo deleteInfo = fileInfos[index];

            string text = deleteInfo.FullName;
            deleteText.Text = text;

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Gallery.SelectedIndex = (index + 1) % images.Count;
                annotations.RemoveAt(index);
                images.RemoveAt(index);
                deleteInfo.Delete();
                fileInfos.RemoveAt(index);
            }
        }
    }
}
