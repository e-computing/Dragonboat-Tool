using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Drachenboot_Tools
{
    public partial class frmSplashScreen : Form
    {
        public frmSplashScreen()
        {
            InitializeComponent();
        }

        private void timeSplash_Tick(object sender, EventArgs e)
        {
            this.Hide();
            frmMain Main = new frmMain();
            Main.Show();
            timeSplash.Enabled = false;
        }
    }
}
