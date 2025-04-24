using System.Windows;

namespace TechFlow.Classes
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached(
                "PasswordLength",
                typeof(int),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(0));

        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }
    }
}