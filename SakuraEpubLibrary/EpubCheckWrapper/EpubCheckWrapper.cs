using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace SakuraEpubLibrary
{
    public static class EpubCheckWrapper
    {
        static List<string> outputLines;    //EpubCheckのOutput;
        static List<string> errorLines;     //EpubCheckのエラー

        static EpubCheckWrapper()
        {
        }

        //EpubCheckが実施できる環境か確認する
        static List<string> CheckEpubCheckEnviroment()
        {
            var javaPath = PostProcess.javaPath;
            var ePubCheckPath = PostProcess.ePubCheckPath;

            var ret = new List<string>();   //エラーメッセージ
            if (File.Exists(javaPath) != true)  //Javaファイルが存在しない?
            {
                ret.Add("Javaのファイル名を設定してください");
            }
            if (Path.GetFileName(javaPath).ToLower().Equals("java.exe") != true)    //ファイル名が異常?
            {
                ret.Add("Javaのファイル名がJava.exeではありません。");
            }
            if (File.Exists(ePubCheckPath) != true)   //epubCheckファイルが存在しない
            {
                ret.Add("ePubCheckのファイル名を設定してください");
            }
            if (Path.GetExtension(ePubCheckPath).ToLower().Equals(".jar") != true) //ePubCheckの拡張子が異常
            {
                ret.Add("ePubCheckの拡張子がjarではありません");
            }
            return (ret);
        }

        //EpubCheckを実施する
        public static bool ExeEpubCheck(string ePubFile)
        {
            //EpubCheckが実施できる状態か確認する
            var exeEpubCheck = MessageBoxResult.Yes;    //仮に実施する、としておく
            var envErr=CheckEpubCheckEnviroment();      //EpubCheckの情報が足りているか?
            
            while((exeEpubCheck==MessageBoxResult.Yes)  //EpubCheck実施で
                &&(envErr.Count!=0))                    //EpubCheckの情報が異常なら
            {
                //エラーメッセージを表示し、EpubCheckを実施するか確認する
                var mes = "EpubCheckの設定が必要です。設定を実施しますか？\n";
                foreach (var m in envErr)
                {
                    mes += (m + "\n");
                }
                exeEpubCheck = MessageBox.Show(mes, "EpubCheck", MessageBoxButton.YesNo);
                if (exeEpubCheck == MessageBoxResult.Yes)   //EpubCheckの設定画面を出す
                {
                    var postProcessSettingDialog = new PostProcessSettingDialog();
                    postProcessSettingDialog.ShowDialog();

                    //入れられたEpubCheckの設定値を確認する
                    envErr = CheckEpubCheckEnviroment();

                }

            }
            if (exeEpubCheck == MessageBoxResult.No)    //EpubCheck中断なら
            {
                MessageBox.Show("EpubCheckを中断しました");
                return (false);                             //中断したのでエラー扱いとする

            }
            else //EpubCheckに必要な値が入っている->EpubCheckを実行する
            {
                var ret = CheckEpub(ePubFile);
                return (ret);           
            }
        }


        //CheckEpubを実施、Output/Errorを表示する
        public static bool CheckEpub(string ePubFile)
        {
            var ret = true; //仮にエラーなしとする

            //メッセージ初期化
            outputLines = new List<string>();
            errorLines = new List<string>();

            //EPUBチェック実施中パネルを表示する
            var checkingDlg = new EpubCheckingDialog();
            checkingDlg.Show();

            var javaPath = PostProcess.javaPath;
            var ePubCheckPath = PostProcess.ePubCheckPath;

            //実行ファイル
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = javaPath;

            //引数
            var args = "-jar "
                        + "\"" + ePubCheckPath + "\" "
                        + " \"" + ePubFile + "\"";
            p.StartInfo.Arguments = args;

            //出力とエラーをストリームに書き込むようにする
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            //OutputDataReceivedとErrorDataReceivedイベントハンドラを追加
            p.OutputDataReceived += OutputDataReceived;
            p.ErrorDataReceived += ErrorDataReceived;


            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;

            //EpubCheckを実行する
            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
            p.Close();

            //結果をダイアログに表示する
            var resultDlg = new EpubCheckResultDialog();

            //Outputをコピーする
            foreach (var output in outputLines)
            {
                resultDlg.outputTextBox.Text += (output + "\n");
            }

            //Errorをコピーする
            if (errorLines.Count > 1)   //エラーがあれば
            {
                foreach (var error in errorLines)
                {
                    resultDlg.errorTextBox.Text += (error + "\n");
                }
                ret = false;    //戻り値をエラーありにする。
            }
            else //エラーなし
            {
                resultDlg.errorTextBox.Text = "エラーはありませんでした。";
            }
            checkingDlg.Close();        //EpubCheck実施中のダイアログを閉じる
            resultDlg.ShowDialog();     //EpubCheck結果を表示する

            return (ret);
        }

        //Output出力イベントハンドラ
        //EpubCheckのコンソール出力を取得する
        static void OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //出力された文字列を表示する
            outputLines.Add(e.Data);
        }

        //エラー出力イベントハンドラ
        //EpubCheckのエラー出力を取得する
        static void ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //エラー出力された文字列を取得する
            errorLines.Add(e.Data);
        }

    }
}
