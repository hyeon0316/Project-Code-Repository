using System;
using System.Runtime.InteropServices;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace MemoryStream
{
    public class WriteMemoryStream
    {
        private byte[] _buffer;
        private Int16 _size;
        private int _capacity;

        public WriteMemoryStream()
        {
            _capacity = 32;
            _buffer = new byte[_capacity];
        }

        /// <summary>
        /// 버퍼에 기록된 byte배열 반환(유효한 배열만)
        /// </summary>
        /// <returns>기록된 byte배열</returns>
        public byte[] GetValidBuffer()
        {
            byte[] retBuffer = new byte[_size];
            Array.Copy(_buffer, 0, retBuffer, 0, _size);
            return retBuffer;
        }

        public void WriteByByte(byte b)
        {
            if (_size + 1 > _capacity)
            {
                int addSize = 0;
                do
                {
                    addSize += 128;
                } while (_size + 1 > _capacity + addSize);

                ReallocBuffer(_capacity + addSize);
            }

            _buffer[_size] = b;
            _size += sizeof(byte);
        }

        public void WriteByBoolean(bool b)
        {
            WriteBytes(BitConverter.GetBytes(b));
        }

        public void WriteByChar(char c)
        {
            WriteBytes(BitConverter.GetBytes(c));
        }

        public void WriteByDouble(double d)
        {
            WriteBytes(BitConverter.GetBytes(d));
        }

        public void WriteByInt16(Int16 i16)
        {
            WriteBytes(BitConverter.GetBytes(i16));
        }

        public void WriteByInt32(Int32 i32)
        {
            WriteBytes(BitConverter.GetBytes(i32));
        }

        public void WriteByInt64(Int64 i64)
        {
            WriteBytes(BitConverter.GetBytes(i64));
        }

        public void WriteByFloat(float f)
        {
            WriteBytes(BitConverter.GetBytes(f));
        }

        public void WriteByVector3(Vector3 vec)
        {
            WriteBytes(BitConverter.GetBytes(vec.x));
            WriteBytes(BitConverter.GetBytes(vec.y));
            WriteBytes(BitConverter.GetBytes(vec.z));
        }

        public void WriteByString(string str)
        {
            char[] cArr = str.ToCharArray();
            WriteBytes(BitConverter.GetBytes((Int16) str.Length));
            WriteBytes(Encoding.Unicode.GetBytes(str));
        }

        public void WriteByUInt16(UInt16 ui16)
        {
            WriteBytes(BitConverter.GetBytes(ui16));
        }

        public void WriteByUInt32(UInt32 ui32)
        {
            WriteBytes(BitConverter.GetBytes(ui32));
        }

        public void WriteByUInt64(UInt64 ui64)
        {
            WriteBytes(BitConverter.GetBytes(ui64));
        }

        public void WriteByObject(IConvertToBytes convertToData)
        {
            convertToData.ToBytes(this);
        }

        public void WriteByStruct(object obj)
        {
            Assert.IsTrue(obj.GetType().IsStruct());
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            WriteBytes(arr);
        }

        /// <summary>
        /// buffer를 더 큰 사이즈로 교체해준다.
        /// 단. 새 capacity가 이전 capacity보다 커야한다
        /// </summary>
        /// <param name="capacity">새 용량</param>
        private void ReallocBuffer(int capacity)
        {
            Assert.IsTrue(capacity > _capacity);
            byte[] newBuffer = new byte[capacity];
            Array.Copy(_buffer, 0, newBuffer, 0, _size);
            _buffer = newBuffer;
            _capacity = capacity;
        }

        /// <summary>
        /// 버퍼에 byte배열을 기록
        /// </summary>
        /// <param name="bytes">기록할 byte배열</param>
        private void WriteBytes(byte[] bytes)
        {
            if (_size + bytes.Length > _capacity)
            {
                int addSize = 0;
                do
                {
                    addSize += 128;
                } while (_size + bytes.Length > _capacity + addSize);

                ReallocBuffer(_capacity + addSize);
            }

            Array.Copy(bytes, 0, _buffer, _size, bytes.Length);
            _size += (Int16) bytes.Length;
        }
    }
}
