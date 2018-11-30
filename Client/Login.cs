using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.ActiveControl = tbUsername;
            this.AcceptButton = bLogin;
        }

        public string username;

        private void bLogin_Click(object sender, EventArgs e)
        {
            if(tbUsername.Text == "")
            {
                MessageBox.Show("Please enter something!");
            }
            else
            {
                username = tbUsername.Text;
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
