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
            var pacFile = @"C:\Users\saube_000\AppData\Local\Temp\EPUB4dogs";
            var epubFile = @"C:\Users\saube_000\AppData\Local\Temp\EPUB4dogs.epub";

            Archiver.ArchiveEpub(pacFile, epubFile);

        }
    }
}
