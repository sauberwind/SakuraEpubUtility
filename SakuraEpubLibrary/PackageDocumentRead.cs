using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace SakuraEpubLibrary
{
    public partial class PackageDocument
    {
        //テキスト要素のパスを取得する
        public static List<string> GetTextItems(string packFile)
        {
            var textItems = new List<string>(); //戻すテキスト要素
            
            textItems.Add(packFile.Replace('/','\\'));      //パッケージドキュメントもテキスト要素
                                                            //ディレクトリ区切り文字に入れ替える

            //パッケージドキュメントを読み込む
            var doc = new XDocument();
            try
            {
                doc = XDocument.Load(packFile);
            }
            catch(Exception ex)
            {
                throw(new Exception("パッケージ文書を読み込めませんでした"+ex.ToString()));
            }

            //manifestを取得する
            var manifestNode = doc.Descendants().Where(e => e.Name.LocalName == "manifest").FirstOrDefault();
            if (manifestNode == null)  //manifestノードがなければ異常
            {
                throw (new Exception("manifestノードがありません"));
            }

            //itemからXHTMLのhrefを取得する
            var epubPath = Path.GetDirectoryName(packFile); //パッケージドキュメントのパスを取得する
            var itemNodes = manifestNode.Descendants().Where(e => e.Name.LocalName == "item")       //itemの中から、必要なプロパティを持つものをフィルタする
                                                .Where(e => e.Attribute("media-type") != null)
                                                .Where(e => e.Attribute("href") != null);
            if (itemNodes != null)  //パスが取得できるitem要素があれば
            {
                var textPathes = itemNodes.Where(e => e.Attribute("media-type").Value == "application/xhtml+xml")   //XHTML
                                        .Select(e =>ConvIriToLocalPath(e.Attribute("href").Value,epubPath));        //IRIから変換
                if (textPathes != null) //テキスト要素があれば
                {
                    textItems.AddRange(textPathes);
                }
            }
            return (textItems);
        }

        //item要素のhref(IRI形式)からローカルパスに変換する
        static string ConvIriToLocalPath(string iri,string path)
        {
            var ret = Path.Combine(path, iri.Replace('/', '\\'));
            return(ret);
        }
    }
}
