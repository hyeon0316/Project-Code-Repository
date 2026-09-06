using System;

namespace BackendSharedLib
{
    public class RewardException : Exception
    {
        public int Code { get; }
        public string Detail { get; }

        public RewardException(int code, string detail) : base(detail)
        {
            Code = code;
            Detail = detail;
        }
    }
}
