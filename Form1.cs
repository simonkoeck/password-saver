using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PasswordEncryption {
    public partial class Form1 : Form {

        SQL db;
        private string password;
        public Form1() {
            InitializeComponent();            
        }

        private static string prompt(string caption, string error) {
            Form prompt = new Form() {
                Width = 420,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel1 = new Label() { Left = 20, Top = 23, Text = "Password: " };
            TextBox password = new TextBox() { Left = 80, Top = 20, Width = 200 };
            password.PasswordChar = '*';
            Button confirmation = new Button() { Text = "Ok", Left = 300, Width = 80, Top = 18, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(password);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel1);
            if (error != null) {
                prompt.Height += 23;
                Label errortxt = new Label() { Left = 80, Top = 53, Text = error };
                errortxt.Width = 200;
                errortxt.ForeColor = Color.Red;
                prompt.Controls.Add(errortxt);
            }
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? password.Text : "";
        }
        public static string promptnewpassword() {
            Form prompt = new Form() {
                Width = 420,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "New Password",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel1 = new Label() { Left = 20, Top = 23, Text = "New Password: " };
            TextBox password = new TextBox() { Left = 80, Top = 20, Width = 200 };
            Button confirmation = new Button() { Text = "Ok", Left = 300, Width = 80, Top = 18, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(password);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel1);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? password.Text : "";
        }


        private void Form1_Load(object sender, EventArgs e) {
            db = new SQL("data.db");
            ArrayList data;

            //PROMPT USER LOGIN
            string error = null;
            while (true) {
                string pr = prompt("Login", error);
                if(pr == null || pr == "") {
                    Application.Exit();
                    return;
                }
                password = Cipher.CreateMD5(pr);                
                data = db.getAllEntries(password);
                error = "Access denied";
                if (data == null) continue;
                else break;
            }


            listEntries.View = View.Details;
            ColumnHeader h1 = new ColumnHeader();
            h1.Text = "Password";
            h1.Width = 100;
            listEntries.Columns.Clear();
            listEntries.Columns.Add(h1);
            ColumnHeader h2 = new ColumnHeader();
            h2.Text = "Username";
            listEntries.Columns.Add(h2);
            ColumnHeader h3 = new ColumnHeader();
            h3.Text = "Website";
            h3.Width = 150;
            listEntries.Columns.Add(h3);
            listEntries.Items.Clear();
            for (int i = 0; i < data.Count; i++) {
                Entry entry = (Entry)data[i];
                string[] row = { entry.password, entry.username, entry.url };
                var listViewItem = new ListViewItem(row);
                listEntries.Items.Add(listViewItem);
            }
        }

        private void btnAddEntry_Click(object sender, EventArgs e) {
            Regex rg = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");
            if (!rg.IsMatch(tbxWebsite.Text)) {
                MessageBox.Show("Please type in a valid URL!");
                return;
            }
            db.addEntry(tbxWebsite.Text, tbxUsername.Text, tbxPassword.Text, password);
            string[] row = { tbxPassword.Text, tbxUsername.Text, tbxWebsite.Text };
            var listViewItem = new ListViewItem(row);
            listEntries.Items.Add(listViewItem);
            MessageBox.Show("Entry successfully added!", "Success");
        }

        private void btnRandom_Click(object sender, EventArgs e) {
            string pw = "";
            Random rndgen = new Random();
            for(int i = 0; i < 16; i++) {
                int r = rndgen.Next(0, 2);
                if(r == 0) {
                    pw += (char)rndgen.Next(33, 93);
                }else if(r == 1) {
                    pw += (char)rndgen.Next(97, 125);
                }
            }
            tbxPassword.Text = pw;
        }

        private void btnChangePW_Click(object sender, EventArgs e) {
            string oldpw = prompt("Gimmi ur cörrent päässwörd", null);
            if(Cipher.CreateMD5(oldpw) != password) {
                MessageBox.Show("Hmm thats the wrong password!");
                return;
            }
            string newpw = promptnewpassword();
            db.newPassword(password, Cipher.CreateMD5(newpw));
            password = Cipher.CreateMD5(newpw);
        }

        private void listEntries_SelectedIndexChanged(object sender, EventArgs e) {
            if (listEntries.SelectedItems.Count == 0) return;
            Clipboard.SetText(listEntries.SelectedItems[0].Text);
        }

        private void listEntries_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Delete) {
                DialogResult dialogResult = MessageBox.Show("Are you sure you wanna delete this entry?", "Delete?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) {                    
                    db.deleteEntry(listEntries.SelectedItems[0].SubItems[2].Text);
                    listEntries.SelectedItems[0].Remove();
                    MessageBox.Show("Entry deleted!", "Deleted");
                }
            }
        }
    }
}
