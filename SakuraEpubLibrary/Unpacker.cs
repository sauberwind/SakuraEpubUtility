using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
namespace SakuraEpubLibrary
{
    public static class Unpacker
    {
        //テンポラリにEpubファイルを解凍する
        public static string UnpackEpubToTemp(string ePubPath)
        {
            var epubDir = "";   //EPUBを解凍した先のディレクトリ

            if (File.Exists(ePubPath) != true)  //ファイルがなければ異常終了
            {
                throw(new Exception("EPUBファイルがありません:"+ePubPath));
            }

            //解凍先はテンポラリディレクトリ+ファイル名(hoge.epub→c:\users\XXXXX\AppData\Local\temp\hoge
            var tempDir = Path.GetTempPath();   //テンポラリディレクトリ
            tempDir =Path.Combine(tempDir, Path.GetFileNameWithoutExtension(ePubPath));
            if (Directory.Exists(tempDir) == true)  //既にディレクトリがあれば
            {
                var ret = MessageBox.Show("テンポラリディレクトリを消去しますか?" + tempDir, "", MessageBoxButton.OKCancel);
                if (ret == MessageBoxResult.OK) //解凍先のディレクトリを削除する
                {
                    Directory.Delete(tempDir, true);
                }
                else //解凍先を上書きできなかったため、そのまま終了する
                {
                    return (epubDir);
                }
            }

            //ZIPとして解凍する
            try
            {
                ZipFile.ExtractToDirectory(ePubPath, tempDir);
                epubDir = tempDir;                              //解凍先を戻り値に格納する
            }
            catch (InvalidDataException ex) //書庫が壊れていた
            {
                MessageBox.Show("EPUBファイルが破損しています");
            }
            catch (Exception ex)
            {
                MessageBox.Show("解凍でエラーが発生しました" + ex.ToString());
            }
            return (epubDir);
        }
    }
}
