using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Windows;

namespace TechFlow.Pages
{
    /// <summary>
    /// Логика взаимодействия для TasksPage.xaml
    /// </summary>
    public partial class TasksPage : Page
    {
        public ObservableCollection<ProjectTask> Tasks { get; set; }
        public event Action<ProjectTask> OnTaskSelected;
        //public Frame ContentFrame { get; set; }


        public TasksPage()
        {
            InitializeComponent();
            LoadTasks();
            DataContext = this;
        }

        private void LoadTasks()
        {
            TaskFromDb taskFromDb = new TaskFromDb();
            List<ProjectTask> taskList = taskFromDb.LoadTasks();
            Tasks = new ObservableCollection<ProjectTask>(taskList);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedTask = (ProjectTask)((DataGrid)sender).SelectedItem;
            OnTaskSelected?.Invoke(selectedTask);
        }





        public int CurrentProjectCount
        {
            get
            {
                return Tasks.Count;
            }
        }

        public int CurrentActiveProjectsCount
        {
            get
            {
                int activeCount = 0;
                foreach (var task in Tasks)
                {
                    if (task.StatusName == "Активный")
                    {
                        activeCount++;
                    }
                }
                return activeCount;
            }
        }
    }
}
