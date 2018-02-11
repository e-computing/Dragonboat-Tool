using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace Update
{

    class Program
    {
        static void Main(string[] args)
        {
            Program P = new Program();
            string GerUp = "Update abgeschlossen";
            string GerKey = "Beliebige Taste drücken um zu beenden";
            string EngUp = "Update finished";
            string EngKey = "Please press a key";
            string EXE = "Drachenboot Tools.exe";
            string URL = "http://blue-programming.bplaced.net/DragonboatTool/Deutsch/";
            Thread.Sleep(5000);
            P.Download(URL, EXE);
            Console.WriteLine(GerUp);
            Console.WriteLine(GerKey);
            Console.ReadKey();
            Process.Start(EXE);
           

        }
        void Download(string link, string file)
        {
            WebClient wClient = new WebClient();
            wClient.DownloadFile(new Uri(link+file), AppDomain.CurrentDomain.BaseDirectory + file);
        }
        
    }
}
