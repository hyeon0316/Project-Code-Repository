using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;


public class PlayerInfo : MonoBehaviour
{ 
   public int ID { get; set; }
   public int Hp { get; set; }
   public TextMeshProUGUI NameText;
   public TextMeshProUGUI HpText;
   public Image CharacterImage;
   public Image GrayScale;

   [SerializeField] private Button _button;
   [SerializeField] private Image _emoteImage;

   private Sequence _sequence;

   /// <summary>
   /// 어떤 플레이를 관전 중인지 표시하는 오브젝트
   /// </summary>
   [SerializeField] private GameObject _arrow;

   [SerializeField] private TextMeshProUGUI _debugText;

   private void Start()
   {
      _button.onClick.AddListener(SetView);
   }

   public void SetActiveArrow(bool isActive)
   {
      _arrow.SetActive(isActive);
   }
   
   public void SetView()
   {
      if (InGame.CurGameType == EGameType.Ready) //준비상태일때 관전
      {
         for (int i = 0; i < Global.MaxRoomPlayer; i++)
         {
            Player player = PlayerManager.Instance.Players[i];
            player.gameObject.transform.localPosition = ID == player.ID ? Vector3.zero : new Vector3(0, 3000, 0);
         }
      }
      else //전투상태일때 관전
      {
         WindowManager.Instance.GetInGame().BattleWindow.SetBattleView(ID);
      }

      WindowManager.Instance.GetInGame().PlayersMap.SetOrderView(transform.GetSiblingIndex()); //버튼 클릭으로 했을때 인덱스 오류 방지
      WindowManager.Instance.GetInGame().PlayersMap.SetArrow(ID);
   }

   public void ShowEmotion(Sprite useSprite)
   {
      _emoteImage.sprite = useSprite;
      _sequence = DOTween.Sequence()
         .SetAutoKill(false)
         .OnStart(() =>
         {
            _emoteImage.transform.localScale = Vector3.zero;
         })
         .Append(_emoteImage.transform.DOScale(1, 1).SetEase(Ease.Linear))
         .Join(_emoteImage.transform.DORotate(new Vector3(0, 0, 25), 1f).SetEase(Ease.InSine))
         .Insert(1f, _emoteImage.transform.DORotate(new Vector3(0, 0, -25), 1f).SetEase(Ease.InSine))
         .Insert(2f, _emoteImage.transform.DORotate(new Vector3(0, 0, 25), 1f).SetEase(Ease.InSine))
         .Append(_emoteImage.transform.DOScale(0, 1).SetEase(Ease.Linear))
         .Join(_emoteImage.transform.DORotate(new Vector3(0, 0, -25), 1f).SetEase(Ease.InSine));
   }
}
