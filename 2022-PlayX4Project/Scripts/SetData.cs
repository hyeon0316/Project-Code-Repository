using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetData 
{
   //플레이어
   public int PlayerHp;
   public int PlayerPower;
   public float PlayerSpeed;
   //컬티스트
   public int CultistHp;
   public int CultistPower;
   public float CultistSpeed;
   //어쌔신
   public int AssassinHp;
   public int AssassinPower;
   public float AssassinSpeed;
   //빅컬티스트
   public int BigCultistHp;
   public int BigCultistPower;
   public float BigCultistSpeed;
   //트위스트
   public int TwistedHp;
   public int TwistedPower;
   public float TwistedSpeed;
   //브링거
   public int BringerHp;
   public int BringerPower;
   public float BringerSpeed;
   //네크로멘서
   public int NecromancerHp;
   public int NecromancerPower;
   public float NecromancerSpeed;
   //데몬
   public int DemonHp;
   public int DemonPower;
   public float DemonSpeed;

   public SetData()
   {
      PlayerHp = 100;
      PlayerPower = 10;
      PlayerSpeed = 5;
      CultistHp = 80;
      CultistPower = 10;
      CultistSpeed = 2.5f;
      AssassinHp = 100;
      AssassinPower = 5;
      AssassinSpeed = 3;
      BigCultistHp = 160;
      BigCultistPower = 10;
      BigCultistSpeed = 2;
      TwistedHp = 120;
      TwistedPower = 10;
      TwistedSpeed = 2.5f;
      BringerHp = 360;
      BringerPower = 5;
      BringerSpeed = 3;
      NecromancerHp = 300;
      NecromancerPower = 5;
      NecromancerSpeed = 3;
      DemonHp = 420;
      DemonPower = 5;
      DemonSpeed = 2;
   }
}
