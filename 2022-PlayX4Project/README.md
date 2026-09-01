# 2022 PlayX4프로젝트 - Treasure Of Dungeon

## 프로젝트 소개
- 기간 : 2022/03/18 ~ 2022/05/12
- 장르 : 액션 어드벤처(2.5D)
- 플랫폼 : PC
- 도구 : Unity, C#
- 인원 : 3인 (프로그래머 2, 그래픽/사운드 1)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 팀장, 프로그래머, 기획
- kimehunsu(김은수) :  그래픽, 사운드
- kjcy(주찬영) : 프로그래머

## 코드

### 오브젝트 풀링

보스가 폭탄과 화염구를 반복 생성하는데, 생성과 파괴를 반복하면 전투 중 프레임이 흔들림.
직전 프로젝트에서 이펙트를 매번 `Instantiate` / `Destroy` 하던 방식을 고침.

- 원본

```csharp
GameObject firstSkill = Instantiate(skill_First, pos, Quaternion.identity);
Destroy(firstSkill, 2f);
```

- 수정

폭탄·폭발 이펙트·화염구를 각각 `Queue`로 관리하고, 시작 시 미리 생성해 둠.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Life/Enemy/Demon.cs#L74-L134

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 풀 고갈 시 처리 없음 | `Dequeue` 전에 개수를 확인하지 않는 경로가 있어 예외 가능 | 고갈 시 동적 확장 |
| 풀 규모가 실사용과 맞지 않음 | 오브젝트의 동시 활성 규모는 최대 2~3개 수준. 풀링은 다량의 생성 및 파괴가 몰릴 때 이득인데 현재 규모에선 이득 없음 | 단순 Instantiate, Destory 사용 무방 |

<br></br>
### 대화 시스템

**1. 명령어를 만나면 분기, 아니면 일반 출력**

대사 목록을 `Queue`에 담고 하나씩 꺼내며 명령어 여부를 확인.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L87-L141

기획자가 인스펙터의 대사 목록에 `MoveCamera` 한 줄을 넣으면 그 시점에 카메라가 이동함. 코드 수정이 필요 없음.

**2. `Delete`로 일회성 대사 제거**

한 번만 나와야 하는 대사 뒤에 `Delete`를 넣으면, 그 지점까지의 대사를 목록에서 지움.
다음에 다시 말을 걸면 이후 대사부터 시작함.
```csharp
public void RemoveDataUntilTaget(string target)
{
    int index = _sentences.FindIndex(s => s.Equals(target));
    if (index != -1)
    {
        _sentences.RemoveRange(0, index + 1);
    }
}
```

<br></br>

### 캐릭터 상속 구조와 인터페이스 분리

직전 프로젝트는 모든 적이 하나의 부모를 상속받고, 부모가 이동·공격·사망을 모두 들고 있었음.
근접만 하는 적도 원거리 공격 함수를 물려받는 구조라, 적 종류가 늘수록 쓰지 않는 코드가 따라다녔음.

- 원본

부모 클래스에 이동과 공격이 모두 들어있고, 하위 클래스가 필요한 것만 골라 씀.
```csharp
public class LivingEntity : MonoBehaviour
{
    public float health { get; protected set; }
    public virtual void OnDamage(float damage) { ... }
    public virtual void Die() { ... }
}
```
사망 처리도 `Die()` 하나에 몰려 있어, 콜라이더 정리부터 드랍 생성까지 한 메서드에서 처리했음.

- 수정

공통 상태는 `Life`가 갖고, **행동은 인터페이스로 분리**해 필요한 적만 구현하게 함.
```
Life (추상)                     // 체력, 공격력, 속도, 피격, 사망
├── Player
└── Enemy (추상)                // 넉백, 경직, 사망 이벤트
    ├── Cultist    : IEnemyMove, IEnemyAttack
    ├── Assassin   : IEnemyMove, IEnemyAttack
    ├── Necromancer: IEnemyMove              //근접 공격이 없는 소환형
    └── Demon      : IEnemyMove, IEnemyAttack
```

<br></br>

### JSON 외부 데이터로 밸런스 분리

시연 현장에서 난이도 피드백을 받으면 그 자리에서 수치를 고쳐야 함.
직전 프로젝트는 체력과 공격력이 코드와 프리팹에 흩어져 있어, 수정할 때마다 에디터를 열고 빌드를 다시 만들어야 했음.

- 수정

스탯을 빌드 외부의 `setting.Json`으로 분리하고, 실행 시 읽어 각 캐릭터에 주입.
파일이 없으면 인스펙터에 등록한 기본값(`StatObj`)으로 생성하므로 첫 실행도 정상 동작함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Manager/JsonToDataManager.cs#L9-L66

<br></br>

### 커스텀 에디터로 아이템 데이터 입력

아이템이 소모품과 재료 두 종류인데, 재료는 회복량과 쿨타임이 필요 없음.
하나의 클래스로 두면 재료 아이템에도 사용하지 않는 칸이 계속 보임.

- 원본

모든 필드가 항상 노출되어, 채우지 말아야 할 칸을 실수로 채워도 확인할 방법이 없었음.

- 수정

`CustomEditor`로 인스펙터를 직접 그려, 선택한 타입에 필요한 필드만 표시.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Life/Player/Inventory/Item.cs#L13-L50
      
      
