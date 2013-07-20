using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using SakuraEpubUtility;
using SakuraEpubLibrary;
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

    public class EpubDocument
    {
        public EpubMetaData metaData { set; get; }
        public ConvertOptions opt { set; get; }
        public string novelFileName { set; get; }           //小説本文のパス
        public string coverImageFileName { set; get; }      //表紙画像のパス

        //テンプレートファイルをチェックする
        static public void CheckEpubTemplate()
        {
            var templateDir = GetTemplateDirectory();
            
            if (Directory.Exists(templateDir) != true)
            {
                throw new Exception("テンプレートディレクトリがありません");
            }
            //EPUBに最低限必要なファイルがあるか確認する　異常時は例外が投げられる
            Archiver.CheckEpubDir(templateDir);
        }

        //テンプレートからEpubファイルを作成する
        public bool GenerateEpubDocument()
        {
            //ファイルの上書きを確認する
            var epubFileName = GetEpubFileName();
            if (File.Exists(epubFileName))
            {
                var overwrite = MessageBox.Show("ファイルを上書きしますか?", "Nov2Epub", MessageBoxButton.OKCancel);
                if (overwrite == MessageBoxResult.OK)
                {
                    File.Delete(epubFileName);
                }
                else //上書きしないならそのまま終了
                {
                    return false;
                }
            }

            //identifierを読み込む
            IdentifierDictionary.LoadIdnentiersDictionary();

            //テンプレートファイルをテンポラリディレクトリにコピーする
            var templateDir = GetTemplateDirectory();       //テンプレートディレクトリ
            var tempDir = Path.GetTempPath();               //テンポラリディレクトリ
            var epubDirName = Path.Combine(tempDir,Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));  //ランダム
            CopyDirectory(templateDir, epubDirName);        //コピーする

            //パッケージドキュメントを更新する
            var packFile = ContainerXML.GetPackageDocument(epubDirName);
            PackageDocument.WriteMetaData(packFile, metaData);

            //表紙ファイルを更新する
            var coverImageDst = Path.Combine(epubDirName, @"OEBPS\Images\Cover.JPG");
            File.Copy(coverImageFileName, coverImageDst,true);

            //扉ページを更新する
            var titlePageDst = Path.Combine(epubDirName, @"OEBPS\Text\Title.xhtml");
            InfoPage.UpdateTitlePage(titlePageDst, metaData);

            //テキストを更新する
            var textTemplateFile = Path.Combine(epubDirName, @"OEBPS\Text\Text.xhtml");
            var headerAnchors = TextConverter.ConvertText(novelFileName, textTemplateFile, opt);

            //本文の縦書き/横書きをCSSに設定する
            var textCssTemplateFile = Path.Combine(epubDirName, @"OEBPS\Styles\Text.css");
            TextStyleSheet.SetOrientationStyle(textCssTemplateFile,metaData.isVertical);

            //目次の縦書き/横書きをCSSに設定する
            var navCssTemplateFile = Path.Combine(epubDirName, @"OEBPS\Styles\Nav.css");
            TextStyleSheet.SetOrientationStyle(navCssTemplateFile, metaData.isVertical);


            //ナビゲーションドキュメントを作成する
            var navFileName = Path.Combine(epubDirName, @"OEBPS\Text\nav.xhtml");
            NavigationDocument.WriteNavigationDocument(navFileName, headerAnchors);

            //奥付ページを更新する
            var ColophonPage = Path.Combine(epubDirName, @"OEBPS\Text\Colophon.xhtml");
            InfoPage.UpdateTitlePage(ColophonPage, metaData);

            //不要ファイルを削除する
            var thumbsFile = Path.Combine(epubDirName, @"OEBPS\Images\Thumbs.db");
            if (File.Exists(thumbsFile) == true)
            {
                File.Delete(thumbsFile);
            }

            //EPubファイルを作成する
            Archiver.ArchiveEpubWithPostProcess(epubDirName, epubFileName);

            //テンポラリディレクトリを削除する
            Directory.Delete(epubDirName,true);
           
            //Identifierを更新する
            IdentifierDictionary.SaveIdentifierDictionary();

            return true;
        }

        //EPUBファイルの出力先を取得する
        public string GetEpubFileName()
        {
            //小説ファイルと同ディレクトリ、同ファイル名、拡張子だけ置き換える
            var epubFileDir = Path.GetDirectoryName(novelFileName);
            var epubFileName = Path.GetFileNameWithoutExtension(novelFileName) + ".epub";
            epubFileName = Path.Combine(epubFileDir, epubFileName);

            return (epubFileName);
        }


        //テンプレートのディレクトリを取得する
        static string GetTemplateDirectory()
        {
            var exeDir = System.Reflection.Assembly.GetExecutingAssembly().Location;    //exeのあるディレクトリ
            var templateDir = Path.Combine(Path.GetDirectoryName(exeDir), "Template");

            return (templateDir);
        }
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName);
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
