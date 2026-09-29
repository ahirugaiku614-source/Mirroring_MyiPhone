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

        private bool _isServerRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            // LibVLC（動画再生エンジン）の初期化
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            // 画面のコントロール（VlcPlayer）に再生エンジンを紐付ける
            VlcPlayer.MediaPlayer = _mediaPlayer;

            
            this.Unloaded += MainWindow_Unloaded;
        }

        private void ToggleMirroringButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isServerRunning)
            {
                StartMirroringServer();
            }
            else
            {
                StopMirroringServer();
            }
        }

        private async void StartMirroringServer()
        {

            try
            {

                StopMirroringServer();

                //  UxPlayを「画面なし・データ転送モード」で起動
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

                //  自作アプリ内のVLCプレイヤーで、UxPlayからの映像ストリームを受信開始
                // ※UxPlayが標準で配信するネットワークアドレス（RTSPプロトコルなど）を指定

                using (var media = new Media(_libVLC, new Uri("rtsp://127.0.0.1:7000/stream")))
                {
                    _mediaPlayer.Play(media);
                }

                //UxPlayがポートを開設して通信準備が整うまで1.5秒ほど待機する
                await System.Threading.Tasks.Task.Delay(1500);

                // VLCでストリームの受信用準備
                using (var media = new Media(_libVLC, new Uri("rtsp://127.0.0.1:7000/stream")))
                {
                    _mediaPlayer.Play(media);
                }

                _isServerRunning = true;
                ToggleMirroringButton.Content = "ミラーリング停止 (信号オフ)";
                ToggleMirroringButton.Background = System.Windows.Media.Brushes.Crimson;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"起動エラー: {ex.Message}");
            }



        }

        private void StopMirroringServer()
        {
            _mediaPlayer?.Stop();

            try
            {
                if (_uxPlayProcess != null && !_uxPlayProcess.HasExited)
                {
                    _uxPlayProcess.Kill();
                    _uxPlayProcess.Dispose();
                    _uxPlayProcess = null;
                }
            }
            catch { }

            _isServerRunning = false;
            ToggleMirroringButton.Content = "ミラーリング開始 (信号送信)";
            ToggleMirroringButton.Background = new System.Windows.Media.BrushConverter().ConvertFromString("#007ACC") as System.Windows.Media.Brush;
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