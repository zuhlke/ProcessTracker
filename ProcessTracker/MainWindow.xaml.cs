using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace ProcessTracker
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, PTObserver
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PTManager manager = (PTManager)this.DataContext;
            manager.SetObserver(this);
            dateView.ItemsSource = manager.GetDateTable().DefaultView;
        }

        private void dateView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)e.AddedItems[0];
            DateTime datetime = (DateTime)row.Row.ItemArray[0];
            PTManager manager = (PTManager) this.DataContext;
            processInfoView.ItemsSource = manager.GetProcessInfoTableForDate(datetime).DefaultView;
            Console.WriteLine("Mouse Down");
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            PTManager manager = (PTManager)this.DataContext;
            manager.CleanUp();
        }

        public void update() 
        {
            PTManager manager = (PTManager)this.DataContext;
            dateView.ItemsSource = manager.GetDateTable().DefaultView;
            if (dateView.SelectedItem != null) 
            {
                DateTime datetime = (DateTime)((DataRowView)dateView.SelectedItem).Row.ItemArray[0];
                processInfoView.ItemsSource = manager.GetProcessInfoTableForDate(datetime).DefaultView;
            }
        }
    }
}
