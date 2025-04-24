using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using TechFlow.Classes;
using TechFlow.Models;
using TechFlow.Pages;
using Path = System.IO.Path;

namespace TechFlow.Windows
{
    public partial class ProjectManagement : Window
    {
        private readonly Dictionary<Type, Page> _pages = new Dictionary<Type, Page>();
        private Button _currentActiveButton;
        private bool _isEditProfileOpen;
        private bool _isTransitionInProgress;
        private readonly Stack<Page> _navigationStack = new Stack<Page>();

        public ProjectManagement()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;


            InitializeComponent();
            InitializePages();
            InitializeApplication();
        }

        private void InitializePages()
        {
            // Инициализация всех страниц
            _pages[typeof(ProjectsPage)] = new ProjectsPage();
            _pages[typeof(ProjectStagesPage)] = new ProjectStagesPage();
            _pages[typeof(TeamsPage)] = new TeamsPage();
            _pages[typeof(TasksPage)] = new TasksPage();
            _pages[typeof(TimesheetPage)] = new TimesheetPage();
            _pages[typeof(AdminPanelPage)] = new AdminPanelPage();
            _pages[typeof(EditProfilePage)] = new EditProfilePage();

            // Подписка на события выбора
            if (_pages[typeof(ProjectsPage)] is ProjectsPage projectsPage)
                projectsPage.OnProjectSelected += HandleSelection;

            if (_pages[typeof(TasksPage)] is TasksPage tasksPage)
                tasksPage.OnTaskSelected += HandleSelection;

            if (_pages[typeof(TeamsPage)] is TeamsPage teamsPage)
                teamsPage.OnTeamSelected += HandleSelection;

            if (_pages[typeof(ProjectStagesPage)] is ProjectStagesPage stagesPage)
                stagesPage.OnProjectStageSelected += HandleSelection;
        }

        private void InitializeApplication()
        {
            Authorization.OnUserUpdated += UpdateUserInfo;
            ContentFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            LoadUserData();

            // Первоначальная загрузка страницы проектов
            var initialPage = _pages[typeof(ProjectsPage)];
            ContentFrame.Content = initialPage;
            ActivateButton(ProjectButton);
        }

        private void LoadUserData()
        {
            if (Authorization.currentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован!");
                return;
            }

            UpdateUserInfo(Authorization.currentUser);
        }

