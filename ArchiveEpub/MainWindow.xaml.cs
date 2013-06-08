using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace SakuraEpubUtility
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //テキストボックスに前回値を入れる
            ePubDirPathTextBox.Text=Properties.Settings.Default.epubDirDefault;
            javaPathTextBox.Text=Properties.Settings.Default.javaPathDefault;
            EpubCheckPathTextBox.Text = Properties.Settings.Default.epubCheckPathDefault;

            UpdateGenerateEpubStatus();
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
            UpdateGenerateEpubStatus();
        }

        //EPUB生成ボタンの実行可否を更新する
        void UpdateGenerateEpubStatus()
        {
            bool isEnable = true;   //仮にEPUB生成可能とする

            //Epubにするディレクトリが存在するか？
            isEnable &=Directory.Exists(ePubDirPathTextBox.Text);

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
            GenerateEpub.IsEnabled = isEnable;
        
        }

        //EpubCheckを使用するチェックボックスのイベントハンドラ
        private void useEpubCheck_Click(object sender, RoutedEventArgs e)
        {
            UpdateGenerateEpubStatus();
        }

        async private void GenerateEpub_Click(object sender, RoutedEventArgs e)
        {
            GenerateEpub.IsEnabled = false; //ボタンを押せなくする

            //設定を保存する
            Properties.Settings.Default.epubDirDefault = ePubDirPathTextBox.Text;
            Properties.Settings.Default.javaPathDefault = javaPathTextBox.Text;
            Properties.Settings.Default.epubCheckPathDefault = EpubCheckPathTextBox.Text;

            Properties.Settings.Default.Save();
            var srcDir = ePubDirPathTextBox.Text;
            var dstDir = Path.GetDirectoryName(srcDir);             //親ディレクトリを取得
            var dstFileName = Path.GetFileName(srcDir) + ".epub";   //ファイル名はディレクトリ名.epub
            var dstFile = Path.Combine(dstDir, dstFileName);

            if (File.Exists(dstFile)==true)   //既にファイルが存在していたら
            {
                var result = MessageBox.Show("上書きしますか?", "質問", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) //上書きするなら
                {
                    File.Delete(dstFile);
                }
                else
                {
                    GenerateEpub.IsEnabled = true;  //押せるように戻す
                    return;

                }
            }

            //アーカイブする
            await EpubArchiver.ArchiveEpub(srcDir, dstFile);

            //EpubCheck
            if (useEpubCheck.IsChecked == true)   //EpubCheckありなら
            {
                var epubChecker = new EpubCheckWrapper(javaPathTextBox.Text, EpubCheckPathTextBox.Text, dstFile);
                epubChecker.CheckEpub();
            }
            else
            {
                MessageBox.Show("EPUBが生成されました");
            }
            GenerateEpub.IsEnabled = true;  //押せるように戻す
        }
    }
}
