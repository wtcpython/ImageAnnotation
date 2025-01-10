using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace ImageAnnotation
{
    public partial class App : Application
    {
        public static List<MainWindow> mainWindows = [];
        public static Config config;
        public static string settingsPath = "./Assets/settings.json";
        public static string AppTitle = "图像标注";
        public static List<string> imageTypes = [".jpg", ".png", ".jpeg"];

        public App()
        {
            this.InitializeComponent();
        }

        public static Config LoadConfig()
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(settingsPath));
            return config;
        }

        public static void SaveConfig()
        {
            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            File.WriteAllText(settingsPath, jsonString);
        }

        public static MainWindow GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (MainWindow window in mainWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        public static MainWindow CreateWindow()
        {
            MainWindow window = new();
            window.Closed += (s, e) =>
            {
                mainWindows.Remove(window);
                SaveConfig();
            };
            mainWindows.Add(window);
            return window;
        }

        public static void SetBackdrop(Window window)
        {
            if (MicaController.IsSupported())
            {
                window.SystemBackdrop = new MicaBackdrop();
            }
            else
            {
                window.SystemBackdrop = new DesktopAcrylicBackdrop();
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = CreateWindow();
            ((MainWindow)m_window).SetContent(new StartPage());

            m_window.Activate();
        }
        public static Window Window => m_window;
        private static Window m_window;
    }
}