        // Метод для создания анимации прозрачности
        private Storyboard CreateOpacityAnimation(FrameworkElement target, double toValue, TimeSpan duration)
        {
            var animation = new DoubleAnimation
            {
                To = toValue,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
            storyboard.Children.Add(animation);

            return storyboard;
        }

        // Основной метод навигации
        private async Task NavigateToPage(Page page)
        {
            if (_isTransitionInProgress || ContentFrame.Content == page)
                return;

            _isTransitionInProgress = true;

            try
            {
                // Сохраняем текущую страницу в стек (кроме EditProfilePage)
                if (ContentFrame.Content != null && !(page is EditProfilePage))
                {
                    _navigationStack.Push(ContentFrame.Content as Page);
                }

                // Анимация выхода старой страницы
                if (ContentFrame.Content is FrameworkElement oldPage)
                {
                    var exitStoryboard = CreateOpacityAnimation(oldPage, 0, TimeSpan.FromMilliseconds(200));
                    await exitStoryboard.BeginAsync(oldPage);
                }

                // Обновляем контент
                ContentFrame.Content = page;

                // Анимация появления новой страницы
                if (page is FrameworkElement newPage)
                {
                    // Устанавливаем начальную прозрачность новой страницы
                    newPage.Opacity = 0;

                    // Кэшируем страницу для плавной отрисовки
                    newPage.CacheMode = new BitmapCache();

                    // Даем время для рендеринга страницы
                    await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

                    // Создаем и запускаем анимацию появления новой страницы
                    var enterStoryboard = CreateOpacityAnimation(newPage, 1, TimeSpan.FromMilliseconds(200));
                    await enterStoryboard.BeginAsync(newPage);
                }

                // Обновляем активную кнопку для текущей страницы
                UpdateActiveButtonForPage(page.GetType());
            }
            finally
            {
                _isTransitionInProgress = false;
            }
        }

        // Метод для навигации назад
        public async Task GoBack()
        {
            if (_isTransitionInProgress || _navigationStack.Count == 0)
                return;

            _isTransitionInProgress = true;

            try
            {
                // Анимация исчезновения текущей страницы
                if (ContentFrame.Content is FrameworkElement oldPage)
                {
                    var exitStoryboard = CreateOpacityAnimation(oldPage, 0, TimeSpan.FromMilliseconds(200));
                    await exitStoryboard.BeginAsync(oldPage);
                }

                // Получаем предыдущую страницу из стека
                var previousPage = _navigationStack.Pop();

                // Навигация назад
                ContentFrame.Content = previousPage;

                // Анимация появления предыдущей страницы
                if (previousPage is FrameworkElement newPage)
                {
                    newPage.Opacity = 0;
                    await Task.Delay(10); // Мгновенная задержка перед запуском анимации

                    // Анимация появления страницы
                    var enterStoryboard = CreateOpacityAnimation(newPage, 1, TimeSpan.FromMilliseconds(200));
                    await enterStoryboard.BeginAsync(newPage);
                }

                // Обновляем активную кнопку для предыдущей страницы
                UpdateActiveButtonForPage(previousPage.GetType());
            }
            finally
            {
                _isTransitionInProgress = false;
            }
        }



        private void UpdateActiveButtonForPage(Type pageType)
        {
            Dispatcher.Invoke(() =>
            {
                if (pageType == typeof(ProjectsPage))
                    ActivateButton(ProjectButton);
                else if (pageType == typeof(TasksPage))
                    ActivateButton(TaskButton);
                else if (pageType == typeof(TeamsPage))
                    ActivateButton(TeamButton);
                else if (pageType == typeof(ProjectStagesPage))
                    ActivateButton(ProjectStageButton);
                else if (pageType == typeof(TimesheetPage))
                    ActivateButton(TimesheetButton);
                else
                    ActivateButton(null);
            });
        }

        private void ActivateButton(Button activeButton)
        {
            if (_currentActiveButton == activeButton) return;

            if (_currentActiveButton != null)
            {
                _currentActiveButton.Style = (Style)FindResource("MenuButtonStyle");
            }

            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
            }

            _currentActiveButton = activeButton;
        }

        private async void MainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickAnimation = (Storyboard)FindResource("AvatarClickAnimation");
            clickAnimation.Begin(mainGrid);

            if (_isEditProfileOpen)
            {
                // Возвращаемся на предыдущую страницу
                await GoBack();
                _isEditProfileOpen = false;
            }
            else
            {
                // Сохраняем текущую страницу перед переходом на страницу редактирования
                if (ContentFrame.Content != null && !(ContentFrame.Content is EditProfilePage))
                {
                    _navigationStack.Push(ContentFrame.Content as Page);
                }
                await NavigateToPage(_pages[typeof(EditProfilePage)]);
                _isEditProfileOpen = true;
            }

