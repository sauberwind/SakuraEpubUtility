using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace SakuraEpubLibrary
{
    public static class Archiver
    {
        //フォルダをEpubファイルに圧縮する(後処理つき)
        public static void ArchiveEpubWithPostProcess
            (
                 string srcDir,             //Epubにするディレクトリ
                 string dstFile             //出力するEpubファイル名
            )
        {
            //Epubファイルに圧縮する
            var isArchived = ArchiveEpub(srcDir, dstFile);

            //後処理を実施する
            if (isArchived != true) //Epubファイルが生成できなかった
            {
                return; //終了する。エラーメッセージなどは下の階層で出力される
            }
            else //Epubファイルが生成された.後処理を実行する。
            {
                PostProcess.DoPostProcess(dstFile);
            }
        }


        //フォルダをEpubファイルに圧縮する。異常時はメッセージボックスを表示してfalseを返す
        public static bool ArchiveEpub(string srcDir, string dstFile)
        {
            bool ret = false;   //仮に生成失敗とする

            var errorMes = CheckEpubDir(srcDir);    //Epubにできるか確認する
            if (errorMes.Count == 0)                //EPUBにできるなら
            {
                try
                {
                    //パッケージ文書のmodified属性を更新する。
                    var packFile= ContainerXML.GetPackageDocument(srcDir);
                    PackageDocument.UpdatePackageDocument(packFile);

                    //EPUBを作成する
                    PackEpub(srcDir, dstFile);

                    ret = true; //生成成功
                }
                catch (Exception ex)
                {
                    errorMes.Add(ex.ToString());
                }
            }
            if (errorMes.Count > 0) //異常があったなら
            {
                var err = "";
                foreach (var mes in errorMes)
                {
                    err += mes + "\n";
                }
                MessageBox.Show("EPUB生成に失敗しました\n" + err);
            }
            return (ret);
        }
        //フォルダをEpubファイルに圧縮する
        static void PackEpub(string srcDir, string dstFile)
        {
            //Epubファイルを作成する
            using(var zip=ZipStorer.Create(dstFile,string.Empty))
            {
                zip.EncodeUTF8 = true;  //要?

                //mimetypeファイルを書き込む
                var mimeTypeFile = Path.Combine(srcDir, "mimetype");
                using(var m=new MemoryStream(File.ReadAllBytes(mimeTypeFile)))
                {
                    m.Position = 0;
                    zip.AddStream(ZipStorer.Compression.Store, "mimetype",m, DateTime.Now, string.Empty);

                }
                //mimetype以外のファイルを書き込む
                WriteEpubFilesToZip(zip, srcDir);
            }
        }

        //Epubにファイルを追加する(mimetypeを除く)
        private static void WriteEpubFilesToZip(ZipStorer zip,string srcDir)
        {
            var files = Directory.GetFiles(srcDir, "*", SearchOption.AllDirectories);           //全ファイル
            var targetFiles = files.Where(e => Path.GetFileName(e).Equals("mimetype") != true)  //mimetypeを除く
                .Select(e => new FileInfo(e));

            foreach (var targetFile in targetFiles)
            {
                var ext = targetFile.Extension;
                var compression = new ZipStorer.Compression();
                switch (ext)
                {
                    case "jpg": //画像ファイルは圧縮しない(時間の無駄なので)
                    case "JPEG":
                    case "png":
                    case "PNG":
                    case "gif":
                    case "GIF":
                        compression = ZipStorer.Compression.Store;
                        break;
                    case "EPUB":
                    case "epub":
                        continue;   //EPUBファイルは格納しない
                    default:
                        compression = ZipStorer.Compression.Deflate;  //通常のファイルは圧縮する
                        break;
                }
                //対象を書き込む
                using (var ms = new MemoryStream(File.ReadAllBytes(targetFile.FullName)))
                {
                    ms.Position = 0;    //ファイルの先頭からコピー
                    var fileNameInZip = GetRelPath(targetFile.FullName, srcDir);    //zip内でのファイル名
                    zip.AddStream(compression, fileNameInZip, ms, DateTime.Now, string.Empty);
                }
            }
        }
        //相対パスに変換する(\->/)
        static string GetRelPath(string fileName, string baseDir)
        {
            var relPath = fileName.Remove(0, baseDir.Count() + 1);  //+1はディレクトリ直下の\
            var ret = relPath.Replace('\\','/');

            return (ret);
        }

        //EPUBを作成するのに必要なファイルが存在しているか確認する
        public static List<string> CheckEpubDir(string srcDir)
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
                    var packageDocumentPath = ContainerXML.GetPackageDocument(srcDir);
                    if (File.Exists(packageDocumentPath) != true)   //パッケージ文書が存在するか
                    {
                        errorMes.Add("パッケージ文書が存在しません");
                    }
                }
            }
            return (errorMes);
        }
    }
}
