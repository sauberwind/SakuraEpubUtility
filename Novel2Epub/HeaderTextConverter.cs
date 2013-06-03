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
            : base(srcFile,templateFile, format,opt)
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
            ComvertToTaggedLines();
        }

    }
}
