namespace MemoryStream
{
    public interface IConvertToBytes
    {
        /// <summary>
        /// 데이터 직렬화
        /// </summary>
        void ToBytes(WriteMemoryStream writeStream);
    }
}
