using System;
using System.Text;
using System.Xml;
using System.Windows;

namespace SakuraEpubLibrary
{
    public static class XMLUtilities
    {
        //XMLファイルを読み込み、エンコーディングを返す
        public static Encoding GetEncodingFormat(string file)
        {
            var enc = Encoding.GetEncoding("shift_jis");    //仮にshift-JISとする。
            try
            {
                var reader = new XmlTextReader(file);
                reader.Read();
                enc = Encoding.GetEncoding(reader.Encoding.WebName);
                reader.Close();
            }
            catch (Exception ex)    //ファイルがない/XMLとして異常など
            {
                ;   //デフォルトのshift-JISを返す
            }
            return(enc);
 
        }

    }
}
