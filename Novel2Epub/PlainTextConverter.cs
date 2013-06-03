using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SakuraEpubUtility
{
    //プレーンテキストをXHTMLにする
    class PlainTextConverter:TextComverterMethods
    {

        public PlainTextConverter(string srcFile,string templateFile, TextFormat format, ConvertOptions opt)
            : base(srcFile,templateFile, format,opt)
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
        public override void Output()
        {
            var outString = ""; //bodyタグに書く文字列
            foreach(var line in lines)
            {
                outString+=line+"\n";
            }
            InsertBody(outString);
        }

    }
}
