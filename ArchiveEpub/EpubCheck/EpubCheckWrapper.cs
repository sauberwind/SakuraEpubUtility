using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveEpub
{
    public class EpubCheckWrapper
    {
        string javaPath;        //Java実行ファイルのパス
        string epubCheckPath;   //EpubCheckのパス
        string epubPath;        //EPUBファイルのパス
        List<string> outputLines;   //EpubCheckのOutput;
        List<string> errorLines;    //EpubCheckのエラー

        public EpubCheckWrapper(string java, string epubCheck, string epub)
        {
            javaPath = java;
            epubCheckPath = epubCheck;
            epubPath = epub;

            outputLines = new List<string>();
            errorLines = new List<string>();
        }

        //CheckEpubを実施、Output/Errorを表示する
        public void CheckEpub()
        {
            //EPUBチェック実施中パネルを表示する
            var checkingDlg = new EpubCheckingDialog();
            checkingDlg.Show();


            var p = new System.Diagnostics.Process();

            //実行ファイル
            p.StartInfo.FileName = javaPath;

            //引数
            var args = "-jar "
                        + "\"" + epubCheckPath + "\" "
                        + " \"" + epubPath + "\"";
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

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.WaitForExit();
            p.Close();

            var resultDlg = new EpubCheckResultDialog();

            //Outputをコピーする
            foreach (var output in outputLines)
            {
                resultDlg.outputTextBox.Text += (output + "\n");
            }

            //Errorをコピーする
            if (errorLines.Count > 1)
            {
                foreach (var error in errorLines)
                {
                    resultDlg.errorTextBox.Text += (error + "\n");
                }
            }
            else
            {
                resultDlg.errorTextBox.Text = "エラーはありませんでした。";
            }
            checkingDlg.Close();
            resultDlg.ShowDialog();
        }

        //Output出力イベントハンドラ
        void OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //出力された文字列を表示する
            outputLines.Add(e.Data);
        }

        //エラー出力イベントハンドラ
        void ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //エラー出力された文字列を取得する
            errorLines.Add(e.Data);
        }

    }
}
