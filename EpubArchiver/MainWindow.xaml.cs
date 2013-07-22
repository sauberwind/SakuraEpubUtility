using System.Windows;
using System.Windows.Controls;
using SakuraEpubLibrary;

namespace SakuraEpubUtility
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //対象EPUBディレクトリのデフォルト値を読み込む
            ePubDirPathTextBox.Text = Properties.Settings.Default.epubDirDefault;

            //後処理のデフォルト値を読み込む
            UpdatePostProcessEnableStatus();                //後処理可否を描画に反映する
            if (PostProcess.isEnableEpubCheck() == true)    //EpubCheck可能なら
            {
                useEpubCheck.IsChecked = true;              //EpubCheckする
            }
        }
        //ファイルがドラッグされてきたらうける
        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        //ファイルがドロップされたらテキストボックスに表示する
        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                var textBox = sender as TextBox;
                textBox.Text = files[0];
            }
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateGenerateEpubStatus();
        }
        private void GenerateEpub_Click(object sender, RoutedEventArgs e)
        {
            //ボタンの表示をEpub生成中にする
            var btn = sender as Button;
            btn.IsEnabled = false;
            btn.Content = "EPUBを生成しています";

            //EPUBのディレクトリをデフォルト値に設定する
            Properties.Settings.Default.epubDirDefault = ePubDirPathTextBox.Text;
            Properties.Settings.Default.Save();

            //後処理設定を読み込む
            PostProcess.executeEpubCheck = useEpubCheck.IsChecked;              //EpubCheck
            PostProcess.executeKindePreViewer = execKindlePreviewer.IsChecked;  //KindlePreviewer

            //Epubを生成する
            var srcDir = ePubDirPathTextBox.Text;
            var dstFile = srcDir + ".epub";
            Archiver.ArchiveEpubWithPostProcess(srcDir,dstFile);

            //ボタンの表示を戻す
            btn.IsEnabled = true;
            btn.Content = "EPUBを生成する";
        }

        //後処理設定ボタン
        private void PostProcessSettingClick(object sender, RoutedEventArgs e)
        {
            //後処理設定ダイアログを開く
            var postProcessSettingDialog = new PostProcessSettingDialog();
            postProcessSettingDialog.ShowDialog();

            //後処理の実行可否を決定する
            UpdatePostProcessEnableStatus();
        }

        //後処理の実施可否を更新する
        void UpdatePostProcessEnableStatus()
        {
            //EpubCheckの実施可否
            var epubCheckEnable = PostProcess.isEnableEpubCheck();
            if (epubCheckEnable != true)    //EpubCheckが実行できないなら
            {
                useEpubCheck.IsChecked = false;     //実施しない
                useEpubCheck.IsEnabled = false;     //Disable
            }
            else    //EpubCheck実施可能なら
            {
                useEpubCheck.IsEnabled = true;
            }

            //KindlePreviewerの実施可否
            var kpvEnable = PostProcess.isEnableExecuteKindlePrevier();
            if (kpvEnable != true)    //Kindle Previewerが実行できないなら
            {
                execKindlePreviewer.IsChecked = false;    //実施しない
                execKindlePreviewer.IsEnabled = false;    //Disable
            }
            else    //実施可能なら
            {
                execKindlePreviewer.IsEnabled = true;
            }
        }
    }
}
