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
using TechFlow.Windows;

namespace TechFlow.Pages
{
    public partial class AvatarsPage : Page
    {
        private EditProfilePage editProfilePage;
        public AvatarsPage(EditProfilePage editProfilePage)
        {
            InitializeComponent();
            this.editProfilePage = editProfilePage;
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
            if (clickedImage != null)
            {
                string avatarPath = clickedImage.Source.ToString();
                editProfilePage.UpdateAvatar(avatarPath);
            }
        }
    }
}
