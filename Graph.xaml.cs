using OxyPlot.Series;
using OxyPlot;
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
using System.Runtime.Intrinsics.Arm;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : Page
    {
        public PlotModel graph;
        public LineSeries s1;
        public LineSeries s2;
        public Graph()
        {
            // create plotmodel and line series features 
            InitializeComponent();
            DataContext = this;
            graph = new PlotModel { Title = "Scatter plot"}; // create new graph
            s1 = new LineSeries
            {
                StrokeThickness = 0,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Blue,
                MarkerType = MarkerType.Circle
            };
            s2 = new LineSeries
            {
                StrokeThickness = 1,
                MarkerSize = 0,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.None
            };
        }
        public PlotModel ScatterModel { get; set; } // get scatter model

        // return to linear regression page
        private void BackGraph_Clicked(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page5 home = new Page5();

            frame.NavigationService.Navigate(home);

        }
    }
}
