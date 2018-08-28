/*
 * 北风之神SOCKET框架(ZYSocket)
 *  Borey Socket Frame(ZYSocket)
 *  by luyikk@126.com QQ:547386448
 *  Updated 2012-07-18 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;



namespace ZYSocket.share
{
   
   


    /// <summary>
    /// 数据包读取类
    /// （此类的功能是讲通讯数据包重新转换成.NET 数据类型）
    /// </summary>
    public class ReadBytes
    {

        public static BuffFormatType ObjFormatType { get; set; }

        static ReadBytes()
        {

            ObjFormatType = BuffFormatType.protobuf;

        }


        protected int current;

        public byte[] Data { get; set; }

        protected int startIndex;
        protected int endlengt;

        /// <summary>
        /// 额外处理是否调用成功，可以判断是否解密成功
        /// </summary>
        public bool IsDataExtraSuccess { get; set; }

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int Length { get; set; }


        public static Encoding Encode { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 当前其位置
        /// </summary>
        public int Postion
        {
            get
            {
                return current;
            }

            set
            {               
                Interlocked.Exchange(ref current, value);
            }
        }

        public virtual void Reset()
        {
            current = 0;
        }


        public ReadBytes()
        {
                    
        }

        public ReadBytes(Byte[] data)
        {
            Data = data;
            this.Length = Data.Length;
            current = 0;
            IsDataExtraSuccess = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex">需要载入数据额外处理的开始位置</param>
        /// <param name="length">需要载入数据额外处理的数据长度 -1为，开始INDEX到结束位置,-2就是保留最后1位</param>
        ///  <param name="dataExtraCallBack"> 数据包在读取前需要额外的处理回调方法。（例如加密，解压缩等）</param>
        public ReadBytes(Byte[] data, int startIndex, int length, Func<byte[],byte[]> dataExtraCallBack)
        {
            try
            {
                this.startIndex = startIndex;
                this.Length = data.Length;
                if (length < 0)
                {
                    endlengt = (data.Length + length + 1) - startIndex;

                }
                else
                {
                    endlengt = length;
                }

                byte[] handBytes = new byte[this.startIndex];

                unsafe
                {
                    fixed (byte* datap = &data[0])
                    fixed (byte* handbytesp = &handBytes[0])
                        Buffer.MemoryCopy(datap, handbytesp, handBytes.Length, handBytes.Length);
                    //Buffer.BlockCopy(data, 0, handBytes, 0, handBytes.Length); //首先保存不需要解密的数组

                    byte[] endBytes = new byte[data.Length - (startIndex + endlengt)];

                    fixed (byte* datap = &data[startIndex + endlengt])
                    fixed (byte* endbytesp = &endBytes[0])
                        Buffer.MemoryCopy(datap, endbytesp, endBytes.Length, endBytes.Length);
                    //Buffer.BlockCopy(data, (startIndex + endlengt), endBytes, 0, endBytes.Length); //首先保存不需要解密的数组

                    byte[] NeedExByte = new byte[endlengt];

                    fixed (byte* datap = &data[startIndex])
                    fixed (byte* needbytesp = &NeedExByte[0])
                        Buffer.MemoryCopy(datap, needbytesp, NeedExByte.Length, NeedExByte.Length);
                    //Buffer.BlockCopy(data, startIndex, NeedExByte, 0, NeedExByte.Length);

                    if (dataExtraCallBack != null)
                    NeedExByte = dataExtraCallBack(NeedExByte);

                    Data = new byte[handBytes.Length + NeedExByte.Length + endBytes.Length]; //重新整合解密完毕后的数据包

                    fixed (byte* handbytep = &handBytes[0])
                    fixed (byte* Databytep = &Data[0])
                        Buffer.MemoryCopy(handbytep, Databytep, Data.Length, handBytes.Length);
                    //Buffer.BlockCopy(handBytes, 0, Data, 0, handBytes.Length);

                    fixed (byte* needexbytep = &NeedExByte[0])
                    fixed (byte* Databytep = &Data[handBytes.Length])
                        Buffer.MemoryCopy(needexbytep, Databytep, Data.Length, NeedExByte.Length);
                    //Buffer.BlockCopy(NeedExByte, 0, Data, handBytes.Length, NeedExByte.Length);

                    fixed (byte* endbytesp = &endBytes[0])
                    fixed (byte* Databytep = &Data[handBytes.Length + NeedExByte.Length])
                        Buffer.MemoryCopy(endbytesp, Databytep, Data.Length, endBytes.Length);
                    // Buffer.BlockCopy(endBytes, 0, Data, (handBytes.Length + NeedExByte.Length), endBytes.Length);
                }
                current = 0;
                IsDataExtraSuccess = true;
            }
            catch
            {
                IsDataExtraSuccess = false;
            }
        }


#region return 整数
        /// <summary>
        /// 读取内存流中的头2位并转换成整型
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual short ReadInt16()
        {
            short values = BitConverter.ToInt16(Data, current);
            current = Interlocked.Add(ref current, 2);
            return values;
        }

        /// <summary>
        /// 读取内存流中的头2位并转换成无符号整型
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual ushort ReadUint16()
        {
            ushort values = BitConverter.ToUInt16(Data, current);
            current = Interlocked.Add(ref current, 2);
            return values;
        }


        /// <summary>
        /// 读取内存流中的头4位并转换成整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual int ReadInt32()
        {

            int values = BitConverter.ToInt32(Data, current);
            current = Interlocked.Add(ref current, 4);
            return values;

        }

        /// <summary>
        /// 读取内存流中的头4位并转换成无符号整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual uint ReadUInt32()
        {

            uint values = BitConverter.ToUInt32(Data, current);
            current = Interlocked.Add(ref current, 4);
            return values;

        }


        /// <summary>
        /// 读取内存流中的头8位并转换成长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual long ReadInt64()
        {

            long values = BitConverter.ToInt64(Data, current);
            current = Interlocked.Add(ref current, 8);
            return values;

        }


        /// <summary>
        /// 读取内存流中的头8位并转换成无符号长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual ulong ReadUInt64()
        {

            ulong values = BitConverter.ToUInt64(Data, current);
            current = Interlocked.Add(ref current, 8);
            return values;

        }

        /// <summary>
        /// 读取内存流中的首位
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual byte ReadByte()
        {

            byte values = (byte)Data[current];
            current = Interlocked.Increment(ref current);
            return values;

        }

#endregion

#region 整数
        /// <summary>
        /// 读取内存流中的头2位并转换成整型
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadInt16(out short values)
        {

            try
            {
                values = BitConverter.ToInt16(Data, current);
                current = Interlocked.Add(ref current, 2);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }

        /// <summary>
        /// 读取内存流中的头2位并转换成无符号整型
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public virtual bool ReadUint16(out ushort values)
        {

            try
            {
                values = BitConverter.ToUInt16(Data, current);
                current = Interlocked.Add(ref current, 2);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }


        /// <summary>
        /// 读取内存流中的头4位并转换成整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual bool ReadInt32(out int values)
        {
            try
            {
                values = BitConverter.ToInt32(Data, current);
                current = Interlocked.Add(ref current, 4);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }

        /// <summary>
        /// 读取内存流中的头4位并转换成无符号整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual bool ReadUInt32(out uint values)
        {
            try
            {
                values = BitConverter.ToUInt32(Data, current);
                current = Interlocked.Add(ref current, 4);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }


        /// <summary>
        /// 读取内存流中的头8位并转换成长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual bool ReadInt64(out long values)
        {
            try
            {
                values = BitConverter.ToInt64(Data, current);
                current = Interlocked.Add(ref current, 8);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }


        /// <summary>
        /// 读取内存流中的头8位并转换成无符号长整型
        /// </summary>
        /// <param name="ms">内存流</param>
        /// <returns></returns>
        public virtual bool ReadUInt64(out ulong values)
        {
            try
            {
                values = BitConverter.ToUInt64(Data, current);
                current = Interlocked.Add(ref current, 8);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }

        /// <summary>
        /// 读取内存流中的首位
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadByte(out byte values)
        {
            try
            {
                values = (byte)Data[current];
                current = Interlocked.Increment(ref current);
                return true;
            }
            catch
            {
                values = 0;
                return false;
            }
        }

#endregion
        
#region return 浮点数


        /// <summary>
        /// 读取内存流中的头4位并转换成单精度浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual float ReadFloat()
        {
            float values = BitConverter.ToSingle(Data, current);
            current = Interlocked.Add(ref current, 4);
            return values;
        }


        /// <summary>
        /// 读取内存流中的头8位并转换成浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual double ReadDouble()
        {

            double values = BitConverter.ToDouble(Data, current);
            current = Interlocked.Add(ref current, 8);
            return values;

        }


#endregion

#region 浮点数


        /// <summary>
        /// 读取内存流中的头4位并转换成单精度浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadFloat(out float values)
        {

            try
            {
                values = BitConverter.ToSingle(Data, current);
                current = Interlocked.Add(ref current, 4);
                return true;
            }
            catch
            {
                values = 0.0f;
                return false;
            }
        }


        /// <summary>
        /// 读取内存流中的头8位并转换成浮点数
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadDouble(out double values)
        {

            try
            {
                values = BitConverter.ToDouble(Data, current);
                current = Interlocked.Add(ref current, 8);
                return true;
            }
            catch
            {
                values = 0.0;
                return false;
            }
        }


#endregion

#region return 布尔值
        /// <summary>
        /// 读取内存流中的头1位并转换成布尔值
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadBoolean()
        {

            bool values = BitConverter.ToBoolean(Data, current);
            current = Interlocked.Add(ref current, 1);
            return values;

        }

#endregion

#region 布尔值
        /// <summary>
        /// 读取内存流中的头1位并转换成布尔值
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadBoolean(out bool values)
        {

            try
            {
                values = BitConverter.ToBoolean(Data, current);
                current = Interlocked.Add(ref current, 1);
                return true;
            }
            catch
            {
                values = false;
                return false;
            }
        }

#endregion
        
#region  return 字符串
        /// <summary>
        /// 读取内存流中一段字符串
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public string ReadString()
        {

            int lengt = ReadInt32();

            if (lengt == 0)
                return "";

            byte[] buf = new byte[lengt];

            unsafe
            {
                fixed (byte* datap = &Data[current])
                fixed (byte* bufp = &buf[0])
                    Buffer.MemoryCopy(datap, bufp, buf.Length, buf.Length);
                //Buffer.BlockCopy(Data, current, buf, 0, buf.Length);
            }
            string values = Encode.GetString(buf, 0, buf.Length);

            current = Interlocked.Add(ref current, lengt);

            return values;

        }
#endregion

#region 字符串
        /// <summary>
        /// 读取内存流中一段字符串
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadString(out string values)
        {
          
            try
            {
                if (ReadInt32(out int lengt))
                {
                    if (lengt == 0)
                    {
                        values = "";
                        return true;
                    }

                    byte[] buf = new byte[lengt];

                    unsafe
                    {
                        fixed (byte* datap = &Data[current])
                        fixed (byte* bufp = &buf[0])
                            Buffer.MemoryCopy(datap, bufp, buf.Length, buf.Length);
                        //Buffer.BlockCopy(Data, current, buf, 0, buf.Length);
                    }                  

                    values = Encode.GetString(buf, 0, buf.Length);

                    current = Interlocked.Add(ref current, lengt);

                    return true;

                }
                else
                {
                    values = "";
                    return false;
                }
            }
            catch
            {
                values = "";
                return false;
            }

        }
#endregion


#region return  数据

        public virtual byte[] ReadByteArray(int lengt)
        {

            if (lengt == 0)
                return new byte[0];

            byte[] values = new byte[lengt];
            unsafe
            {
                fixed (byte* datap = &Data[current])
                fixed (byte* valuesp = &values[0])
                    Buffer.MemoryCopy(datap, valuesp, values.Length, values.Length);
                //Buffer.BlockCopy(Data, current, values, 0, values.Length);
            }
            current = Interlocked.Add(ref current, lengt);
            return values;

        }


        /// <summary>
        /// 读取内存流中一段数据
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual byte[] ReadByteArray()
        {          
            int lengt = ReadInt32();

            if (lengt == 0)
                return new byte[0];

            byte[] values = new byte[lengt];
            unsafe
            {
                fixed (byte* datap = &Data[current])
                fixed (byte* valuesp = &values[0])
                    Buffer.MemoryCopy(datap, valuesp, values.Length, values.Length);

                //Buffer.BlockCopy(Data, current, values, 0, values.Length);
            }
          
            current = Interlocked.Add(ref current, lengt);
            return values;
        }
#endregion

#region 数据

        public virtual bool ReadByteArray(out byte[] values, int lengt)
        {

            try
            {

                if (lengt == 0)
                {
                    values = new byte[0];
                    return true;
                }

                values = new byte[lengt];
                unsafe
                {
                    fixed (byte* datap = &Data[current])
                    fixed (byte* valuesp = &values[0])
                        Buffer.MemoryCopy(datap, valuesp, values.Length, values.Length);
                    //Buffer.BlockCopy(Data, current, values, 0, values.Length);
                }
               
                current = Interlocked.Add(ref current, lengt);
                return true;


            }
            catch
            {
                values = null;
                return false;
            }

        }


        /// <summary>
        /// 读取内存流中一段数据
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public virtual bool ReadByteArray(out byte[] values)
        {
          
            try
            {
                if (ReadInt32(out int lengt))
                {
                    if (lengt == 0)
                    {
                        values = new byte[0];
                        return true;
                    }

                    values = new byte[lengt];                  
                    unsafe
                    {
                        fixed (byte* datap = &Data[current])
                        fixed (byte* valuesp = &values[0])
                            Buffer.MemoryCopy(datap, valuesp, values.Length, values.Length);
                        //Buffer.BlockCopy(Data, current, values, 0, values.Length);
                    }
                    current = Interlocked.Add(ref current, lengt);
                    return true;

                }
                else
                {
                    values = null;
                    return false;
                }
            }
            catch
            {
                values = null;
                return false;
            }

        }
#endregion

#region 对象

        ///// <summary>
        ///// 把字节反序列化成相应的对象
        ///// </summary>
        ///// <param name="pBytes">字节流</param>
        ///// <returns>object</returns>
        //protected virtual object DeserializeObject(byte[] pBytes)
        //{
        //    object _newOjb = null;
        //    if (pBytes == null)
        //        return _newOjb;
        //    System.IO.MemoryStream _memory = new System.IO.MemoryStream(pBytes);
        //    _memory.Position = 0;
        //    BinaryFormatter formatter = new BinaryFormatter();
        //  //  formatter.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
        //    _newOjb = formatter.Deserialize(_memory);
        //    _memory.Close();
        //    return _newOjb;
        //}

        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
        /// <returns>object</returns>
        public virtual T DeserializeObject<T>(byte[] pBytes)
        {

            switch (ObjFormatType)
            {

                case BuffFormatType.Binary:
                    {
                        object _newOjb = null;
                        if (pBytes == null)
                            return (T)_newOjb;
                        System.IO.MemoryStream _memory = new System.IO.MemoryStream(pBytes)
                        {
                            Position = 0
                        };
                        BinaryFormatter formatter = new BinaryFormatter();                     
                        _newOjb = formatter.Deserialize(_memory);
                        _memory.Close();
                        return (T)_newOjb;
                    }
                case BuffFormatType.XML:
                    {
                        string xml = Encoding.UTF8.GetString(pBytes);
                        Object result = new object();
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                        using (Stream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
                        {
                            XmlReader xmlReader = XmlReader.Create(stream);

                            return (T)xmlSerializer.Deserialize(xmlReader);

                        }
                      
                    }           
#if Net4
                case BuffFormatType.MsgPack:
                    {
                        return MsgPack.Serialization.SerializationContext.Default.GetSerializer<T>().UnpackSingleObject(pBytes);
                    }             

#endif
                case BuffFormatType.protobuf:
                    {
                        using (MemoryStream stream = new MemoryStream(pBytes))
                        {
                            return ProtoBuf.Serializer.Deserialize<T>(stream);
                        }
                    }
                default:
                    {
                        using (MemoryStream stream = new MemoryStream(pBytes))
                        {
                            return ProtoBuf.Serializer.Deserialize<T>(stream);
                        }
                    }


            }





        }






        /// <summary>
        /// 读取一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool ReadObject<T>(out T obj)
        {
           
            if (this.ReadByteArray(out byte[] data))
            {
                obj = DeserializeObject<T>(data);
                return true;
            }
            else
            {
                obj = default(T);
                return false;
            }

        }



        /// <summary>
        /// 读取一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T ReadObject<T>()
        {

            byte[] data = this.ReadByteArray();
            T obj = DeserializeObject<T>(data);
            return obj;


        }

#endregion

    }


}
