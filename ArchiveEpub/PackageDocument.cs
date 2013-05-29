using System;
using System.Linq;
using System.Xml.Linq;

namespace ArchiveEpub
{
    class PackageDocument
    {
        //パッケージドキュメントを更新する
        public static void UpdatePackageDocument(string srcFile)
        {
            XNamespace package = "http://www.idpf.org/2007/opf";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";


            //opfファイルを読み込む
            var doc = XDocument.Load(srcFile);

            //Epub3であればlastmodifiedを書き換える
            var packNode = doc.Descendants().Where(e => e.Name.LocalName == "package");
            var packnode1 = packNode.First();
            var epubVer = packnode1.Attribute("version").Value;
            if (epubVer.Equals("3.0"))
            {
                var metaNodes = doc.Descendants().Where(e => e.Name.LocalName == "meta");   //metaデータを取得する
                var hasProps = metaNodes.Where(e => e.Attribute("property") != null);       //property属性を持つものを取得する
                var lastModifiedNode = hasProps.Where(e => e.Attribute("property").Value == "dcterms:modified").FirstOrDefault();
                var utc = DateTime.UtcNow;                          //協定世界時を取得
                lastModifiedNode.Value = (utc.ToString("s") + 'Z'); //YYYY-MM-DDThh:mm:ssZ
            }
            doc.Save(srcFile);
        }
    }
    //identifierをGUIDで書き換える
    //var idNode = doc.Descendants(dc + "identifier").First();
    //var guid = Guid.NewGuid();
    //idNode.Value = guid.ToString();


}
