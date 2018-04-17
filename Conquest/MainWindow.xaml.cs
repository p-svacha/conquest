using Conquest.MapGeneration;
using Conquest.MapClasses;
using Conquest.Model;
using Conquest.UI;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Conquest.Windows;

namespace Conquest
{
    public partial class MainWindow : Window
    {
        private GameModel Model;

        public MainWindow()
        {
            InitializeComponent();
            UIManager UIManager = new UIManager(InfoPanel, MapPanel, CoordinatesLabel, NearestBorderLabel, PlayerOrder, GraphNumCountry, GraphArmy, GraphDensity);
            Model = new GameModel(this, UIManager, AutoRun, RunSpeed, true);

            MapPanel.Children.Add(Model.GetMapImage());

            MouseMove += new MouseEventHandler(MainWindowMouseMoved);
            MouseDown += new MouseButtonEventHandler(MainWindowMouseDown);

            new Thread(() =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Model.Update();
                    });
                }
            }).Start();
        }

        private void MainWindowMouseMoved(object sender, MouseEventArgs e)
        {
            Model.MouseMove(sender, e);
        }

        private void MainWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.MouseDown(sender, e);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Model.StartGame(Convert.ToInt32(NumPlayers.Text), Convert.ToInt32(NumStartingCountries.Text), Convert.ToInt32(NumStartingArmy.Text));
        }

        private void NextTurn_Click(object sender, RoutedEventArgs e)
        {
            Model.NextTurn();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Model.StopGame();
        }

        private void GenerateMap_Click(object sender, RoutedEventArgs e)
        {
            Model.GenerateMap(int.Parse(MapWidth.Text), int.Parse(MapHeight.Text), int.Parse(MinCountrySize.Text), int.Parse(CountriesPerOcean.Text), int.Parse(WaterConnectionMinCountryDistance.Text), int.Parse(WaterConnectionMaxAirlineDistance.Text), (float)(CountryAmountScale.Value));
            MapPanel.Children.Clear();
            MapPanel.Children.Add(Model.GetMapImage());
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            MapSelection selectionWindow = new MapSelection();
            selectionWindow.Show();
            this.Close();
        }
    }
}
