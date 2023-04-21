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
using System.Windows.Shapes;

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page5.xaml
    /// </summary>
    public partial class Page5 : Window
    {
        public Page5()
        {
            InitializeComponent();
        }

        private void ToHome_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page1 home = new Page1();

            frame.NavigationService.Navigate(home);

        }

        private void FileButton_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void CalcButton_Clicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
