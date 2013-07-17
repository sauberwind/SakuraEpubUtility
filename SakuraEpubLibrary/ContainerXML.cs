using System;
using System.Linq;
using System.Xml.Linq;
using System.IO;
namespace SakuraEpubLibrary
{
    public static class ContainerXML
    {
        //パッケージ文書をcontainer.xmlから取得する
        public static string GetPackageDocument(string epubPath)
        {
            var ret = "";

            var container = Path.Combine(epubPath,@"META-INF\container.xml");

            if (File.Exists(container) != true)    //コンテナファイルが存在しない
            {
                throw (new Exception(container + "が存在しません"));
            }

            //container.xmlファイルからパッケージ文書を取得する
            try
            {
                var doc = XElement.Load(container);
                var rootFileNodes = doc.Descendants().Where(e => e.Name.LocalName == "rootfile");   //rootfileタグ
                var rootNode = rootFileNodes.Where(e => e.Attribute("full-path") != null).First();  //パスが入っているrootfileタグ
                var rootFilePath = rootNode.Attribute("full-path").Value;                           //パスを取得する

                ret = Path.Combine(epubPath, rootFilePath);   //パッケージ文書パスを保存
            }
            catch (Exception ex)
            {
                throw (new Exception(container + "に異常があります"+ex.ToString()));
            }

            return (ret);
        }
    }
}
