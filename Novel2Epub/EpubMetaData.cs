using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SakuraEpubUtility
{
    public struct EpubMetaData
    {
        public string title;            //タイトル
        public string author;           //作者
        public string publisher;        //出版社
        public bool isRightToLeft;      //ページ送り(右→左か?=縦書きか?)
        public bool isVertical;         //縦書きか？
    }
}
