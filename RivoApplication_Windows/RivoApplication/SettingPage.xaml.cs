using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Rivo;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Text;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace RivoApplication
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        static int screenreader = -1;
        private DispatcherTimer dispatcherTimer;
        int L3=0;
        int L4 = 0;
        int L3method = 0;
        int L4method = 0;
        static string initialL3;
        static string initialL4;
        static string initialL3method;
        static string initialL4method;
        static string initialscreenreader;
        public SettingPage()
        {
            this.InitializeComponent();
            dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Tick += new EventHandler<Object>(dispatcherTimer_Tick);
            Init();




        }
        private async void Init() {
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
            var result=await device.GetScreenReader();
            byte[] screenreader=new byte[2];
            Array.Copy(result, 10, screenreader, 0, 2);
            
            
            MainPage root = MainPage.Current;
           
          
            string reader1 = Encoding.UTF8.GetString(screenreader);
            if (reader1 == "11")
                initialscreenreader = "iOS Voice Over";
            if (reader1 == "21")
                initialscreenreader = "Android Talkback";
            else
                initialscreenreader = "watchOS";
            
            Init2();
        }
        private async void Init2() {
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
            var result = await device.GetL3L4Language();
            for (int a = 0; a < result.Length; a++)
                Debug.WriteLine(result[a]);
            byte[] payload = new byte[11];
            Array.Copy(result,8,payload,0,11);
            string realresult = Encoding.UTF8.GetString(payload);
            string l3 = realresult.Substring(0, 2);
            string l3m = realresult.Substring(3,2);
            string l4 = realresult.Substring(6,2);
            string l4m = realresult.Substring(9,2);
            if (l3 == "0") initialL3 = "None";
            if (l3 == "10") initialL3 = "숫자";
            if (l3 == "20") initialL3 = "영어";
            if (l3 == "30") initialL3 = "한글";
            if (l4 == "0") initialL3 = "None";
            if (l4 == "10") initialL4 = "숫자";
            if (l4 == "20") initialL4 = "영어";
            if (l4 == "30") initialL4= "한글";
            if (l3m == "0") initialL3method = "None";
            if (l3m == "21") initialL3method = "EWQ";
            if (l3m == "22") initialL3method = "ABC";
            if (l3m == "31") initialL3method = "리보";
            if (l3m == "32") initialL3method = "나랏글";
            if (l3m == "33") initialL3method = "천지인";
            if (l4m == "0") initialL4method = "None";
            if (l4m == "21") initialL4method = "EWQ";
            if (l4m == "22") initialL4method = "ABC";
            if (l4m == "31") initialL4method = "리보";
            if (l4m == "32") initialL4method = "나랏글";
            if (l4m == "33") initialL4method = "천지인";

            status.Text = "Status: "+initialscreenreader+"," + initialL3 + " " + initialL3method + " " + initialL4 + " " + " " + initialL4method;





        }
        private void dispatcherTimer_Tick(object sender, object e)
        {
            MainPage root = MainPage.Current;
            root.Denotify();
            dispatcherTimer.Stop();


        }
        private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "영어")
                L4 = 20;
            if (text == "숫자")
                L4 = 10;
            if (text == "한글")
                L4 = 30;
            MainPage page = MainPage.Current;
            page.Notify(text);

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
            string passer = screenreader.ToString();
            byte[] topass1 = Encoding.UTF8.GetBytes(passer);
            byte[] topass = new byte[3];
            topass[0] = 0x1;
            Array.Copy(topass1, 0, topass, 1, topass1.Length);
            
           var result=await device.SetScreenReader(topass);
            MainPage page = MainPage.Current;
            page.Notify("Success");
            dispatcherTimer.Start();

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            screenreader = 11;
            MainPage page = MainPage.Current;
        
            dispatcherTimer.Start();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            screenreader = 21;
            MainPage page = MainPage.Current;
        
            dispatcherTimer.Start();
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            screenreader = 12;
            MainPage page = MainPage.Current;
          
            dispatcherTimer.Start();
        }

        private async void Button_Click2(object sender, RoutedEventArgs e)
        {
           
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
            string passer = L3.ToString()+","+L3method.ToString()+","+L4.ToString()+","+L4method.ToString();

            byte[] topass = new byte[passer.Length+1];
            topass[0] = 0x1;
           
            byte[] topass1 = Encoding.UTF8.GetBytes(passer);
            Array.Copy(topass1, 0, topass, 1, topass1.Length);
     
            string result = await device.SetL3L4Language(topass);

            MainPage page = MainPage.Current;
            page.Notify("Language Change Success");
            dispatcherTimer.Start();
            var change = await device.GetL3L4Language();
         
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "영어")
                L3 = 20;
            if (text == "숫자")
                L3 = 10;
            if (text == "한글")
                L3 = 30;
            MainPage page = MainPage.Current;
            page.Notify(text);
            dispatcherTimer.Start();
        }

        private void L3Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "리보")
                L3method = 31;
            if (text == "천지인")
                L3method = 32;
            if (text == "나랏글")
                L3method = 33;
            if (text == "ABC")
                L3method = 22;
            if (text == "EWQ")
                L3method = 21;
            MainPage page = MainPage.Current;
            page.Notify(text);
            dispatcherTimer.Start();
        }

   

        private void L4Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
            if (text == "리보")
                L4method = 31;
            if (text == "천지인")
                L4method = 32;
            if (text == "나랏글")
                L4method = 33;
            if (text == "ABC")
                L4method = 22;
            if (text == "EWQ")
                L4method = 21;
            MainPage page = MainPage.Current;
            page.Notify(text);
            dispatcherTimer.Start();
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
            DateTime time = DateTime.Now;
            string Time = time.ToString();
            string year=Time.Substring(0,4);
            string month = Time.Substring(5,2);
            string date = Time.Substring(8,2);
            string ampm=Time.Substring(10,4);
            byte realhour;
            string hour = null;

            string min = null;
            string sec = null;
            if (Time.Length == 21)
            {
               hour = Time.Substring(14, 1);

                min = Time.Substring(16, 2);
              sec = Time.Substring(19, 2);
            }
            if (Time.Length == 22)
            {
              hour = Time.Substring(14, 2);

              min = Time.Substring(17, 2);
                sec = Time.Substring(20, 2);
            }
           
            realhour = byte.Parse(hour);
            if (ampm.ToString() == " 오후 ")
                realhour += 12;
            short realyear = short.Parse(year);
            byte realmonth = byte.Parse(month);
            byte realdate = byte.Parse(date);
            byte realsec = byte.Parse(sec);
            byte realmin = byte.Parse(min);
            MainPage page = MainPage.Current;
            byte[] topass = new byte[10];
            topass[0] = 0x1;
            topass[1] = 0x0;
            topass[2] = (byte) (realyear & 0xFF00);
            topass[3] = (byte) (realyear & 0xFF);
            topass[4] = realmonth;
            topass[5] = realdate;
            topass[6] = realhour;
            topass[7] = realmin;
            topass[8] = realsec;
            topass[9] = 0;
           var result=await device.SetDateandTime(topass);

            page.Notify("Success");
            dispatcherTimer.Start();


        }
    }

 



}
