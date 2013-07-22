using SakuraEpubUtility.Properties;
using System;
using System.IO;
using System.Threading.Tasks;
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

            LoadDefaults();                 //テキストボックスに前回起動時の値を読み込む

            //後処理のデフォルト値を読み込む
            UpdatePostProcessEnableStatus();                //後処理可否を描画に反映する
            if (PostProcess.isEnableEpubCheck() == true)    //EpubCheck可能なら
            {
                useEpubCheck.IsChecked = true;              //EpubCheckする
            }
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

                //後処理設定を読み込む
                PostProcess.executeEpubCheck = useEpubCheck.IsChecked;              //EpubCheck
                PostProcess.executeKindePreViewer = execKindlePreviewer.IsChecked;  //KindlePreviewer

                var isEpubGen = ePubDoc.GenerateEpubDocument();

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

            Properties.Settings.Default.Save();
        }

        //EPUB生成ボタンの実行可否を更新する
        void UpdateGenerateEpubStatus()
        {
            bool isEnable = true;   //仮にEPUB生成可能とする

            isEnable &= (title.Text.Length > 0);    //タイトルが入力済みか
            isEnable &= (author.Text.Length > 0);   //著者名が入力済みか
            isEnable &= File.Exists(novel.Text);    //小説ファイルが存在するか?
            isEnable &= File.Exists(cover.Text);    //カバー画像ファイルが存在するか

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
