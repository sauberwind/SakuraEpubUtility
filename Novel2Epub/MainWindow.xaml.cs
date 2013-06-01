using System.Windows;
using System.Windows.Controls;
using System.IO;
using Novel2Epub.Properties;
using System.Threading;
using System.Threading.Tasks;
using ArchiveEpub;
namespace Novel2Epub
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //テキストボックスに前回値を入れる
            title.Text = Settings.Default.titleDefault;
            author.Text = Settings.Default.authorDefault;
            novel.Text = Settings.Default.novelFileDefault;
            publisher.Text = Settings.Default.publisherDefault;
            javaPathTextBox.Text = Settings.Default.javaPathDefault;
            EpubCheckPathTextBox.Text = Settings.Default.epubCheckPathDefault;

            UpdateGenerateEpubStatus();
        }

        //EPUB生成ボタンが押された
        private async void GenerateEPUB(object sender, RoutedEventArgs e)
        {
            //仮コード
            var opt = new ConvertOptions();
            opt.hasTag = false;
            opt.isSpaceIndented = false;

            var novelFile = "";
            var epubDir = "";



            //ステータスダイアログを表示する
            var statusDialog = new StatusDialog();
            statusDialog.Show();
            
            //入力値を確認する
            statusDialog.status.Text = "入力値を確認しています。";
            await Task.Run(() => Thread.Sleep(3000));

            //テンプレートを確認する
            statusDialog.status.Text = "テンプレートを確認しています。";
            EpubArchiver.CheckEpubDir(epubDir);
            
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



            if (false)
            {
                //設定を保存する
                Settings.Default.titleDefault = title.Text;
                Settings.Default.authorDefault = author.Text;
                Settings.Default.novelFileDefault = novel.Text;
                Settings.Default.publisherDefault = publisher.Text;
                Settings.Default.javaPathDefault = javaPathTextBox.Text;
                Settings.Default.epubCheckPathDefault = EpubCheckPathTextBox.Text;

                Properties.Settings.Default.Save();
            }
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
