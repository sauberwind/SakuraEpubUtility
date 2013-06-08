using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SakuraEpubUtility
{
    class XHtmlConverter : TextConverterMethods
    {
        public XHtmlConverter(string srcFile, string templateFile, ConvertOptions opt)
            : base(srcFile, templateFile, opt)
        {
            var doc = XDocument.Load(srcFile);
            var headerNodes = doc.Descendants().Where(e => (e.Name.LocalName == "h1") || (e.Name.LocalName == "h2"));
            var h1Cnt = 0;  //h1の個数・Identifierに使用する
            var h2Cnt = 0;  //h2の個数・Identifierに使用する

            foreach (var header in headerNodes)
            {
                var ha = new HeaderAnchor();
                var id = "";
                if (header.Name.LocalName == "h1")  //h1なら
                {
                    //ヘッダ情報を作成する
                    h1Cnt++;
                    id = "h1-" + h1Cnt.ToString();
                    ha = new HeaderAnchor(1, id, header.Value);
                }
                else    //h2なら
                {
                    //ヘッダ情報を作成する
                    h2Cnt++;
                    id = "h2-" + h2Cnt.ToString();
                    ha = new HeaderAnchor(2, id, header.Value);
                }
                headerAnchors.Add(ha);              //アンカーをリストに追加する
                header.SetAttributeValue("id", id);  //ノードにidを設定する
            }
            doc.Save(templateFile); //テンプレートファイルに上書きする

        }
        //プロセスは処理なし

        //出力処理はプリプロセスで終了している
        public override void Output()
        {
        }
    }
}
