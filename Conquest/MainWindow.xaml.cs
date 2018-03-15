using Conquest.Model;
using Conquest.UI;
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
            UIManager UIManager = new UIManager(InfoPanel, CoordinatesLabel);
            Model = new GameModel(UIManager);
            
            BitmapImage bitImg = new BitmapImage(new Uri("../../Resources/Maps/test4.png", UriKind.Relative));
            MapColumn.Width = new GridLength(bitImg.PixelWidth);
            bitImg.CreateOptions = BitmapCreateOptions.None;
            Model.SetMap(bitImg);

            Map.Children.Add(Model.GetMapImage());

            MouseMove += new MouseEventHandler(MainWindowMouseMoved);
            MouseDown += new MouseButtonEventHandler(MainWindowMouseDown);
        }

        private void MainWindowMouseMoved(object sender, MouseEventArgs e)
        {
            Model.MouseMove(sender, e);
        }

        private void MainWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.MouseDown(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Model.StartGame(Convert.ToInt32(NumPlayers.Text), Convert.ToInt32(NumStartingCountries.Text), Convert.ToInt32(NumStartingArmy.Text));
        }
    }
}
