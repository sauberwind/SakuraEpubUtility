using SakuraEpubLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EpubReToucher
{
    public partial class MainWindow : Window
    {
        List<string> tempDirsToDelete;
        string tempDir = "";        //Epubを解凍したテンポラリディレクトリ
        string epubFile;            //編集前のEPUBファイル
        string orgSource = "";      //編集前のソースの内容
        string targetSource = "";   //編集対象のソースコード
        ViewModel vm;

        public MainWindow()
        {
            vm = new ViewModel();       //InitializeComponentでエディタが変更される=vm参照されるため宣言を先に行う。
            InitializeComponent();
            this.DataContext = vm;
            vm.sourceChanged = false;

            //後処理のデフォルト値を読み込む
            UpdatePostProcessEnableStatus();                //後処理可否を描画に反映する
            if (PostProcess.isEnableEpubCheck() == true)    //EpubCheck可能なら
            {
                useEpubCheck.IsChecked = true;              //EpubCheckする
            }
            ePubGenBtn.IsEnabled = false;                   //仮に生成できない状態とする
            tempDirsToDelete = new List<string>();          //削除対象ディレクトリを初期化
        }

        //ファイルリストのアイテムがダブルクリックされた。エディタ要素に表示する
        private void OnSelectTextFile(object sender, MouseButtonEventArgs e)
        {
            if (vm.sourceChanged == true)   //ソースが変更されているならば保存するか確認する。
            {
                var dlgRet = MessageBox.Show("ソースが変更されています。保存して次のソースを表示しますか？", "EpubReToucher", MessageBoxButton.YesNoCancel);
                if (dlgRet == MessageBoxResult.Cancel) //キャンセルならなにもせず終了
                {
                    return;
                }
                else if (dlgRet == MessageBoxResult.Yes)   //保存する
                {
                    EditorSaveClick(null, null);
                }
                else    //保存せずに次に進む
                {
                }
            }
            //選択されているアイテムを取得する
            var listBox = sender as ListBox;
            var selectedFile = listBox.SelectedItem.ToString();             //選択されたテキストファイルを取得する
            selectedFile = Path.Combine(tempDir, selectedFile);             //ファイル名をフルパスにする
            SetToSourceEditor(selectedFile);                                //エディタエリアに表示する
        }

        //エディタエリアにファイルを表示する
        private void SetToSourceEditor(string sourceFile)
        {
            fileLabel.Content = "file:" + epubFile;                     //ファイル名をラベルに書き込む

            var enc = XMLUtilities.GetEncodingFormat(sourceFile);       //エンコードを取得する
            string str = File.ReadAllText(sourceFile, enc);             //ファイルを読み込む

            orgSource = str;                                            //変更前ソースとして保持する
            sourceEditor.Text = str;                                    //エディタに表示する
            targetSource = sourceFile;                                  //変更先として保存する
        }

        //エディタにユーザが書き込むごとに呼び出される
        private void SourceChanged(object sender, TextChangedEventArgs e)
        {
            var sourceTextBox = sender as TextBox;
            var str = sourceTextBox.Text;           //編集後のソースを取得する
            if (orgSource.Equals(str) != true)      //ソースが変更されていれば
            {
                vm.sourceChanged = true;
            }
            else
            {
                vm.sourceChanged = false;
            }
        }

        //エディタのCancelボタンが押下された。編集結果を破棄する
        private void EditorCancelClick(object sender, RoutedEventArgs e)
        {
            //変更を破棄するか確認する
            var ret = MessageBox.Show("変更を破棄しますか?", "EpubReToucher", MessageBoxButton.YesNo);
            if (ret == MessageBoxResult.Yes)    //破棄するなら
            {
                vm.sourceChanged = false;   //ソース変化なし
                orgSource = "";             //変更前ソースをクリア
                sourceEditor.Text = "";     //エディタエリアをクリア
                targetSource = "";          //編集対象ソースをクリア
            }
        }
        //エディタの保存ボタンが押下されたのでファイルを上書きする
        private void EditorSaveClick(object sender, RoutedEventArgs e)
        {
            vm.sourceChanged = false;                                   //ソース変化なし
            orgSource = sourceEditor.Text;                              //現在のソースを変更前に保存する
            var enc = XMLUtilities.GetEncodingFormat(targetSource);     //対象のソースのエンコードを取得する
            System.IO.File.WriteAllText(targetSource, orgSource, enc);  //ファイルを上書きする
        }

        //EPUBファイルを書き出す
        private void ArchiveEpubClick(object sender, RoutedEventArgs e)
        {
            //ボタンの表示をEpub生成中にする
            var btn = sender as Button;
            btn.IsEnabled = false;
            btn.Content = "EPUBを生成しています";

            //バックアップファイルを作成する
            var backFileName = Path.ChangeExtension(epubFile, ".bak");
            if (File.Exists(backFileName) == true)  //バックアップファイルが既にあったので削除する
            {
                File.Delete(backFileName);
            }
            File.Move(epubFile, backFileName);  //Epubファイルをバックアップファイルにする

            //Epubファイルを書き出す
            PostProcess.executeEpubCheck = useEpubCheck.IsChecked;              //後処理を設定する
            PostProcess.executeKindePreViewer = execKindlePreviewer.IsChecked;
            
            //Epubファイルを出力する
            Archiver.ArchiveEpubWithPostProcess(tempDir, epubFile);

            //ボタンの表示を戻す
            btn.IsEnabled = true;
            btn.Content = "EPUBを生成する";
        }

        //編集対象Epubファイルのテキストボックス
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

        //EPUBファイルがドロップされたらテキストボックスに表示し、読み込む
        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                var textBox = sender as TextBox;
                var fileName = files[0];

                //拡張子がepubでなければエラーメッセージを出して終了する
                if (Path.GetExtension(fileName).ToLower().Equals(".epub") != true)
                {
                    MessageBox.Show("EPUBファイルではありません");
                    return;
                }
                
                //Epubファイルなので読み込みを行う
                epubFile = fileName;        //対象ファイルとして設定する
                textBox.Text = files[0];    //TextBoxに表示する

                tempDir = Unpacker.UnpackEpubToTemp(epubFile);              //Epubをテンポラリフォルダに解凍する
                tempDirsToDelete.Add(tempDir);                              //削除対象に追加

                var pacDoc = ContainerXML.GetPackageDocument(tempDir);      //パッケージ文書を取得する
                var textItems = PackageDocument.GetTextItems(pacDoc);       //Epub内のテキストファイルを取得する
                textItems = textItems.OrderBy(i => i).ToList();             //ソートする

                //ファイルをリストボックスに追加する
                fileLists.Items.Clear();                //現在のアイテムを削除する
                foreach (var item in textItems)
                {
                    fileLists.Items.Add(item.Remove(0, tempDir.Length + 1));   //相対パス部分だけを表示する
                }

                ePubGenBtn.IsEnabled = true;    //Epub生成ボタンを動作可能にする
            }
        }

        //後処理設定ボタン
        void PostProcessSettingClick(object sender, RoutedEventArgs e)
        {
            //後処理設定ダイアログを開く
            var postProcessSettingDialog = new PostProcessSettingDialog();
            postProcessSettingDialog.ShowDialog();
        }

        //
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

        //プログラム終了・テンポラリディレクトリを削除する
        private void OnClosed(object sender, System.EventArgs e)
        {

            var existTempDirs = tempDirsToDelete.Distinct() //重複を削除する
                                                .Where(d => Directory.Exists(d) == true);   //存在するディレクトリ
            try
            {
                foreach (var dir in existTempDirs)
                {
                    Directory.Delete(dir,true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("テンポラリディレクトリの削除に失敗しました");
            }
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        //ソース変更の有無・save/cancelに使用する
        bool _sourceChanged;
        public bool sourceChanged
        {
            get
            {
                return (_sourceChanged);
            }
            set
            {
                _sourceChanged = value;
                NotifyPropertyChanged("sourceChanged");
            }
        }
        //表示更新
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
