using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zeroconf;

namespace Mirroring_iPhone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //画面がロードされたらAirPlayの通知を開始する
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                StartAirPlayAdvertisement();
                StatusText.Text = "iPhoneの「画面ミラーリング」を開いて確認してください...";
            }
            catch (Exception ex)
            {
                StatusText.Text = "エラーが発生しました: " + ex.Message;
            }
        }

        private void StartAirPlayAdvertisement()
        {
            string MacAddress = GetMacAddress();
            if(string.IsNullOrEmpty(MacAddress))
            {
                throw new Exception("MACアドレスの取得に失敗しました。");
            }

            
        }

        //MACアドレスを取得するメソッド
        private string GetMacAddress()
        {   //パソコンにあるすべてのネットワーク機器（カード）をリストアップ
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                //通信可能なネットワーク機器で、ループバックでないものを選択
                if (nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }

            return string.Empty;
        }


    }
}