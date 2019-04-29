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
using Microsoft.Win32;

namespace CustomerLogger
{
    /// <summary>
    /// Interaction logic for NewPasswordWindow.xaml
    /// </summary>
    public partial class NewPasswordWindow : Window
    {
        //  Constructor ///////////////////////////////////////////////////////////////////////////

        public NewPasswordWindow()
        {
            InitializeComponent();
        }

        //  Private Functions   ///////////////////////////////////////////////////////////////////

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\CustomerLogger");

            byte[] data = System.Text.Encoding.ASCII.GetBytes(NewPassword_PswdBox.Password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            string sHashedPassword = System.Text.Encoding.ASCII.GetString(data);

            reg.SetValue("Admin_Password", sHashedPassword, RegistryValueKind.String);

            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfirmPassword_PswdBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPassword_PswdBox.Password = "";
            NewPassword_Unconfirmed();
        }

        private void ConfirmPassword_PswdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ConfirmPassword_PswdBox.Password == NewPassword_PswdBox.Password)
                NewPassword_Confirmed();
            else
                NewPassword_Unconfirmed();
        }

        private void NewPassword_Confirmed()
        {
            ConfirmPassword_PswdBox.Background = System.Windows.Media.Brushes.LightGreen;
            ConfirmPassword_PswdBox.BorderBrush = System.Windows.Media.Brushes.LawnGreen;

            Ok_Button.IsEnabled = true;
            Ok_Button.Focus();
        }

        private void NewPassword_Unconfirmed()
        {
            ConfirmPassword_PswdBox.Background = System.Windows.Media.Brushes.DarkRed;
            ConfirmPassword_PswdBox.BorderBrush = System.Windows.Media.Brushes.Red;

            Ok_Button.IsEnabled = false;
        }
    }
}
