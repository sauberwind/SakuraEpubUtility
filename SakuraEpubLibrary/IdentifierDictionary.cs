using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SakuraEpubLibrary.Properties;

namespace SakuraEpubLibrary
{
    public static class IdentifierDictionary
    {
        public static Dictionary<string, string> identifiers;  //書名とIDの組み合わせ

        //タイトルとIDの辞書を読みだす
        public static void LoadIdnentiersDictionary()
        {
            identifiers = new Dictionary<string, string>();
            if (Settings.Default.titleIDs != null)          //IDの記録があれば
            {
                for (var i = 0; i < Settings.Default.titleIDs.Count / 2; i++)
                {
                    identifiers.Add(
                        Settings.Default.titleIDs[i * 2],           //タイトル
                        Settings.Default.titleIDs[i * 2 + 1]        //ID
                        );
                }
            }
        }

        //identifierとtitleのペアを更新する
        public static void SaveIdentifierDictionary()
        {
            Settings.Default.titleIDs = new StringCollection();

            foreach (var ids in identifiers)
            {
                Settings.Default.titleIDs.Add(ids.Key);         //タイトル
                Settings.Default.titleIDs.Add(ids.Value);       //ID
           
            }
            Settings.Default.Save();    //書き込む
        }
    }
}
