using System.Windows;
using System.Windows.Controls;

namespace SakuraEpubLibrary
{
    //後処理設定ダイアログ
    public partial class PostProcessSettingDialog : Window
    {
        public PostProcessSettingDialog()
        {
            InitializeComponent();

            //現在の設定値を読み込む
            javaPathTextBox.Text = PostProcess.javaPath;
            ePubCheckPathTextBox.Text = PostProcess.ePubCheckPath;
            KindePreviewerPathTextBox.Text = PostProcess.kindlePreViewerPath;
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
            
        }
        //OKが押されたので設定値を更新する
        private void OkClick(object sender, RoutedEventArgs e)
        {
            //設定値を更新する
            PostProcess.javaPath=javaPathTextBox.Text;
            PostProcess.ePubCheckPath=ePubCheckPathTextBox.Text;
            PostProcess.kindlePreViewerPath=KindePreviewerPathTextBox.Text;

            //デフォルト値を更新する
            PostProcess.SaveDefaults();

            //ダイアログを閉じる
            Close();
        }

        //キャンセルされたので、設定値を更新せずに閉じる
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
