namespace MemoryStream
{
    public interface IConvertToData
    {
        /// <summary>
        /// 직렬화 Byte배열 객체로 변환
        /// </summary>
        void ToData(ReadMemoryStream readStream);
    }
}
