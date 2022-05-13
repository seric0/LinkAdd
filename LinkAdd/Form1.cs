using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace LinkAdd
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {                        
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()!@:%_\+.~#?&\/\/=]*)";

            if (textBox1.Text != "" && Regex.IsMatch(textBox1.Text, pattern, RegexOptions.IgnoreCase))
            {
                richTextBox1.Clear();
                WebRequest myWebRequest = WebRequest.Create(textBox1.Text);
                WebResponse myWebResponse = myWebRequest.GetResponse();
                Stream ReceiveStream = myWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader readStream = new StreamReader(ReceiveStream, encode);
                Char[] read = new Char[256];
                int count = readStream.Read(read, 0, 256);
                progressBar1.Value = 0;
                while (count > 0)
                {
                    if (progressBar1.Value != 100) { progressBar1.Value = progressBar1.Value + 1; }
                    String str = new String(read, 0, count);
                    count = readStream.Read(read, 0, 256);
                    richTextBox1.AppendText(str);
                }
                string html_text = richTextBox1.Text;
                int start = html_text.IndexOf("<title>");
                int end = html_text.IndexOf("</title>", start);
                string str1 = html_text.Substring(start, end - start + "</title>".Length);
                string str2 = str1.Remove(0, 7);
                int ind = str2.Length - 8;
                string title = str2.Remove(ind);
                label1.Text = title;
                try
                {
                    string connectionString = @"Data Source=C:\Links\LinksDB.db";
                    string sqlExpression = "INSERT INTO Link (reference, linkname) VALUES ('" + textBox1.Text + "', '" + label1.Text + "')";
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        SQLiteCommand command = new SQLiteCommand();
                        command.Connection = connection;
                        command.CommandText = sqlExpression;
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                Select_Value();
                myWebResponse.Close();
                readStream.Close();
            }            
            else
            {
                MessageBox.Show("Введите адрес сайта!");
            }
        }

        public void Select_Value()
        {
            string sqlExpression = "SELECT * FROM Link";
            using (var connection = new SQLiteConnection(@"Data Source=C:\Links\LinksDB.db"))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(sqlExpression, connection);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            var id = reader.GetValue(0);
                            var reference = reader.GetValue(1);
                            var linkname = reader.GetValue(2);
                            dataGridView1.Rows.Add(id, reference, linkname);
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(@"C:\Links\LinksDB.db"))
            {
                SQLiteConnection.CreateFile(@"C:\Links\LinksDB.db");
            }
            try
            {
                using (var connection = new SQLiteConnection(@"Data Source=C:\Links\LinksDB.db"))
                {
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand();
                    command.Connection = connection;
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Link (id INTEGER PRIMARY KEY AUTOINCREMENT, reference TEXT, linkname TEXT)";
                    command.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }                                   
            Select_Value();
        }
    }
}