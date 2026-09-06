using BackEnd;
using BackendSharedLib;

namespace BackendFunction
{
    public static class UserDataParam
    {
        public static Param Get(int character)
        {
            var param = new Param();
            var userData = new UserData();
            userData.StartingCharacter = character;
            param.Add("userData", userData);
            return param;
        }
    }
}