using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
namespace SakuraEpubUtility
{
    //tocに書き込むヘッダの位置
    public class HeaderAnchor
    {
        public HeaderAnchor()
        {
        }

        public HeaderAnchor(int lv, string id, string text)
        {
            level = lv;
            identifier = id;
            str = text;
        }

        public int level;               //ヘッダのレベル
        public string identifier;       //ヘッダのID
        public string str;              //ヘッダ文字列
    }


    public class NavigationDocument
    {
        public static void WriteNavigationDocument(string template, List<HeaderAnchor> headers)
        {
            XNamespace ns ="http://www.w3.org/1999/xhtml";

            var doc = XDocument.Load(template); //テンプレートを読み込む

            var navNode = doc.Descendants().Where(e => e.Name.LocalName == "nav").First();  //nav要素
            var firstLevelList = new XElement(ns + "ol");    //見出し1に対応するolタグ
            navNode.Add(firstLevelList);                //olタグを追加する

            //ヘッダをnavのリストに追加する
            var currentLevel = 1;   //見出しレベル1とする
            XElement currentLevel1Node=null;    //レベル1ノード(次にレベル2が来た時に追加する先(li)
            XElement currentLevel2Node=null;    //レベル2ノード(次にレベル2が来た時に追加する先(ol)

            foreach (var header in headers)
            {
                //リンク先の文字列を作成する
                var linkPath = "Text.xhtml#" + header.identifier;

                //ヘッダレベルで階層分けを行う
                if (header.level == 1)
                {
                    currentLevel = 1; //ヘッダレベル1

                    //li要素を作成する
                    var header1Node = new XElement(ns+"li");    //ノードを作成する
                    firstLevelList.Add(header1Node);            //ルートのol要素に追加する

                    //リンクを作成する
                    var anchorNode = new XElement(ns+"a");
                    anchorNode.SetAttributeValue("href", linkPath);
                    anchorNode.Value = header.str;
                    header1Node.Add(anchorNode);

                    currentLevel1Node = header1Node;        //レベル2の追加先として記憶する
                }
                else //ヘッダレベルが2なら
                {
                    if (currentLevel == 1)  //レベル1→2ならolタグを挿入する
                    {
                        var header2ListNode = new XElement(ns+"ol");
                        if (currentLevel1Node == null)
                        {
                            throw(new Exception("見出し2が見出し1より先に現れました"));
                        }
                        currentLevel1Node.Add(header2ListNode); //見出し1にolを追加する
                        currentLevel2Node=header2ListNode;      //見出し2の追加先を更新
                    }
                    //li要素を作成する
                    var header2Node = new XElement(ns+"li");   //ノードを作成する
                    currentLevel2Node.Add(header2Node);     //見出し2のolにノードを追加する

                    //リンクを作成する
                    var anchorNode = new XElement(ns+"a");
                    anchorNode.SetAttributeValue("href", linkPath);
                    anchorNode.Value = header.str;
                    header2Node.Add(anchorNode);

                    currentLevel=2;
                }
            }
            //XMLファイルに書きだす(templateファイルに上書きする)
            doc.Save(template);
        }
    }
}
