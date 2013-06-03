using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SakuraEpubUtility;

namespace Console4Check
{
    class Program
    {
        static void Main(string[] args)
        {
            //OverWriteOpf();
            ConvertText2Xhtml();


        }

        static void ConvertText2Xhtml()
        {
            var textFile = @"c:\temp\パラレル.txt";
            var textFormat = TextFormat.PLAIN_TEXT_WITH_HEADER;
            var templateFile = @"C:\temp\Text.xhtml";

            var opt = new ConvertOptions();
            opt.hasTag = false;
            opt.isSpaceIndented = false;

            TextComveter.ConvertText(textFile, templateFile, textFormat, opt);
        }

        static void OverWriteOpf()
        {
            var opf = @"C:\temp\hoge\OEBPS\content.opf";
            var meta = new EpubMetaData();
            meta.title = "タイトル";
            meta.author = "著者";
            meta.publisher = "出版社";
            meta.isRightToLeft = true;
            

            var ids =new System.Collections.Specialized.StringCollection();
            ids.Add("パラレル");
            ids.Add("123");
            ids.Add("タイトル");
            ids.Add("456");
            ids.Add("hoge");

            var epubDoc = new EpubDocument();
            epubDoc.SetIdnentiersDictionary(ids);

            PackageDocument.WriteMetaData(opf, meta);

        }
    }
}
