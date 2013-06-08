using SakuraEpubUtility.Properties;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace SakuraEpubUtility
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //テキストボックスに前回起動時の値を読み込む
            LoadDefaults();

            UpdateGenerateEpubStatus();
        }

        //テキストボックスに前回起動時の値を入れる
        private void LoadDefaults()
        {

            title.Text = Settings.Default.titleDefault;                 //タイトル
            author.Text = Settings.Default.authorDefault;               //著者
            publisher.Text = Settings.Default.publisherDefault;         //発行元

            cover.Text = Settings.Default.coverDefault;                 //カバー
            novel.Text = Settings.Default.novelFileDefault;             //本文

            javaPathTextBox.Text = Settings.Default.javaPathDefault;            //java
            EpubCheckPathTextBox.Text = Settings.Default.epubCheckPathDefault;  //EpubCheck
        }

        //EPUB生成ボタンが押された
        private async void GenerateEPUB(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;     //ボタンをdisableにする
            btn.IsEnabled = false;
            try
            {
                //
                //EPUBを作成する
                //

                //テンプレートを確認する
                btn.Content = "テンプレートを確認しています。";
                await Task.Run(() => EpubDocument.CheckEpubTemplate());

                //メタデータを取得する
                var ePubDoc = new EpubDocument();
                var metaData = new EpubMetaData();
                metaData.title = title.Text;                            //タイトル
                metaData.author = author.Text;                          //著者
                metaData.publisher = publisher.Text;                    //出版社
                metaData.isRightToLeft = (bool)isVertical.IsChecked;    //縦書きであれば右→左
                metaData.isVertical = (bool)isVertical.IsChecked;       //縦書きか？
                ePubDoc.metaData = metaData;
                
                //ファイル情報を取得する
                ePubDoc.coverImageFileName = cover.Text;    //表紙画像
                ePubDoc.novelFileName = novel.Text;         //本文

                //テキストフォーマットを取得する
                var opt = new ConvertOptions();
                opt.hasTag = (bool)hasTag.IsChecked;                    //修飾タグの有無
                opt.isSpaceIndented = (bool)isSpaceIndented.IsChecked;  //インデントがスペースか？

                if (isPlaneText.IsChecked == true)                      //プレーンテキスト
                {
                    opt.format = TextFormat.PLAIN_TEXT;
                }
                else if (isHeaddedText.IsChecked == true)               //*でヘッダを示すテキスト
                {
                    opt.format = TextFormat.PLAIN_TEXT_WITH_HEADER;
                }
                else                                                    //XHTML
                {
                    opt.format = TextFormat.XHTML;
                }
                ePubDoc.opt = opt;

                //生成処理実行
                btn.Content = "EPUBを作成しています";
                var isEpubGen = await ePubDoc.GenerateEpubDocument();

                if((isEpubGen==true)&&( useEpubCheck.IsChecked==true))  //EpubCheckありなら
                {
                    btn.Content = "EpubCheckを実施しています";
                    var epubFileName = ePubDoc.GetEpubFileName();
                    var epubChecker = new EpubCheckWrapper(javaPathTextBox.Text, EpubCheckPathTextBox.Text, epubFileName);
                    epubChecker.CheckEpub();
                }
                //設定を保存する
                SaveDefaults();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            btn.IsEnabled = true;   //ボタンをEnableに戻す
            btn.Content = "EPUB3ファイルを生成する";
        }

        //テキストボックスの値をセーブする
        private void SaveDefaults()
        {
            Settings.Default.titleDefault = title.Text;
            Settings.Default.authorDefault = author.Text;
            Settings.Default.publisherDefault = publisher.Text;

            Settings.Default.novelFileDefault = novel.Text;
            Settings.Default.coverDefault = cover.Text;
            Settings.Default.javaPathDefault = javaPathTextBox.Text;
            Settings.Default.epubCheckPathDefault = EpubCheckPathTextBox.Text;

            Properties.Settings.Default.Save();
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
            GenerateEpub.IsEnabled = isEnable;
            if (isEnable == true)   //入力値がValidであればsaveする
            {
                SaveDefaults();
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
        //テキストボックスが更新された
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateGenerateEpubStatus(); //EPUB生成可否を判定する
        }
    }
}
