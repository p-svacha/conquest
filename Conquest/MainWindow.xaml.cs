using Conquest.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Conquest
{
    public partial class MainWindow : Window
    {
        private GameModel Model;
        private Image MapImage;

        public MainWindow()
        {
            InitializeComponent();
            Model = new GameModel();
            
            BitmapImage bitImg = new BitmapImage(new Uri("../../Resources/Maps/test2.png", UriKind.Relative));
            bitImg.CreateOptions = BitmapCreateOptions.None;
            Model.SetMap(bitImg);

            MapImage = Model.GetMapImage();
            Map.Children.Add(MapImage);

            MapImage.MouseMove += new MouseEventHandler(MouseMove);
            MapImage.MouseDown += new MouseButtonEventHandler(MouseDown);
        }

        new void MouseMove(object sender, MouseEventArgs e)
        {
            CoordinatesLabel.Content = (int)e.GetPosition(MapImage).X + " / " + (int)e.GetPosition(MapImage).Y;
        }

        new void MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Model.FloodFill((int)e.GetPosition(MapImage).X, (int)e.GetPosition(MapImage).Y, Model.GetMapPixel((int)e.GetPosition(MapImage).X, (int)e.GetPosition(MapImage).Y), Color.FromArgb(0, 255, 0, 0));
        }


    }
}
