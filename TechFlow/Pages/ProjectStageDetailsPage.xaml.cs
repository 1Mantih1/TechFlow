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
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class ProjectStageDetailsPage : Page
    {
        public ProjectStageDetailsPage()
        {
            InitializeComponent();
        }

        private async void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as ProjectManagement;
            if (mainWindow != null)
            {
                await mainWindow.GoBack();
            }
        }
    }
}
