using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordEncryption {
    class SQL {
        private SQLiteConnection db;
        public SQL(string filename) {
            createSQLite(filename);
        }

        private void createSQLite(string filename) {
            SQLiteConnection.CreateFile(filename);
            db = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            db.Open();
            execute("CREATE TABLE IF NOT EXISTS data(url text, user text, password text);");
        }

        public void execute(string sql) {
            SQLiteCommand command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
        }

        public ArrayList getAllEntries(string password) {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM data", db);
            System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader();
            ArrayList result = new ArrayList();
            while (reader.Read()) {
                string website = (string)reader[0];
                string username = (string)reader[1];
                string pw = Cipher.Decrypt((string)reader[2], password);
                if (pw == null) return null;
                result.Add(new Entry(website, username, pw));
            }
            return result;
        }

        public void addEntry(string website, string username, string password, string passphrase) {
            execute("INSERT INTO data(url, user, password) VALUES ('" + website + "', '" + username + "', '" + Cipher.Encrypt(password, passphrase) + "');");
        }

        public void deleteEntry(string url) {
            execute("DELETE FROM data WHERE url='" + url + "';");
        }

        public void newPassword(string oldpw, string newpw) {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM data", db);
            System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                string website = (string)reader[0];
                string pw = Cipher.Decrypt((string)reader[2], oldpw);
                execute("UPDATE data SET password='" + Cipher.Encrypt(pw, newpw) + "' WHERE url='" + website + "';");
            }
        }


    }

    public class Entry {

        public string url;
        public string username;
        public string password;

        public Entry(string url, string username, string password) {
            this.url = url;
            this.username = username;
            this.password = password;
        }
    }
}
