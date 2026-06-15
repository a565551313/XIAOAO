using System;
using System.IO;
using System.Text;

namespace GameServerApp.Common
{
    public class MMO_MemoryStream : MemoryStream
    {
        public MMO_MemoryStream()
        {

        }

        public MMO_MemoryStream(byte[] buffer) : base(buffer)
        {

        }

        #region Short
        /// <summary>
        /// 从流中读取一个short数据
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            byte[] arr = new byte[2];
            base.Read(arr, 0, 2);
            return BitConverter.ToInt16(arr, 0);
        }

        /// <summary>
        /// 把一个short数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteShort(short value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region UShort
        /// <summary>
        /// 从流中读取一个ushort数据
        /// </summary>
        /// <returns></returns>
        public ushort ReadUShort()
        {
            byte[] arr = new byte[2];
            base.Read(arr, 0, 2);
            return BitConverter.ToUInt16(arr, 0);
        }

        /// <summary>
        /// 把一个ushort数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteUShort(ushort value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region Int
        /// <summary>
        /// 从流中读取一个int数据
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToInt32(arr, 0);
        }

        /// <summary>
        /// 把一个int数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region UInt
        /// <summary>
        /// 从流中读取一个uint数据
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToUInt32(arr, 0);
        }

        /// <summary>
        /// 把一个uint数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt(uint value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region Long
        /// <summary>
        /// 从流中读取一个long数据
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            byte[] arr = new byte[8];
            base.Read(arr, 0, 8);
            return BitConverter.ToInt64(arr, 0);
        }

        /// <summary>
        /// 把一个long数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region ULong
        /// <summary>
        /// 从流中读取一个ulong数据
        /// </summary>
        /// <returns></returns>
        public ulong ReadULong()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToUInt64(arr, 0);
        }

        /// <summary>
        /// 把一个ulong数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteULong(ulong value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region Float
        /// <summary>
        /// 从流中读取一个float数据
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            byte[] arr = new byte[4];
            base.Read(arr, 0, 4);
            return BitConverter.ToSingle(arr, 0);
        }

        /// <summary>
        /// 把一个float数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        #region Double
        /// <summary>
        /// 从流中读取一个double数据
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            byte[] arr = new byte[8];
            base.Read(arr, 0, 8);
            return BitConverter.ToDouble(arr, 0);
        }

        /// <summary>
        /// 把一个double数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            byte[] arr = BitConverter.GetBytes(value);
            base.Write(arr, 0, arr.Length);
        }
        #endregion

        public string ReadASCIIString()
        {

            ushort len = this.ReadUShort();
            byte[] textAscii = new byte[len];
            base.Read(textAscii, 0, len);


            //将字节转化成汉字

            return Encoding.Default.GetString(textAscii);


        }


        public void WriteASCIIString(string str)
        {

            //这里我们将采用2字节一个汉字的方法来取出汉字的16进制码
            byte[] textbuf = Encoding.Default.GetBytes(str);
            string textAscii = string.Empty;//用来存储转换过后的ASCII码
            for (int i = 0; i < textbuf.Length; i++)
            {
                textAscii += textbuf[i].ToString("X");
            }
            string textStr = string.Empty;
            int k = 0;//字节移动偏移量
            byte[] buffer = new byte[textAscii.Length / 2];//存储变量的字节
            for (int i = 0; i < textAscii.Length / 2; i++)
            {
                //每两位合并成为一个字节
                buffer[i] = byte.Parse(textAscii.Substring(k, 2), System.Globalization.NumberStyles.HexNumber);
                k = k + 2;
            }
            WriteUShort((ushort)buffer.Length);
            base.Write(buffer, 0, buffer.Length);

        }
        #region Bool
        /// <summary>
        /// 从流中读取一个bool数据
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return base.ReadByte() == 1;
        }

        /// <summary>
        /// 把一个bool数据写入流
        /// </summary>
        /// <param name="value"></param>
        public void WriteBool(bool value)
        {
            base.WriteByte((byte)(value == true ? 1 : 0));
        }
        #endregion

        #region UTF8String
        /// <summary>
        /// 从流中读取一个sting数组
        /// </summary>
        /// <returns></returns>
        public string ReadUTF8String()
        {

            ushort len = this.ReadUShort();
            byte[] arr = new byte[len];
            base.Read(arr, 0, len);
            return Encoding.UTF8.GetString(arr);

        }

        /// <summary>
        /// 把一个string数据写入流
        /// </summary>
        /// <param name="str"></param>
        public void WriteUTF8String(string str)
        {
            byte[] arr = Encoding.UTF8.GetBytes(str);
            if (arr.Length > 65535)
            {
                throw new InvalidCastException("字符串超出范围");
            }
            WriteUShort((ushort)arr.Length);
            base.Write(arr, 0, arr.Length);
        }
        public string ReadDefaultString(int len)
        {

            // ushort len = this.ReadUShort();
            byte[] arr = new byte[len];
            base.Read(arr, 0, len);
            return Encoding.Default.GetString(arr);
        }

        /// <summary>
        /// 把一个string数据写入流
        /// </summary>
        /// <param name="str"></param>
        public void WriteDefaultString(string str)
        {
            byte[] arr = Encoding.Default.GetBytes(str);
            if (arr.Length > 65535)
            {
                throw new InvalidCastException("字符串超出范围");
            }
            WriteUShort((ushort)arr.Length);
            base.Write(arr, 0, arr.Length);
        }
        #endregion
    }
}