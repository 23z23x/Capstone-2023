﻿using System;
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

namespace AMLA
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        //To About
        private void ToAbout_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page2 about = new Page2();

            frame.NavigationService.Navigate(about);
        }

        private void ToGenetic_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page3 about = new Page3();

            frame.NavigationService.Navigate(about);
        }

        private void ToBayes_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page4 about = new Page4();

            frame.NavigationService.Navigate(about);
        }

        private void ToReg_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page5 about = new Page5();

            frame.NavigationService.Navigate(about);

        }

        private void ToApriori_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page6 about = new Page6();

            frame.NavigationService.Navigate(about);

        }

        private void ToCART_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            var frame = parentWindow.FindName("MainFrame") as Frame;

            Page7 about = new Page7();

            frame.NavigationService.Navigate(about);

        }

    }
}
