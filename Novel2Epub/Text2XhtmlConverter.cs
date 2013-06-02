using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace SakuraEpubUtility
{
    public struct ConvertOptions
    {
        public bool hasTag;            //タグが中に記入されていれば、本文内をコンバートしない
        public bool isSpaceIndented;   //空白文字でインデント指定されいている
    }

    //テキストファイルをXHTML(BODYの中のみ)に変更する
    static class Text2XhtmlConverter
    {
        public static string Convert2Xhtml(string srcFileName,ConvertOptions opt)
        {
            var enc = System.Text.Encoding.GetEncoding("utf-8");
            var ret = "";

            var lines = File.ReadAllLines(srcFileName, enc);    //全行を取得する

            foreach (var line in lines) //全行に対して
            {
                var outLine = line;         //仮に出力行=入力行とする

                if (line.Length != 0)  //改行のみでなければ
                {
                    if (opt.hasTag == false)    //タグを含んでいないなら
                    {
                        outLine = HttpUtility.HtmlEncode(outLine);  //禁止文字を置き換える
                    }
                    if (opt.isSpaceIndented == true)    //空白文字でインデントされていたら
                    {
                        outLine = @"<p>" + outLine;
                    }
                    else //インデントされていなければ、会話文と地の文を分ける
                    {
                        if (IsTalkLine(line) == true)   //会話文であればクラスをつける
                        {
                            outLine = @"<p class=""talk"">" + outLine;
                        }
                        else //地の文であればクラスなし
                        {
                            outLine = @"<p>" + outLine;
                        }
                    }
                }
                else    //改行のみであればbrタグを入れる
                {
                    outLine = @"<p><br />";
                }
                outLine += @"</p>\n";   //pタグを閉じて改行を追加する

                ret += outLine; //戻り値に行を追加する
            }

            return ret;


        }
        //会話文かを判定する
        static bool IsTalkLine(string line)
        {
            bool ret = false;

            if ((line.StartsWith("「") == true          //カギ括弧で開始なら
                || (line.StartsWith("『") == true)))
            {
                ret = true;
            }

            return ret;
        }

    }
}
