using System;
using System.Linq;
using System.Xml.Linq;
using SakuraEpubUtility;

namespace SakuraEpubUtility
{
    class PackageDocument
    {
        //パッケージドキュメントを更新する
        public static void UpdatePackageDocument(string packFile)
        {
            XNamespace package = "http://www.idpf.org/2007/opf";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";


            //opfファイルを読み込む
            var doc = XDocument.Load(packFile);

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
            doc.Save(packFile);
        }

        //メタデータを書き込む
        public static void WriteMetaData(string packFile, EpubMetaData ePubDat)
        {
            var doc = XDocument.Load(packFile);     //ファイルを読み込む

            //書名
            var titleNode = doc.Descendants().Where(e => e.Name.LocalName == "title").First();
            titleNode.Value = ePubDat.title;

            //著者
            var authorNode = doc.Descendants().Where(e => e.Name.LocalName == "creator").First();
            authorNode.Value = ePubDat.author;

            //出版
            var publisherNode = doc.Descendants().Where(e => e.Name.LocalName == "publisher").First();
            publisherNode.Value = ePubDat.publiser;

            //ID identifier
            var idNode = doc.Descendants().Where(e => e.Name.LocalName == "identifier").First();
            if (idNode.Value == "") //IDが空白であれば書き換える
            {
                if (EpubDocument.identifiers.ContainsKey(ePubDat.title) == true)    //書名が見つかれば
                {
                    idNode.Value = EpubDocument.identifiers[ePubDat.title];         //記録されたIDを書き込む
                }
                else //見つからなければUUIDを記載する
                {
                    var guid = "urn:uuid:" +  Guid.NewGuid();           //GUIDを作成する
                    idNode.Value=guid;                                  //identifierに記載する
                    EpubDocument.identifiers.Add(ePubDat.title, guid);  //辞書に追加する
                }
            }
            doc.Save(packFile); //一旦書き込む
            UpdatePackageDocument(packFile);    //日付を更新する
        }
    }
}
