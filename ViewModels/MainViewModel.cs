using Passwortspeicher.Models;
using Passwortspeicher.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Passwortspeicher.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storageService = new();
    private List<Benutzer> _benutzerListe = new();
    private Benutzer? _aktiverBenutzer;
    private List<PasswortEintrag> _alleEintraege = new();

    private byte[]? _masterKey;
    private RegisterWindow? _currentRegisterWindow;
    private ChangePasswordWindow? _currentChangePasswordWindow;

    public MainViewModel()
    {
        _benutzerListe = _storageService.LoadUsers();
        Eintraege = new ObservableCollection<PasswortEintrag>();

        LoginCommand = new RelayCommand(ExecuteLogin);
        LogoutCommand = new RelayCommand(ExecuteLogout, _ => IsLoggedIn);

        OpenRegisterWindowCommand = new RelayCommand(OpenRegisterWindow);
        CloseRegisterWindowCommand = new RelayCommand(_ => CloseRegisterWindow());
        ExecuteRegisterCommand = new RelayCommand(ExecuteRegister);

        OpenChangePasswordWindowCommand = new RelayCommand(OpenChangePasswordWindow, _ => IsLoggedIn);
        CloseChangePasswordWindowCommand = new RelayCommand(_ => CloseChangePasswordWindow());
        ExecuteChangePasswordCommand = new RelayCommand(ExecuteChangePassword);

        SaveEintragCommand = new RelayCommand(ExecuteSaveEintrag, _ => IsLoggedIn);
        DeleteEintragCommand = new RelayCommand(ExecuteDeleteEintrag, _ => SelectedEintrag != null);
        NewEintragCommand = new RelayCommand(_ => ClearInputFields(), _ => IsLoggedIn);
        GeneratePasswordCommand = new RelayCommand(_ => NeuPasswort = CryptoService.GenerateRandomPassword());

        // Toggle Commands mit Notification-Sync
        ToggleLoginPasswordVisibilityCommand = new RelayCommand(_ =>
        {
            IsLoginPasswordVisible = !IsLoginPasswordVisible;
            OnPropertyChanged(nameof(InputPasswort));
        });

        ToggleRegPasswordVisibilityCommand = new RelayCommand(_ =>
        {
            IsRegPasswordVisible = !IsRegPasswordVisible;
            OnPropertyChanged(nameof(RegPasswort));
        });

        ToggleRegPasswordWdhVisibilityCommand = new RelayCommand(_ =>
        {
            IsRegPasswordWdhVisible = !IsRegPasswordWdhVisible;
            OnPropertyChanged(nameof(RegPasswortWdh));
        });

        ToggleOldPasswordVisibilityCommand = new RelayCommand(_ =>
        {
            IsOldPasswordVisible = !IsOldPasswordVisible;
            OnPropertyChanged(nameof(OldPasswort));
        });

        ToggleNewPasswordVisibilityCommand = new RelayCommand(_ =>
        {
            IsNewPasswordVisible = !IsNewPasswordVisible;
            OnPropertyChanged(nameof(NewPasswort));
        });

        ToggleNewPasswordWdhVisibilityCommand = new RelayCommand(_ =>
        {
            IsNewPasswordWdhVisible = !IsNewPasswordWdhVisible;
            OnPropertyChanged(nameof(NewPasswortWdh));
        });
    }

    // Login Properties
    public string InputBenutzername { get; set; } = string.Empty;
    public string InputPasswort { get; set; } = string.Empty;

    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set { _isLoggedIn = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLoggedOut)); }
    }
    public bool IsLoggedOut => !IsLoggedIn;

    // Registrierung Properties
    private string _regBenutzername = string.Empty;
    public string RegBenutzername
    {
        get => _regBenutzername;
        set { _regBenutzername = value; OnPropertyChanged(); }
    }

    private string _regPasswort = string.Empty;
    public string RegPasswort
    {
        get => _regPasswort;
        set { _regPasswort = value; OnPropertyChanged(); }
    }

    private string _regPasswortWdh = string.Empty;
    public string RegPasswortWdh
    {
        get => _regPasswortWdh;
        set { _regPasswortWdh = value; OnPropertyChanged(); }
    }

    // Login-Passwort ändern Properties
    private string _oldPasswort = string.Empty;
    public string OldPasswort
    {
        get => _oldPasswort;
        set { _oldPasswort = value; OnPropertyChanged(); }
    }

    private string _newPasswort = string.Empty;
    public string NewPasswort
    {
        get => _newPasswort;
        set { _newPasswort = value; OnPropertyChanged(); }
    }

    private string _newPasswortWdh = string.Empty;
    public string NewPasswortWdh
    {
        get => _newPasswortWdh;
        set { _newPasswortWdh = value; OnPropertyChanged(); }
    }

    // Sichtbarkeits-Flags
    private bool _isLoginPasswordVisible;
    public bool IsLoginPasswordVisible
    {
        get => _isLoginPasswordVisible;
        set { _isLoginPasswordVisible = value; OnPropertyChanged(); }
    }

    private bool _isRegPasswordVisible;
    public bool IsRegPasswordVisible
    {
        get => _isRegPasswordVisible;
        set { _isRegPasswordVisible = value; OnPropertyChanged(); }
    }

    private bool _isRegPasswordWdhVisible;
    public bool IsRegPasswordWdhVisible
    {
        get => _isRegPasswordWdhVisible;
        set { _isRegPasswordWdhVisible = value; OnPropertyChanged(); }
    }

    private bool _isOldPasswordVisible;
    public bool IsOldPasswordVisible
    {
        get => _isOldPasswordVisible;
        set { _isOldPasswordVisible = value; OnPropertyChanged(); }
    }

    private bool _isNewPasswordVisible;
    public bool IsNewPasswordVisible
    {
        get => _isNewPasswordVisible;
        set { _isNewPasswordVisible = value; OnPropertyChanged(); }
    }

    private bool _isNewPasswordWdhVisible;
    public bool IsNewPasswordWdhVisible
    {
        get => _isNewPasswordWdhVisible;
        set { _isNewPasswordWdhVisible = value; OnPropertyChanged(); }
    }

    // Suche & DataGrid
    private string _suchText = string.Empty;
    public string SuchText
    {
        get => _suchText;
        set
        {
            _suchText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public ObservableCollection<PasswortEintrag> Eintraege { get; set; }

    private PasswortEintrag? _selectedEintrag;
    public PasswortEintrag? SelectedEintrag
    {
        get => _selectedEintrag;
        set
        {
            _selectedEintrag = value;
            OnPropertyChanged();
            if (_selectedEintrag != null)
            {
                NeuWebsiteOderProgramm = _selectedEintrag.WebsiteOderProgramm;
                NeuBenutzername = _selectedEintrag.Benutzername;
                NeuPasswort = _selectedEintrag.Passwort;
                NeuBemerkung1 = _selectedEintrag.Bemerkung1;
                NeuBemerkung2 = _selectedEintrag.Bemerkung2;
            }
        }
    }

    // Eingabefelder Formular
    private string _neuWebsiteOderProgramm = string.Empty;
    public string NeuWebsiteOderProgramm
    {
        get => _neuWebsiteOderProgramm;
        set { _neuWebsiteOderProgramm = value; OnPropertyChanged(); }
    }

    private string _neuBenutzername = string.Empty;
    public string NeuBenutzername
    {
        get => _neuBenutzername;
        set { _neuBenutzername = value; OnPropertyChanged(); }
    }

    private string _neuPasswort = string.Empty;
    public string NeuPasswort
    {
        get => _neuPasswort;
        set { _neuPasswort = value; OnPropertyChanged(); }
    }

    private string _neuBemerkung1 = string.Empty;
    public string NeuBemerkung1
    {
        get => _neuBemerkung1;
        set { _neuBemerkung1 = value; OnPropertyChanged(); }
    }

    private string _neuBemerkung2 = string.Empty;
    public string NeuBemerkung2
    {
        get => _neuBemerkung2;
        set { _neuBemerkung2 = value; OnPropertyChanged(); }
    }

    // Commands
    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand OpenRegisterWindowCommand { get; }
    public ICommand CloseRegisterWindowCommand { get; }
    public ICommand ExecuteRegisterCommand { get; }
    public ICommand OpenChangePasswordWindowCommand { get; }
    public ICommand CloseChangePasswordWindowCommand { get; }
    public ICommand ExecuteChangePasswordCommand { get; }
    public ICommand SaveEintragCommand { get; }
    public ICommand DeleteEintragCommand { get; }
    public ICommand NewEintragCommand { get; }
    public ICommand GeneratePasswordCommand { get; }

    public ICommand ToggleLoginPasswordVisibilityCommand { get; }
    public ICommand ToggleRegPasswordVisibilityCommand { get; }
    public ICommand ToggleRegPasswordWdhVisibilityCommand { get; }
    public ICommand ToggleOldPasswordVisibilityCommand { get; }
    public ICommand ToggleNewPasswordVisibilityCommand { get; }
    public ICommand ToggleNewPasswordWdhVisibilityCommand { get; }

    // --- Methoden ---

    private void ExecuteLogin(object? obj)
    {
        string hashedInputName = CryptoService.HashUsername(InputBenutzername);
        var user = _benutzerListe.FirstOrDefault(u => u.Benutzername == hashedInputName);

        if (user != null && CryptoService.VerifyPassword(InputPasswort, user.PasswortHash, user.Salt))
        {
            _aktiverBenutzer = user;
            _masterKey = CryptoService.DeriveKey(InputPasswort, user.Salt);
            IsLoggedIn = true;

            _alleEintraege = user.Eintraege.Select(e => new PasswortEintrag
            {
                WebsiteOderProgramm = CryptoService.Decrypt(e.WebsiteOderProgramm, _masterKey),
                Benutzername = CryptoService.Decrypt(e.Benutzername, _masterKey),
                Passwort = CryptoService.Decrypt(e.Passwort, _masterKey),
                Bemerkung1 = CryptoService.Decrypt(e.Bemerkung1, _masterKey),
                Bemerkung2 = CryptoService.Decrypt(e.Bemerkung2, _masterKey)
            }).ToList();

            ApplyFilter();
            ClearInputFields();
        }
        else
        {
            MessageBox.Show("Ungültige Anmeldedaten!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExecuteLogout(object? obj)
    {
        if (_masterKey != null)
        {
            Array.Clear(_masterKey, 0, _masterKey.Length);
            _masterKey = null;
        }

        _aktiverBenutzer = null;
        IsLoggedIn = false;

        InputBenutzername = string.Empty;
        InputPasswort = string.Empty;
        OnPropertyChanged(nameof(InputBenutzername));
        OnPropertyChanged(nameof(InputPasswort));

        Eintraege.Clear();
        _alleEintraege.Clear();
        ClearInputFields();
    }

    private void OpenRegisterWindow(object? obj)
    {
        RegBenutzername = string.Empty;
        RegPasswort = string.Empty;
        RegPasswortWdh = string.Empty;

        _currentRegisterWindow = new RegisterWindow
        {
            DataContext = this,
            Owner = Application.Current.MainWindow
        };
        _currentRegisterWindow.ShowDialog();
    }

    private void CloseRegisterWindow()
    {
        _currentRegisterWindow?.Close();
        _currentRegisterWindow = null;
    }

    private void ExecuteRegister(object? obj)
    {
        if (string.IsNullOrWhiteSpace(RegBenutzername) || string.IsNullOrWhiteSpace(RegPasswort))
        {
            MessageBox.Show("Bitte geben Sie einen Benutzernamen und ein Passwort ein.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RegPasswort != RegPasswortWdh)
        {
            MessageBox.Show("Die Passwörter stimmen nicht überein. Bitte korrigieren Sie Ihre Eingabe.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string hashedRegName = CryptoService.HashUsername(RegBenutzername);
        if (_benutzerListe.Any(u => u.Benutzername == hashedRegName))
        {
            MessageBox.Show("Benutzer ist schon vorhanden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string salt = CryptoService.GenerateSalt();
        string hash = CryptoService.HashPassword(RegPasswort, salt);

        var neuBenutzer = new Benutzer
        {
            Benutzername = hashedRegName,
            PasswortHash = hash,
            Salt = salt
        };

        _benutzerListe.Add(neuBenutzer);
        _storageService.SaveUsers(_benutzerListe);

        MessageBox.Show("Registrierung erfolgreich! Sie können sich nun anmelden.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        CloseRegisterWindow();
    }

    private void OpenChangePasswordWindow(object? obj)
    {
        OldPasswort = string.Empty;
        NewPasswort = string.Empty;
        NewPasswortWdh = string.Empty;

        _currentChangePasswordWindow = new ChangePasswordWindow
        {
            DataContext = this,
            Owner = Application.Current.MainWindow
        };
        _currentChangePasswordWindow.ShowDialog();
    }

    private void CloseChangePasswordWindow()
    {
        _currentChangePasswordWindow?.Close();
        _currentChangePasswordWindow = null;
    }

    private void ExecuteChangePassword(object? obj)
    {
        if (_aktiverBenutzer == null || _masterKey == null) return;

        if (string.IsNullOrWhiteSpace(OldPasswort) || string.IsNullOrWhiteSpace(NewPasswort) || string.IsNullOrWhiteSpace(NewPasswortWdh))
        {
            MessageBox.Show("Bitte füllen Sie alle Felder aus.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!CryptoService.VerifyPassword(OldPasswort, _aktiverBenutzer.PasswortHash, _aktiverBenutzer.Salt))
        {
            MessageBox.Show("Das alte Passwort ist nicht korrekt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (OldPasswort == NewPasswort)
        {
            MessageBox.Show("Altes und neues Passwort sind gleich, bitte anderes Passwort auswählen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (NewPasswort != NewPasswortWdh)
        {
            MessageBox.Show("Die neuen Passwörter stimmen nicht überein. Bitte korrigieren Sie Ihre Eingabe.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string newSalt = CryptoService.GenerateSalt();
        byte[] newMasterKey = CryptoService.DeriveKey(NewPasswort, newSalt);

        _aktiverBenutzer.Salt = newSalt;
        _aktiverBenutzer.PasswortHash = CryptoService.HashPassword(NewPasswort, newSalt);

        _aktiverBenutzer.Eintraege = _alleEintraege.Select(e => new PasswortEintrag
        {
            WebsiteOderProgramm = CryptoService.Encrypt(e.WebsiteOderProgramm, newMasterKey),
            Benutzername = CryptoService.Encrypt(e.Benutzername, newMasterKey),
            Passwort = CryptoService.Encrypt(e.Passwort, newMasterKey),
            Bemerkung1 = CryptoService.Encrypt(e.Bemerkung1, newMasterKey),
            Bemerkung2 = CryptoService.Encrypt(e.Bemerkung2, newMasterKey)
        }).ToList();

        Array.Clear(_masterKey, 0, _masterKey.Length);
        _masterKey = newMasterKey;

        _storageService.SaveUsers(_benutzerListe);

        MessageBox.Show("Das Master-Passwort wurde erfolgreich geändert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        CloseChangePasswordWindow();
    }

    private void ExecuteSaveEintrag(object? obj)
    {
        if (_aktiverBenutzer == null || _masterKey == null) return;

        if (string.IsNullOrWhiteSpace(NeuWebsiteOderProgramm) ||
            string.IsNullOrWhiteSpace(NeuBenutzername) ||
            string.IsNullOrWhiteSpace(NeuPasswort))
        {
            MessageBox.Show("Website/Programm, Benutzername und Passwort sind Pflichtfelder!",
                            "Pflichtfelder fehlen", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedEintrag != null)
        {
            SelectedEintrag.WebsiteOderProgramm = NeuWebsiteOderProgramm;
            SelectedEintrag.Benutzername = NeuBenutzername;
            SelectedEintrag.Passwort = NeuPasswort;
            SelectedEintrag.Bemerkung1 = NeuBemerkung1;
            SelectedEintrag.Bemerkung2 = NeuBemerkung2;
        }
        else
        {
            var neuerEintrag = new PasswortEintrag
            {
                WebsiteOderProgramm = NeuWebsiteOderProgramm,
                Benutzername = NeuBenutzername,
                Passwort = NeuPasswort,
                Bemerkung1 = NeuBemerkung1,
                Bemerkung2 = NeuBemerkung2
            };
            _alleEintraege.Add(neuerEintrag);
        }

        _aktiverBenutzer.Eintraege = _alleEintraege.Select(e => new PasswortEintrag
        {
            WebsiteOderProgramm = CryptoService.Encrypt(e.WebsiteOderProgramm, _masterKey),
            Benutzername = CryptoService.Encrypt(e.Benutzername, _masterKey),
            Passwort = CryptoService.Encrypt(e.Passwort, _masterKey),
            Bemerkung1 = CryptoService.Encrypt(e.Bemerkung1, _masterKey),
            Bemerkung2 = CryptoService.Encrypt(e.Bemerkung2, _masterKey)
        }).ToList();

        _storageService.SaveUsers(_benutzerListe);
        ApplyFilter();
        ClearInputFields();
    }

    private void ExecuteDeleteEintrag(object? obj)
    {
        if (_aktiverBenutzer != null && SelectedEintrag != null && _masterKey != null)
        {
            var result = MessageBox.Show(
                $"Soll dieser Eintrag für '{SelectedEintrag.WebsiteOderProgramm}' wirklich gelöscht werden?",
                "Eintrag löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _alleEintraege.Remove(SelectedEintrag);

            _aktiverBenutzer.Eintraege = _alleEintraege.Select(e => new PasswortEintrag
            {
                WebsiteOderProgramm = CryptoService.Encrypt(e.WebsiteOderProgramm, _masterKey),
                Benutzername = CryptoService.Encrypt(e.Benutzername, _masterKey),
                Passwort = CryptoService.Encrypt(e.Passwort, _masterKey),
                Bemerkung1 = CryptoService.Encrypt(e.Bemerkung1, _masterKey),
                Bemerkung2 = CryptoService.Encrypt(e.Bemerkung2, _masterKey)
            }).ToList();

            _storageService.SaveUsers(_benutzerListe);
            ApplyFilter();
            ClearInputFields();
        }
    }

    private void ApplyFilter()
    {
        Eintraege.Clear();
        var query = _alleEintraege.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SuchText))
        {
            string term = SuchText.ToLower();
            query = query.Where(e => (e.WebsiteOderProgramm != null && e.WebsiteOderProgramm.ToLower().Contains(term)) ||
                                     (e.Benutzername != null && e.Benutzername.ToLower().Contains(term)));
        }

        foreach (var eintrag in query)
        {
            Eintraege.Add(eintrag);
        }
    }

    private void ClearInputFields()
    {
        SelectedEintrag = null;
        NeuWebsiteOderProgramm = string.Empty;
        NeuBenutzername = string.Empty;
        NeuPasswort = string.Empty;
        NeuBemerkung1 = string.Empty;
        NeuBemerkung2 = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}