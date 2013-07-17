using System;
using System.Collections.Generic;
using System.IO;

namespace SakuraEpubLibrary
{
    //Epubにするディレクトリの妥当性を確認する
    public static class Validator
    {
        //Epubディレクトリの妥当性を確認する。戻り値はエラーメッセージ
        public static List<string> ValidateEpubDir(string srcDir)
        {
            var errorMes = new List<string>();

            //srcDirの直下にmimetypeファイルが存在するか
            if (File.Exists(srcDir + @"\mimetype") != true)
            {
                errorMes.Add("mimetypeファイルが存在しません。");
            }
            //srcDirの直下にMETA-INFディレクトリが存在するか
            if (Directory.Exists(srcDir + @"\META-INF") != true)
            {
                errorMes.Add("META-INFディレクトリが存在しません");
            }
            else //META-INFディレクトリが存在した
            {
                var container = srcDir + @"\META-INF\container.xml";
                //container.xmlファイルが存在するか
                if (File.Exists(container) != true)
                {
                    errorMes.Add("container.xmlが存在しません");
                }
                else
                {
                    //container.xmlファイルからパッケージ文書を取得する
                    try
                    {
                        var packDocPath = ContainerXML.GetPackageDocument(srcDir);
                        if (File.Exists(packDocPath) != true)   //パッケージ文書が存在するか
                        {
                            errorMes.Add("パッケージ文書が存在しません");
                        }
                    }
                    catch(Exception ex)
                    {
                        errorMes.Add(ex.ToString());
                    }
                }
            }
            return (errorMes);
        }
    }
}
