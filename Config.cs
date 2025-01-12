using System.Collections.ObjectModel;
using Windows.Foundation;

namespace ImageAnnotation
{
    public class Annotation
    {
        public string ImageTag { get; set; }
        public int ImageTagIndex { get; set; }
        public Point StartGlobalPoint { get; set; }
        public Point StartLocalPoint { get; set; }
        public Point EndGlobalPoint { get; set; }
        public Point EndLocalPoint { get; set; }

        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsValid { get; set; }
    }

    public class Project
    {
        public string Folder {  get; set; }
        public string Name { get; set; }

        public ObservableCollection<string> Tokens { get; set; }
    }
    public class Config
    {
        public ObservableCollection<Project> Projects {  get; set; }
    }
}
