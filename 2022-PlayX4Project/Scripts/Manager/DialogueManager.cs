using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DialogueManager : SingletonAwake<DialogueManager>
{
    private const string DELETE_SENTENCE = "Delete";
    private const string REPEAT_SENTENCE = "Repeat";
    private const string MOVE_CAMERA = "MoveCamera";
    private const string RESET_CAMERA = "ResetCamera";

    public Npc CurNpc { get; set; }
    public bool IsTalking { get; private set; }

    [SerializeField] private RectTransform[] _letterBox;
    [SerializeField] private GameObject _TalkPanel;
    [SerializeField] private Text _talkText;
    [SerializeField] private GameObject _NextObj;
    [SerializeField] private float _normalDelayTime;
    [SerializeField] private float _fastDelayTime;

    private Queue<string> _sentences = new Queue<string>();
    private bool _isTyping = false;
    private string _currentSentence;
    private float _textDelay;
    private Vector3 _fixedPanelVec;

    protected override void Awake()
    {
        base.Awake();
        _textDelay = _normalDelayTime;
    }

    private void Update()
    {
        TalkCheck();
    }

    private void TalkCheck()
    {
        if (IsTalking)
        {
            _TalkPanel.transform.position = _fixedPanelVec;

            //대화 진행
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!_isTyping)
                {
                    NextSentence();
                    _NextObj.gameObject.SetActive(false);
                }
                else
                {
                    _textDelay = _fastDelayTime;
                }
            }
        }
    }
    
    public void ActiveLetterBox()
    {
        StartCoroutine(LetterBoxOnCo());
    }


    /// <summary>
    /// 대화내용들을 저장시켜 하나씩 꺼내 사용
    /// </summary>
    /// <param name="lines">대화 내용들</param>
    public void OnDialogue(List<string> lines)
    {
        IsTalking = true;
        _sentences.Clear();
        foreach (string line in lines)
        {
            _sentences.Enqueue(line);
        }
        NextSentence();
    }

    private  void NextSentence()
    {
        string curSentense = _sentences.Peek();
        if (curSentense.Equals(DELETE_SENTENCE))
        {
            HandleDeleteSentence();
        }
        else if (curSentense.Equals(REPEAT_SENTENCE))
        {
            HandleRepeatSentence();
        }
        else if (curSentense.Equals(MOVE_CAMERA))
        {
            HandleMoveCamera();
        }
        else if(curSentense.Equals(RESET_CAMERA))
        {
            HandleResetCamera();
        }
        else
        {
            HandleNormalSentence();
        }
    }

    private void HandleDeleteSentence()
    {
        CurNpc.RemoveDataUntilTaget(DELETE_SENTENCE);
        CloseTalkPanel();
    }

    private void HandleRepeatSentence()
    {
        CloseTalkPanel();
    }

    private void HandleMoveCamera()
    {
        _sentences.Dequeue();
        CameraManager.Instance.SetTarget(CachingManager.Instance().SecondFloorCameraPos);
        PlayerManager.Instance.Player.ChangeDirection(false);
    }

    private void HandleResetCamera()
    {
        CameraManager.Instance.SetTarget(CurNpc.gameObject);
        PlayerManager.Instance.Player.DelayChangeDirCo(2f, true);
        HandleNormalSentence();
    }

    private void HandleNormalSentence()
    {
        SetActiveTalkPanel(false);
        Invoke(nameof(DelayNextTalk), 0.5f);
    }

    /// <summary>
    /// 대화창이 꺼질때 발생하는 이벤트들
    /// </summary>
    private void CloseTalkPanel()
    {
        PlayerManager.Instance.SetActivePlayerUI(true);
        PlayerManager.Instance.Player.ReMove();
        IsTalking = false; 
        CameraManager.Instance.SetTarget(PlayerManager.Instance.Player.gameObject);
        SetActiveTalkPanel(false);
        StartCoroutine(LetterBoxOffCo());
        Invoke(nameof(ReTalk), 0.01f);
    }
    
    private void DelayNextTalk()
    {
        SetActiveTalkPanel(true);
        _currentSentence = _sentences.Dequeue();
        _isTyping = true;
        TypingAsync(_currentSentence).ContinueWith(FillText).Forget();
    }

    /// <summary>
    /// 대화를 마치고 제자리에서 다시 대화를 할 경우 바로 상호작용 가능하도록 설정
    /// </summary>
    private void ReTalk()
    {
        if (Vector3.Distance(PlayerManager.Instance.Player.transform.position, CurNpc.transform.position) <= 1.2f)
        {
            CurNpc.ReTalk();
        }
    }

    /// <summary>
    /// 텍스트를 한글자씩 출력
    /// </summary>
    private async UniTask TypingAsync(string line)
    {
        StringBuilder sb = new StringBuilder();
        _talkText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            sb.Append(letter);
            _talkText.text = sb.ToString(); 
            if (_textDelay == _normalDelayTime)
            {
                SoundManager.Instance.Play("Object/Talk", SoundType.Effect);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_textDelay));
        }
    }

    private void FillText()
    {
        _isTyping = false;
        _textDelay = _normalDelayTime;
        _NextObj.gameObject.SetActive(true);
    }
    
    private IEnumerator LetterBoxOnCo()
    {
        float time = 0f;
        while (time <= 1.0f)
        {
            time += Time.deltaTime * 2;
            LetterBoxMove(time);
            yield return null;
        }
    }
    
    private IEnumerator LetterBoxOffCo()
    {
        float time = 1f;
        while (time >= 0f)
        {
            time -= Time.deltaTime * 2;
            LetterBoxMove(time);
            yield return null;
        }
    }
    
    /// <summary>
    /// 레터박스를 움직이는 함수
    /// </summary>
    /// <param name="value">값이 0일 때 레터박스가 사라지고 1일 때 보임</param>
    private void LetterBoxMove(float value)
    {
        _letterBox[0].anchoredPosition = Vector2.Lerp(new Vector2(_letterBox[0].anchoredPosition.x, -150),
            new Vector2(_letterBox[0].anchoredPosition.x, 0), value);
        
        _letterBox[1].anchoredPosition = Vector2.Lerp(new Vector2(_letterBox[1].anchoredPosition.x, 150),
            new Vector2(_letterBox[1].anchoredPosition.x, 0), value);
    }

    public void SetPanelPos(Vector3 pos)
    {
        _fixedPanelVec = Camera.main.WorldToScreenPoint(pos);
    }

    public void SetActiveTalkPanel(bool isActive)
    {
        _TalkPanel.SetActive(isActive);

        if (!isActive) IsTalking = false;
    }
}
