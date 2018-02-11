using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenboot_Tools
{
    [Serializable]
    public class clsTeams
    {
        public string TeamName;
        public List<string> sPaddler = new List<string>();
        public List<clsAufstellung> cAufstellungListe10 = new List<clsAufstellung>();
        public List<clsAufstellung> cAufstellungListe5 = new List<clsAufstellung>();
    }
}
