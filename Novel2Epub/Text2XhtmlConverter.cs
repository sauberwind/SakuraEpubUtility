using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace SakuraEpubUtility
{
    //入力となるテキストファイルのフォーマット
    public enum TextFormat
    {
        PLAIN_TEXT,                 //プレーンテキスト
        PLAIN_TEXT_WITH_HEADER,     //プレーンテキストに*によるヘッダ
        XHTML                       //XHTML　なにもせずコピーする
    }
    //変換オプション
    public struct ConvertOptions
    {
        public bool hasTag;            //タグが中に記入されていれば、本文内の<や&をコンバートしない
        public bool isSpaceIndented;   //空白文字でインデント指定されいている
        public TextFormat format;
    }



    public class TextComveter
    {
        static public void ConvertText(string srcFile, string templateFile, ConvertOptions opt)
        {
            var method = new TextComverterMethods(srcFile, templateFile, opt);

            //フォーマットによりコンバーターを切替える
            switch (opt.format)
            {
                case TextFormat.PLAIN_TEXT: //プレーンテキスト
                    method = new PlainTextConverter(srcFile, templateFile, opt);
                    break;
                case TextFormat.PLAIN_TEXT_WITH_HEADER:
                    method = new HeaderTextConverter(srcFile, templateFile, opt);
                    break;
            }
            method.PreProcess();
            method.Process();
            method.Output();
        }
    }




    public class TextComverterMethods
    {

        protected List<string> lines;
        protected ConvertOptions option;
        protected string templateFile;
        protected List<HeaderAnchor> headerAnchors;
        

        public TextComverterMethods()
        {
        }

        public TextComverterMethods(string srcFile, string templateFile, ConvertOptions opt)
        {
            this.templateFile = templateFile;
            this.option = opt;
            var enc = System.Text.Encoding.GetEncoding("utf-8");
            headerAnchors = new List<HeaderAnchor>();
            lines = File.ReadAllLines(srcFile, enc).ToList();       //全行を取得する
        }

        public virtual void PreProcess() { }
        public virtual void Process() { }
        public virtual void Output()
        {
            var outString = ""; //bodyタグに書く文字列
            foreach (var line in lines)
            {
                outString += line + "\n";
            }
            InsertBody(outString);
        }
        //XHTML禁止文字を置き換える
        protected void EscapeInvalidChar()
        {
            var proccessedLines = new List<string>();
            foreach (var line in lines)
            {
                if (option.hasTag != true)  //タグなしであれば
                {
                    proccessedLines.Add(HttpUtility.HtmlEncode(line));  //禁止文字を置き換える
                }
            }
            lines = proccessedLines;

        }
        //pタグで囲う・
        protected string AddPTagtoLine(string line)
        {
            var outline = line;       //出力行
            if (outline.Length == 0)
            {
                outline = "<br />";
            }
            if (option.isSpaceIndented == true) //スペース字下げなら、単純にpタグを追加する
            {
                outline = "<p>" + outline + "</p>";
            }
            else //スペース字下げでないなら、会話文とクラス分けを行う
            {
                if (IsTalkLine(line) == true)   //会話文であればクラスをつける
                {
                    outline = @"<p class=""talk"">" + outline;
                }
                else //地の文であればクラスなし
                {
                    outline = @"<p>" + outline;
                }
            }
            outline += @"</p>";
            return (outline);
        }
        //本文を挿入して書き込む
        protected void InsertBody(string bodyString)
        {
            var enc = System.Text.Encoding.GetEncoding("utf-8");
            var templateText = File.ReadAllText(templateFile, enc);    //全行を取得する

            //</body>タグを目印に上書きする
            var outText = templateText.Replace("</body>", bodyString + "\n</body>");

            File.WriteAllText(templateFile, outText);

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
