using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
namespace SakuraEpubLibrary
{
    //Epub生成後の後処理
    public static class PostProcess
    {
        public static string javaPath { set; get; }            //java.exeのパス
        public static string ePubCheckPath { set; get; }       //ePubCheckのパス
        public static string kindlePreViewerPath { set; get; } //KindlePreviewerのパス

        //デフォルト値は後処理なし
        public static bool? executeEpubCheck { set; get; }      //ePubCheckを実行するか否か
        public static bool? executeKindePreViewer { set; get; } //KindlePreviewerを起動するか否か
                                                                //EpubCheckの結果により上書きされるためDoPostProcessごとに書き込むこと
        static PostProcess()
        {
            javaPath = "";              //仮に全て値なしとする
            ePubCheckPath = "";
            kindlePreViewerPath = "";

            var settingFile = GetPostProcessSettingFile();
            if (File.Exists(settingFile) == true)   //設定ファイルがあれば
            {
                try
                {
                    var doc = XDocument.Load(settingFile);
                    javaPath = doc.Descendants("javaPath").First().Value;
                    ePubCheckPath = doc.Descendants("ePubCheckPath").First().Value;
                    kindlePreViewerPath = doc.Descendants("kindlePreViewerPath").First().Value;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("後処理設定ファイルを読み出すことができませんでした");
                }
            }
        }
        //PostProcessのデータを格納するXMLファイルへのパス
        //c:\user\username\AppData\Roaming\SakuraEpubUtility\PostProcessSetting.xml
        static string GetPostProcessSettingFile()
        {
            string ret = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ret = Path.Combine(ret, "SakuraEpubUtility");
            ret = Path.Combine(ret, "PostProcessSettting.xml");

            return (ret);
        }

        //現在の値をデフォルト値に書き込む
        public static void SaveDefaults()
        {



            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "true"));

            var postProcessRootNode = new XElement("postProcessRoot");
            xml.Add(postProcessRootNode);

            //javaPathのノードを追加する
            var javaPathNode = new XElement("javaPath");
            javaPathNode.Value = javaPath;
            postProcessRootNode.Add(javaPathNode);

            //EpubCheckのノードを追加するする
            var epubCheckNode = new XElement("ePubCheckPath");
            epubCheckNode.Value = ePubCheckPath;
            postProcessRootNode.Add(epubCheckNode);

            //KindlePreviwerのノードを追加する
            var kindlePreviewerNode = new XElement("kindlePreViewerPath");
            kindlePreviewerNode.Value = kindlePreViewerPath;
            postProcessRootNode.Add(kindlePreviewerNode);

            var settingFile = GetPostProcessSettingFile();
            try
            {
                var dirName = Path.GetDirectoryName(settingFile);
                if (Directory.Exists(dirName) != true)   //ディレクトリが存在しなければ作成する
                {
                    Directory.CreateDirectory(dirName);
                }

                xml.Save(settingFile);
            }
            catch (Exception ex)
            {

                MessageBox.Show("後処理設定ファイルの書き込みに失敗しました");
            }
        }

        //後処理を実施する(epubはepubファイルのフルパス名)
        public static void DoPostProcess(string epub)
        {
            try
            {
                if (File.Exists(epub) != true)  //epubファイルがなければ例外を投げて終了する
                {
                    throw (new Exception("EPUBファイルが生成されていません"));
                }
                if ((executeEpubCheck != true) && (executeKindePreViewer != true))  //後処理なしなら
                {
                    MessageBox.Show("EPUBファイルが生成されました\n"+epub);
                }

                if (executeEpubCheck == true)  //ePubCheckを実施するなら
                {
                    var hasNoError = EpubCheckWrapper.ExeEpubCheck(epub);
                    
                    //エラーがあれば、KindlePreviewerを起動するかどうか確認する
                    if ((executeKindePreViewer == true)         //Previewer起動指定で
                        && (hasNoError != true))                //EpubCheckでエラーがあれば
                    {
                        var result = MessageBox.Show("EpubCheckでエラーがありました。KindlePreviewerを起動しますか?",
                            "EpubCheck Result", MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.Cancel)  //Cancelが選択されたら
                        {
                            executeKindePreViewer = false;      //KidlePreviewerを起動しない
                        }
                    }
                }
                if (executeKindePreViewer == true)  //KindlePreviewerを起動するなら
                {
                    ExecuteKindlePreviewer(epub);   //KindlePreviewerを起動する
                }
            }
            catch (Exception ex)    //異常発生時は表示して終了
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //KindlePreviewerを起動する
        static void ExecuteKindlePreviewer(string epub)
        {
            if (File.Exists(kindlePreViewerPath) != true)   //KPが存在するか?
            {
                throw(new Exception("KindlePreviewerのパスが異常です"));
            }
            else if (Path.GetFileName(kindlePreViewerPath).Equals("KindlePreviewer.exe") != true)  //ファイル名が正しいか
            {
                throw (new Exception("KindlePreviewerのファイル名が異常です"));
            }
            else
            {
                //KindlePreviewerを起動する
                Process.Start(kindlePreViewerPath, epub);
            }
        }
        
        //EpubCheckの実行可否を返す
        public static bool isEnableEpubCheck()
        {
            bool ret = true;    //仮に実行可とする
            ret &= File.Exists(javaPath);   //Javaファイルが存在するか
            ret &= Path.GetFileName(javaPath).ToLower().Equals("java.exe");  //Javaのファイル名が正しいか
            ret &= File.Exists(ePubCheckPath);  //EpubCheckファイルが存在するか
            ret &= Path.GetExtension(ePubCheckPath).ToLower().Equals(".jar");   //jarファイルか?

            return (ret);
        }

        //KindlePreviewerの起動可否を返す
        public static bool isEnableExecuteKindlePrevier()
        {
            bool ret = true;    //仮に起動可とする

            ret &= File.Exists(kindlePreViewerPath);    //KPが存在するか
            ret &= Path.GetFileName(kindlePreViewerPath).Equals("KindlePreviewer.exe"); //ファイル名が正しいか

            return (ret);
        }

    }
}
