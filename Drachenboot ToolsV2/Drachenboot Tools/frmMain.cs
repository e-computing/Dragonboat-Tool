using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Drawing.Imaging;

namespace Drachenboot_Tools
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        //STRG+M+O
        DataTable dt;
        List<string> sName = new List<string>();
        List<int> iGewicht = new List<int>();
        public List<string> sSeite = new List<string>();
        List<string> sGeschlaecht = new List<string>();
        List<string> sSteuermann = new List<string>();
        List<string> sPaddlerRechts = new List<string>();
        List<string> sPaddlerLinks = new List<string>();
        List<clsTeams> Teams = new List<clsTeams>();
        clsTeams spTeam = new clsTeams();
        string LastSIR1, LastSIR2, LastSIR3, LastSIR4, LastSIR5, LastSIR6, LastSIR7, LastSIR8, LastSIR9, LastSIR10, LastSIT, LastSIS;
        string LastSIL1, LastSIL2, LastSIL3, LastSIL4, LastSIL5, LastSIL6, LastSIL7, LastSIL8, LastSIL9, LastSIL10;
        double GewichtRechts, GewichtLinks, GesamtGewicht;
        int Frauen, Männer;
        bool Anfang = false;
        Bitmap memoryImage;

        int AnzTeams=0;
        int Version_alt = 0;
        int Version = 0;

        

        List<string> sTeamName = new List<string>();

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            bool checkconnection = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (checkconnection && CheckInternetConnection())
            {
                if (File.Exists("v.version"))
                {

                    StreamReader scanner = new StreamReader("v.version");
                    try
                    {
                        Version_alt = Convert.ToInt32(scanner.ReadLine());
                    }
                    catch { Version_alt = 0; }
                    scanner.Close();
                }
                Download("http://blue-programming.bplaced.net/DragonboatTool/Deutsch/", "v.version");
                if (File.Exists("v.version"))
                {
                    StreamReader scanner = new StreamReader("v.version");
                    try
                    {
                        Version = Convert.ToInt32(scanner.ReadLine());
                    }
                    catch { Version = Version_alt + 1; }
                    scanner.Close();
                }
                if (Version > Version_alt || Version_alt > Version)
                {
                    if (File.Exists("Update.exe"))
                    {
                        Process.Start("Update.exe");
                        Application.Exit();
                    }
                    else
                    {
                        Download("http://blue-programming.bplaced.net/DragonboatTool/Deutsch/", "Update.exe");
                        Process.Start("Update.exe");
                        Application.Exit();
                    }
                }
            }
            

            #region Image
            try
            {
                pBLogo5Bank.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "logo.jpg");
                pBLogo10Bank.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "logo.jpg");
                pBLogoTeam.Image = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + "logo.jpg");
            }
            catch{            }
            #endregion

            #region DeSerialisierung
            DeSerialisierung();
            #endregion
            #region DataGridView
            dt = new DataTable("Paddler");
            dt.Columns.Add(new DataColumn("Name"));
            dt.Columns.Add(new DataColumn("Gewicht", typeof(int)));
            dt.Columns.Add(new DataColumn("Geschlecht"));
            dt.Columns.Add(new DataColumn("Seite"));
            dt.Columns.Add(new DataColumn("Steuermann"));
            if (sName.Count == 0)
            {
                AddNewRow(dt, "Beispiel-Name", 80, "Männlich", "Rechts","X");
            }
            for (int i = 0; i < sName.Count; i++)
            {
                AddNewRow(dt, sName[i], iGewicht[i], sGeschlaecht[i], sSeite[i],sSteuermann[i]);
                
            }
            dGVPaddler.DataSource = dt;
            #endregion
            #region Voreinstellungen
            for (int i = 0; i < AnzTeams; i++)
            {
                lBTeams.Items.Add(Teams[i].TeamName);
            }
            for(int i = 0; i < sName.Count;i++)
            {
                cBPaddler.Items.Add(sName[i]);
            }
            for (int i = 0; i < Teams.Count; i++)
            {
                cBTeams.Items.Add(Teams[i].TeamName);
            }
            cBBootsTyp.SelectedIndex = 0;
            #endregion

            SpreadsheetMLHelper.ExportDataTableToWorksheet(dt, "datenTable");
            //SeitenSortieren();
            //ListToAufstellung();
            linkLabel1.Links.Add(0, 8, "www.dragonboatclub.de");
            linkLabel2.Links.Add(0, 8, "www.facebook.com/bluedragonspremier/");

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                lBPaddlerTeam.Items.Clear();
                if (lBTeams.Items.Count == 0)
                {
                    lbTeamNameBearbeiten.Text = "Füge Teams hinzu";
                }
                else
                {

                    if (lBTeams.SelectedIndex > -1)
                    {
                        lbTeamNameBearbeiten.Text = lBTeams.SelectedItem.ToString();
                        for (int i = 0; i < Teams[lBTeams.SelectedIndex].sPaddler.Count; i++)
                        {
                            lBPaddlerTeam.Items.Add(Teams[lBTeams.SelectedIndex].sPaddler[i]);
                        }
                        AnzahlPaddlerTeamSeite();
                    }
                }
            }
            else if (tabControl1.SelectedIndex == 3)
            {
                cBAufstellungZuweisen();
                if (cBTeams.SelectedIndex > -1 && cBAufstellungListe.SelectedIndex > -1)
                {
                    if (cBBootsTyp.SelectedIndex == 0)
                    {
                        for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung.Count; i++)
                        {
                            if (Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[i] != "")
                            {
                                lBErsatz.Items.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[i]);
                            }
                        }
                    }
                    else if (cBBootsTyp.SelectedIndex == 1)
                    {
                        for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung.Count; i++)
                        {
                            if (Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[i] != "")
                            {
                                lBErsatz.Items.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[i]);
                            }
                        }
                    }
                }
            }
        }

        private void SeitenSortieren()
        {
            sPaddlerRechts.Clear();
            sPaddlerLinks.Clear();
            int SortierungsIndexRechts = 0;
            int SortierungsIndexLinks = 0;
            int SortierungsIndexBeides = 0;

            for (int i = 0; i < sSeite.Count; i++)
            {
                if (sSeite[i] == "Rechts")
                {
                    SortierungsIndexRechts = sSeite.IndexOf("Rechts", SortierungsIndexRechts);
                    sPaddlerRechts.Add(sName[SortierungsIndexRechts]);
                    SortierungsIndexRechts++;
                }
                else if (sSeite[i] == "Links")
                {
                    SortierungsIndexLinks = sSeite.IndexOf("Links", SortierungsIndexLinks);
                    sPaddlerLinks.Add(sName[SortierungsIndexLinks]);
                    SortierungsIndexLinks++;
                }
                else
                {
                    SortierungsIndexBeides = sSeite.IndexOf("Beides", SortierungsIndexBeides);
                    sPaddlerLinks.Add(sName[SortierungsIndexBeides]);
                    sPaddlerRechts.Add(sName[SortierungsIndexBeides]);
                    SortierungsIndexBeides++;
                }
            }
            cBErstesZuweisen();
        }

        private void cBErstesZuweisen()
        {
            #region Paddler Rechts
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R1.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R1.Items.Add(sPaddlerRechts[i]);
                }
            }

            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R2.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R2.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R3.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R3.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R4.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R4.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R5.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R5.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R6.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R6.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R7.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R7.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R8.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R8.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R9.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R9.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10R10.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10R10.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10Steuer.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10Steuer.Items.Add(sPaddlerRechts[i]);
                }
            }
            for (int i = 0; i < sPaddlerRechts.Count; i++)
            {
                if (!cB10Trommel.Items.Contains(sPaddlerRechts[i]))
                {
                    cB10Trommel.Items.Add(sPaddlerRechts[i]);
                }
            }
            #endregion
            #region Paddler Links
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L1.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L1.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L2.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L2.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L3.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L3.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L4.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L4.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L5.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L5.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L6.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L6.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L7.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L7.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L8.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L8.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L9.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L9.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10L10.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10L10.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10Steuer.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10Steuer.Items.Add(sPaddlerLinks[i]);
                }
            }
            for (int i = 0; i < sPaddlerLinks.Count; i++)
            {
                if (!cB10Trommel.Items.Contains(sPaddlerLinks[i]))
                {
                    cB10Trommel.Items.Add(sPaddlerLinks[i]);
                }
            }

            #endregion
        }

        private void AddNewRow(DataTable dt, string name, int gewicht, string geschlaecht, string seite,string steuer)
        {
            DataRow dr = dt.NewRow();

            dr[0] = name; dr[1] = gewicht; dr[2] = geschlaecht; dr[3] = seite; dr[4] = steuer;
            dt.Rows.Add(dr);
        }

        private void AddNewListRow(DataTable dt, string name, int gewicht, string geschlaecht, string seite,string steuermann)
        {
            sName.Add(name);
            iGewicht.Add(gewicht);
            sSeite.Add(seite);
            sGeschlaecht.Add(geschlaecht);
            sSteuermann.Add(steuermann);

            DataRow dr = dt.NewRow();
            dr[0] = name; dr[1] = gewicht; dr[2] = geschlaecht; dr[3] = seite; dr[4] = steuermann;
            dt.Rows.Add(dr);

            Serialisierung();

        }
        private void Serialisierung()
        {
            
            var x = new clsSerialisierung();
            for (int i = 0; i < sName.Count; i++)
            {
                x.Paddler.sName.Add(sName[i]);
                x.Paddler.iGewicht.Add(iGewicht[i]);
                x.Paddler.sSeite.Add(sSeite[i]);
                x.Paddler.sGeschlaecht.Add(sGeschlaecht[i]);
                x.Paddler.sSteuermann.Add(sSteuermann[i]);
            }
            x.AnzTeams = AnzTeams;
            for (int i = 0; i < AnzTeams; i++)
            {
                x.Speicher.TeamListe.Add(Teams[i]);
                int Count = Teams[i].sPaddler.Count;

                int AutstellungCount10Liste = Teams[i].cAufstellungListe10.Count;
                int AutstellungCount5Liste = Teams[i].cAufstellungListe5.Count;
                for (int k = 0; k < Count; k++)
                {
                    x.Speicher.TeamListe[i].sPaddler.Add(Teams[i].sPaddler[k]);
                    x.Speicher.TeamListe[i].sPaddler.RemoveAt(x.Speicher.TeamListe[i].sPaddler.Count - 1);
                }

                for (int k = 0; k < AutstellungCount10Liste; k++)
                {
                    int AufstellungSpez10Liste = Teams[i].cAufstellungListe10[k].sAufstellung.Count;
                    x.Speicher.TeamListe[i].cAufstellungListe10[k].AufstellungName = Teams[i].cAufstellungListe10[k].AufstellungName;
                    for (int j = 0; j < AufstellungSpez10Liste; j++)
                    {
                        x.Speicher.TeamListe[i].cAufstellungListe10[k].sAufstellung.Add(Teams[i].cAufstellungListe10[k].sAufstellung[j]);
                        x.Speicher.TeamListe[i].cAufstellungListe10[k].sAufstellung.RemoveAt(x.Speicher.TeamListe[i].cAufstellungListe10[k].sAufstellung.Count-1);
                    }
                }
                for (int k = 0; k < AutstellungCount5Liste; k++)
                {
                    int AufstellungSpez5Liste = Teams[i].cAufstellungListe5[k].sAufstellung.Count;
                    x.Speicher.TeamListe[i].cAufstellungListe5[k].AufstellungName = Teams[i].cAufstellungListe5[k].AufstellungName;
                    for (int j = 0; j < AufstellungSpez5Liste; j++)
                    {
                        x.Speicher.TeamListe[i].cAufstellungListe5[k].sAufstellung.Add(Teams[i].cAufstellungListe5[k].sAufstellung[j]);
                        x.Speicher.TeamListe[i].cAufstellungListe5[k].sAufstellung.RemoveAt(x.Speicher.TeamListe[i].cAufstellungListe5[k].sAufstellung.Count - 1);
                    }
                }
            }
            
            clsSerialisierung.Serialize(x);
            Verschlüsseln();
        }
        private void DeSerialisierung()
        {
            Entschlüsseln();
           
                var x = new clsSerialisierung();
                x = clsSerialisierung.DeSerialize();
                for (int i = 0; i < x.Paddler.sName.Count; i++)
                { 
                    try
                    {
                        sName.Add(x.Paddler.sName[i]);
                        iGewicht.Add(x.Paddler.iGewicht[i]);
                        sSeite.Add(x.Paddler.sSeite[i]);
                        sGeschlaecht.Add(x.Paddler.sGeschlaecht[i]);
                        sSteuermann.Add(x.Paddler.sSteuermann[i]);
                    }
                    catch
                    {
                        MessageBox.Show("Fehlerhafte Save Datei", "Fehler");
                    }

                }
                try
                {
                    AnzTeams = x.AnzTeams;
                }
                catch
                {
                    MessageBox.Show("Fehlerhafte Save Datei", "Fehler");
                }
                for (int i = 0; i < x.AnzTeams; i++)
                {
                    try
                    {
                        Teams.Add(x.Speicher.TeamListe[i]);
                        int Count = x.Speicher.TeamListe[i].sPaddler.Count;

                        int AutstellungCount10Liste = x.Speicher.TeamListe[i].cAufstellungListe10.Count;
                        int AutstellungCount5Liste = x.Speicher.TeamListe[i].cAufstellungListe5.Count;

                        for (int j = 0; j < Count; j++)
                        {
                            Teams[i].sPaddler.Add(x.Speicher.TeamListe[i].sPaddler[j]);
                            Teams[i].sPaddler.RemoveAt(Teams[i].sPaddler.Count - 1);
                        }
                        for (int k = 0; k < AutstellungCount10Liste; k++)
                        {
                            int AufstellungSpez10Liste = x.Speicher.TeamListe[i].cAufstellungListe10[k].sAufstellung.Count;
                            Teams[i].cAufstellungListe10[k].AufstellungName = x.Speicher.TeamListe[i].cAufstellungListe10[k].AufstellungName;
                            for (int j = 0; j < AufstellungSpez10Liste; j++)
                            {
                                Teams[i].cAufstellungListe10[k].sAufstellung.Add(x.Speicher.TeamListe[i].cAufstellungListe10[k].sAufstellung[j]);
                                Teams[i].cAufstellungListe10[k].sAufstellung.RemoveAt(Teams[i].cAufstellungListe10[k].sAufstellung.Count - 1);
                            }
                        }
                        for (int k = 0; k < AutstellungCount5Liste; k++)
                        {
                            int AufstellungSpez5Liste = x.Speicher.TeamListe[i].cAufstellungListe5[k].sAufstellung.Count;
                            Teams[i].cAufstellungListe5[k].AufstellungName = x.Speicher.TeamListe[i].cAufstellungListe5[k].AufstellungName;
                            for (int j = 0; j < AufstellungSpez5Liste; j++)
                            {
                                Teams[i].cAufstellungListe5[k].sAufstellung.Add(x.Speicher.TeamListe[i].cAufstellungListe5[k].sAufstellung[j]);
                                Teams[i].cAufstellungListe5[k].sAufstellung.RemoveAt(Teams[i].cAufstellungListe5[k].sAufstellung.Count - 1);
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Fehlerhafte Save Datei", "Fehler");
                    }
                }

         
            Verschlüsseln();
        }
        private void cmdPaddlerplus_Click(object sender, EventArgs e)
        {
            string cmdSeite;
            string cmdGeschlaecht;
            string cmdSteuermann;
            if (!sName.Contains(txtName.Text))
            {
                if (sName.Count == 0)
                {
                    try
                    {
                        dt.Rows.RemoveAt(0);
                    }
                    catch
                    { }
                }
                if (rbLinks.Checked == true)
                {
                    cmdSeite = "Links";
                    sPaddlerLinks.Add(txtName.Text);
                }
                else if(rbRechts.Checked == true)
                {
                    cmdSeite = "Rechts";
                    sPaddlerRechts.Add(txtName.Text);
                }
                else
                {
                    cmdSeite = "Beides";
                    sPaddlerLinks.Add(txtName.Text);
                }
                if (rbMaenlich.Checked == true)
                {
                    cmdGeschlaecht = "Männlich";
                }
                else
                {
                    cmdGeschlaecht = "Weiblich";
                }
                if (checkSteuermann.Checked == true)
                {
                    cmdSteuermann = "X";
                }
                else
                {
                    cmdSteuermann = "";
                }
                if (txtName.Text == "")
                {
                    MessageBox.Show("Gib bitte einen Namen ein", "Fehler");
                }
                else
                {
                    AddNewListRow(dt, txtName.Text, Convert.ToInt32(numGewicht.Value), cmdGeschlaecht, cmdSeite, cmdSteuermann);
                    cBPaddler.Items.Add(txtName.Text);
                }
            }
            else
            {
                MessageBox.Show("Name bereits vorhanden", "Fehler");
            }
        }

        private void cmdPaddlerMinus_Click(object sender, EventArgs e)
        {
            int Index;
            if (txtPaddlerLöschenName.Text != "" && sName.Contains(txtPaddlerLöschenName.Text))
            {
                Index = sName.IndexOf(txtPaddlerLöschenName.Text);
                sPaddlerLinks.Remove(txtPaddlerLöschenName.Text);
                sPaddlerRechts.Remove(txtPaddlerLöschenName.Text);
                #region ComboBox Remove
                cB10R1.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R2.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R3.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R4.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R5.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R6.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R7.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R8.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R9.Items.Remove(txtPaddlerLöschenName.Text);
                cB10R10.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L1.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L2.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L3.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L4.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L5.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L6.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L7.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L8.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L9.Items.Remove(txtPaddlerLöschenName.Text);
                cB10L10.Items.Remove(txtPaddlerLöschenName.Text);
                cB10Trommel.Items.Remove(txtPaddlerLöschenName.Text);
                cB10Steuer.Items.Remove(txtPaddlerLöschenName.Text);
                #endregion
                cBPaddler.Items.Remove(txtPaddlerLöschenName.Text);

                for (int i = 0; i < Teams.Count; i++)
                {
                    Teams[i].sPaddler.Remove(txtPaddlerLöschenName.Text);
                }

                dt.Rows.RemoveAt(Index);
                iGewicht.RemoveAt(Index);
                sGeschlaecht.RemoveAt(Index);
                sSeite.RemoveAt(Index);
                sName.Remove(txtPaddlerLöschenName.Text);
                sSteuermann.RemoveAt(Index);
                Serialisierung();
                txtPaddlerLöschenName.Text = "";
            }
            else if (txtPaddlerLöschenName.Text == "")
            {
                MessageBox.Show("Gebe einen Namen zum Löschen ein", "Fehler");
            }
            else
            {
                MessageBox.Show("Name konnte nicht gefunden werden", "Fehler");
            }

        }

        private void cmdPaddlerBearbeiten_Click(object sender, EventArgs e)
        {
            if (txtPaddlerBearbeiten.Text != "")
            {
                int Index = sName.IndexOf(txtPaddlerBearbeiten.Text);
                txtPaddlerBearbeiten.Enabled = false;
                gbBearbeitung.Enabled = true;
                numGewichtBearbeiten.Value = iGewicht[Index];
                if (sGeschlaecht[Index] == "Weiblich")
                {
                    rbWeiblichBearbeiten.Checked = true;
                }
                else
                {
                    rbMännlichBearbeiten.Checked = true;
                }
                if (sSeite[Index] == "Rechts")
                {
                    rbRechtsBearbeiten.Checked = true;
                }
                else if (sSeite[Index] == "Links")
                {
                    rbLinksBearbeiten.Checked = true;
                }
                else if (sSeite[Index] == "Beides")
                {
                    rbBeidesBearbeiten.Checked = true;
                }
                if (sSteuermann[Index] == "X")
                {
                    checkSteuermannBearbeiten.Checked = true;
                }
                else
                {
                    checkSteuermannBearbeiten.Checked = false;
                }
            }
            else
            {
                MessageBox.Show("Gebe den Paddler Namen ein den du bearbeiten möchtest", "Fehler");
            }
        }

        private void cmdPaddlerBearbeitenSpeichern_Click(object sender, EventArgs e)
        {
            string cmdSeite;
            string cmdGeschlaecht;
            string cmdSteuermann;
            if (rbLinksBearbeiten.Checked == true)
            {
                cmdSeite = "Links";
            }
            else if(rbRechtsBearbeiten.Checked == true)
            {
                cmdSeite = "Rechts";
            }
            else
            {
                cmdSeite = "Beides";
            }
            if (rbMännlichBearbeiten.Checked == true)
            {
                cmdGeschlaecht = "Männlich";
            }
            else
            {
                cmdGeschlaecht = "Weiblich";
            }
            if (checkSteuermannBearbeiten.Checked == true)
            {
                cmdSteuermann = "X";
            }
            else
            {
                cmdSteuermann = "";
            }
            int Index = sName.IndexOf(txtPaddlerBearbeiten.Text);
            sGeschlaecht[Index] = cmdGeschlaecht;
            sSteuermann[Index] = cmdSteuermann;
            sSeite[Index] = cmdSeite;
            iGewicht[Index] = Convert.ToInt32(numGewichtBearbeiten.Value);
            dt.Rows[Index][1] = Convert.ToInt32(numGewichtBearbeiten.Value);
            dt.Rows[Index][2] = cmdGeschlaecht;
            dt.Rows[Index][3] = cmdSeite;
            dt.Rows[Index][4] = cmdSteuermann;
            gbBearbeitung.Enabled = false;
            txtPaddlerBearbeiten.Enabled = true;
            Serialisierung();
            BerechneDifferenzen();
        }

        private void cmdTeamPlus_Click(object sender, EventArgs e)
        {
            if (txtTeamName.Text != "")
            {
                clsTeams PlusTeam = new clsTeams();
                AnzTeams++;
                PlusTeam.TeamName = txtTeamName.Text;
                lBTeams.Items.Add(txtTeamName.Text);
                Teams.Add(PlusTeam);
                cBTeams.Items.Add(txtTeamName.Text);
                lBTeams.SelectedItem = txtTeamName.Text;
                Serialisierung();
            }
            else
            {
                MessageBox.Show("Gebe einen Team Name ein", "Fehler");
            }
        }

        private void cmdTeamMinus_Click(object sender, EventArgs e)
        {
            if (lBTeams.SelectedIndex < 0)
            {
                MessageBox.Show("Makiere ein Team welches du löschen möchtest", "Fehler");
            }
            else
            {
                cBTeams.Items.Remove(lBTeams.SelectedItem);
                Teams.RemoveAt(lBTeams.SelectedIndex);
                lBTeams.Items.RemoveAt(lBTeams.SelectedIndex);
                AnzTeams--;
                Serialisierung();
            }
        }

        private void cmdPaddlerTeamHinzufügen_Click(object sender, EventArgs e)
        {
            if (lBTeams.SelectedIndex < 0) { MessageBox.Show("Kein Team gewählt", "Fehler"); }
            else
            {
                if (!lBPaddlerTeam.Items.Contains(cBPaddler.Text) && cBPaddler.SelectedIndex >-1)
                {
                    Teams[lBTeams.SelectedIndex].sPaddler.Add(cBPaddler.Text);
                    lBPaddlerTeam.Items.Add(cBPaddler.Text);
                    Serialisierung();
                }
                else
                {
                    MessageBox.Show("Paddler ist bereits in diesem Team", "Fehler");
                }
            }
            AnzahlPaddlerTeamSeite();
        }

        private void cmdPaddlerTeamLöschen_Click(object sender, EventArgs e)
        {
            if (lBPaddlerTeam.SelectedIndex < 0) { MessageBox.Show("Kein Paddler gewählt", "Fehler"); }
            else
            {
                if (lBTeams.SelectedIndex > -1)
                {
                    Teams[lBTeams.SelectedIndex].sPaddler.Remove(lBPaddlerTeam.SelectedItem.ToString());
                    lBPaddlerTeam.Items.Remove(lBPaddlerTeam.SelectedItem.ToString());
                    Serialisierung();
                }
            }
            AnzahlPaddlerTeamSeite();
        }

        private void lBTeams_SelectedIndexChanged(object sender, EventArgs e)
        {
            lBPaddlerTeam.Items.Clear();
            if (lBTeams.Items.Count == 0)
            {
                lbTeamNameBearbeiten.Text = "Füge Teams hinzu";
            }
            else
            {
                lbTeamNameBearbeiten.Text = "Wähle ein Team aus";
                if (lBTeams.SelectedIndex > -1)
                {
                    lbTeamNameBearbeiten.Text = lBTeams.SelectedItem.ToString();
                    for (int i = 0; i < Teams[lBTeams.SelectedIndex].sPaddler.Count; i++)
                    {
                        lBPaddlerTeam.Items.Add(Teams[lBTeams.SelectedIndex].sPaddler[i]);
                    }
                    AnzahlPaddlerTeamSeite();         
                }
                
            }
            
            
        }

        private void cBBootsTyp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBBootsTyp.SelectedIndex == 0)
            {
                
                p10BankBoot.Visible = true;
                p5BankBoot.Visible = false;
            }
            else
            {

                p10BankBoot.Visible = false;
                p5BankBoot.Visible = true;
            }
            Anfang = false;
            txtAufstellungName.Text = "";
            cBAufstellungListe.SelectedIndex = -1;
            cBAufstellungsListe();
            ErsatzToListBox();
            cBAufstellungZuweisen();
            BerechneGesamtGewicht();
            BerechneDifferenzen();
            AufstellungSIZuweisen();
        }

        private void cBTeams_SelectedIndexChanged(object sender, EventArgs e)
        {
            cBAufstellungsListe();
            cBAufstellungZuweisen();
            ErsatzToListBox();
            Anfang = false;
            AufstellungSIZuweisen();
           
        }

        private void cBAufstellungListe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBAufstellungListe.SelectedIndex > -1)
            {
                txtAufstellungName.Text = cBAufstellungListe.SelectedItem.ToString();
            }
            else
            {
                txtAufstellungName.Text = "";
            }

            if (cBTeams.SelectedIndex > -1 && cBAufstellungListe.SelectedIndex > -1)
            {
                ErsatzToListBox();
                AufstellungToCB();
                AufstellungSIZuweisen();
                if (cBBootsTyp.SelectedIndex == 0)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung.Count; i++)
                    {
                        if (Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[i] != "")
                        {
                            lBErsatz.Items.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[i]);
                        }
                    }
                }
                else if (cBBootsTyp.SelectedIndex == 1)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung.Count; i++)
                    {
                        if (Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[i] != "")
                        {
                            lBErsatz.Items.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[i]);
                        }
                    }
                }

            }
           
        }

        #region SelectedIndexChangedRechts
        private void cB10R1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R1.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR1) && LastSIR1 != "" && cB10R2.Text != LastSIR1 && cB10R3.Text != LastSIR1 && cB10R4.Text != LastSIR1 && cB10R5.Text != LastSIR1 && cB10R6.Text != LastSIR1 && cB10R7.Text != LastSIR1 && cB10R8.Text != LastSIR1 && cB10R9.Text != LastSIR1 && cB10R10.Text != LastSIR1 && cB10L1.Text != LastSIR1 && cB10L2.Text != LastSIR1 && cB10L3.Text != LastSIR1 && cB10L4.Text != LastSIR1 && cB10L5.Text != LastSIR1 && cB10L6.Text != LastSIR1 && cB10L7.Text != LastSIR1 && cB10L8.Text != LastSIR1 && cB10L9.Text != LastSIR1 && cB10L10.Text != LastSIR1 && cB10Trommel.Text != LastSIR1 && cB10Steuer.Text != LastSIR1)
                {
                    lBErsatz.Items.Add(LastSIR1);
                }
            }
            catch { }
            if (cB10R1.SelectedItem != null)
            {
                LastSIR1 = cB10R1.SelectedItem.ToString();
            }
            else
            {
                LastSIR1 = "";
            }

        }

        private void cB10R2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R2.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR2) && LastSIR2 != "" && cB10R1.Text != LastSIR2 && cB10R3.Text != LastSIR2 && cB10R4.Text != LastSIR2 && cB10R5.Text != LastSIR2 && cB10R6.Text != LastSIR2 && cB10R7.Text != LastSIR2 && cB10R8.Text != LastSIR2 && cB10R9.Text != LastSIR2 && cB10R10.Text != LastSIR2 && cB10L1.Text != LastSIR2 && cB10L2.Text != LastSIR2 && cB10L3.Text != LastSIR2 && cB10L4.Text != LastSIR2 && cB10L5.Text != LastSIR2 && cB10L6.Text != LastSIR2 && cB10L7.Text != LastSIR2 && cB10L8.Text != LastSIR2 && cB10L9.Text != LastSIR2 && cB10L10.Text != LastSIR2 && cB10Trommel.Text != LastSIR2 && cB10Steuer.Text != LastSIR2)
                {
                    lBErsatz.Items.Add(LastSIR2);
                }
            }
            catch { }
            if (cB10R2.SelectedItem != null)
            {
                LastSIR2 = cB10R2.SelectedItem.ToString();
            }
            else
            {
                LastSIR2 = "";
            }
        }

        private void cB10R3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R3.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR3) && LastSIR3 != "" && cB10R1.Text != LastSIR3 && cB10R2.Text != LastSIR3 && cB10R4.Text != LastSIR3 && cB10R5.Text != LastSIR3 && cB10R6.Text != LastSIR3 && cB10R7.Text != LastSIR3 && cB10R8.Text != LastSIR3 && cB10R9.Text != LastSIR3 && cB10R10.Text != LastSIR3 && cB10L1.Text != LastSIR3 && cB10L2.Text != LastSIR3 && cB10L3.Text != LastSIR3 && cB10L4.Text != LastSIR3 && cB10L5.Text != LastSIR3 && cB10L6.Text != LastSIR3 && cB10L7.Text != LastSIR3 && cB10L8.Text != LastSIR3 && cB10L9.Text != LastSIR3 && cB10L10.Text != LastSIR3 && cB10Trommel.Text != LastSIR3 && cB10Steuer.Text != LastSIR3)

                {
                    lBErsatz.Items.Add(LastSIR3);
                }
            }
            catch { }
            if (cB10R3.SelectedItem != null)
            {
                LastSIR3 = cB10R3.SelectedItem.ToString();
            }
            else
            {
                LastSIR3 = "";
            }
        }

        private void cB10R4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R4.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR4) && LastSIR4 != "" && cB10R1.Text != LastSIR4 && cB10R2.Text != LastSIR4 && cB10R3.Text != LastSIR4 && cB10R5.Text != LastSIR4 && cB10R6.Text != LastSIR4 && cB10R7.Text != LastSIR4 && cB10R8.Text != LastSIR4 && cB10R9.Text != LastSIR4 && cB10R10.Text != LastSIR4 && cB10L1.Text != LastSIR4 && cB10L2.Text != LastSIR4 && cB10L3.Text != LastSIR4 && cB10L4.Text != LastSIR4 && cB10L5.Text != LastSIR4 && cB10L6.Text != LastSIR4 && cB10L7.Text != LastSIR4 && cB10L8.Text != LastSIR4 && cB10L9.Text != LastSIR4 && cB10L10.Text != LastSIR4 && cB10Trommel.Text != LastSIR4 && cB10Steuer.Text != LastSIR4)

                {
                    lBErsatz.Items.Add(LastSIR4);
                }
            }
            catch { }
            if (cB10R4.SelectedItem != null)
            {
                LastSIR4 = cB10R4.SelectedItem.ToString();
            }
            else
            {
                LastSIR4 = "";
            }
        }

        private void cB10R5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R5.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR5) && LastSIR5 != "" && cB10R1.Text != LastSIR5 && cB10R2.Text != LastSIR5 && cB10R3.Text != LastSIR5 && cB10R4.Text != LastSIR5 && cB10R6.Text != LastSIR5 && cB10R7.Text != LastSIR5 && cB10R8.Text != LastSIR5 && cB10R9.Text != LastSIR5 && cB10R10.Text != LastSIR5 && cB10L1.Text != LastSIR5 && cB10L2.Text != LastSIR5 && cB10L3.Text != LastSIR5 && cB10L4.Text != LastSIR5 && cB10L5.Text != LastSIR5 && cB10L6.Text != LastSIR5 && cB10L7.Text != LastSIR5 && cB10L8.Text != LastSIR5 && cB10L9.Text != LastSIR5 && cB10L10.Text != LastSIR5 && cB10Trommel.Text != LastSIR5 && cB10Steuer.Text != LastSIR5)

                {
                    lBErsatz.Items.Add(LastSIR5);
                }
            }
            catch { }
            if (cB10R5.SelectedItem != null)
            {
                LastSIR5 = cB10R5.SelectedItem.ToString();
            }
            else
            {
                LastSIR5 = "";
            }
        }

        private void cB10R6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R6.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR6) && LastSIR6 != "" && cB10R1.Text != LastSIR6 && cB10R2.Text != LastSIR6 && cB10R3.Text != LastSIR6 && cB10R4.Text != LastSIR6 && cB10R5.Text != LastSIR6 && cB10R7.Text != LastSIR6 && cB10R8.Text != LastSIR6 && cB10R9.Text != LastSIR6 && cB10R10.Text != LastSIR6 && cB10L1.Text != LastSIR6 && cB10L2.Text != LastSIR6 && cB10L3.Text != LastSIR6 && cB10L4.Text != LastSIR6 && cB10L5.Text != LastSIR6 && cB10L6.Text != LastSIR6 && cB10L7.Text != LastSIR6 && cB10L8.Text != LastSIR6 && cB10L9.Text != LastSIR6 && cB10L10.Text != LastSIR6 && cB10Trommel.Text != LastSIR6 && cB10Steuer.Text != LastSIR6)

                {
                    lBErsatz.Items.Add(LastSIR6);
                }
            }
            catch { }
            if (cB10R6.SelectedItem != null)
            {
                LastSIR6 = cB10R6.SelectedItem.ToString();
            }
            else
            {
                LastSIR6 = "";
            }
        }

        private void cB10R7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R7.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR7) && LastSIR7 != ""  && cB10R1.Text != LastSIR7 && cB10R2.Text != LastSIR7 && cB10R3.Text != LastSIR7 && cB10R4.Text != LastSIR7 && cB10R5.Text != LastSIR7 && cB10R6.Text != LastSIR7 && cB10R8.Text != LastSIR7 && cB10R9.Text != LastSIR7 && cB10R10.Text != LastSIR7 && cB10L1.Text != LastSIR7 && cB10L2.Text != LastSIR7 && cB10L3.Text != LastSIR7 && cB10L4.Text != LastSIR7 && cB10L5.Text != LastSIR7 && cB10L6.Text != LastSIR7 && cB10L7.Text != LastSIR7 && cB10L8.Text != LastSIR7 && cB10L9.Text != LastSIR7 && cB10L10.Text != LastSIR7 && cB10Trommel.Text != LastSIR7 && cB10Steuer.Text != LastSIR7)
                {
                    lBErsatz.Items.Add(LastSIR7);
                }
            }
            catch { }
            if (cB10R7.SelectedItem != null)
            {
                LastSIR7 = cB10R7.SelectedItem.ToString();
            }
            else
            {
                LastSIR7 = "";
            }
        }

        private void cB10R8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R8.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR8) && LastSIR8 != "" && cB10R1.Text != LastSIR8 && cB10R2.Text != LastSIR8 && cB10R3.Text != LastSIR8 && cB10R4.Text != LastSIR8 && cB10R5.Text != LastSIR8 && cB10R6.Text != LastSIR8 && cB10R7.Text != LastSIR8 && cB10R9.Text != LastSIR8 && cB10R10.Text != LastSIR8 && cB10L1.Text != LastSIR8 && cB10L2.Text != LastSIR8 && cB10L3.Text != LastSIR8 && cB10L4.Text != LastSIR8 && cB10L5.Text != LastSIR8 && cB10L6.Text != LastSIR8 && cB10L7.Text != LastSIR8 && cB10L8.Text != LastSIR8 && cB10L9.Text != LastSIR8 && cB10L10.Text != LastSIR8 && cB10Trommel.Text != LastSIR8 && cB10Steuer.Text != LastSIR8)
                {
                    lBErsatz.Items.Add(LastSIR8);
                }
            }
            catch { }
            if (cB10R8.SelectedItem != null)
            {
                LastSIR8 = cB10R8.SelectedItem.ToString();
            }
            else
            {
                LastSIR8 = "";
            }
        }

        private void cB10R9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R9.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR9) && LastSIR9 != "" && cB10R1.Text != LastSIR9 && cB10R2.Text != LastSIR9 && cB10R3.Text != LastSIR9 && cB10R4.Text != LastSIR9 && cB10R5.Text != LastSIR9 && cB10R6.Text != LastSIR9 && cB10R7.Text != LastSIR9 && cB10R8.Text != LastSIR9 && cB10R10.Text != LastSIR9 && cB10L1.Text != LastSIR9 && cB10L2.Text != LastSIR9 && cB10L3.Text != LastSIR9 && cB10L4.Text != LastSIR9 && cB10L5.Text != LastSIR9 && cB10L6.Text != LastSIR9 && cB10L7.Text != LastSIR9 && cB10L8.Text != LastSIR9 && cB10L9.Text != LastSIR9 && cB10L10.Text != LastSIR9 && cB10Trommel.Text != LastSIR9 && cB10Steuer.Text != LastSIR9)
                {
                    lBErsatz.Items.Add(LastSIR9);
                }
            }
            catch { }
            if (cB10R9.SelectedItem != null)
            {
                LastSIR9 = cB10R9.SelectedItem.ToString();
            }
            else
            {
                LastSIR9 = "";
            }
        }

        private void cB10R10_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10R10.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR10) && LastSIR10 != "" && cB10R1.Text != LastSIR10 && cB10R2.Text != LastSIR10 && cB10R3.Text != LastSIR10 && cB10R4.Text != LastSIR10 && cB10R5.Text != LastSIR10 && cB10R6.Text != LastSIR10 && cB10R7.Text != LastSIR10 && cB10R8.Text != LastSIR10 && cB10R9.Text != LastSIR10 && cB10L1.Text != LastSIR10 && cB10L2.Text != LastSIR10 && cB10L3.Text != LastSIR10 && cB10L4.Text != LastSIR10 && cB10L5.Text != LastSIR10 && cB10L6.Text != LastSIR10 && cB10L7.Text != LastSIR10 && cB10L8.Text != LastSIR10 && cB10L9.Text != LastSIR10 && cB10L10.Text != LastSIR10 && cB10Trommel.Text != LastSIR10 && cB10Steuer.Text != LastSIR10)
                {
                    lBErsatz.Items.Add(LastSIR10);
                }
            }
            catch { }
            if (cB10R10.SelectedItem != null)
            {
                LastSIR10 = cB10R10.SelectedItem.ToString();
            }
            else
            {
                LastSIR10 = "";
            }
        }
        #endregion

        #region SelectedIndexChangedTrommelSteuer
        private void cB10Trommel_SelectedIndexChanged(object sender, EventArgs e)
        {            
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10Trommel.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIT) && LastSIT != "" && cB10R1.Text != LastSIT && cB10R2.Text != LastSIT && cB10R3.Text != LastSIT && cB10R4.Text != LastSIT && cB10R5.Text != LastSIT && cB10R6.Text != LastSIT && cB10R7.Text != LastSIT && cB10R8.Text != LastSIT && cB10R9.Text != LastSIT && cB10R10.Text != LastSIT && cB10L1.Text != LastSIT && cB10L2.Text != LastSIT && cB10L3.Text != LastSIT && cB10L4.Text != LastSIT && cB10L5.Text != LastSIT && cB10L6.Text != LastSIT && cB10L7.Text != LastSIT && cB10L8.Text != LastSIT && cB10L9.Text != LastSIT && cB10L10.Text != LastSIT && cB10Steuer.Text != LastSIT)
                {
                    lBErsatz.Items.Add(LastSIT);
                }
            }
            catch { }
            if (cB10Trommel.SelectedItem != null)
            {
                LastSIT = cB10Trommel.SelectedItem.ToString();
            }
            else
            {
                LastSIT = "";
            }
        }

        private void cB10Steuer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10Steuer.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIS) && LastSIS != "" && cB10R1.Text != LastSIS && cB10R2.Text != LastSIS && cB10R3.Text != LastSIS && cB10R4.Text != LastSIS && cB10R5.Text != LastSIS && cB10R6.Text != LastSIS && cB10R7.Text != LastSIS && cB10R8.Text != LastSIS && cB10R9.Text != LastSIS && cB10R10.Text != LastSIS && cB10L1.Text != LastSIS && cB10L2.Text != LastSIS && cB10L3.Text != LastSIS && cB10L4.Text != LastSIS && cB10L5.Text != LastSIS && cB10L6.Text != LastSIS && cB10L7.Text != LastSIS && cB10L8.Text != LastSIS && cB10L9.Text != LastSIS && cB10L10.Text != LastSIS && cB10Trommel.Text != LastSIS)
                {
                    lBErsatz.Items.Add(LastSIS);
                }
            }
            catch { }
            if (cB10Steuer.SelectedItem != null)
            {
                LastSIS = cB10Steuer.SelectedItem.ToString();
            }
            else
            {
                LastSIS = "";
            }
        }
        #endregion

        #region SelectedIndexChangedLinks
        private void cB10L1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L1.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL1) && LastSIL1 != "" && cB10R1.Text != LastSIL1 && cB10R2.Text != LastSIL1 && cB10R3.Text != LastSIL1 && cB10R4.Text != LastSIL1 && cB10R5.Text != LastSIL1 && cB10R6.Text != LastSIL1 && cB10R7.Text != LastSIL1 && cB10R8.Text != LastSIL1 && cB10R9.Text != LastSIL1 && cB10R10.Text != LastSIL1 && cB10L2.Text != LastSIL1 && cB10L3.Text != LastSIL1 && cB10L4.Text != LastSIL1 && cB10L5.Text != LastSIL1 && cB10L6.Text != LastSIL1 && cB10L7.Text != LastSIL1 && cB10L8.Text != LastSIL1 && cB10L9.Text != LastSIL1 && cB10L10.Text != LastSIL1 && cB10Trommel.Text != LastSIL1 && cB10Steuer.Text != LastSIL1)
                {
                    lBErsatz.Items.Add(LastSIL1);
                }
            }
            catch { }
            if (cB10L1.SelectedItem != null)
            {
                LastSIL1 = cB10L1.SelectedItem.ToString();
            }
            else
            {
                LastSIL1 = "";
            }
        }

        private void cB10L2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L2.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL2) && LastSIL2 != "" && cB10R1.Text != LastSIL2 && cB10R2.Text != LastSIL2 && cB10R3.Text != LastSIL2 && cB10R4.Text != LastSIL2 && cB10R5.Text != LastSIL2 && cB10R6.Text != LastSIL2 && cB10R7.Text != LastSIL2 && cB10R8.Text != LastSIL2 && cB10R9.Text != LastSIL2 && cB10R10.Text != LastSIL2 && cB10L1.Text != LastSIL2 && cB10L3.Text != LastSIL2 && cB10L4.Text != LastSIL2 && cB10L5.Text != LastSIL2 && cB10L6.Text != LastSIL2 && cB10L7.Text != LastSIL2 && cB10L8.Text != LastSIL2 && cB10L9.Text != LastSIL2 && cB10L10.Text != LastSIL2 && cB10Trommel.Text != LastSIL2 && cB10Steuer.Text != LastSIL2)
                {
                    lBErsatz.Items.Add(LastSIL2);
                }
            }
            catch { }
            if (cB10L2.SelectedItem != null)
            {
                LastSIL2 = cB10L2.SelectedItem.ToString();
            }
            else
            {
                LastSIL2 = "";
            }
        }

        private void cB10L3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L3.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL3) && LastSIL3 != "" && cB10R1.Text != LastSIL3 && cB10R2.Text != LastSIL3 && cB10R3.Text != LastSIL3 && cB10R4.Text != LastSIL3 && cB10R5.Text != LastSIL3 && cB10R6.Text != LastSIL3 && cB10R7.Text != LastSIL3 && cB10R8.Text != LastSIL3 && cB10R9.Text != LastSIL3 && cB10R10.Text != LastSIL3 && cB10L1.Text != LastSIL3 && cB10L2.Text != LastSIL3 && cB10L4.Text != LastSIL3 && cB10L5.Text != LastSIL3 && cB10L6.Text != LastSIL3 && cB10L7.Text != LastSIL3 && cB10L8.Text != LastSIL3 && cB10L9.Text != LastSIL3 && cB10L10.Text != LastSIL3 && cB10Trommel.Text != LastSIL3 && cB10Steuer.Text != LastSIL3)
                {
                    lBErsatz.Items.Add(LastSIL3);
                }
            }
            catch { }
            if (cB10L3.SelectedItem != null)
            {
                LastSIL3 = cB10L3.SelectedItem.ToString();
            }
            else
            {
                LastSIL3 = "";
            }
        }

        private void cB10L4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L4.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL4) && LastSIL4 != "" && cB10R1.Text != LastSIL4 && cB10R2.Text != LastSIL4 && cB10R3.Text != LastSIL4 && cB10R4.Text != LastSIL4 && cB10R5.Text != LastSIL4 && cB10R6.Text != LastSIL4 && cB10R7.Text != LastSIL4 && cB10R8.Text != LastSIL4 && cB10R9.Text != LastSIL4 && cB10R10.Text != LastSIL4 && cB10L1.Text != LastSIL4 && cB10L2.Text != LastSIL4 && cB10L3.Text != LastSIL4 && cB10L5.Text != LastSIL4 && cB10L6.Text != LastSIL4 && cB10L7.Text != LastSIL4 && cB10L8.Text != LastSIL4 && cB10L9.Text != LastSIL4 && cB10L10.Text != LastSIL4 && cB10Trommel.Text != LastSIL4 && cB10Steuer.Text != LastSIL4)
                {
                    lBErsatz.Items.Add(LastSIL4);
                }
            }
            catch { }
            if (cB10L4.SelectedItem != null)
            {
                LastSIL4 = cB10L4.SelectedItem.ToString();
            }
            else
            {
                LastSIL4 = "";
            }
        }

        private void cB10L5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L5.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL5) && LastSIL5 != "" && cB10R1.Text != LastSIL5 && cB10R2.Text != LastSIL5 && cB10R3.Text != LastSIL5 && cB10R4.Text != LastSIL5 && cB10R5.Text != LastSIL5 && cB10R6.Text != LastSIL5 && cB10R7.Text != LastSIL5 && cB10R8.Text != LastSIL5 && cB10R9.Text != LastSIL5 && cB10R10.Text != LastSIL5 && cB10L1.Text != LastSIL5 && cB10L2.Text != LastSIL5 && cB10L3.Text != LastSIL5 && cB10L4.Text != LastSIL5 && cB10L6.Text != LastSIL5 && cB10L7.Text != LastSIL5 && cB10L8.Text != LastSIL5 && cB10L9.Text != LastSIL5 && cB10L10.Text != LastSIL5 && cB10Trommel.Text != LastSIL5 && cB10Steuer.Text != LastSIL5)
                {
                    lBErsatz.Items.Add(LastSIL5);
                }
            }
            catch { }
            if (cB10L5.SelectedItem != null)
            {
                LastSIL5 = cB10L5.SelectedItem.ToString();
            }
            else
            {
                LastSIL5 = "";
            }
        }

        private void cB10L6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L6.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL6) && LastSIL6 != "" && cB10R1.Text != LastSIL6 && cB10R2.Text != LastSIL6 && cB10R3.Text != LastSIL6 && cB10R4.Text != LastSIL6 && cB10R5.Text != LastSIL6 && cB10R6.Text != LastSIL6 && cB10R7.Text != LastSIL6 && cB10R8.Text != LastSIL6 && cB10R9.Text != LastSIL6 && cB10R10.Text != LastSIL6 && cB10L1.Text != LastSIL6 && cB10L2.Text != LastSIL6 && cB10L3.Text != LastSIL6 && cB10L4.Text != LastSIL6 && cB10L5.Text != LastSIL6 && cB10L7.Text != LastSIL6 && cB10L8.Text != LastSIL6 && cB10L9.Text != LastSIL6 && cB10L10.Text != LastSIL6 && cB10Trommel.Text != LastSIL6 && cB10Steuer.Text != LastSIL6)
                {
                    lBErsatz.Items.Add(LastSIL6);
                }
            }
            catch { }
            if (cB10L6.SelectedItem != null)
            {
                LastSIL6 = cB10L6.SelectedItem.ToString();
            }
            else
            {
                LastSIL6 = "";
            }
        }

        private void cB10L7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L7.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL7) && LastSIL7 != "" && cB10R1.Text != LastSIL7 && cB10R2.Text != LastSIL7 && cB10R3.Text != LastSIL7 && cB10R4.Text != LastSIL7 && cB10R5.Text != LastSIL7 && cB10R6.Text != LastSIL7 && cB10R7.Text != LastSIL7 && cB10R8.Text != LastSIL7 && cB10R9.Text != LastSIL7 && cB10R10.Text != LastSIL7 && cB10L1.Text != LastSIL7 && cB10L2.Text != LastSIL7 && cB10L3.Text != LastSIL7 && cB10L4.Text != LastSIL7 && cB10L5.Text != LastSIL7 && cB10L6.Text != LastSIL7 && cB10L8.Text != LastSIL7 && cB10L9.Text != LastSIL7 && cB10L10.Text != LastSIL7 && cB10Trommel.Text != LastSIL7 && cB10Steuer.Text != LastSIL7)
                {
                    lBErsatz.Items.Add(LastSIL7);
                }
            }
            catch { }
            if (cB10L7.SelectedItem != null)
            {
                LastSIL7 = cB10L7.SelectedItem.ToString();
            }
            else
            {
                LastSIL7 = "";
            }
        }

        private void cB10L8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L8.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL8) && LastSIL8 != "" && cB10R1.Text != LastSIL8 && cB10R2.Text != LastSIL8 && cB10R3.Text != LastSIL8 && cB10R4.Text != LastSIL8 && cB10R5.Text != LastSIL8 && cB10R6.Text != LastSIL8 && cB10R7.Text != LastSIL8 && cB10R8.Text != LastSIL8 && cB10R9.Text != LastSIL8 && cB10R10.Text != LastSIL8 && cB10L1.Text != LastSIL8 && cB10L2.Text != LastSIL8 && cB10L3.Text != LastSIL8 && cB10L4.Text != LastSIL8 && cB10L5.Text != LastSIL8 && cB10L6.Text != LastSIL8 && cB10L7.Text != LastSIL8 && cB10L9.Text != LastSIL8 && cB10L10.Text != LastSIL8 && cB10Trommel.Text != LastSIL8 && cB10Steuer.Text != LastSIL8)
                {
                    lBErsatz.Items.Add(LastSIL8);
                }
            }
            catch { }
            if (cB10L8.SelectedItem != null)
            {
                LastSIL8 = cB10L8.SelectedItem.ToString();
            }
            else
            {
                LastSIL8 = "";
            }
        }

        private void cB10L9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L9.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL9) && LastSIL9 != "" && cB10R1.Text != LastSIL9 && cB10R2.Text != LastSIL9 && cB10R3.Text != LastSIL9 && cB10R4.Text != LastSIL9 && cB10R5.Text != LastSIL9 && cB10R6.Text != LastSIL9 && cB10R7.Text != LastSIL9 && cB10R8.Text != LastSIL9 && cB10R9.Text != LastSIL9 && cB10R10.Text != LastSIL9 && cB10L1.Text != LastSIL9 && cB10L2.Text != LastSIL9 && cB10L3.Text != LastSIL9 && cB10L4.Text != LastSIL9 && cB10L5.Text != LastSIL9 && cB10L6.Text != LastSIL9 && cB10L7.Text != LastSIL9 && cB10L8.Text != LastSIL9 && cB10L10.Text != LastSIL9 && cB10Trommel.Text != LastSIL9 && cB10Steuer.Text != LastSIL9)
                {
                    lBErsatz.Items.Add(LastSIL9);
                }
            }
            catch { }
            if (cB10L9.SelectedItem != null)
            {
                LastSIL9 = cB10L9.SelectedItem.ToString();
            }
            else
            {
                LastSIL9 = "";
            }
        }

        private void cB10L10_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB10L10.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL10) && LastSIL10 != "" && cB10R1.Text != LastSIL10 && cB10R2.Text != LastSIL10 && cB10R3.Text != LastSIL10 && cB10R4.Text != LastSIL10 && cB10R5.Text != LastSIL10 && cB10R6.Text != LastSIL10 && cB10R7.Text != LastSIL10 && cB10R8.Text != LastSIL10 && cB10R9.Text != LastSIL10 && cB10R10.Text != LastSIL10 && cB10L1.Text != LastSIL10 && cB10L2.Text != LastSIL10 && cB10L3.Text != LastSIL10 && cB10L4.Text != LastSIL10 && cB10L5.Text != LastSIL10 && cB10L6.Text != LastSIL10 && cB10L7.Text != LastSIL10 && cB10L8.Text != LastSIL10 && cB10L9.Text != LastSIL10 && cB10Trommel.Text != LastSIL10 && cB10Steuer.Text != LastSIL10)
                {
                    lBErsatz.Items.Add(LastSIL10);
                }
            }
            catch { }
            if (cB10L10.SelectedItem != null)
            {
                LastSIL10 = cB10L10.SelectedItem.ToString();
            }
            else
            {
                LastSIL10 = "";
            }
        }
        #endregion

        #region cB5SelectedIndex

        private void cb5Trommel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5Trommel.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIT) && LastSIT != "" && cB5R1.Text != LastSIT && cB5R2.Text != LastSIT && cB5R3.Text != LastSIT && cB5R4.Text != LastSIT && cB5R5.Text != LastSIT && cB5L1.Text != LastSIT && cB5L2.Text != LastSIT && cB5L3.Text != LastSIT && cB5L4.Text != LastSIT && cB5L5.Text != LastSIT && cB5Steuer.Text != LastSIT)
                {
                    lBErsatz.Items.Add(LastSIT);
                }
            }
            catch { }
            if (cB5Trommel.SelectedItem != null)
            {
                LastSIT = cB5Trommel.SelectedItem.ToString();
            }
            else
            {
                LastSIT = "";
            }
        }

        private void cB5LSteuer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5Steuer.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIS) && LastSIS != "" && cB5R1.Text != LastSIS && cB5R2.Text != LastSIS && cB5R3.Text != LastSIS && cB5R4.Text != LastSIS && cB5R5.Text != LastSIS && cB5L1.Text != LastSIS && cB5L2.Text != LastSIS && cB5L3.Text != LastSIS && cB5L4.Text != LastSIS && cB5L5.Text != LastSIS && cB5Steuer.Text != LastSIS)
                {
                    lBErsatz.Items.Add(LastSIS);
                }
            }
            catch { }
            if (cB5Steuer.SelectedItem != null)
            {
                LastSIS = cB5Steuer.SelectedItem.ToString();
            }
            else
            {
                LastSIS = "";
            }
        }

        private void cB5L1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5L1.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL1) && LastSIL1 != "" && cB5R1.Text != LastSIL1 && cB5R2.Text != LastSIL1 && cB5R3.Text != LastSIL1 && cB5R4.Text != LastSIL1 && cB5R5.Text != LastSIL1 && cB5L2.Text != LastSIL1 && cB5L3.Text != LastSIL1 && cB5L4.Text != LastSIL1 && cB5L5.Text != LastSIL1 && cB5Steuer.Text != LastSIL1 && cB5Steuer.Text != LastSIL1)
                {
                    lBErsatz.Items.Add(LastSIL1);
                }
            }
            catch { }
            if (cB5L1.SelectedItem != null)
            {
                LastSIL1 = cB5L1.SelectedItem.ToString();
            }
            else
            {
                LastSIL1 = "";
            }
        }

        private void cB5L2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5L2.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL2) && LastSIL2 != "" && cB5R1.Text != LastSIL2 && cB5R2.Text != LastSIL2 && cB5R3.Text != LastSIL2 && cB5R4.Text != LastSIL2 && cB5R5.Text != LastSIL2 && cB5L1.Text != LastSIL2 && cB5L3.Text != LastSIL2 && cB5L4.Text != LastSIL2 && cB5L5.Text != LastSIL2 && cB5Steuer.Text != LastSIL2 && cB5Steuer.Text != LastSIL2)
                {
                    lBErsatz.Items.Add(LastSIL2);
                }
            }
            catch { }
            if (cB5L2.SelectedItem != null)
            {
                LastSIL2 = cB5L2.SelectedItem.ToString();
            }
            else
            {
                LastSIL2 = "";
            }
        }

        private void cB5L3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5L3.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL3) && LastSIL3 != "" && cB5R1.Text != LastSIL3 && cB5R2.Text != LastSIL3 && cB5R3.Text != LastSIL3 && cB5R4.Text != LastSIL3 && cB5R5.Text != LastSIL3 && cB5L1.Text != LastSIL3 && cB5L2.Text != LastSIL3 && cB5L4.Text != LastSIL3 && cB5L5.Text != LastSIL3 && cB5Steuer.Text != LastSIL3 && cB5Steuer.Text != LastSIL3)

                {
                    lBErsatz.Items.Add(LastSIL3);
                }
            }
            catch { }
            if (cB5L3.SelectedItem != null)
            {
                LastSIL3 = cB5L3.SelectedItem.ToString();
            }
            else
            {
                LastSIL3 = "";
            }
        }

        private void cB5L4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5L4.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL4) && LastSIL4 != "" && cB5R1.Text != LastSIL4 && cB5R2.Text != LastSIL4 && cB5R3.Text != LastSIL4 && cB5R4.Text != LastSIL4 && cB5R5.Text != LastSIL4 && cB5L1.Text != LastSIL4 && cB5L2.Text != LastSIL4 && cB5L3.Text != LastSIL4 && cB5L5.Text != LastSIL4 && cB5Steuer.Text != LastSIL4 && cB5Steuer.Text != LastSIL4)

                {
                    lBErsatz.Items.Add(LastSIL4);
                }
            }
            catch { }
            if (cB5L4.SelectedItem != null)
            {
                LastSIL4 = cB5L4.SelectedItem.ToString();
            }
            else
            {
                LastSIL4 = "";
            }
        }

        private void cB5L5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5L5.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIL5) && LastSIL5 != "" && cB5R1.Text != LastSIL5 && cB5R2.Text != LastSIL5 && cB5R3.Text != LastSIL5 && cB5R4.Text != LastSIL5 && cB5R5.Text != LastSIL5 && cB5L1.Text != LastSIL5 && cB5L2.Text != LastSIL5 && cB5L3.Text != LastSIL5 && cB5L4.Text != LastSIL5 && cB5Steuer.Text != LastSIL5 && cB5Steuer.Text != LastSIL5)

                {
                    lBErsatz.Items.Add(LastSIL5);
                }
            }
            catch { }
            if (cB5L5.SelectedItem != null)
            {
                LastSIL5 = cB5L5.SelectedItem.ToString();
            }
            else
            {
                LastSIL5 = "";
            }
        }

        private void cB5R1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5R1.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR1) && LastSIR1 != "" && cB5L5.Text != LastSIR1 && cB5R2.Text != LastSIR1 && cB5R3.Text != LastSIR1 && cB5R4.Text != LastSIR1 && cB5R5.Text != LastSIR1 && cB5L1.Text != LastSIR1 && cB5L2.Text != LastSIR1 && cB5L3.Text != LastSIR1 && cB5L4.Text != LastSIR1 && cB5Steuer.Text != LastSIR1 && cB5Steuer.Text != LastSIR1)

                {
                    lBErsatz.Items.Add(LastSIR1);
                }
            }
            catch { }
            if (cB5R1.SelectedItem != null)
            {
                LastSIR1 = cB5R1.SelectedItem.ToString();
            }
            else
            {
                LastSIR1 = "";
            }
        }

        private void cB5R2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5R2.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR2) && LastSIR2 != "" && cB5L5.Text != LastSIR2 && cB5R1.Text != LastSIR2 && cB5R3.Text != LastSIR2 && cB5R4.Text != LastSIR2 && cB5R5.Text != LastSIR2 && cB5L1.Text != LastSIR2 && cB5L2.Text != LastSIR2 && cB5L3.Text != LastSIR2 && cB5L4.Text != LastSIR2 && cB5Steuer.Text != LastSIR2 && cB5Steuer.Text != LastSIR2)

                {
                    lBErsatz.Items.Add(LastSIR2);
                }
            }
            catch { }
            if (cB5R2.SelectedItem != null)
            {
                LastSIR2 = cB5R2.SelectedItem.ToString();
            }
            else
            {
                LastSIR2 = "";
            }
        }

        private void cB5R3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5R3.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR3) && LastSIR3 != "" && cB5L5.Text != LastSIR3 && cB5R1.Text != LastSIR3 && cB5R2.Text != LastSIR3 && cB5R4.Text != LastSIR3 && cB5R5.Text != LastSIR3 && cB5L1.Text != LastSIR3 && cB5L2.Text != LastSIR3 && cB5L3.Text != LastSIR3 && cB5L4.Text != LastSIR3 && cB5Steuer.Text != LastSIR3 && cB5Steuer.Text != LastSIR3)
                {
                    lBErsatz.Items.Add(LastSIR3);
                }
            }
            catch { }
            if (cB5R3.SelectedItem != null)
            {
                LastSIR3 = cB5R3.SelectedItem.ToString();
            }
            else
            {
                LastSIR3 = "";
            }
        }

        private void cB5R4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5R4.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR4) && LastSIR4 != "" && cB5L5.Text != LastSIR4 && cB5R1.Text != LastSIR4 && cB5R2.Text != LastSIR4 && cB5R3.Text != LastSIR4 && cB5R5.Text != LastSIR4 && cB5L1.Text != LastSIR4 && cB5L2.Text != LastSIR4 && cB5L3.Text != LastSIR4 && cB5L4.Text != LastSIR4 && cB5Steuer.Text != LastSIR4 && cB5Steuer.Text != LastSIR4)
                {
                    lBErsatz.Items.Add(LastSIR4);
                }
            }
            catch { }
            if (cB5R4.SelectedItem != null)
            {
                LastSIR4 = cB5R4.SelectedItem.ToString();
            }
            else
            {
                LastSIR4 = "";
            }
        }

        private void cB5R5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Anfang == false)
            {
                BerechneDifferenzen();
            }
            lBErsatz.Items.Remove(cB5R5.SelectedItem.ToString());
            try
            {
                if (!lBErsatz.Items.Contains(LastSIR5) && LastSIR5 != "" && cB5L5.Text != LastSIR5 && cB5R1.Text != LastSIR5 && cB5R2.Text != LastSIR5 && cB5R3.Text != LastSIR5 && cB5R4.Text != LastSIR5 && cB5L1.Text != LastSIR5 && cB5L2.Text != LastSIR5 && cB5L3.Text != LastSIR5 && cB5L4.Text != LastSIR5 && cB5Steuer.Text != LastSIR5 && cB5Steuer.Text != LastSIR5)

                {
                    lBErsatz.Items.Add(LastSIR5);
                }
            }
            catch { }
            if (cB5R5.SelectedItem != null)
            {
                LastSIR5 = cB5R5.SelectedItem.ToString();
            }
            else
            {
                LastSIR5 = "";
            }
        }

        #endregion

        #region Gewicht
        private void BerechneGewichtRechts()
        {
            GewichtRechts = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R1.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R2.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R3.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R4.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R5.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R6.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R7.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R8.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R9.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R10.Text)]; }
                catch { }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R1.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R2.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R3.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R4.Text)]; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R5.Text)]; }
                catch { }
            }
            lblGewichtRechts.Text = GewichtRechts.ToString();
            BerechneGesamtGewicht();
        }
        private void BerechneGewichtLinks()
        {
            GewichtLinks = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L1.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L2.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L3.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L4.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L5.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L6.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L7.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L8.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L9.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L10.Text)]; }
                catch { }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L1.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L2.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L3.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L4.Text)]; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L5.Text)]; }
                catch { }
            }

            lblGewichtLinks.Text = GewichtLinks.ToString();
            BerechneGesamtGewicht();
        }

        private void BerechneGewichtRechtsPro()
        {
            GewichtRechts = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R1.Text)] * 0.2575 ; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R2.Text)] * 0.32; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R3.Text)] * 0.3625; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R4.Text)] * 0.39; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R5.Text)] * 0.40 ; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R6.Text)] * 0.40; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R7.Text)] * 0.385; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R8.Text)] * 0.35; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R9.Text)] * 0.2925; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB10R10.Text)] * 0.21; }
                catch { }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R1.Text)]*0.3185; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R2.Text)]*0.35; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R3.Text)]*0.3625; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R4.Text)]*0.339; }
                catch { }
                try { GewichtRechts += iGewicht[sName.IndexOf(cB5R5.Text)]*0.2875; }
                catch { }
            }
            lblGewichtRechts.Text = GewichtRechts.ToString();
            BerechneGesamtGewicht();
        }
        private void BerechneGewichtLinksPro()
        {
            GewichtLinks = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L1.Text)] * 0.2575; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L2.Text)] *0.32; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L3.Text)] *0.3625; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L4.Text)] *0.39; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L5.Text)] *0.40; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L6.Text)]* 0.40; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L7.Text)]* 0.385; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L8.Text)]* 0.35; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L9.Text)]* 0.2925; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB10L10.Text)]* 0.21; }
                catch { }
                try { GewichtLinks += 2 * 0.45; }
                catch { }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L1.Text)]*0.3185; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L2.Text)]*0.35; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L3.Text)]*0.3625; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L4.Text)]*0.339; }
                catch { }
                try { GewichtLinks += iGewicht[sName.IndexOf(cB5L5.Text)]*0.2875; }
                catch { }
                try { GewichtLinks += 2 * 0.4; }
                catch { }
            }

            lblGewichtLinks.Text = GewichtLinks.ToString();
            BerechneGesamtGewicht();
        }

        private int BerechneVorne()
        {
            int Vorne = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { Vorne += iGewicht[sName.IndexOf(cB10Trommel.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R1.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R2.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R3.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R4.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R5.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L1.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L2.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L3.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L4.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L5.Text)]; }
                catch { }
                return Vorne;
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { Vorne += iGewicht[sName.IndexOf(cB5Trommel.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5R1.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5R2.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5L1.Text)]; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5L2.Text)]; }
                catch { }
                return Vorne;
            }
            else
            {
                return Vorne;
            }
        }
        private int BerechneHinten()
        {
            int Hinten = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { Hinten += iGewicht[sName.IndexOf(cB10Steuer.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R6.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R7.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R8.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R9.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R10.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L6.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L7.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L8.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L9.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L10.Text)]; }
                catch { }
                return Hinten;
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { Hinten += iGewicht[sName.IndexOf(cB5Steuer.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5R4.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5R5.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5L4.Text)]; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5L5.Text)]; }
                catch { }
                return Hinten;
            }
            else
            {
                return Hinten;
            }
        }

        private double BerechneVornePro()
        {
            double Vorne = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { Vorne += iGewicht[sName.IndexOf(cB10Trommel.Text)]*5.1; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R1.Text)]*3.3775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R2.Text)]*2.5775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R3.Text)]*1.7775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R4.Text)]*0.9775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10R5.Text)]*0.1775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L1.Text)] * 3.3775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L2.Text)] * 2.5775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L3.Text)] * 1.7775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L4.Text)] * 0.9775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB10L5.Text)] * 0.1775; }
                catch { }
                return (Vorne / 3.13);
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { Vorne += iGewicht[sName.IndexOf(cB5Trommel.Text)]*3.56; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5R1.Text)]*1.4775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5R2.Text)]*0.5825; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5L1.Text)] * 1.4775; }
                catch { }
                try { Vorne += iGewicht[sName.IndexOf(cB5L2.Text)] * 0.5825; }
                catch { }
                return (Vorne / 2.405);
            }
            else
            {
                return Vorne;
            }
        }
        private double BerechneHintenPro()
        {
            double Hinten = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { Hinten += iGewicht[sName.IndexOf(cB10Steuer.Text)]*4.545; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R6.Text)]*0.6225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R7.Text)]*1.4225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R8.Text)]*2.2225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R9.Text)]*3.0225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10R10.Text)]*3.8225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L6.Text)] * 0.6225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L7.Text)] * 1.4225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L8.Text)] * 2.2225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L9.Text)] * 3.0225; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB10L10.Text)] * 3.8225; }
                catch { }
                return (Hinten/3.13);
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { Hinten += iGewicht[sName.IndexOf(cB5Steuer.Text)]*2.84; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5R3.Text)]*0.3125; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5R4.Text)]*1.2075; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5R5.Text)]*2.1025; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5L3.Text)] * 0.3125; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5L4.Text)] * 1.2075; }
                catch { }
                try { Hinten += iGewicht[sName.IndexOf(cB5L5.Text)] * 2.1025; }
                catch { }
                return (Hinten/2.405);
            }
            else
            {
                return Hinten;
            }
        }

        private void BerechneGesamtGewicht()
        {
            GesamtGewicht = 0;
            GesamtGewicht += GewichtLinks;
            GesamtGewicht += GewichtRechts;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                try { GesamtGewicht += iGewicht[sName.IndexOf(cB10Trommel.Text)]; }
                catch { }
                try { GesamtGewicht += iGewicht[sName.IndexOf(cB10Steuer.Text)]; }
                catch { }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                try { GesamtGewicht += iGewicht[sName.IndexOf(cB5Trommel.Text)]; }
                catch { }
                try { GesamtGewicht += iGewicht[sName.IndexOf(cB5Steuer.Text)]; }
                catch { }
            }
            lblGesamtGewicht.Text = GesamtGewicht.ToString();

            //ZähleFrauen();
            //ZähleMänner();
        }
        private void BerechneDifferenzen()
        {
            double Vorne, Hinten;
            if (!cBoxDynGewichte.Checked)
            {
                BerechneGewichtLinks();
                BerechneGewichtRechts();
                Vorne = BerechneVorne();
                Hinten = BerechneHinten();
            }
            else
            {
                BerechneGewichtLinksPro();
                BerechneGewichtRechtsPro();
                Vorne = BerechneVornePro();
                Hinten = BerechneHintenPro();
            }
            if (cBBootsTyp.SelectedIndex == 0)
            {
                double RLDif10;
                double VHDif10;
                RLDif10 = GewichtRechts - GewichtLinks;
                VHDif10 = Vorne - Hinten;
                if (RLDif10 > 0)
                {
                    lbl10GDifLinks.Visible = false;
                    lbl10GDifRechts.Visible = true;
                    lbl10GDifRechts.Text = Math.Round(Math.Abs(RLDif10),3).ToString() + " kg";
                }
                else if (RLDif10 < 0)
                {
                    lbl10GDifRechts.Visible = false;
                    lbl10GDifLinks.Visible = true;
                    lbl10GDifLinks.Text = Math.Round(Math.Abs(RLDif10),3).ToString() + " kg";
                }
                else
                {
                    lbl10GDifRechts.Visible = false;
                    lbl10GDifLinks.Visible = false;
                }
                if (VHDif10 > 0)
                {
                    lbl10GDifHinten.Visible = false;
                    lbl10GDifVorne.Visible = true;
                    lbl10GDifVorne.Text = Math.Round(Math.Abs(VHDif10),3).ToString() + " kg";
                }
                else if (VHDif10 < 0)
                {
                    lbl10GDifVorne.Visible = false;
                    lbl10GDifHinten.Visible = true;
                    lbl10GDifHinten.Text = Math.Round(Math.Abs(VHDif10),3).ToString() + " kg";
                }
                else
                {
                    lbl10GDifVorne.Visible = false;
                    lbl10GDifHinten.Visible = false;
                }
            }
            else if (cBBootsTyp.SelectedIndex == 1)
            {
                double RLDif5;
                double VHDif5;
                RLDif5 = GewichtRechts - GewichtLinks;
                VHDif5 = Vorne - Hinten;
                if (RLDif5 > 0)
                {
                    lbl5GDifLinks.Visible = false;
                    lbl5GDifRechts.Visible = true;
                    lbl5GDifRechts.Text = Math.Round(Math.Abs(RLDif5),3).ToString() + " kg";
                }
                else if (RLDif5 < 0)
                {
                    lbl5GDifRechts.Visible = false;
                    lbl5GDifLinks.Visible = true;
                    lbl5GDifLinks.Text = Math.Round(Math.Abs(RLDif5),3).ToString() + " kg";
                }
                else
                {
                    lbl5GDifRechts.Visible = false;
                    lbl5GDifLinks.Visible = false;
                }
                if (VHDif5 > 0)
                {
                    lbl5GDifHinten.Visible = false;
                    lbl5GDifVorne.Visible = true;
                    lbl5GDifVorne.Text = Math.Round(Math.Abs(VHDif5),3).ToString() + " kg";
                }
                else if (VHDif5 < 0)
                {
                    lbl5GDifVorne.Visible = false;
                    lbl5GDifHinten.Visible = true;
                    lbl5GDifHinten.Text = Math.Round(Math.Abs(VHDif5),3).ToString() + " kg";
                }
                else
                {
                    lbl5GDifVorne.Visible = false;
                    lbl5GDifHinten.Visible = false;
                }
            }
        }
        #endregion

        #region Drucken
        private void cmd_Drucken_Click(object sender, EventArgs e)
        {
            CaptureScreen();
            printDocument1.Print();
        }
        
        private void CaptureScreen()
        {
            printDocument1.DefaultPageSettings.Landscape = true;
            Graphics myGraphics = p10BankBoot.CreateGraphics();
            Size s = tabRennen.Size;
            memoryImage = new Bitmap(s.Width-10, s.Height-20, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(this.Location.X + 20 + tabRennen.Location.X, this.Location.Y + p10BankBoot.Location.Y + 50, 0, 0, s);
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }
        #endregion

        private void cmdSpeichern_Click(object sender, EventArgs e)
        {
            if (txtAufstellungName.Text != "")
            {
                if (cBAufstellungListe.Items.Contains(txtAufstellungName.Text))
                {
                    if (cBTeams.SelectedIndex > -1)
                    {
                        if (cBBootsTyp.SelectedIndex == 0)
                        {
                            for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe10.Count; i++)
                            {
                                if (txtAufstellungName.Text == Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].AufstellungName)
                                {
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Clear();
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10Trommel.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L1.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L2.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L3.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L4.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L5.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L6.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L7.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L8.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L9.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10L10.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R1.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R2.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R3.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R4.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R5.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R6.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R7.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R8.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R9.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10R10.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].sAufstellung.Add(cB10Steuer.Text);
                                }
                            }
                        }
                        else if (cBBootsTyp.SelectedIndex == 1)
                        {
                            for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe5.Count; i++)
                            {
                                if (txtAufstellungName.Text == Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].AufstellungName)
                                {
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Clear();
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5Trommel.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5L1.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5L2.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5L3.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5L4.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5L5.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5R1.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5R2.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5R3.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5R4.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5R5.Text);
                                    Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].sAufstellung.Add(cB5Steuer.Text);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (cBTeams.SelectedIndex > -1)
                    {
                        cBAufstellungListe.Items.Add(txtAufstellungName.Text);
                        cBAufstellungLöschen.Items.Add(txtAufstellungName.Text);
                        if (cBBootsTyp.SelectedIndex == 0)
                        {
                            clsAufstellung Aufstellung = new clsAufstellung();
                            Aufstellung.AufstellungName = txtAufstellungName.Text;
                            Aufstellung.sAufstellung.Clear();
                            Aufstellung.sAufstellung.Add(cB10Trommel.Text);
                            Aufstellung.sAufstellung.Add(cB10L1.Text);
                            Aufstellung.sAufstellung.Add(cB10L2.Text);
                            Aufstellung.sAufstellung.Add(cB10L3.Text);
                            Aufstellung.sAufstellung.Add(cB10L4.Text);
                            Aufstellung.sAufstellung.Add(cB10L5.Text);
                            Aufstellung.sAufstellung.Add(cB10L6.Text);
                            Aufstellung.sAufstellung.Add(cB10L7.Text);
                            Aufstellung.sAufstellung.Add(cB10L8.Text);
                            Aufstellung.sAufstellung.Add(cB10L9.Text);
                            Aufstellung.sAufstellung.Add(cB10L10.Text);
                            Aufstellung.sAufstellung.Add(cB10R1.Text);
                            Aufstellung.sAufstellung.Add(cB10R2.Text);
                            Aufstellung.sAufstellung.Add(cB10R3.Text);
                            Aufstellung.sAufstellung.Add(cB10R4.Text);
                            Aufstellung.sAufstellung.Add(cB10R5.Text);
                            Aufstellung.sAufstellung.Add(cB10R6.Text);
                            Aufstellung.sAufstellung.Add(cB10R7.Text);
                            Aufstellung.sAufstellung.Add(cB10R8.Text);
                            Aufstellung.sAufstellung.Add(cB10R9.Text);
                            Aufstellung.sAufstellung.Add(cB10R10.Text);
                            Aufstellung.sAufstellung.Add(cB10Steuer.Text);
                            Teams[cBTeams.SelectedIndex].cAufstellungListe10.Add(Aufstellung);
                        }
                        else if (cBBootsTyp.SelectedIndex == 1)
                        {
                            clsAufstellung Aufstellung = new clsAufstellung();
                            Aufstellung.AufstellungName = txtAufstellungName.Text;
                            Aufstellung.sAufstellung.Clear();
                            Aufstellung.sAufstellung.Add(cB5Trommel.Text);
                            Aufstellung.sAufstellung.Add(cB5L1.Text);
                            Aufstellung.sAufstellung.Add(cB5L2.Text);
                            Aufstellung.sAufstellung.Add(cB5L3.Text);
                            Aufstellung.sAufstellung.Add(cB5L4.Text);
                            Aufstellung.sAufstellung.Add(cB5L5.Text);
                            Aufstellung.sAufstellung.Add(cB5R1.Text);
                            Aufstellung.sAufstellung.Add(cB5R2.Text);
                            Aufstellung.sAufstellung.Add(cB5R3.Text);
                            Aufstellung.sAufstellung.Add(cB5R4.Text);
                            Aufstellung.sAufstellung.Add(cB5R5.Text);
                            Aufstellung.sAufstellung.Add(cB5Steuer.Text);
                            Teams[cBTeams.SelectedIndex].cAufstellungListe5.Add(Aufstellung);
                        }
                    }
                }
                Serialisierung();
            }
            else
            {
                MessageBox.Show("Gibt bitte einen Aufstellungsnamen ein!", "Fehler");
            }
        }


        private void cmdAufstellungLöschen_Click(object sender, EventArgs e)
        {
            if (cBAufstellungLöschen.SelectedIndex > -1 && cBTeams.SelectedIndex > -1)
            {
                if (cBBootsTyp.SelectedIndex == 0)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe10.Count; i++)
                    {
                        if (cBAufstellungLöschen.SelectedItem.ToString() == Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].AufstellungName)
                        {
                            Teams[cBTeams.SelectedIndex].cAufstellungListe10.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe10[i]);
                        }
                    }
                }
                else if (cBBootsTyp.SelectedIndex == 1)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe5.Count; i++)
                    {
                        if (cBAufstellungLöschen.SelectedItem.ToString() == Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].AufstellungName)
                        {
                            Teams[cBTeams.SelectedIndex].cAufstellungListe5.Remove(Teams[cBTeams.SelectedIndex].cAufstellungListe5[i]);
                        }
                    }
                }
                cBAufstellungListe.Items.Remove(cBAufstellungLöschen.SelectedItem.ToString());
                cBAufstellungLöschen.Items.Remove(cBAufstellungLöschen.SelectedItem.ToString());
            }
            Serialisierung();
        }

        private void ZähleFrauen()
        {
            Frauen = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                if (cB10L1.Text != "" && sGeschlaecht[sName.IndexOf(cB10L1.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L2.Text != "" && sGeschlaecht[sName.IndexOf(cB10L2.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L3.Text != "" && sGeschlaecht[sName.IndexOf(cB10L3.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L4.Text != "" && sGeschlaecht[sName.IndexOf(cB10L4.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L5.Text != "" && sGeschlaecht[sName.IndexOf(cB10L5.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L6.Text != "" && sGeschlaecht[sName.IndexOf(cB10L6.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L7.Text != "" && sGeschlaecht[sName.IndexOf(cB10L7.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L8.Text != "" && sGeschlaecht[sName.IndexOf(cB10L8.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L9.Text != "" && sGeschlaecht[sName.IndexOf(cB10L9.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10L10.Text != "" && sGeschlaecht[sName.IndexOf(cB10L10.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R1.Text != "" && sGeschlaecht[sName.IndexOf(cB10R1.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R2.Text != "" && sGeschlaecht[sName.IndexOf(cB10R2.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R3.Text != "" && sGeschlaecht[sName.IndexOf(cB10R3.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R4.Text != "" && sGeschlaecht[sName.IndexOf(cB10R4.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R5.Text != "" && sGeschlaecht[sName.IndexOf(cB10R5.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R6.Text != "" && sGeschlaecht[sName.IndexOf(cB10R6.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R7.Text != "" && sGeschlaecht[sName.IndexOf(cB10R7.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R8.Text != "" && sGeschlaecht[sName.IndexOf(cB10R8.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R9.Text != "" && sGeschlaecht[sName.IndexOf(cB10R9.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB10R10.Text != "" && sGeschlaecht[sName.IndexOf(cB10R10.Text)] == "Weiblich")
                {
                    Frauen++;
                }
            }
            else
            {
                if (cB5L1.Text != "" && sGeschlaecht[sName.IndexOf(cB5L1.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5L2.Text != "" && sGeschlaecht[sName.IndexOf(cB5L2.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5L3.Text != "" && sGeschlaecht[sName.IndexOf(cB5L3.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5L4.Text != "" && sGeschlaecht[sName.IndexOf(cB5L4.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5L5.Text != "" && sGeschlaecht[sName.IndexOf(cB5L5.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5R1.Text != "" && sGeschlaecht[sName.IndexOf(cB5R1.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5R2.Text != "" && sGeschlaecht[sName.IndexOf(cB5R2.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5R3.Text != "" && sGeschlaecht[sName.IndexOf(cB5R3.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5R4.Text != "" && sGeschlaecht[sName.IndexOf(cB5R4.Text)] == "Weiblich")
                {
                    Frauen++;
                }
                if (cB5R5.Text != "" && sGeschlaecht[sName.IndexOf(cB5R5.Text)] == "Weiblich")
                {
                    Frauen++;
                }
            }
            lblFrauenAnz.Text = Frauen.ToString();
        }
        private void ZähleMänner()
        {
            
            Männer = 0;
            if (cBBootsTyp.SelectedIndex == 0)
            {
                if (cB10L1.Text != "" && sGeschlaecht[sName.IndexOf(cB10L1.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L2.Text != "" && sGeschlaecht[sName.IndexOf(cB10L2.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L3.Text != "" && sGeschlaecht[sName.IndexOf(cB10L3.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L4.Text != "" && sGeschlaecht[sName.IndexOf(cB10L4.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L5.Text != "" && sGeschlaecht[sName.IndexOf(cB10L5.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L6.Text != "" && sGeschlaecht[sName.IndexOf(cB10L6.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L7.Text != "" && sGeschlaecht[sName.IndexOf(cB10L7.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L8.Text != "" && sGeschlaecht[sName.IndexOf(cB10L8.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L9.Text != "" && sGeschlaecht[sName.IndexOf(cB10L9.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10L10.Text != "" && sGeschlaecht[sName.IndexOf(cB10L10.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R1.Text != "" && sGeschlaecht[sName.IndexOf(cB10R1.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R2.Text != "" && sGeschlaecht[sName.IndexOf(cB10R2.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R3.Text != "" && sGeschlaecht[sName.IndexOf(cB10R3.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R4.Text != "" && sGeschlaecht[sName.IndexOf(cB10R4.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R5.Text != "" && sGeschlaecht[sName.IndexOf(cB10R5.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R6.Text != "" && sGeschlaecht[sName.IndexOf(cB10R6.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R7.Text != "" && sGeschlaecht[sName.IndexOf(cB10R7.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R8.Text != "" && sGeschlaecht[sName.IndexOf(cB10R8.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R9.Text != "" && sGeschlaecht[sName.IndexOf(cB10R9.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB10R10.Text != "" && sGeschlaecht[sName.IndexOf(cB10R10.Text)] == "Männlich")
                {
                    Männer++;
                }
            }
            else
            {
                if (cB5L1.Text != "" && sGeschlaecht[sName.IndexOf(cB5L1.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5L2.Text != "" && sGeschlaecht[sName.IndexOf(cB5L2.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5L3.Text != "" && sGeschlaecht[sName.IndexOf(cB5L3.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5L4.Text != "" && sGeschlaecht[sName.IndexOf(cB5L4.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5L5.Text != "" && sGeschlaecht[sName.IndexOf(cB5L5.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5R1.Text != "" && sGeschlaecht[sName.IndexOf(cB5R1.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5R2.Text != "" && sGeschlaecht[sName.IndexOf(cB5R2.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5R3.Text != "" && sGeschlaecht[sName.IndexOf(cB5R3.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5R4.Text != "" && sGeschlaecht[sName.IndexOf(cB5R4.Text)] == "Männlich")
                {
                    Männer++;
                }
                if (cB5R5.Text != "" && sGeschlaecht[sName.IndexOf(cB5R5.Text)] == "Männlich")
                {
                    Männer++;
                }
            }
            lblMännerAnz.Text = Männer.ToString();

        }

        private void AnzahlPaddlerTeamSeite()
        {
            int TeamRechts = 0;
            int TeamLinks = 0;
            int TeamBeides = 0;
            for (int i = 0; i < lBPaddlerTeam.Items.Count; i++ )
            {

                if (sSeite[sName.IndexOf(lBPaddlerTeam.Items[i].ToString())] == "Rechts")
                {
                    TeamRechts++;
                }
                else if (sSeite[sName.IndexOf(lBPaddlerTeam.Items[i].ToString())] == "Links")
                {
                    TeamLinks++;
                }
                else if (sSeite[sName.IndexOf(lBPaddlerTeam.Items[i].ToString())] == "Beides")
                {
                    TeamBeides++;
                }
            }
            lblPaddlerBeides.Text = TeamBeides.ToString();
            lblPaddlerRechts.Text = TeamRechts.ToString();
            lblPaddlerLinks.Text = TeamLinks.ToString();

        }

        private void cBAufstellungZuweisen()
        {
            if (cBBootsTyp.SelectedIndex == 0 && cBTeams.SelectedIndex > -1)
            {
                cB10R1.Items.Clear();
                cB10R2.Items.Clear();
                cB10R3.Items.Clear();
                cB10R4.Items.Clear();
                cB10R5.Items.Clear();
                cB10R6.Items.Clear();
                cB10R7.Items.Clear();
                cB10R8.Items.Clear();
                cB10R9.Items.Clear();
                cB10R10.Items.Clear();
                cB10L1.Items.Clear();
                cB10L2.Items.Clear();
                cB10L3.Items.Clear();
                cB10L4.Items.Clear();
                cB10L5.Items.Clear();
                cB10L6.Items.Clear();
                cB10L7.Items.Clear();
                cB10L8.Items.Clear();
                cB10L9.Items.Clear();
                cB10L10.Items.Clear();
                cB10Trommel.Items.Clear();
                cB10Steuer.Items.Clear();


                #region 10er

                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R1.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R1.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R2.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R2.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R3.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R3.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R4.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R4.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R5.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R5.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R6.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R6.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R7.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R7.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R8.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R8.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R9.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R9.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10R10.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10R10.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                                
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L1.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L1.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L2.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L2.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L3.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L3.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L4.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L4.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L5.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L5.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L6.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L6.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L7.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L7.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L8.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L8.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L9.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L9.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10L10.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB10L10.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                

                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10Trommel.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]))
                    {
                        cB10Trommel.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB10Steuer.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && sSteuermann[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "X")
                    {
                        cB10Steuer.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                #endregion
               
            }
            else if (cBBootsTyp.SelectedIndex == 1 && cBTeams.SelectedIndex > -1)
            {
                cB5R1.Items.Clear();
                cB5R2.Items.Clear();
                cB5R3.Items.Clear();
                cB5R4.Items.Clear();
                cB5R5.Items.Clear();
                cB5L1.Items.Clear();
                cB5L2.Items.Clear();
                cB5L3.Items.Clear();
                cB5L4.Items.Clear();
                cB5L5.Items.Clear();
                cB5Trommel.Items.Clear();
                cB5Steuer.Items.Clear();
                #region 5er
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5R1.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5R1.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5R2.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5R2.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5R3.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5R3.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5R4.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5R4.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5R5.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Rechts" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5R5.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5L1.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5L1.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5L2.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5L2.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5L3.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5L3.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5L4.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5L4.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5L5.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && (sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Links" || sSeite[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "Beides"))
                    {
                        cB5L5.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5Trommel.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]))
                    {
                        cB5Trommel.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    if (!cB5Steuer.Items.Contains(Teams[cBTeams.SelectedIndex].sPaddler[i]) && sSteuermann[sName.IndexOf(Teams[cBTeams.SelectedIndex].sPaddler[i])] == "X")
                    {
                        cB5Steuer.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                    }
                }
                #endregion
            }
        }

        private void cBAufstellungsListe()
        {
            cBAufstellungListe.Items.Clear();
            cBAufstellungLöschen.Items.Clear();
            if (cBTeams.SelectedIndex > -1)
            {
                if (cBBootsTyp.SelectedIndex == 0)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe10.Count; i++)
                    {
                        cBAufstellungListe.Items.Add(Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].AufstellungName);
                        cBAufstellungLöschen.Items.Add(Teams[cBTeams.SelectedIndex].cAufstellungListe10[i].AufstellungName);
                    }
                }
                else if (cBBootsTyp.SelectedIndex == 1)
                {
                    for (int i = 0; i < Teams[cBTeams.SelectedIndex].cAufstellungListe5.Count; i++)
                    {
                        cBAufstellungListe.Items.Add(Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].AufstellungName);
                        cBAufstellungLöschen.Items.Add(Teams[cBTeams.SelectedIndex].cAufstellungListe5[i].AufstellungName);
                    }
                }
            }
        }

        private void AufstellungToCB()
        {
            Anfang =true;
            if (cBBootsTyp.SelectedIndex == 0 && cBTeams.SelectedIndex >= 0 && cBAufstellungListe.SelectedIndex >= 0 )
            {
                try { cB10Trommel.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[0]; }
                catch { } 
                try {cB10L1.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[1];  }
                catch { }
                try {cB10L2.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[2];  }
                catch { }
                try {cB10L3.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[3];  }
                catch { }
                try {cB10L4.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[4]; }
                catch { }
                try {cB10L5.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[5]; }
                catch { }
                try {cB10L6.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[6];  }
                catch { }
                try {cB10L7.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[7];  }
                catch { }
                try {cB10L8.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[8]; }
                catch { }
                try {cB10L9.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[9];  }
                catch { }
                try {cB10L10.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[10]; }
                catch { }
                try {cB10R1.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[11]; }
                catch { }
                try {cB10R2.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[12];  }
                catch { }
                try {cB10R3.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[13];  }
                catch { }
                try {cB10R4.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[14];  }
                catch { }
                try {cB10R5.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[15];  }
                catch { }
                try {cB10R6.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[16]; }
                catch { } 
                try {cB10R7.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[17];  }
                catch { }
                try {cB10R8.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[18];  }
                catch { }
                try {cB10R9.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[19];  }
                catch { }
                try {cB10R10.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[20];  }
                catch { }
                try { cB10Steuer.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[21]; }
                catch { }
                Anfang = false;
                BerechneDifferenzen();
            }
            if (cBBootsTyp.SelectedIndex == 1 && cBTeams.SelectedIndex >= 0 && cBAufstellungListe.SelectedIndex >= 0)
            {
                try { cB5Trommel.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[0]; }
                catch { }
                try { cB5L1.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[1]; }
                catch { }
                try { cB5L2.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[2]; }
                catch { }
                try { cB5L3.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[3]; }
                catch { }
                try { cB5L4.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[4]; }
                catch { }
                try { cB5L5.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[5]; }
                catch { }
                try { cB5R1.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[6]; }
                catch { }
                try { cB5R2.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[7]; }
                catch { }
                try { cB5R3.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[8]; }
                catch { }
                try { cB5R4.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[9]; }
                catch { }
                try { cB5R5.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[10]; }
                catch { }
                try { cB5Steuer.Text = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[11]; }
                catch { }
                Anfang = false;
                BerechneDifferenzen();
            }
            Anfang = false;
        }

        private void ErsatzToListBox()
        {
            if(cBTeams.SelectedIndex >-1)
            {
                lBErsatz.Items.Clear();
                for (int i = 0; i < Teams[cBTeams.SelectedIndex].sPaddler.Count; i++)
                {
                    lBErsatz.Items.Add(Teams[cBTeams.SelectedIndex].sPaddler[i]);
                }
            }
        }

        private void AufstellungSIZuweisen()
        {
            if (cBTeams.SelectedIndex > -1 && cBAufstellungListe.SelectedIndex > -1)
            {
                if (cBBootsTyp.SelectedIndex == 0)
                {
                    LastSIT = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[0];
                    LastSIL1 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[1];
                    LastSIL2 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[2];
                    LastSIL3 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[3];
                    LastSIL4 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[4];
                    LastSIL5 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[5];
                    LastSIL6 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[6];
                    LastSIL7 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[7];
                    LastSIL8 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[8];
                    LastSIL9 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[9];
                    LastSIL10 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[10];
                    LastSIR1 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[11];
                    LastSIR2 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[12];
                    LastSIR3 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[13];
                    LastSIR4 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[14];
                    LastSIR5 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[15];
                    LastSIR6 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[16];
                    LastSIR7 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[17];
                    LastSIR8 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[18];
                    LastSIR9 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[19];
                    LastSIR10 = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[20];
                    LastSIS = Teams[cBTeams.SelectedIndex].cAufstellungListe10[cBAufstellungListe.SelectedIndex].sAufstellung[21];
                }
                else if (cBBootsTyp.SelectedIndex == 1)
                {
                    LastSIT = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[0];
                    LastSIL1 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[1];
                    LastSIL2 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[2];
                    LastSIL3 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[3];
                    LastSIL4 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[4];
                    LastSIL5 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[5];
                    LastSIR1 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[6];
                    LastSIR2 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[7];
                    LastSIR3 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[8];
                    LastSIR4 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[9];
                    LastSIR5 = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[10];
                    LastSIS = Teams[cBTeams.SelectedIndex].cAufstellungListe5[cBAufstellungListe.SelectedIndex].sAufstellung[11];
                }
            }
        }

        private void cBoxDynGewichte_CheckedChanged(object sender, EventArgs e)
        {
            BerechneDifferenzen();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        void Download(string link, string file)
        {
            WebClient wClient = new WebClient();
            wClient.DownloadFile(new Uri(link + file), AppDomain.CurrentDomain.BaseDirectory + file);
        }
        public static bool CheckInternetConnection()
        {
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send("www.google.de", 500);

                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private void cmdLogoLaden_Click(object sender, EventArgs e)
        {
            OpenFileDialog FileDialog = new OpenFileDialog();
            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                
                try
                {
                    // Bild laden, kann vom Typ bmp, jpg, tif, gif, ico, emf, png, wmf sein
                    pBLogo10Bank.Image = Image.FromFile(FileDialog.FileName);
                    pBLogo5Bank.Image = Image.FromFile(FileDialog.FileName);
                    pBLogoTeam.Image = Image.FromFile(FileDialog.FileName);
                }
                catch
                {
                }
                pBLogo10Bank.Image.Save("logo.jpg");
            }
        }

        void Verschlüsselnold()
        {
            TextReader textReader = new StreamReader(clsConst.Save);
            string text = textReader.ReadToEnd();
            textReader.Close();
            try
            {
                byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(text);
                text = Convert.ToBase64String(encbuff);
            }
            catch { }
            TextWriter textWriter = new StreamWriter(clsConst.Save);
            textWriter.Write(text);
            textWriter.Close();
        }

        void Entschlüsselnold()
        {
            TextReader textReader = new StreamReader(clsConst.Save);
            string text = textReader.ReadToEnd();
            textReader.Close();
            try
            {
                byte[] decbuff = Convert.FromBase64String(text);
                text = System.Text.Encoding.UTF8.GetString(decbuff);
            }
            catch { }
            TextWriter textWriter = new StreamWriter(clsConst.Save);
            textWriter.Write(text);
            textWriter.Close();
        }

        private static void RenameFile(string path, string newName)
        {
            var fileInfo = new FileInfo(path);
            File.Move(path, fileInfo.Directory.ToString() + newName);
        }

        void Verschlüsseln()
        {
            if (File.Exists(clsConst.Save))
            {
                TextReader textReader = new StreamReader(clsConst.Save);
                string text = textReader.ReadToEnd();
                textReader.Close();
                try
                {
                    byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(text);
                    text = Convert.ToBase64String(encbuff);
                }
                catch { }
                TextWriter textWriter = new StreamWriter(clsConst.Save);
                textWriter.Write(text);
                textWriter.Close();
                if (File.Exists(clsConst.SaveCry))
                {
                    File.Delete(clsConst.SaveCry);
                }
                RenameFile(clsConst.Save, @"\Save");
                try
                {
                    File.Delete(clsConst.Save);
                }
                catch { }
            }
        }

        void Entschlüsseln()
        {
            string text = "";
            if (File.Exists(clsConst.SaveCry))
            {

                try
                {
                    RenameFile(clsConst.SaveCry, @"\Save.xml");
                    TextReader textReader = new StreamReader(clsConst.Save);
                    text = textReader.ReadToEnd();
                    textReader.Close();
                    try
                    {
                        byte[] decbuff = Convert.FromBase64String(text);
                        text = System.Text.Encoding.UTF8.GetString(decbuff);
                    }
                    catch { }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Fail");
                }

                TextWriter textWriter = new StreamWriter(clsConst.Save);
                textWriter.Write(text);
                textWriter.Close();
            }
            else if (File.Exists(clsConst.Save))
            {
                try
                {
                    TextReader textReader = new StreamReader(clsConst.Save);
                    text = textReader.ReadToEnd();
                    textReader.Close();
                    try
                    {
                        byte[] decbuff = Convert.FromBase64String(text);
                        text = System.Text.Encoding.UTF8.GetString(decbuff);
                    }
                    catch { }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Fail");
                }

                TextWriter textWriter = new StreamWriter(clsConst.Save);
                textWriter.Write(text);
                textWriter.Close();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=NVGXHWSMTH3M4");
        }
    }
}
