using System;
using System.Threading.Tasks;
using System.Windows;
using Makaretu.Dns;

namespace Mirroring_iPhone.AirPlayReceiver
{
    public class AirPlayService
    {
        private MulticastService _mdns;
        private ServiceDiscovery _discovery;

        public async Task StartBroadcasting()
        {
            try
            {
                _mdns = new MulticastService();
                _discovery = new ServiceDiscovery(_mdns);

                // --- 1. AirPlayプロファイル（画面ミラーリング用） ---
                var airplayProfile = new ServiceProfile("MyPC-Mirroring", "_airplay._tcp", 7000);
                airplayProfile.AddProperty("deviceid", "00:11:22:33:44:55");
                airplayProfile.AddProperty("features", "0x5A7FFFF7,0x1E"); // ミラーリング対応フラグ
                airplayProfile.AddProperty("model", "AppleTV3,2");         // AppleTVとして偽装
                airplayProfile.AddProperty("srcvers", "220.68");           // iOSが要求するバージョン情報
                airplayProfile.AddProperty("flags", "0x44");               // 画面転送を許可するフラグ
                airplayProfile.AddProperty("vv", "2");                     // ビデオバージョン
                airplayProfile.AddProperty("pi", "b08f5a79-db29-4384-b456-a4784d9e6055"); // 機器の固有ID
                airplayProfile.AddProperty("pk", "b07727d6f6cd6e08b58ede525ed3c81231f28b49dbb0cb444005b58321e29e92"); // 暗号化用の公開鍵（ダミー）

                // --- 2. RAOPプロファイル（音声転送・必須要件） ---
                var raopProfile = new ServiceProfile("001122334455@MyPC-Mirroring", "_raop._tcp", 7000);
                raopProfile.AddProperty("am", "AppleTV3,2");
                raopProfile.AddProperty("ch", "2");
                raopProfile.AddProperty("cn", "0,1,2,3");
                raopProfile.AddProperty("da", "true");
                raopProfile.AddProperty("et", "0,3,5");
                raopProfile.AddProperty("md", "0,1,2");
                raopProfile.AddProperty("pw", "false");
                raopProfile.AddProperty("sr", "44100");
                raopProfile.AddProperty("ss", "16");
                raopProfile.AddProperty("tp", "UDP");
                raopProfile.AddProperty("vn", "65537");
                raopProfile.AddProperty("vs", "220.68");

                // 広告を登録して発信開始
                _discovery.Advertise(airplayProfile);
                _discovery.Advertise(raopProfile);
                _mdns.Start();

                System.Windows.MessageBox.Show("✅ mDNS広告（完全版）の開始に成功しました！\nコントロールセンターの「画面ミラーリング」を確認してください。", "通信結果");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ mDNSエラーが発生しました:\n{ex.Message}", "エラー");
            }
        }
    }
}
