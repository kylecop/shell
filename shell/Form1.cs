using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace shell
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            GetLocalUserAccounts();
        }
        
        private void GetLocalUserAccounts()
        {

            ManagementObjectSearcher usersSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_UserAccount");
            ManagementObjectCollection users = usersSearcher.Get();

            var localUsers = users.Cast<ManagementObject>().Where(
                u => (bool)u["LocalAccount"] == true &&
                     //(bool)u["Disabled"] == false &&
                     //(bool)u["Lockout"] == false &&
                     //int.Parse(u["SIDType"].ToString()) == 1 &&
                     u["Name"].ToString() != "HomeGroupUser$");

            
            foreach (ManagementObject user in users)
            {
                string username = user["Name"].ToString();
                string path = @"C:\Users\" + username + @"\NTUSER.DAT";
                string shellLocation = "";
                listView1.Items.Add(new ListViewItem(new[] { username, getReg(username)}));

            }//HKEY_USERS\S-1-5-21-798898283-2169997705-2806997698-1000\Software\Microsoft\Windows NT\CurrentVersion\Winlogon
        }
        private string getReg(string username)
        {
            string result = "";
            int flag = 0;

            string user_path = @"C:\Users\";
            string subkey_exists = "Shell";

            //if (Directory.Exists(user_path))
            {
                //string[] sub_dirs = Directory.GetDirectories(user_path);

                //foreach (string dir in sub_dirs)
                {


                    string wimHivePath = @"c:\users\" + username + @"\ntuser.dat";
                    //MessageBox.Show(wimHivePath);
                    string loadedHiveKey = RegistryInterop.Load(wimHivePath);

                    RegistryKey rk = Registry.Users.OpenSubKey(loadedHiveKey);

                    if (rk != null)
                    {
                        RegistryKey ExistKey = rk.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", RegistryKeyPermissionCheck.ReadWriteSubTree);
                        
                        if (ExistKey != null)
                            if(ExistKey.GetValue("shell").ToString() != null)
                                result = ExistKey.GetValue("shell").ToString();
                            
                        ExistKey.Close();
                        

                        rk.Close();
                    }

                    RegistryInterop.Unload();

                }
            }
            
            return result;
        }
        private void SetReg(string username, string value)
        {
            string loadedHiveKey = RegistryInterop.Load(@"c:\users\" + username + @"\ntuser.dat");
            RegistryKey rk = Registry.Users.OpenSubKey(loadedHiveKey, true);
            if (rk != null)
            {
                RegistryKey ExistKey = rk.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                ExistKey.SetValue(@"Shell", value, RegistryValueKind.String);
                ExistKey.Close();
                rk.Close();
            }
            RegistryInterop.Unload();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                button1.Enabled = true;
            else
                button1.Enabled = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            //MessageBox.Show(listView1.SelectedItems[0].Text);
            SetReg(listView1.SelectedItems[0].Text, @"c:\test.exe");
            listView1.Items.Clear();
            GetLocalUserAccounts();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            //MessageBox.Show(listView1.SelectedItems[0].Text);
            SetReg(listView1.SelectedItems[0].Text, "explorer.exe");
            listView1.Items.Clear();
            GetLocalUserAccounts();
        }
    }

}
