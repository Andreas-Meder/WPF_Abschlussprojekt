using Passwortspeicher.ViewModels;
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

namespace Passwortspeicher;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
    }

    private void RegPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.RegPasswort != pb.Password) vm.RegPasswort = pb.Password;
        }
    }

    private void RegPasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is MainViewModel vm)
        {
            if (pb.IsVisible && pb.Password != vm.RegPasswort) pb.Password = vm.RegPasswort;
        }
    }

    private void RegPasswordWdhBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.RegPasswortWdh != pb.Password) vm.RegPasswortWdh = pb.Password;
        }
    }

    private void RegPasswordWdhBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is MainViewModel vm)
        {
            if (pb.IsVisible && pb.Password != vm.RegPasswortWdh) pb.Password = vm.RegPasswortWdh;
        }
    }
}