using System;
using System.Runtime.InteropServices;
using System.Text;
using Packet;
using UnityEngine;
using UnityEngine.Assertions;

namespace MemoryStream
{
    public class ReadMemoryStream
    {
        public int RestByte => _buffer.Length - _curIndex;

        public bool CanMakePacket
        {
            get
            {
                if (RestByte < 2)
                    return false;
                return RestByte >= BitConverter.ToUInt16(_buffer, _curIndex);
            }
        }

        public int CurPacketSize => BitConverter.ToUInt16(_buffer, _curIndex);

        private readonly byte[] _buffer;
        private int _curIndex;

        public ReadMemoryStream(byte[] buffer)
        {
            _buffer = buffer;
            _curIndex = 0;
        }

        public byte[] GetBuffer()
        {
            return _buffer;
        }

        public byte[] GetRestBuffer()
        {
            byte[] restBuffer = new byte[RestByte];
            Array.Copy(_buffer, _curIndex, restBuffer, 0, RestByte);
            return restBuffer;
        }

        public EPacketType GetPacketType()
        {
            return (EPacketType) _buffer[_curIndex + sizeof(Int16)]; // size 뒤에 type이 있기 때문
        }

        public byte ReadByByte()
        {
            Assert.IsTrue(_curIndex + sizeof(byte) <= _buffer.Length);
            return _buffer[_curIndex++];
        }

        public bool ReadByBoolean()
        {
            Assert.IsTrue(_curIndex + sizeof(bool) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(bool);
            return BitConverter.ToBoolean(_buffer, index);
        }

        public char ReadByChar()
        {
            Assert.IsTrue(_curIndex + sizeof(char) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(char);
            return BitConverter.ToChar(_buffer, index);
        }

        public double ReadByDouble()
        {
            Assert.IsTrue(_curIndex + sizeof(double) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(double);
            return BitConverter.ToDouble(_buffer, index);
        }

        public short ReadByInt16()
        {
            Assert.IsTrue(_curIndex + sizeof(short) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(short);
            return BitConverter.ToInt16(_buffer, index);
        }

        public int ReadByInt32()
        {
            Assert.IsTrue(_curIndex + sizeof(int) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(int);
            return BitConverter.ToInt32(_buffer, index);
        }

        public long ReadByInt64()
        {
            Assert.IsTrue(_curIndex + sizeof(long) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(long);
            return BitConverter.ToInt64(_buffer, index);
        }

        public float ReadByFloat()
        {
            Assert.IsTrue(_curIndex + sizeof(float) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(float);
            return BitConverter.ToSingle(_buffer, index);
        }

        public string ReadByString()
        {
            Assert.IsTrue(_curIndex + sizeof(Int16) <= _buffer.Length);
            int length = BitConverter.ToInt16(_buffer, _curIndex) * sizeof(char);
            Assert.IsTrue(_curIndex + sizeof(short) + length <= _buffer.Length);

            int index = _curIndex + sizeof(short);
            _curIndex += sizeof(short) + length;
            return Encoding.Unicode.GetString(_buffer, index, length);
        }

        public string ReadByString(int length)
        {
            Assert.IsTrue(_curIndex + sizeof(Int16) <= _buffer.Length);
            length *= sizeof(char);
            Assert.IsTrue(_curIndex + length <= _buffer.Length);

            int index = _curIndex;
            _curIndex += length;
            return Encoding.Unicode.GetString(_buffer, index, length * sizeof(char));
        }

        public ushort ReadByUInt16()
        {
            Assert.IsTrue(_curIndex + sizeof(ushort) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(ushort);
            return BitConverter.ToUInt16(_buffer, index);
        }

        public uint ReadByUInt32()
        {
            Assert.IsTrue(_curIndex + sizeof(uint) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(uint);
            return BitConverter.ToUInt32(_buffer, index);
        }

        public ulong ReadByUInt64()
        {
            Assert.IsTrue(_curIndex + sizeof(ulong) <= _buffer.Length);

            int index = _curIndex;
            _curIndex += sizeof(ulong);
            return BitConverter.ToUInt64(_buffer, index);
        }

        public Vector3 ReadByVector3()
        {
            Assert.IsTrue(_curIndex + sizeof(float) * 3 <= _buffer.Length);
            Vector3 vec;
            vec.x = BitConverter.ToSingle(_buffer, _curIndex);
            vec.y = BitConverter.ToSingle(_buffer, _curIndex + sizeof(float));
            vec.z = BitConverter.ToSingle(_buffer, _curIndex + sizeof(float) * 2);
            _curIndex += sizeof(float) * 3;
            return vec;
        }

        public void ReadByObject(IConvertToData convertToData)
        {
            convertToData.ToData(this);
        }

        public T ReadByStruct<T>() where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            Assert.IsTrue(_curIndex + size <= _buffer.Length);

            int index = _curIndex;
            _curIndex += size;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(_buffer, index, ptr, size);
            T obj = (T) Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return obj;
        }
    }
}