            e.Handled = true;
        }

        public void HandleSelection(object selectedItem)
        {
            Dispatcher.Invoke(() => {
                Page page = null;

                switch (selectedItem)
                {
                    case Project selectedProject:
                        page = new ProjectDetailsPage(selectedProject.ProjectId);
                        break;
                    case ProjectStage selectedStage:
                        page = new ProjectStageDetailsPage { DataContext = selectedStage };
                        break;
                    case Team selectedTeam:
                        page = new TeamDetailsPage(selectedTeam);
                        break;
                    case ProjectTask selectedTask:
                        page = new TaskDetailsPage { DataContext = selectedTask };
                        break;
                }

                if (page != null)
                {
                    NavigateToPage(page).ConfigureAwait(false);
                }
            });
        }

        private void UpdateUserInfo(User updatedUser)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    userName.Text = $"{updatedUser.LastName} {updatedUser.FirstName}";
                    userRole.Text = updatedUser.RoleName ?? "Пользователь"; // Добавлено отображение роли
                    LoadUserAvatar(updatedUser.ImagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении информации пользователя: {ex.Message}");
                    SetDefaultAvatar();
                }
            });
        }

        private void LoadUserAvatar(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                SetDefaultAvatar();
                return;
            }

            // Пробуем несколько вариантов расположения файла
            string[] possiblePaths =
            {
        // 1. Проверяем как абсолютный путь
        imagePath,
        
        // 2. Проверяем относительно корня приложения
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath),
        
        // 3. Проверяем относительно папки avatar (для разработки)
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../avatar", Path.GetFileName(imagePath)),
        
        // 4. Проверяем в папке Resources
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", Path.GetFileName(imagePath))
    };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                        bitmap.EndInit();
                        bitmap.Freeze();

                        AvatarImage.Source = bitmap;
                        return;
                    }
                }
                catch { /* Продолжаем проверять другие пути */ }
            }

            // Если ни один вариант не сработал
            SetDefaultAvatar();
        }

        private void SetDefaultAvatar()
        {
            string[] defaultPaths =
            {
        // 1. Стандартный путь в папке Resources
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Images", "default_avatar.png"),
        
        // 2. Альтернативный путь в папке avatar
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../avatar", "man1.png"),
        
        // 3. Встроенный ресурс
        "pack://application:,,,/YourAppName;component/Resources/default_avatar.png"
    };

            foreach (var path in defaultPaths)
            {
                try
                {
                    if (path.StartsWith("pack:") || File.Exists(path))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        bitmap.EndInit();
                        bitmap.Freeze();

                        AvatarImage.Source = bitmap;
                        return;
                    }
                }
                catch { /* Продолжаем пробовать другие пути */ }
            }

            // Если совсем ничего не работает
            AvatarImage.Source = null;
        }

        private async void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActiveButton != ProjectButton)
            {
                await NavigateToPage(_pages[typeof(ProjectsPage)]);
                _isEditProfileOpen = false;
            }
        }

        private async void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActiveButton != TaskButton)
            {
                await NavigateToPage(_pages[typeof(TasksPage)]);
                _isEditProfileOpen = false;
            }
        }

        private async void TeamButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActiveButton != TeamButton)
            {
                await NavigateToPage(_pages[typeof(TeamsPage)]);
                _isEditProfileOpen = false;
            }
        }

        private async void ProjectStageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActiveButton != ProjectStageButton)
            {
                await NavigateToPage(_pages[typeof(ProjectStagesPage)]);
                _isEditProfileOpen = false;
            }
        }

        private async void TimesheetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActiveButton != TimesheetButton)
            {
                await NavigateToPage(_pages[typeof(TimesheetPage)]);
                _isEditProfileOpen = false;
            }
        }

        private async void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            // Если текущая страница уже является AdminPanelPage, возвращаемся назад
            if (ContentFrame.Content is AdminPanelPage)
            {
                await GoBack();
                _isEditProfileOpen = false;
            }
            else
            {
                // Сохраняем текущую страницу перед переходом на AdminPanelPage
                if (ContentFrame.Content != null)
                {
                    _navigationStack.Push(ContentFrame.Content as Page);
                }
                await NavigateToPage(_pages[typeof(AdminPanelPage)]);
                _isEditProfileOpen = false;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new Authorization();
            authWindow.Show();
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.BrowserBack)
            {
                e.Handled = true;
                GoBack().ConfigureAwait(false);
            }
            base.OnKeyDown(e);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowMaximize;
            }
            else
            {
                WindowState = WindowState.Maximized;
                ((PackIcon)((Button)sender).Content).Kind = PackIconKind.WindowRestore;
            }
        }



        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double radius = (WindowState == WindowState.Maximized) ? 0 : 16;

            // Убедись, что границы обрезаются корректно
            this.Clip = new RectangleGeometry(
                new Rect(0, 0, ActualWidth, ActualHeight),
                radius,
                radius
            );

            // Обнови CornerRadius только здесь, чтобы синхронизировать с Clip
            MainBorder.CornerRadius = new CornerRadius(radius);
        }




        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}