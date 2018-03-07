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

        public MainWindow()
        {
            InitializeComponent();
            Model = new GameModel();
            
            WriteableBitmap bitmap = new WriteableBitmap((int) (Map.Width), (int) (Map.Height), 96, 96, PixelFormats.Bgr32, null);
            BitmapImage bitImg = new BitmapImage(new Uri("../../Resources/Maps/test.png", UriKind.Relative));
            bitImg.CreateOptions = BitmapCreateOptions.None;
            Model.SetMap(bitImg);
            for(int y = 0; y < bitImg.PixelHeight; y++)
            {
                for(int x = 0; x < bitImg.PixelWidth; x++)
                {
                    Model.SetMapPixel(x, y, Model.GetMapPixel(x,y));
                }
            }

            Image mapImage = Model.GetMapImage();
            Map.Children.Add(mapImage);

            mapImage.MouseMove += new MouseEventHandler(MouseMove);
        }

        new void MouseMove(object sender, MouseEventArgs e)
        {
            CoordinatesLabel.Content = (int)e.GetPosition(img).X + " / " + (int)e.GetPosition(img).Y;
        }


    }
}
