using System;
using System.Diagnostics;
using System.Windows;
using LibVLCSharp.Shared;

namespace Mirroring_iPhone
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private Process _uxPlayProcess;

        public MainWindow()
        {
            InitializeComponent();

            // LibVLC（動画再生エンジン）の初期化
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            // 画面のコントロール（VlcPlayer）に再生エンジンを紐付ける
            VlcPlayer.MediaPlayer = _mediaPlayer;

            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. UxPlayを「画面なし・データ転送モード」で起動
            // -nh: 本体の画面（ウィンドウ）を作らない
            // -asink dummy -vsink dummy: 映像と音声をPC画面に出さず、内部処理に回す
             string uxPlayPath = System.IO.Path.Combine(
                 AppDomain.CurrentDomain.BaseDirectory,
                "uxplay",
                "uxplay-windows.exe"
             );

            _uxPlayProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = uxPlayPath,
                    Arguments = "-nh -asink dummy -vsink dummy",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _uxPlayProcess.Start();

            // 2. 自作アプリ内のVLCプレイヤーで、UxPlayからの映像ストリームを受信開始
            // ※UxPlayが標準で配信するネットワークアドレス（RTSPプロトコルなど）を指定します
            // 一般的なUxPlayの配信ストリーム、またはlocalhostのミラーリングポートをキャッチします
            using (var media = new Media(_libVLC, new Uri("rtsp://127.0.0.1:7000/stream")))
            {
                _mediaPlayer.Play(media);
            }
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            // アプリ終了時にVLCとUxPlayを安全に解放・終了する
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();

            try
            {
                if (_uxPlayProcess != null && !_uxPlayProcess.HasExited)
                {
                    _uxPlayProcess.Kill();
                    _uxPlayProcess.Dispose();
                }
            }
            catch { }
        }
    }
}