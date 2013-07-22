using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SakuraEpubLibrary;
using System.Xml;
using System.Xml.Linq;

namespace DebugConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Archiver.ArchiveEpub(pacFile, epubFile);
            System.Diagnostics.ProcessStartInfo psi =
    new System.Diagnostics.ProcessStartInfo("test.html");
            foreach (string s in psi.Verbs)
            {
                Console.WriteLine(s);
            }

        }
    }
}
