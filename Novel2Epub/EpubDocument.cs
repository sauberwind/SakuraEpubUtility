using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SakuraEpubUtility;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Specialized;
namespace SakuraEpubUtility
{
    public struct EpubMetaData
    {
        public string title;            //タイトル
        public string author;           //作者
        public string publiser;         //出版社
    }

    public class EpubDocument
    {
        public string title{set;get;}               //タイトル
        public string author{set;get;}              //作者
        public string publiser { set; get; }        //出版社
        public string novelPath { set; get; }       //小説本文のパス
        public string coverImagePath { set; get; }  //表紙画像のパス

        public static Dictionary<string, string> identifiers;  //書名とIDの組み合わせ

        const string TEMPLATE = "template"; //テンプレート格納ディレクトリ

        //タイトルとIDの辞書を作成する
        public void SetIdnentiersDictionary(StringCollection idDics)
        {
            identifiers = new Dictionary<string, string>();

            for (var i = 0; i < idDics.Count / 2; i++)
            {
                identifiers.Add(
                    idDics[i*2],        //タイトル
                    idDics[i*2+1]       //ID
                    );
            }
        }

        //テンプレートファイルをチェックする
        public void CheckEpubTemplate()
        {
            var templateDir = GetTemplateDirectory();
            
            if (Directory.Exists(templateDir) != true)
            {
                throw new Exception("テンプレートディレクトリがありません");
            }
            //EPUBに最低限必要なファイルがあるか確認する異常時は例外が投げられる
            EpubArchiver.CheckEpubDir(templateDir);
        }

        //テンプレートからEpubファイルを作成する
        public void GenerateEpubDocument()
        {
            //テンプレートファイルをテンポラリディレクトリにコピーする
            var templateDir = GetTemplateDirectory();   //テンプレートディレクトリ
            var tempDir = Path.GetTempPath();                //テンポラリディレクトリ
            var epubDirName = Path.Combine(tempDir,Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));  //ランダム
            FileSystem.CopyDirectory(templateDir, epubDirName);     //コピーする

            //パッケージドキュメントを更新する
            var epubDat = new EpubMetaData();
            epubDat.title = title;
            epubDat.author = author;
            epubDat.publiser = publiser;

        }

        //テンプレートのディレクトリを取得する
        string GetTemplateDirectory()
        {
            var exeDir = System.Reflection.Assembly.GetExecutingAssembly().Location;    //exeのあるディレクトリ
            var templateDir = Path.Combine(Path.GetDirectoryName(exeDir), TEMPLATE);

            return (templateDir);
        }
    }
}
