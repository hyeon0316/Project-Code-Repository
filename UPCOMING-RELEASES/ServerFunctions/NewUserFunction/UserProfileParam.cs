using System;
using BackEnd;

namespace BackendFunction
{
    public class UserProfile
    {
        public string NickName;
        public int ProfileIconID;
        public string Comment;
        public int Level;
    }

    public static class UserProfileParam
    {
        public static Param Get(string nickName, string uid)
        {
            var param = new Param();
            var profile = new UserProfile();
            profile.NickName = nickName;
            profile.Comment = "";
            profile.Level = 1;
            param.Add("uid", uid);
            param.Add("profileData", profile);
            return param;
        }
    }
}