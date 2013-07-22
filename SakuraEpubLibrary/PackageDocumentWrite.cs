using System;
using System.Linq;
using System.Xml.Linq;

namespace SakuraEpubLibrary
{
    //パッケージ文書
    public partial class PackageDocument
    {
        static XNamespace NS_PACKAGE = "http://www.idpf.org/2007/opf";
        static XNamespace NS_DC = "http://purl.org/dc/elements/1.1/";

        //パッケージ文書を更新する
        public static void UpdatePackageDocument(string packFile)
        {
            var doc = new XDocument();

            //opfファイルを読み込む
            try
            {
                doc = XDocument.Load(packFile);
            }
            catch (Exception ex)
            {
                throw(new Exception("パッケージ文書を読み込めませんでした"+ex.ToString()));
            }

            //Epub3であればlastmodifiedを書き換える
            var packNode = doc.Descendants().Where(e => (e.Name.LocalName == "package")&&(e.Name.Namespace==NS_PACKAGE)).FirstOrDefault();
            if (packNode == null)   //パッケージ文書が存在しなければ
            {
                throw (new Exception("パッケージ文書にpackage要素がありません"));
            }
            if (packNode.Attribute("version") == null)  //version属性がないなら
            {
                throw (new Exception("パッケージ文書にEPUBバージョンがありません"));
            }
            var epubVer = packNode.Attribute("version").Value;
            if (epubVer.Equals("3.0"))  //EPUB3ならmodified属性を上書きする
            {
                try
                {
                    var lastModifiedNode 
                        = doc.Descendants()
                        .Where(e => e.Name.LocalName == "meta")                                         //metaタグで
                        .Where(e=>e.Attribute("property").Value=="dcterms:modified").First();           //属性がmodified
                    var utc = DateTime.UtcNow;                          //協定世界時を取得
                    lastModifiedNode.Value = (utc.ToString("s") + 'Z'); //YYYY-MM-DDThh:mm:ssZ
                }
                catch (Exception ex)
                {
                    throw (new Exception("パッケージ文書にmodifiedが存在しません"+ex.ToString()));
                }
                try
                {
                    doc.Save(packFile);
                }
                catch(Exception ex)
                {
                    throw(new Exception("パッケージ文書の上書きに失敗しました"+ex.ToString()));
                }
            }
        }

