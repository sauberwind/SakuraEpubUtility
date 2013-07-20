using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace SakuraEpubUtility
{
    class TextStyleSheet
    {
        //スタイルシートの縦書き/横書きを設定する
        public static void SetOrientationStyle(string fileName,bool isVertical)
        {
            var enc = System.Text.Encoding.GetEncoding("utf-8");
            var allText = File.ReadAllText(fileName, enc);       //全行を取得する
            
            //正規表現を使って、writingmodeの箇所を置換する
            var regEx = new Regex("writing-mode:.*;");
            if (isVertical == true)     //縦書きなら
            {
                allText = regEx.Replace(allText, "writing-mode: vertical-rl;");
            }
            else                        //横書きなら
            {
                allText = regEx.Replace(allText, "writing-mode: horizontal-tb;");
            }
            File.WriteAllText(fileName, allText);
        }
    }
}
