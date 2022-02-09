using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//getmtusize 함수 에뮬레이터로 작동하는지까지 확인하기

namespace Rivo
{




    public abstract class RivoDevice
    {

        int mtu = 20;
        bool mtuconfirmed = false;



        public RivoDevice()
        {

        }

        public static long byteToInt(byte[] bytes)
        {

            long newValue = 0;
            newValue |= (((long)bytes[0]) << 24) & 0xFF000000;
            newValue |= (((long)bytes[1]) << 16) & 0xFF0000;
            newValue |= (((long)bytes[2]) << 8) & 0xFF00;
            newValue |= (((long)bytes[3])) & 0xFF;
            return newValue;

        }

        public uint FileCRC(byte[] data)
    {
         uint crc=0xFFFFFFFF;

        
            for (uint i = 0; i<data.Length; i++)
            {
            crc = crc ^ data[i];
                for (uint j = 8; j > 0; j--)
                    {
                    crc = (crc >> 1) ^ (0xEDB88320U & ((Convert.ToBoolean(crc & 1)) ? 0xFFFFFFFF : 0));
                     }
            }
         return ~crc;
    }

       public ushort CRC16(byte[] data)
        {
            ushort crc = 0xFFFF;

            for (uint i = 0; i < data.Length; i++)
            {
                crc = (ushort)((byte)(crc >> 8) | (crc << 8));
                crc ^= data[i];
                crc ^= (ushort)((byte)(crc & 0xFF) >> 4);
                crc ^= (ushort)((crc << 8) << 4);
                crc ^= (ushort)(((crc & 0xFF) << 4) << 1);
            }
            return crc;
        }


        public bool CRC16_CHECK(byte[] data)
        {
            if (data[6] == 0x87)
            {
                Console.WriteLine("CRC Error");
                return false;
            }

            int bufferlen = data.Length - 10;

            byte[] realData = new byte[bufferlen];
            Array.Copy(data, 6, realData, 0, bufferlen);
            ushort crc = CRC16(realData);
            bufferlen += 6;
            ushort realCrc = 0;

            realCrc |= (ushort)((data[bufferlen++]) & 0xFF);
            realCrc |= (ushort)((data[bufferlen++] << 8) & 0xFF00);

            if (crc == realCrc)
            {
                return true;
                Console.WriteLine("CRC Checking == TRUE");
            }
            else
            {
                Debug.WriteLine("crc wrong"+ realCrc+"   "+crc);
                return false;
                Console.WriteLine("CRC Checking == FALSE");
            }
        }

        public bool recvFrameCheck(byte[] recvFrame, int recvSize, string id)
        {
            byte[] utf8bytes = System.Text.Encoding.UTF8.GetBytes(id);

            if (!(recvFrame[0] == (byte)'a' &&
                          recvFrame[1] == (byte)'t' &&
                          recvFrame[2] == (byte)id[0] &&
                          recvFrame[3] == (byte)id[1] &&
                          recvFrame[recvSize - 2] == 0x0d &&
                          recvFrame[recvSize - 1] == 0x0a))
            {
                Debug.WriteLine("at0d0aiswrong");
                return false;
            }
            else
            {

                if (CRC16_CHECK(recvFrame) == true) return true;
                else
                {
                    Debug.WriteLine("crc is wrong");
                    return false; }
            }
        }

        public byte[] composeSendframe(string id, byte[] data)
        {
            byte[] utf8bytes = System.Text.Encoding.UTF8.GetBytes(id);

            int totalLength = data.Length + 10;
            byte[] frame = new byte[totalLength];
            int i = 0;
            frame[i++] = (byte)'A';
            frame[i++] = (byte)'T';
            frame[i++] = (utf8bytes[0]);
            frame[i++] = (utf8bytes[1]);
            frame[i++] = (byte)(data.Length); // assume little endian
            frame[i++] = (byte)(data.Length >> 8);
            data.CopyTo(frame, i);
            i += data.Length;
            ushort crc = CRC16(data);
            frame[i++] = (byte)(crc);
            frame[i++] = (byte)(crc >> 8);
            frame[i++] = 0x0d;
            frame[i++] = 0x0a;
            //totalLength = i;

            return frame;
        }



        public abstract Task<byte[]> ReadAndWrite(byte[] sendData); //베이스로직은여기에

