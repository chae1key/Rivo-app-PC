﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class BatteryPage : Page
    {
        public BatteryPage()
        {
            this.InitializeComponent();
        }

        private void Battery_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(MainPage.Current.bleDeviceName().DeviceId);
            GattCharacteristic writer = MainPage.Current.writerName();
            GattCharacteristic reader = MainPage.Current.readerName();
            BLEDevice device = new BLEDevice(writer, reader);
             await device.FindMyRivo();
       
        }
    }
}