        //メタデータを書き込む
        public static void WriteMetaData(string packFile, EpubMetaData ePubDat)
        {
            var doc = new XDocument();
            try
            {
                doc = XDocument.Load(packFile);     //ファイルを読み込む
            }
            catch (Exception ex)
            {
                throw (new Exception("パッケージ文書を読み込めませんでした" + ex.ToString()));
            }

 
            //DC名前空間のノードをまとめて取得しておく
            var dcNodes = doc.Descendants().Where(e => e.Name.Namespace == NS_DC);

            //書名
            var titleNode =dcNodes.Where(e => e.Name.LocalName == "title").FirstOrDefault();
            if (titleNode == null)
            {
                throw (new Exception("パッケージ文書にtitle要素がありません"));
            }
            titleNode.Value = ePubDat.title;

            //著者
            var authorNode = dcNodes.Where(e => e.Name.LocalName == "creator").FirstOrDefault();
            if (authorNode == null)
            {
                throw (new Exception("パッケージ文書にcreator要素がありません"));
            }
            authorNode.Value = ePubDat.author;

            //出版
            var publisherNode = dcNodes.Where(e => e.Name.LocalName == "publisher").FirstOrDefault();
            if (publisherNode == null)
            {
                throw (new Exception("パッケージ文書にcreator要素がありません"));
            }
            publisherNode.Value = ePubDat.publisher;

            //ID identifier
            var packNode = doc.Descendants().Where(e => (e.Name.LocalName == "package") && (e.Name.Namespace == NS_PACKAGE)).FirstOrDefault();
            if (packNode == null)   //パッケージ文書が存在しなければ
            {
                throw (new Exception("パッケージ文書にpackage要素がありません"));
            }
            //unique-identifierに対応する要素を取得する
            var uniqueIDName = "";  //unique-identifier属性の値
            var uniqueID = packNode.Attributes().Where(e => e.Name == "unique-identifier").FirstOrDefault();
            if (uniqueID != null)   //unique-identifier属性がある
            {
                uniqueIDName = uniqueID.Value;  //unique IDとして使用する要素のid
                if (uniqueIDName.Length == 0)  //unique-identifierが記入されていなければ
                {
                    uniqueIDName = "pub-id";    //pub-idとする
                    packNode.SetAttributeValue("unique-identifier", uniqueIDName);  //パッケージノードのunique-identifier属性を更新する
                }
            }
            else //unique-identifier属性がない
            {
                uniqueIDName = "pub-id";    //pub-idとする
                packNode.SetAttributeValue("unique-identifier", uniqueIDName);  //パッケージノードのunique-identifier属性を更新する
            }
            //identifierノードを更新する
            var idNodes = dcNodes.Where(e => e.Name.LocalName == "identifier")           //identifier要素
                                .Where(e => e.Attribute("id") != null);                  //id属性がある
            var idNode = idNodes.Where(e => e.Attribute("id").Value == uniqueIDName)    //id属性がunique ID
                                .FirstOrDefault();
            if (idNode != null)    //IDノードが見つかったのでIDを書き込む
            {
                WriteUniqueIdentifier(ePubDat.title, idNode);
            }
            else //IDノードが見つからなかったのでノードを追加する
            {
                var idElement = new XElement(NS_DC+"identifier");   //dc:identifier
                idElement.SetAttributeValue("id", uniqueIDName);    //id="pub-id"

                var metadataNode = doc.Descendants().Where(e => e.Name.LocalName == "metadata").FirstOrDefault();
                if (metadataNode != null)
                {
                    metadataNode.Add(idElement);                        //metadataにidentifier要素を追加
                    WriteUniqueIdentifier(ePubDat.title, idElement);    //IDを書き込む
                }
                else //metadataがなかった
                {
                    throw (new Exception("パッケージ文書にpackage要素がありません"));
                }
            }

            //ページ送り方向を書き込む
            XNamespace package = "http://www.idpf.org/2007/opf";
            var spinNode = doc.Descendants()
                .Where(e => (e.Name.LocalName == "spine")&&(e.Name.Namespace==package)).FirstOrDefault();
            if (spinNode == null)   //spineが見つからなかった
            {
                throw (new Exception("パッケージ文書にspine要素がありません"));
            }
            if (ePubDat.isRightToLeft == true) //右から左なら
            {
                spinNode.SetAttributeValue("page-progression-direction", "rtl");
            }
            else    //左から右なら
            {
                spinNode.SetAttributeValue("page-progression-direction", "ltr");
            }
            try
            {
                doc.Save(packFile);
            }
            catch (Exception ex)
            {
                throw (new Exception("パッケージ文書の上書きに失敗しました" + ex.ToString()));
            }
            UpdatePackageDocument(packFile);    //日付を更新する
        }

        //UniqueIdentfierを指定されたノードに書き込む。
        //既知の本であればそのIDを書く/未知であれば辞書に追加する
        private static void WriteUniqueIdentifier(string title, XElement idNode)
        {
            if (idNode.Value != "") //IDが書かれていた場合はなにもしない
            {
                return;
            }
            if(IdentifierDictionary.identifiers.ContainsKey(title) == true)     //書名が見つかれば
            {
                idNode.Value = IdentifierDictionary.identifiers[title];         //記録されたIDを書き込む
            }
            else //見つからなければUUIDを記載する
            {
                var guid = "urn:uuid:" + Guid.NewGuid();                    //GUIDを作成する
                idNode.Value = guid;                                        //identifierに記載する
                IdentifierDictionary.identifiers.Add(title, guid);          //辞書に追加する
            }
        }

        //UniqueIdentifierとして書くUUID文字列を取得する
        static string GetUUIDString()
        {
            var guid = "urn:uuid:" + Guid.NewGuid();
            return (guid);
        }
    }
}
