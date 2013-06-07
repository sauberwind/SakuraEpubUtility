using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Console4Check
{
    class Program
    {
        static void Main(string[] args)
        {
            OverWriteNavigationDocument();
            //OverWriteOpf();
            //ConvertText2Xhtml();


        }

        static void OverWriteNavigationDocument()
        {
            //var nav1 = new HeaderAnchor(1, "id1", "header1");
            //var nav2 = new HeaderAnchor(1, "id2", "header2");
            //var nav3 = new HeaderAnchor(2, "id3", "header3");
            //var nav4 = new HeaderAnchor(2, "id4", "header4");
            //var nav5 = new HeaderAnchor(1, "id5", "header5");
            //var nav6 = new HeaderAnchor(2, "id6", "header6");
            //var navItems = new List<HeaderAnchor>();
            //navItems.Add(nav1);
            //navItems.Add(nav2);
            //navItems.Add(nav3);
            //navItems.Add(nav4);
            //navItems.Add(nav5);
            //navItems.Add(nav6);

            //var template = @"C:\temp\nav.xhtml";
            //var dst = @"C:\temp\dstnav.xhtml";
            //NavigationDocument.WriteNavigationDocument(template, dst, navItems);

        }

        static void ConvertText2Xhtml()
        {
            //var textFile = @"c:\temp\パラレル.txt";

            //var templateFile = @"C:\temp\Text.xhtml";

            //var opt = new ConvertOptions();
            //opt.hasTag = false;
            //opt.isSpaceIndented = false;

            //TextConveter.ConvertText(textFile, templateFile, opt);
        }

        static void OverWriteOpf()
        {
            //var opf = @"C:\temp\hoge\OEBPS\content.opf";
            //var meta = new EpubMetaData();
            //meta.title = "タイトル";
            //meta.author = "著者";
            //meta.publisher = "出版社";
            //meta.isRightToLeft = true;
            

            //var ids =new System.Collections.Specialized.StringCollection();
            //ids.Add("パラレル");
            //ids.Add("123");
            //ids.Add("タイトル");
            //ids.Add("456");
            //ids.Add("hoge");

            //var epubDoc = new EpubDocument();
            //epubDoc.SetIdnentiersDictionary(ids);

            //PackageDocument.WriteMetaData(opf, meta);

        }
    }
}
