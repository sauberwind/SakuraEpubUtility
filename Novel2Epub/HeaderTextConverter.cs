using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SakuraEpubUtility
{
    class HeaderTextConverter : TextComverterMethods
    {
        public HeaderTextConverter(string srcFile, string templateFile, TextFormat format, ConvertOptions opt)
            : base(srcFile, templateFile, format, opt)
        {
        }

        //禁止文字が入っている可能性があるため、置き換える
        public override void PreProcess()
        {
            EscapeInvalidChar();
        }

        //hタグ/pタグ処理を行う
        public override void Process()
        {
            var outLines = new List<string>();
            var h1Cnt = 0;  //h1の個数・Identifierに使用する
            var h2Cnt = 0;  //h2の個数・Identifierに使用する

            foreach (var line in lines)
            {
                //見出しか
                if (line.StartsWith("**") == true)      //H2
                {
                    var str = line.TrimStart('*');          //行頭の*を削除する
                    
                    //ヘッダ情報を作成する
                    var id = "h2-" + h2Cnt.ToString();      //id=h2-*
                    var ha = new HeaderAnchor(2, id, str);
                    headerAnchors.Add(ha);

                    //タグをつけて帰す
                    var outline = "<h2 id=\""+id+"\">"+str+"</h2>"; //<h2 id="h2-3">string</h2>
                    outLines.Add(outline);

                    h2Cnt++;
                }
                else if (line.StartsWith("*") == true)  //H1
                {
                    var str = line.TrimStart('*');          //行頭の*を削除する

                    //ヘッダ情報を作成する
                    var id = "h1-" + h1Cnt.ToString();      //id=h2-*
                    var ha = new HeaderAnchor(1,id,str);
                    headerAnchors.Add(ha);

                    //タグをつけて帰す
                    var outline = "<h1 id=\"" + id + "\">" + str + "</h1>"; //<h1 id="h1-3">string</h1>
                    outLines.Add(outline);

                    h1Cnt++;
                }
                else    //pタグ
                {
                    outLines.Add(AddPTagtoLine(line));
                }
            }
            lines = outLines;
        }

    }
}
