using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using FirebirdSql.Data.FirebirdClient;
using Ionic.Zip;

namespace updater
{
    class Program
    {
        private static string fbconn;
        private static string server;
        private static string file;
        private static string user;
        private static string pass;
        private static string edit;
        private static string ver;
        static void Main(string[] args)
        {
            getSetting();
            getFile();
            File.Delete("Архив.exe");
            ExtractUpd();
            saveVer();
            Process.Start("Архив.exe");

            Environment.Exit(0);


        }

        static void ExtractUpd()
        {
            using (ZipFile zip1 = ZipFile.Read("upd.zip", new ReadOptions()
            {
                Encoding = Encoding.GetEncoding("cp866")
            }))
            {
                foreach (ZipEntry entry in zip1)
                {
                    entry.Extract(Directory.GetCurrentDirectory(), ExtractExistingFileAction.OverwriteSilently);
                }
            }
            File.Delete("upd.zip");
        }

        static void getFile()
        {
            using (FbConnection con = new FbConnection(fbconn))
            {
                con.Open();

                FbCommand com = new FbCommand("select * from upda",con);

                FbDataReader dr = com.ExecuteReader();

                while (dr.Read())
                {
                    if (dr.HasRows)
                    {
                        if (dr["data"]!=null)
                        {
                            ver = dr["ver"].ToString();
                            byte[] data = dr["data"] as byte[];
                            using (var fileS = new FileStream("upd.zip", FileMode.Create, FileAccess.Write))
                            {

                                fileS.Write(data, 0,data.Length);
                            }
                        }
                        
                    }
                }

            }
        }

        static void saveVer()
        {
            XDocument xdoc = new XDocument();
            XElement set = new XElement("setting");
            set.Add(new XElement("file", file));
            set.Add(new XElement("server", server));
            set.Add(new XElement("user", user));
            set.Add(new XElement("pass", pass));
            set.Add(new XElement("edit", edit));
            set.Add(new XElement("ver", ver));
            xdoc.Add(set);
            xdoc.Save("setting.xml");
        }
        static void getSetting()
        {
            if (File.Exists("setting.xml"))
            {
                XDocument xdoc = XDocument.Load("setting.xml");
                file = xdoc.Element("setting").Element("file").Value;
                server = xdoc.Element("setting").Element("server").Value;
                user = xdoc.Element("setting").Element("user").Value;
                pass = xdoc.Element("setting").Element("pass").Value;
                edit = xdoc.Element("setting").Element("edit").Value;
                fbconn =
                    "User = " + user + "; Password = " + pass + "; Database = " + file + "; DataSource = " + server +
                    "; Port = 3050; Charset = utf8;";
            }
        }
    }
}
