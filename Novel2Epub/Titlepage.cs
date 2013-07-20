using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SakuraEpubLibrary;

namespace SakuraEpubUtility
{
    class InfoPage
    {
        public static void UpdateTitlePage(string srcFile, EpubMetaData dat)
        {
            var doc = XDocument.Load(srcFile);
            var idNodes = doc.Descendants().Where(e => e.Attribute("id") != null);    //idを持つノードを取得する

            if (idNodes != null)
            {
                //タイトルを書き込む
                var titleNode = idNodes.Where(e => e.Attribute("id").Value == "title").FirstOrDefault();
                if (titleNode != null)
                {
                    titleNode.Value = dat.title;

                }

                //著者を書き込む
                var authorNode = idNodes.Where(e => e.Attribute("id").Value == "author").FirstOrDefault();
                if (authorNode != null)
                {
                    authorNode.Value = dat.author;
                }

                //出版社を書き込む
                var publisherNode = idNodes.Where(e => e.Attribute("id").Value == "publisher").FirstOrDefault();
                if (publisherNode != null)
                {
                    publisherNode.Value = dat.publisher;
                }
            }
            doc.Save(srcFile);
        }
    }
}
