using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace SakuraEpubUtility
{
    public static class IdentifierDictionary
    {
        public static Dictionary<string, string> identifiers;  //書名とIDの組み合わせ

        const string TEMPLATE = "template"; //テンプレート格納ディレクトリ

        //タイトルとIDの辞書を作成する
        public static void SetIdnentiersDictionary(StringCollection idDics)
        {
            identifiers = new Dictionary<string, string>();

            for (var i = 0; i < idDics.Count / 2; i++)
            {
                identifiers.Add(
                    idDics[i * 2],        //タイトル
                    idDics[i * 2 + 1]       //ID
                    );
            }
        }

    }
}