        public virtual async Task WritePacket(byte[] sendData)
        {

        }
        public virtual async Task WriteDataPacket(byte[] sendData)
        {
        }


        //베이스로직은여기에
        public virtual async Task<byte[]> readPacket()
        {
            // byte[] array = null;
            return null;
        }//베이스로직은여기에





        async Task<byte[]> SendAndReceive(string id, byte[] data)
        {

            /*  if (!mtuconfirmed)
              {
                  mtu = await GetMTUSize();//try 
                  mtuconfirmed = true;

              }
              */

            byte[] sendframe = composeSendframe(id, data);
            Debug.WriteLine("hi");
            //int length = recvFrame[4] + recvFrame[5] * 256 - 2;

            int position;
            int framesize;
            int sendSize;
            bool tobreak = false;
            for (int count = 0; count < 3; count++)
            {
                if (tobreak == true)
                    break;
                position = 0;
                framesize = sendframe.Length;


                //todo write 
                mtu = 125;
                Debug.WriteLine(framesize);
               
                    while (position < framesize)
                {


                    sendSize = Math.Min(mtu, framesize - position);
                    byte[] senddata = new byte[100];
                    Array.Copy(sendframe, position, senddata, 0, sendSize);
                   
                    await WritePacket(senddata);//Array.copy()

                    Thread.Sleep(1000);
                    position += sendSize;

                }
                if (id == "UM" && sendframe[6] == 2)
                    Thread.Sleep(30000);
                if (id == "UM" && sendframe[6] == 3)
                    Thread.Sleep(30000);
                byte[] recvframe = await readPacket();
                while (recvframe.Length <= 6)
                {
                    await WritePacket(sendframe);
                    recvframe = await readPacket();
                }
                int len = 1;
                
                if (recvframe[0] == (byte)'a' && recvframe[1] == (byte)'t')
                {

                    len = recvframe[4] + (recvframe[5] * 256) + 10;
                    Debug.WriteLine("length: " + len);
                    while (recvframe.Length < len)
                    {
                        await WritePacket(sendframe);
                        byte[] temp = await readPacket();
                        Array.Copy(temp, 0, recvframe,0, temp.Length);

                    }
                }

                int recvSize = recvframe.Length;


                if (recvFrameCheck(recvframe, recvSize, id))
                {
                    Debug.WriteLine("Nice frame");
                    //  byte opcode = recvframe[6];
                    byte result = recvframe[7];

                    if (result != 0)
                    {
                        throw new Exception("Result code =" + result);
                    }

                    return recvframe;


                }

                else
                {
                    Thread.Sleep(100);
                    Debug.WriteLine("Bad frame" + id);
                }


            }

            throw new Exception("Retry failed");
        }

        async Task<byte[]> SendAndReceive2(string id, byte[] data)
        {

            /*  if (!mtuconfirmed)
              {
                  mtu = await GetMTUSize();//try 
                  mtuconfirmed = true;

              }
              */

            byte[] sendframe = composeSendframe(id, data);
            Debug.WriteLine("hi");
            //int length = recvFrame[4] + recvFrame[5] * 256 - 2;

            int position;
            int framesize;
            int sendSize;
            bool tobreak = false;
            for (int count = 0; count < 3; count++)
            {
                if (tobreak == true)
                    break;
                position = 0;
                framesize = sendframe.Length;


                //todo write 
                mtu = 125;
                Debug.WriteLine(framesize);
        

                await WriteDataPacket(sendframe);//Array.copy()



                byte[] recvframe = await readPacket();
                while (recvframe.Length <=6 )
                {
                    await WriteDataPacket(sendframe);
                    recvframe = await readPacket();
                }
                int len = 1;
                if (recvframe[0] == (byte)'a' && recvframe[1] == (byte)'t')
                {
                 
                        len = recvframe[4] + (recvframe[5] * 256) + 10;
                  
                    Debug.WriteLine("length: " + len);
                    while (recvframe.Length < len )
                    {
                        Debug.WriteLine("sliced recvframelength:"+recvframe.Length+"reallen:"+len);
                        await WriteDataPacket(sendframe);
                        byte[] temp = await readPacket();
                        if (temp.Length >= len)
                            break;
                      
                    }
                }

             
        
                      
            

                int recvSize = recvframe.Length;


                if (recvFrameCheck(recvframe, recvSize, id))
                {
                    Debug.WriteLine("Nice frame");
                    //  byte opcode = recvframe[6];
                    byte result = recvframe[7];

                    if (result != 0)
                    {
                        throw new Exception("Result code =" + result);
                    }

                    byte[] temp2 = new byte[recvframe.Length];
                    Debug.WriteLine("recvlength: " + recvframe.Length);
                    Array.Copy(recvframe, 8, temp2, 0, len - 12);
                    tobreak = true;
                    return temp2;


                }

                else
                {
                    Thread.Sleep(100);
                    Debug.WriteLine("Bad frame" + id);
                }


            }

            throw new Exception("Retry failed");
        }

      



