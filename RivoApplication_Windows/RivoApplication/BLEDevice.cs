﻿ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Rivo;
using System.Diagnostics;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace RivoApplication
    {
    public class BLEDevice : RivoDevice
       {
        GattCharacteristic reader;
        GattCharacteristic writer;
        GattCharacteristic datawriter;
        public BLEDevice(GattCharacteristic writer, GattCharacteristic reader,GattCharacteristic datawriter) {
            this.reader = reader;
            this.writer = writer;
            this.datawriter = datawriter;
        }
        public BLEDevice(GattCharacteristic writer, GattCharacteristic reader)
        {
            this.reader = reader;
            this.writer = writer;
            
        }
        public BLEDevice() { }


        public override async Task WritePacket(byte[] sendData)
        {
          
           
            IBuffer buffer = sendData.AsBuffer();
            try
            {
                var result = await writer.WriteValueWithResultAsync(buffer);
            
                if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    MainPage page = MainPage.Current;
                    page.Notify("Connect Again");
                }
            }
            catch { }
            
        }
        public override async Task WriteDataPacket(byte[] sendData)
        {
            IBuffer buffer = sendData.AsBuffer();
        
            var result = await datawriter.WriteValueWithResultAsync(buffer);
            Thread.Sleep(1);
           
        }
        public override async Task<byte[]> readPacket()
        {
           
              
                GattReadResult buffer = await reader.ReadValueAsync();
               
                byte[] data;

                CryptographicBuffer.CopyToByteArray(buffer.Value, out data);
               
                    
                var readata = Encoding.UTF8.GetString(data);
               
                return data;
            
      
           

          
        }

        public override Task<byte[]> ReadAndWrite(byte[] sendData)
        {
            throw new NotImplementedException();
        }
    }

}
