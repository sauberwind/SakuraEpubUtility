using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SakuraEpubUtility;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Specialized;
using System.Security.AccessControl;
namespace SakuraEpubUtility
{
    //変換オプション
    public class ConvertOptions
    {
        public bool hasTag;            //タグが中に記入されていれば、本文内の<や&をコンバートしない
        public bool isSpaceIndented;   //空白文字でインデント指定されいている
        public TextFormat format;
    }
    //入力となるテキストファイルのフォーマット
    public enum TextFormat
    {
        PLAIN_TEXT,                 //プレーンテキスト
        PLAIN_TEXT_WITH_HEADER,     //プレーンテキストに*によるヘッダ
        XHTML                       //XHTML　なにもせずコピーする
    }
    public struct EpubMetaData
    {
        public string title;            //タイトル
        public string author;           //作者
        public string publisher;        //出版社
        public bool isRightToLeft;      //ページ送り(右→左か?=縦書きか?)
        public bool isVertical;         //縦書きか？
    }

    public class EpubDocument
    {
        public EpubMetaData metaData { set; get; }
        public ConvertOptions opt { set; get; }
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
        static public void CheckEpubTemplate()
        {
            var templateDir = GetTemplateDirectory();
            
            if (Directory.Exists(templateDir) != true)
            {
                throw new Exception("テンプレートディレクトリがありません");
            }
            //EPUBに最低限必要なファイルがあるか確認する　異常時は例外が投げられる
            EpubArchiver.CheckEpubDir(templateDir);
        }

        //テンプレートからEpubファイルを作成する
        public void GenerateEpubDocument()
        {
            //テンプレートファイルをテンポラリディレクトリにコピーする
            var templateDir = GetTemplateDirectory();       //テンプレートディレクトリ
            var tempDir = Path.GetTempPath();                //テンポラリディレクトリ
            var epubDirName = Path.Combine(tempDir,Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));  //ランダム
            CopyDirectory(templateDir, epubDirName);     //コピーする

            //パッケージドキュメントを更新する
            var packFile = EpubArchiver.GetPackageDocumentPath(epubDirName);    //パッケージドキュメントのファイル名を取得
            PackageDocument.WriteMetaData(packFile, metaData);

            //テキストを更新する
            var textTemplateFile = Path.Combine(epubDirName, @"\OEBPS\Text", "Text.xhtml");


            //ナビゲーションドキュメントを作成する

            //圧縮する


        }

        //テンプレートのディレクトリを取得する
        static string GetTemplateDirectory()
        {
            var exeDir = System.Reflection.Assembly.GetExecutingAssembly().Location;    //exeのあるディレクトリ
            var templateDir = Path.Combine(Path.GetDirectoryName(exeDir), TEMPLATE);

            return (templateDir);
        }
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            FileSystemAccessRule rule = new FileSystemAccessRule(
                                            "Everyone",
                                            FileSystemRights.FullControl,
                                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                            PropagationFlags.None,
                                            AccessControlType.Allow);

            var dirSec = new DirectorySecurity();
            dirSec.AddAccessRule(rule);


            //コピー先のディレクトリがないときは作る
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName,dirSec);
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] !=
                    System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = System.IO.Directory.GetFiles(sourceDirName);
            foreach (string file in files)
                System.IO.File.Copy(file,
                    destDirName + System.IO.Path.GetFileName(file), true);

            //コピー元のディレクトリにあるディレクトリについて、
            //再帰的に呼び出す
            string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
        }

    }
}
