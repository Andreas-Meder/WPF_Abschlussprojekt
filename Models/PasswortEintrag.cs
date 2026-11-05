using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwortspeicher.Models;

public class PasswortEintrag
{
    public string WebsiteOderProgramm { get; set; } = string.Empty;
    public string Benutzername { get; set; } = string.Empty;
    public string Passwort { get; set; } = string.Empty;
    public string Bemerkung1 { get; set; } = string.Empty;
    public string Bemerkung2 { get; set; } = string.Empty;
}