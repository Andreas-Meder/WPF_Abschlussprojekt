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

public partial class ChangePasswordWindow : Window
{
    public ChangePasswordWindow()
    {
        InitializeComponent();
    }

    private void OldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.OldPasswort != pb.Password) vm.OldPasswort = pb.Password;
        }
    }

    private void OldPasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is MainViewModel vm)
        {
            if (pb.IsVisible && pb.Password != vm.OldPasswort) pb.Password = vm.OldPasswort;
        }
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.NewPasswort != pb.Password) vm.NewPasswort = pb.Password;
        }
    }

    private void NewPasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is MainViewModel vm)
        {
            if (pb.IsVisible && pb.Password != vm.NewPasswort) pb.Password = vm.NewPasswort;
        }
    }

    private void NewPasswordWdhBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is PasswordBox pb)
        {
            if (vm.NewPasswortWdh != pb.Password) vm.NewPasswortWdh = pb.Password;
        }
    }

    private void NewPasswordWdhBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is MainViewModel vm)
        {
            if (pb.IsVisible && pb.Password != vm.NewPasswortWdh) pb.Password = vm.NewPasswortWdh;
        }
    }
}