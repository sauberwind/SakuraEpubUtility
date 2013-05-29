using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.IO.Compression;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace ArchiveEpub
{
    class EpubArchiver
    {
        static string packageDocumentPath;

        public static void ArchiveEpub(string srcDirName, string dstFileName)
        {
            packageDocumentPath = null;

            //最低限必要なファイルの有無を確認する
            var errorMes = CheckEpubDir(srcDirName);
            if (errorMes.Count != 0)    //異常があったなら表示して終了する
            {
                string mes="";
                foreach (var err in errorMes)
                {
                    mes += (err + @"\n");
                }
                MessageBox.Show(mes);
                return;
            }

            //パッケージドキュメントのUUIDとmodifiedを更新する
            PackageDocument.UpdatePackageDocument(packageDocumentPath);

            //Epubを作成する
            PackEpub(srcDirName, dstFileName);
        }

        private static void PackEpub(string srcDirName, string dstFileName)
        {
            var srcDir = new DirectoryInfo(srcDirName);

            var files = srcDir.EnumerateFiles();        //ファイルを取得
            var dirs = srcDir.EnumerateDirectories();   //ディレクトリを取得


            //mimetypeファイルを取得する
            var mimeTypeFile = files.FirstOrDefault(e => e.Name == "mimetype");

            //container.xmlファイルを取得する
            var metaInfDir = dirs.First(e => e.Name == "META-INF");
            var containedFiles = metaInfDir.EnumerateFiles();
            var containerXmlFile = containedFiles.FirstOrDefault(e => e.Name == "container.xml");

            //EPUBファイルを作成する
            using (ZipStorer zip = ZipStorer.Create(dstFileName, string.Empty))
            {
                zip.EncodeUTF8 = true;

                //mimetypeファイルを書き込む 先頭・無圧縮
                WriteFileToZip(zip, mimeTypeFile, "mimetype", ZipStorer.Compression.Store);

                //ディレクトリの内容を書き込む
                WriteDirToZip(zip, srcDir, string.Empty);
            }

        }
        //ディレクトリをzipファイルに書き込む
        private static void WriteDirToZip(ZipStorer zip, DirectoryInfo srcDir, string pathInZip)
        {
            var files = srcDir.EnumerateFiles();
            files = files.Where(e => e.Name != "mimetype"); //mimetypeファイルを除く

            foreach (var file in files)
            {
                var ext = file.Extension;

                ZipStorer.Compression compression;
                //ファイル形式によって圧縮形式を変える
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
                WriteFileToZip(zip,file, pathInZip + file.Name, compression);
            }
            //残りのディレクトリを再帰的に書き込む
            var dirs = srcDir.EnumerateDirectories();
            foreach (var dir in dirs)
            {
                WriteDirToZip(zip, dir, pathInZip + dir.Name + "/");
            }

        }
        //ファイルをzipファイルに書き込む
        private static void WriteFileToZip(ZipStorer zip, FileInfo file, string fileNameInZip, ZipStorer.Compression compression)
        {
            using(var m =new MemoryStream(File.ReadAllBytes(file.FullName)))    //対象をファイルから読み出す
            {
                m.Position = 0; //先頭からコピー
                zip.AddStream(compression, fileNameInZip, m, DateTime.Now, String.Empty);   //zipファイルに格納する
            }
        }

        //EPUBを作成するのに必要なファイルが存在しているか確認する
        private static List<string> CheckEpubDir(string srdDir)
        {
            var errorMes = new List<string>();

            //srcDirの直下にmimetypeファイルが存在するか
            if (File.Exists(srdDir + @"\mimetype") != true)
            {
                errorMes.Add("mimetypeファイルが存在しません。");
            }
            //srcDirの直下にMETA-INFディレクトリが存在するか
            if (Directory.Exists(srdDir + @"\META-INF") != true)
            {
                errorMes.Add("META-INFディレクトリが存在しません");
            }
            else //META-INFディレクトリが存在した
            {
                var container = srdDir + @"\META-INF\container.xml";
                //container.xmlファイルが存在するか
                if (File.Exists(container) != true)
                {
                    errorMes.Add("container.xmlが存在しません");
                }
                else
                {
                    //container.xmlファイルからパッケージ文書を取得する
                    var doc = XElement.Load(container);
                    var rootFileNode = doc.Descendants().Where(e => e.Name.LocalName == "rootfile");
                    var rootFilePath = rootFileNode.Attributes("full-path").First().Value;

                    packageDocumentPath = Path.Combine(srdDir, rootFilePath);   //パッケージ文書パスを保存
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