        //FV,LN,SR,VG,RN,IF,RV,MT

        public async Task<byte[]> GetFirmwareVersion()
        {
            var result = await SendAndReceive("FV", new byte[] { 0x0 });
            return result;
        }
        public async Task<byte[]> SetDateandTime(byte[] passed)
        {
            var result = await SendAndReceive("DT", passed);
            return result;
        }
        public async Task<string> SetL3L4Language(byte[] passed)
        {
            string str = "asdf";
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            byte[] resByte = { 0x1, 0x0, 0x0, 0x0, 0x0 };

            Array.Copy(StrByte, 0, resByte, 0, 4);

            var result = await SendAndReceive("LN", passed);

            return System.Text.Encoding.Default.GetString(result);
        }
        public async Task<byte[]> SetScreenReader(byte[] passer)
        {
            var result = await SendAndReceive("SR", passer);
            return result;
        }
        public async Task<byte[]> FindMyRivo()
        {
            byte[] result = await SendAndReceive("RV", new byte[] { 0x0, 0x0 });

            return result;
        }

        public async Task<UInt16> GetMTUSize()
        {

            int framesize;
            for (int count = 0; count < 3; count++)
            {
                //framesize = data.Length + 10;

                byte[] sendframe = composeSendframe("MT", new byte[] { 0x0 });

                await WritePacket(sendframe);
                var client = new UdpClient();


                byte[] recvframe = await readPacket();

                int len;

                int recvSize = recvframe.Length;
                if (recvframe[0] == (byte)'a' && recvframe[1] == (byte)'t')
                {
                    recvFrameCheck(recvframe, recvSize, "MT");
                    len = recvframe[4] + recvframe[5] * 256 + 10;
                    Debug.WriteLine(recvframe[4] + " " + recvframe[5]);
                    byte[] temp = { };
                    Debug.WriteLine(recvframe.Length + "len: " + len);
                    while (recvframe.Length < len)
                    {

                        temp = await readPacket();
                        Array.Copy(temp, 0, recvframe, recvframe.Length, temp.Length - 1);

                    }
                    int mtu = recvframe[8] + recvframe[9] * 256;
                    Debug.WriteLine(recvframe[8] + recvframe[9] * 256);
                    return (UInt16)mtu;
                }

            }
            throw new Exception("exception");
        }

        public async Task<byte[]> GetRivoInfo()
        {
            var result = await SendAndReceive("IF", new byte[] { 0x0 });
            return result;
        }



        public async Task<byte[]> GetRivoStatus()
        {
            var result = await SendAndReceive("RS", new byte[] { 0x0 });
            return result;
        }
        public async Task<byte[]> GetRivoName()
        {
            var result = await SendAndReceive("RN", new byte[] { 0x0 });
            return result;
        }
        public async Task<byte[]> SetRivoName(byte[] newname)
        {
            var result = await SendAndReceive("RN", newname);
            return result;
        }




        public async Task<byte[]> UpdateStart(byte[] passer)
        {
            var result = await SendAndReceive("UM", passer);
            return result;
        }
        public async Task<byte[]> UpdateData(byte[] dataframe)
        {
            var result = await SendAndReceive2("UM", dataframe);
            return result;
        }
        public async Task<byte[]> VerifyData(byte[] topass)
        {
            Debug.WriteLine("verifying");
            var result = await SendAndReceive("UM", topass);
            return result;
        }
        public async Task<byte[]> UpdateEnd(byte[] topass)
        {
            Debug.WriteLine("Update Ending;");
          
            var result = await SendAndReceive("UM", topass);
            return result;
        }


    }

}