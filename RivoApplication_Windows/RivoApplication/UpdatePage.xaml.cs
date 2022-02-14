using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace RivoApplication
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.

    /// </summary>
    public sealed partial class UpdatePage : Page
    {
        static GattCharacteristic writer = MainPage.Current.writerName();
        static GattCharacteristic reader = MainPage.Current.readerName();
        static GattCharacteristic datawriter = MainPage.Current.dataWriterName();
        static BLEDevice device = new BLEDevice(writer, reader, datawriter);
        static BluetoothLEDevice devicename = MainPage.Current.bleDeviceName();

        byte[] verify = new byte[5];
        byte[] end = new byte[2];
        string filename = "3.2.0";
        public UpdatePage()
        {

            this.InitializeComponent();
  
        }
    
        public static byte[] intToFrame(byte[] bufferToSend, int index, int value)
        {
            bufferToSend[index] = (byte)(value & 0xff);
            bufferToSend[index + 1] = (byte)((value >> 8) & 0xff);
            bufferToSend[index + 2] = (byte)((value >> 16) & 0xff);
            bufferToSend[index + 3] = (byte)((value >> 24) & 0xff);

            return bufferToSend;
        }
        public static byte[] shortToFrame(byte[] bufferToSend, int index, short value)
        {
            bufferToSend[index] = (byte)(value & 0xff);
            bufferToSend[index + 1] = (byte)((value >> 8) & 0xff);


            return bufferToSend;
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {            
            byte[] mr3bin = new System.Net.WebClient().DownloadData("https://rivo.me/app/MR3.bin");
            byte[] bt = new System.Net.WebClient().DownloadData("https://rivo.me/app/bt.bin");
            byte[] boot = new System.Net.WebClient().DownloadData("https://rivo.me/app/MR3boot.bin");
           
            byte[] realfilename = Encoding.UTF8.GetBytes(filename);
            int buflength = realfilename.Length + 12;
            int crc=(int)device.FileCRC(boot);
            byte[] updatestart = new byte[buflength];
          
            updatestart[0] = 0x0;
            updatestart[1] = 0x2;
            intToFrame(updatestart,2,boot.Length);
            intToFrame(updatestart,6,crc);
            shortToFrame(updatestart,10,(short)realfilename.Length);
            Array.Copy(realfilename,0,updatestart,12,realfilename.Length);
            var result = await device.UpdateStart(updatestart);
            MainPage root = MainPage.Current;
            root.Notify("Update Starting");
            UpdateStatus.IsActive = true;
            int totalbytesent = 0;
            short seqnum = 0;
            
            
            while (true)
            {
                
                
                if (totalbytesent >= boot.Length)
                    break;
         

                short tobecopy = (short)Math.Min(100,boot.Length-totalbytesent);
                byte[] data = new byte[tobecopy];
                byte[] dataframe = new byte[tobecopy + 7];
               Array.Copy(boot, totalbytesent,data,0, tobecopy);
                short datacrc=(short)device.CRC16(data);
                totalbytesent += tobecopy;
                dataframe[0] = 0x1;
                shortToFrame(dataframe, 1, seqnum);
                shortToFrame(dataframe, 3, datacrc);
                shortToFrame(dataframe, 5, tobecopy);
                Array.Copy(data,0,dataframe,7,data.Length);
                Debug.WriteLine("seqnum: "+seqnum);
                var results = await device.UpdateData(dataframe);
                seqnum++;
            }
            root.Notify("Update Complete");
            
            
            
            verify[0] = 0x2;
            intToFrame(verify, 1, crc);
            
            var verifyresult = await device.VerifyData(verify);
            byte[] recvframe = verifyresult;
            while (recvframe.Length != 12 && recvframe[6] != 2)
            {
               
                recvframe = await device.VerifyData(verify);
                Thread.Sleep(1000);
            }



         
        
            Thread.Sleep(10000);
       
           
            end[0] = 0x3;
            end[1] = 0x0;
            var ends = await device.UpdateEnd(end);
            recvframe = ends;
            while (recvframe.Length != 12 && recvframe[6] != 2)
            {
                
                recvframe = await device.UpdateEnd(end);
                Thread.Sleep(1000);
            }



          
        
            Thread.Sleep(10000);
        


            updatestart[1] = 0x3;
            crc = (int)device.FileCRC(mr3bin);
            intToFrame(updatestart, 2, mr3bin.Length);
            intToFrame(updatestart, 6, crc);
            shortToFrame(updatestart, 10, (short)realfilename.Length);
            Array.Copy(realfilename, 0, updatestart, 12, realfilename.Length);
        
            var startmr3bin=await device.UpdateStart(updatestart);
             totalbytesent = 0;
             seqnum = 0;

       
            while (true)
            {


                if (totalbytesent >= mr3bin.Length)
                    break;


                short tobecopy = (short)Math.Min(100, mr3bin.Length - totalbytesent);
                byte[] data = new byte[tobecopy];
                byte[] dataframe = new byte[tobecopy + 7];
                Array.Copy(mr3bin, totalbytesent, data, 0, tobecopy);
                short datacrc = (short)device.CRC16(data);
                totalbytesent += tobecopy;
                dataframe[0] = 0x1;
                shortToFrame(dataframe, 1, seqnum);
                shortToFrame(dataframe, 3, datacrc);
                shortToFrame(dataframe, 5, tobecopy);
                Array.Copy(data, 0, dataframe, 7, data.Length);
                Debug.WriteLine("seqnum: " + seqnum);
                var results = await device.UpdateData(dataframe);
                seqnum++;
            }
            root.Notify("Update Complete");
          
            

            verify[0] = 0x2;
            intToFrame(verify, 1, crc);
            
            var verifyresult1 = await device.VerifyData(verify);
            while (recvframe.Length != 12 || recvframe[6] != 2)
            {
                recvframe = await device.VerifyData(verify);
                Thread.Sleep(1000);
            }
        
            end[0] = 0x3;
            end[1] = 0x3;
            var ends1 = await device.UpdateEnd(end);
            var verifyresult2 = await device.UpdateEnd(end);
            while (recvframe.Length != 12 || recvframe[6] != 3)
            {
              
                recvframe = await device.UpdateEnd(end);
                Thread.Sleep(1000);
            }
    

       
            UpdateStatus.IsActive = false;
        }
        private async void CharacteristicValue2_Changed(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            
            byte[] recvframe = await device.readPacket();
            while (recvframe[6] != 3 || recvframe.Length != 12)
            {
                
                recvframe = await device.readPacket();
                Thread.Sleep(1000);
            }



            for (int a = 0; a < recvframe.Length; a++)
             
            return;
        }
        



        private async void Button_Click1(object sender, RoutedEventArgs e)
        {
            byte[] bt = new System.Net.WebClient().DownloadData("https://rivo.me/app/bt.bin");
            byte[] realfilename = Encoding.UTF8.GetBytes(filename);
            int buflength = realfilename.Length + 12;
            int crc = (int)device.FileCRC(bt);
            byte[] updatestart = new byte[buflength];
         
            updatestart[0] = 0x0;
            updatestart[1] = 0x2;
            intToFrame(updatestart, 2, bt.Length);
            intToFrame(updatestart, 6, crc);
            shortToFrame(updatestart, 10, (short)realfilename.Length);
            Array.Copy(realfilename, 0, updatestart, 12, realfilename.Length);
            var result = await device.UpdateStart(updatestart);
            MainPage root = MainPage.Current;
            root.Notify("Update Starting");
            int totalbytesent = 0;
            short seqnum = 0;


            while (true)
            {


                if (totalbytesent >= bt.Length)
                    break;


                short tobecopy = (short)Math.Min(100, bt.Length - totalbytesent);
                byte[] data = new byte[tobecopy];
                byte[] dataframe = new byte[tobecopy + 7];
                Array.Copy(bt, totalbytesent, data, 0, tobecopy);
                short datacrc = (short)device.CRC16(data);
                totalbytesent += tobecopy;
                dataframe[0] = 0x1;
                shortToFrame(dataframe, 1, seqnum);
                shortToFrame(dataframe, 3, datacrc);
                shortToFrame(dataframe, 5, tobecopy);
                Array.Copy(data, 0, dataframe, 7, data.Length);
             
                var results = await device.UpdateData(dataframe);
                seqnum++;
            }
            root.Notify("Update Complete");
         


            verify[0] = 0x2;
            intToFrame(verify, 1, crc);

            var verifyresult = await device.VerifyData(verify);
            byte[] recvframe = verifyresult;
            while (recvframe.Length != 12 && recvframe[6] != 2)
            {

                recvframe = await device.VerifyData(verify);
                Thread.Sleep(1000);
            }



          
            Thread.Sleep(10000);


            end[0] = 0x3;
            end[1] = 0x3;
            var ends = await device.UpdateEnd(end);
            recvframe = ends;
            while (recvframe.Length != 12 && recvframe[6] != 2)
            {

                recvframe = await device.UpdateEnd(end);
                Thread.Sleep(1000);
            }



       
            UpdateStatus.IsActive = false;
         
            Thread.Sleep(10000);
           




        }



        private async void Button_Click5(object sender, RoutedEventArgs e)
        {
            UpdateStatus.IsActive = true;
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] raw = wc.DownloadData("https://rivo.me/app/version.app");
            MainPage page = MainPage.Current;
            page.Notify(Encoding.UTF8.GetString(raw));
            string webData = System.Text.Encoding.UTF8.GetString(raw);
            string firmware = webData.Substring(5,5);
            string BT = webData.Substring(23,5);
            var result = await device.GetFirmwareVersion();
            string firm = Encoding.UTF8.GetString(result);
            string myfirm = firm.Substring(13,5);
            string mybt = firm.Substring(22, 5);
            filename = myfirm;
        
            if (myfirm != firmware)
            {
                Update.Visibility = Visibility.Visible;


            }
            if (mybt != BT)
                UpdateBT.Visibility = Visibility.Visible;
            UpdateStatus.IsActive = false;

        }
    }
}
