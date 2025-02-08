using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace ImageAnnotation
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            AppTitleBar.Title = AppWindow.Title = App.AppTitle;
            App.SetBackdrop(this);
            AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
        }

        public void SetContent(UserControl content)
        {
            ContentGrid.Children.Add(content);
        }

        public void SetTitleFileName(string text)
        {
            filePath.Text = text;
        }
    }
}
