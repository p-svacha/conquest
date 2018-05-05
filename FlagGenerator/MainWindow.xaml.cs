using FlagGeneration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlagGeneration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FlagModel Model;

        public MainWindow()
        {
            Model = new FlagModel();
            InitializeComponent();
        }

        private void GenerateFlag_Click(object sender, RoutedEventArgs e)
        {
            Model.GenerateFlag();
            FlagPanel.Children.Clear();
            FlagPanel.Children.Add(Model.GetFlagImage());
        }
    }
}
