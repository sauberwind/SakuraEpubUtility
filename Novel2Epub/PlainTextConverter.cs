using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SakuraEpubUtility
{
    //プレーンテキストをXHTMLにする
    class PlainTextConverter:TextConverterMethods
    {

        public PlainTextConverter(string srcFile,string templateFile,ConvertOptions opt)
            : base(srcFile,templateFile,opt)
        {
        }

        //禁止文字が入っている可能性があるため、置き換える
        public override void PreProcess()
        {
            EscapeInvalidChar();
        }


        //プレーンテキストなので<p><br>を追加する
        public override void Process()
        {
            ComvertToTaggedLines();
        }

        //pタグで囲う。<br/ >を追加する
        void ComvertToTaggedLines()
        {
            var outLines = new List<string>();  //出力行
            foreach (var line in lines)
            {
                var outline = AddPTagtoLine(line);
                outLines.Add(outline);
            }
            lines = outLines;
        }

    }
}
