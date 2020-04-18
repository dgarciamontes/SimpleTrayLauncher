using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace SimpleTrayLauncher
{
    public class TrayApplicationContext : ApplicationContext
    {

        List<MenuObject> menuObjects = new List<MenuObject>();
        string IconName = "";
        string ScriptFolder = "";
        string DoubleClickScript = "";

        private NotifyIcon trayIcon;

        public TrayApplicationContext()
        {
            //Load the App.config file
            if (!LoadAppSettings())
            {
                MessageBox.Show("Error on App.config");
                return;
            }

            //Add ours configured MenuObjects on Tray Menu
            List<MenuItem> menuItemList = new List<MenuItem>();
            menuObjects.ForEach(mo => menuItemList.Add(new MenuItem(mo.Name, ActionFunction)));

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(IconName),
                ContextMenu = new ContextMenu(menuItemList.ToArray()),
                Visible = true
            };

            trayIcon.MouseDoubleClick += new MouseEventHandler(ActionFunction);
        }



        private bool LoadAppSettings()
        {
            NameValueCollection configs = (NameValueCollection)ConfigurationManager.GetSection("ScriptSettings");

            if (configs.AllKeys.Contains("ScriptFolder"))
                ScriptFolder = configs["ScriptFolder"];

            if (configs.AllKeys.Contains("DoubleClickScript"))
            {

                DoubleClickScript = configs.AllKeys.Contains("aDoubleClickScript") ? configs["DoubleClickScript"] : Environment.CurrentDirectory + "\\" + ScriptFolder + "\\" + configs["DoubleClickScript"];
            }


            IconName = configs["IconName"];

            configs.AllKeys.Where(k => k.StartsWith("s")).ToList().ForEach(k =>
            {
                MenuObject mo = new MenuObject();

                //AbsolutePath?
                mo.AbsolutePath = (configs.AllKeys.Contains("a" + k.Substring(1)));

                //Folder
                mo.ScriptFolder = ScriptFolder;

                //Script FileName
                mo.ScriptFileName = mo.AbsolutePath ? configs[k] : Environment.CurrentDirectory + "\\" + mo.ScriptFolder + "\\" + configs[k];

                //Name
                mo.Name = (configs.AllKeys.Contains("n" + k.Substring(1))) ? configs["n" + k.Substring(1)] : k;

                //Position:
                if (Int32.TryParse(k.Substring(1), out int position))
                        mo.ScriptPosition = position;

                


                menuObjects.Add(mo);
            });

            menuObjects = menuObjects.OrderBy(m => m.ScriptPosition).ToList();
            menuObjects.Add(new MenuObject { Name = "Exit" });

            return menuObjects.Count > 1;
        }

        void ActionFunction(object sender, EventArgs e)
        {
            try
            {
                //DoubleClick:
                if (sender is NotifyIcon)
                {
                    System.Diagnostics.Process.Start(DoubleClickScript);
                    return;
                }

                string moName = ((MenuItem)sender).Text;

                if (moName == "Exit")
                {
                    // Hide tray icon, otherwise it will remain shown until user mouses over it
                    trayIcon.Visible = false;

                    Application.Exit();
                }

                else
                {
                    MenuObject m = menuObjects.FirstOrDefault(mo => mo.Name == moName);
                    System.Diagnostics.Process.Start(m.ScriptFileName);
                }
            }
            catch
            {
                MessageBox.Show("Error running Script, check your config file");
            }

        }
    }
}