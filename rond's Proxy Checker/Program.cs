using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leaf.xNet;
using Colorful;
using Console = Colorful.Console;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace rond_s_Proxy_Checker
{
    class Program
    {
        public static Stopwatch watch = new Stopwatch();
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = $"rond's Proxy Checker | Checked: {Checked}/{Total} - Working: {Working} - Bad: {Bad}";
            Folder = DateTime.Now.ToString("dd-MM h-mm-ss");
            if (!Directory.Exists("./Hits")) Directory.CreateDirectory("./Hits");
            if (!Directory.Exists($"./Hits/{Folder}")) Directory.CreateDirectory($"./Hits/{Folder}");
            proxyLoad();
            Console.WriteAscii(" rond's Proxy Checker", Color.FromArgb(111111));
            Logger.Inf("URL to Check against: ", newLine: false);
            URL = Console.ReadLine();
            Logger.Inf("Threads: ", newLine: false);
            int Threads = Convert.ToInt32(Console.ReadLine());

            Console.Clear();
            Console.WriteAscii(" rond's Proxy Checker", Color.FromArgb(111111));
            Console.WriteLine("[1] Http Proxies\n[2] Socks4 Proxies\n[3] Socks5 Proxies");
            Proxy = Convert.ToInt32(Console.ReadLine());

            Console.Clear();
            watch.Start();
            Console.WriteAscii(" rond's Proxy Checker", Color.FromArgb(111111));
            Console.WriteLine($"Started checking {Total} Proxies against {URL}");

            for (int i = 0; i < Threads; i++)
            {
                new Thread(new ThreadStart(DoWork)).Start();
            }

            Console.ReadLine();

        }

        public static string Folder, URL;
        public static object gay = new object();
        public static BlockingCollection<string> Proxies = new BlockingCollection<string>();
        public static int Checked, Working, Bad, Proxy, Total;

        public static void Checker(string LINE)
        {
            var req = new HttpRequest();
            try
            {
                switch (Proxy)
                {
                    case 1:
                        req.Proxy = HttpProxyClient.Parse(LINE);
                        break;
                    case 2:
                        req.Proxy = Socks4ProxyClient.Parse(LINE);
                        break;
                    case 3:
                        req.Proxy = Socks5ProxyClient.Parse(LINE);
                        break;
                    default:
                        req.Proxy = HttpProxyClient.Parse(LINE);
                        break;
                }
                string response = req.Get(URL).ToString();
                if (response.Contains("</title>"))
                {
                    lock (gay)
                    {
                        StreamWriter SR = new StreamWriter($"./Hits/{Folder}/Hits.txt", true);
                        SR.WriteLine(LINE);
                        SR.Close();
                    }
                    Working++;
                    Checked++;
                    Update();
                }
                else
                {
                    Bad++;
                    Checked++;
                    Update();
                }
            }
            catch
            {
                Bad++;
                Checked++;
                Update();
            }
        }




        public static void DoWork()
        {
            while (true)
            {
                string LINE;
                if (Program.Proxies.TryTake(out LINE))
                {
                    Checker(LINE);
                }
                else
                {
                    break;
                }
            }
        }



        public static void Update()
        {
            int CPM = (int)(Checked / watch.Elapsed.TotalSeconds * 60);
            Console.Title = $"rond's Proxy Checker | Checked: {Checked}/{Total} - Working: {Working} - Bad: {Bad} - CPM: {CPM}";
        }

        static void proxyLoad()
        {
            try
            {
                Console.Clear();
                string PATH = LoadProxy();
                using (FileStream fs = new FileStream(PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    string LINE;
                    while ((LINE = sr.ReadLine()) != null)
                    {
                        Program.Proxies.Add(LINE);
                    }
                    if (Program.Proxies.Count > 0)
                    {
                        Total = Program.Proxies.Count;
                    }
                }
            }
            catch
            {
                Colorful.Console.WriteLine("Error Loading proxies...", Color.Red);
            }
        }

        public static string LoadProxy()
        {
            string path = string.Empty;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Load Proxies";
                ofd.Filter = "Text Files(*.txt)|*.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    path = ofd.FileName;
                }
            }
            return path;
        }
    }
}
