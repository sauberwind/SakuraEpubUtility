using System.Windows;
using System.Windows.Controls;
using System.IO;
using SakuraEpubUtility.Properties;
using System.Threading;
using System.Threading.Tasks;
using SakuraEpubUtility;
namespace SakuraEpubUtility
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //テキストボックスに前回値を入れる
            title.Text = Settings.Default.titleDefault;                 //タイトル
            author.Text = Settings.Default.authorDefault;               //著者
            publisher.Text = Settings.Default.publisherDefault;         //発行元

            cover.Text = Settings.Default.coverDefault;                 //カバー
            novel.Text = Settings.Default.novelFileDefault;             //本文

            javaPathTextBox.Text = Settings.Default.javaPathDefault;            //java
            EpubCheckPathTextBox.Text = Settings.Default.epubCheckPathDefault;  //EpubCheck

            UpdateGenerateEpubStatus();
        }

        //EPUB生成ボタンが押された
        private async void GenerateEPUB(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;     //ボタンをdisableにする
            btn.IsEnabled = false;

            //仮コード
            var opt = new ConvertOptions();
            opt.hasTag = false;
            opt.isSpaceIndented = false;


            //ステータスダイアログを表示する
            var statusDialog = new StatusDialog();
            statusDialog.Show();

            //
            //EPUBを作成する
            //
            var ePubDoc = new EpubDocument();
            
            //入力データを反映する
            ePubDoc.title = title.Text;             //タイトル
            ePubDoc.author = author.Text;           //著者
            ePubDoc.publiser = publisher.Text;      //本文
            ePubDoc.coverImagePath = cover.Text;    //表紙画像

            //テンプレートを確認する
            statusDialog.status.Text = "テンプレートを確認しています。";

            //EpubArchiver.CheckEpubDir(epubDir);

            await Task.Run(() => Thread.Sleep(3000));

            //小説ファイルを変換する
            statusDialog.status.Text = "小説ファイルを変換しています。";
            await Task.Run(() => Thread.Sleep(3000));

            //Epubファイルを作成する
            statusDialog.status.Text = "Epubファイルを作成しています。";
            await Task.Run(() => Thread.Sleep(3000));

            //EpubCheckを実行する
            statusDialog.status.Text = "EpubCheckを実行しています。";
            await Task.Run(() => Thread.Sleep(3000));

            statusDialog.Close();



            //設定を保存する
            Settings.Default.titleDefault = title.Text;
            Settings.Default.authorDefault = author.Text;
            Settings.Default.publisherDefault = publisher.Text;

            Settings.Default.novelFileDefault = novel.Text;
            Settings.Default.coverDefault = cover.Text;
            Settings.Default.javaPathDefault = javaPathTextBox.Text;
            Settings.Default.epubCheckPathDefault = EpubCheckPathTextBox.Text;

            Properties.Settings.Default.Save();

            btn.IsEnabled = true;   //ボタンをEnableに戻す
        }

        //EpubCheckを使用するチェックボックスのイベントハンドラ
        private void useEpubCheck_Click(object sender, RoutedEventArgs e)
        {
            UpdateGenerateEpubStatus();
        }
        //EPUB生成ボタンの実行可否を更新する
        void UpdateGenerateEpubStatus()
        {
            bool isEnable = true;   //仮にEPUB生成可能とする

            isEnable &= (title.Text.Length > 0);    //タイトルが入力済みか
            isEnable &= (author.Text.Length > 0);   //著者名が入力済みか
            isEnable &= File.Exists(novel.Text);    //小説ファイルが存在するか?
            isEnable &= File.Exists(cover.Text);    //カバー画像ファイルが存在するか


            //Epubチェックを使用するなら
            if (useEpubCheck.IsChecked == true)
            {
                //javaファイルが存在するか
                isEnable &= File.Exists(javaPathTextBox.Text);

                //ファイル名がjava.exeであるか
                var javaFileName = Path.GetFileName(javaPathTextBox.Text).ToLower();
                isEnable &= javaFileName.Equals("java.exe");

                //EpubCheckファイルが存在するか
                isEnable &= File.Exists(EpubCheckPathTextBox.Text);

                var epubCheckExt = Path.GetExtension(EpubCheckPathTextBox.Text).ToLower();
                isEnable &= epubCheckExt.Equals(".jar");
            }
            //for Debug
            GenerateEpub.IsEnabled = isEnable;
            GenerateEpub.IsEnabled = true;
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
        //テキストボックスが更新された
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateGenerateEpubStatus(); //EPUB生成可否を判定する
        }
    }
}
