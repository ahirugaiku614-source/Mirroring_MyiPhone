using System.Diagnostics;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zeroconf;
using System.IO;
using LibVLCSharp.Shared;

namespace Mirroring_iPhone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            // 1. VLCエンジンの初期化
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            // 2. XAMLに配置した「VlcPlayer」にこのプレイヤーを結びつける
            VlcPlayer.MediaPlayer = _mediaPlayer;

            // 3. アプリが閉じたら安全に解放する設定
            this.Unloaded += MainWindow_Unloaded;

            // 💡 テスト用（あとでAirPlayストリームのURLに書き換えます）
            // StartStreaming("rtsp://サンプルURL");
        }

        public void StartStreaming(string url)
        {
            // 映像ストリームを受け取って再生を開始するメソッド
            var media = new Media(_libVLC, new Uri(url));
            _mediaPlayer.Play(media);
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            // メモリを綺麗に解放する
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}