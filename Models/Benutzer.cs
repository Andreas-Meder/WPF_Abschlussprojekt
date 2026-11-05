using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwortspeicher.Models;

public class Benutzer
{
    public string Benutzername { get; set; } = string.Empty;
    public string PasswortHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public List<PasswortEintrag> Eintraege { get; set; } = new();
}