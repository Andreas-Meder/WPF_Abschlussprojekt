using Passwortspeicher.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Passwortspeicher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.InputPasswort != pb.Password)
            {
                vm.InputPasswort = pb.Password;
            }
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Garantiert das leere Passwortfeld im UI direkt beim Abmelden
        LoginPasswordBox.Clear();
    }
}