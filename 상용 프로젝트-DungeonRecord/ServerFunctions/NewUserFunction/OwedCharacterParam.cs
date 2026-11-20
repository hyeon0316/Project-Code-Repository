using System.Collections.Generic;
using BackEnd;
using BackendSharedLib;

namespace BackendFunction
{
    public static class OwnedCharacterDataParam
    {
        public static Param Get(int characterType)
        {
            var param = new Param();
            var characters = new List<CharacterRecord>();
            var newCharacter = new CharacterRecord();
            newCharacter.Type = characterType;
            characters.Add(newCharacter);
            param.Add("characters", characters);
            return param;
        }
    }
}