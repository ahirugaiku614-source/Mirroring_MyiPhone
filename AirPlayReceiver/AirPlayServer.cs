using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics; // ログ出力用

namespace Mirroring_iPhone.AirPlayReceiver
{
    public class AirPlayServer
    {
        public void Start()
        {
            Task.Run(async () => {
                TcpListener listener = new TcpListener(IPAddress.Any, 7000);
                listener.Start();
                Debug.WriteLine("=== AirPlayサーバー 待受開始 (Port 7000) ===");

                while (true)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        Debug.WriteLine("\n[+] iPhoneからの接続をキャッチしました！");
                        _ = HandleClientAsync(client);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[!] 待受エラー: {ex.Message}");
                    }
                }
            });
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                char[] buffer = new char[4096];

                // iPhoneとの会話を途切れないようにループで待ち受ける
                while (client.Connected)
                {
                    int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // iPhone側から切断されたら終了

                    string request = new string(buffer, 0, bytesRead);
                    Debug.WriteLine($"\n--- 📱 iPhoneからの送信 ---\n{request}");

                    // 通信の順番（CSeq）をiPhoneからのメッセージから読み取る
                    string cseq = "1";
                    Match match = Regex.Match(request, @"CSeq:\s*(?<cseq>\d+)");
                    if (match.Success)
                    {
                        cseq = match.Groups["cseq"].Value;
                    }

                    // iPhoneへ「届いているよ（200 OK）」と決まり文句の返事をする
                    string response = $"RTSP/1.0 200 OK\r\n" +
                                      $"CSeq: {cseq}\r\n" +
                                      $"Server: AirTunes/220.68\r\n\r\n";

                    await writer.WriteAsync(response);
                    Debug.WriteLine($"--- 💻 PCからの返信 ---\nRTSP/1.0 200 OK (CSeq: {cseq}) を返しました。");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[!] 通信エラー: {ex.Message}");
            }
            finally
            {
                client.Close();
                Debug.WriteLine("[-] iPhoneとの通信を切断しました。");
            }
        }
    }
}
