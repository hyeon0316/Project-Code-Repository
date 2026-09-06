# UPCOMING-RELEASES (개발 진행중)

## 프로젝트 소개
- 개발기간 : 2024/03 ~ 진행중
- 장르 : 턴제 던전 RPG(3D)
- 플랫폼 : Mobile (Google Play / iOS 예정)
- 도구 : Unity, C#, 클라우드 서비스(뒤끝)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 클라이언트 전체, 서버 펑션, 빌드 파이프라인

## 프로젝트 구조

어셈블리를 4계층으로 나누고 참조 방향을 한쪽으로 고정함.
계층을 넘는 참조가 컴파일 단계에서 막히므로, 순환 참조가 생기기 전에 드러남.

```
Core ──────── 확장 메서드, 싱글톤, Localize, UIMultiView (의존 없음)
  ↑
Shared ────── Enum, Interface, SO 정의, 서버 테이블 DTO
  ↑
FrameWork ─── Manager, UI(Page/Popup), BDatabase, Addressables
  ↑
GamePlay ──── Dungeon, Character, Item, Shop, Quest ... (실제 게임 로직)

ScriptsEditor ── 위 전부 참조. 에디터 툴 (Editor 플랫폼 전용)
```

```
[클라이언트 — Unity]
ContentsManager   : 모든 게임 시스템의 허브. Get<T>()로 접근
BDatabase         : 서버 CDN + Addressables 테이블 로딩
UIManager         : Page(화면) / Popup(팝업) 이중 스택
EffectExecutor    : 버프·디버프·도트 효과 실행
BattleController  : 턴 순서, 스킬 실행, 전투 진행

        ↕ BFunc 호출 / 검증된 결과 수신

[서버 — AWS Lambda + 뒤끝]
ConsumeItemFunction   : 아이템 소비 검증·차감
DungeonRewardFunction : 던전 보상 지급 (장비 랜덤 롤 포함)
CharacterGachaFunction: 가챠 확률·천장 처리
ShopFunction          : 상점 구매 검증
BackendSharedLib      : 공용 응답·트랜잭션·아이템ID 규격
```

## 코드

### 이펙트 시스템

버프·디버프·출혈·중독·기절이 전부 "효과"지만 생명주기가 서로 다름.
데미지는 즉시 끝나고, 버프는 해제 시점에 되돌려야 하고, 도트는 매 턴 실행되며 연출 대기가 필요함.

이걸 하나의 인터페이스로 묶으면 데미지 핸들러가 쓰지도 않을 `Clear()`와 `Dot()`을 빈 구현으로 갖게 됨.

**1. 생명주기별로 인터페이스를 나눔**

필요한 계약만 갖도록 4종으로 분리함. 핸들러는 자기에게 해당하는 것만 구현함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Effect/Handler/IEffectHandler.cs#L1-L23

| 인터페이스 | 계약 | 해당 효과 |
|---|---|---|
| `IEffectHandler` | `Execute(effect)` | 대상이 없는 효과 (스태미나 회복) |
| `ITargetEffectHandler` | `Execute(effect, context)` | 즉시 끝나는 효과 (데미지, 힐) |
| `IStatusEffectHandler` | `Execute` + `Clear` | 해제가 필요한 효과 (스탯 버프, 기절) |
| `IDotEffectHandler` | `Execute` + `Dot` | 매 턴 실행되는 효과 (출혈, 중독) |

**2. 스탯 버프 핸들러를 enum에서 자동 등록**

버프·디버프는 대상 스탯만 다르고 처리가 같음. 13종을 각각 등록하면 스탯이 늘 때마다 등록도 늘어남.
`EffectType`을 순회하면서 스탯 매핑이 있는 것만 같은 핸들러 인스턴스에 연결함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Effect/EffectExecutor.cs#L23-L38

`EffectTypeExtension`의 `STAT_MAP`에 한 줄만 추가하면 새 버프가 자동으로 동작함.
디버프 부호도 `IsDebuff()` 한 곳에서 결정되므로 핸들러는 부호를 신경쓰지 않음.

**3. 효과 데이터는 코드가 아닌 테이블에**

`EffectEntry`(수치·스택·최대스택·설명 포맷)는 `EffectTable`에서 옴.
서버 검증에 쓰이지 않는 클라 전용 테이블이라 엑셀 → JSON → Addressables 경로로 실림.
스킬 SO는 `EffectIDs` 문자열 배열만 들고 있어서, 효과를 바꿔도 스킬 에셋을 건드리지 않음.

```csharp
public class EffectEntry
{
    public string EffectID;
    public EffectType EffectType;
    public float Value;
    public int Stack;      // 지속 턴 수
    public int MaxStack;   // 0이면 무한 중첩
    public string DescFormatKey;
}
```

**4. 저항 굴림을 부여 시점 한 곳에 둠**

기절·출혈·중독은 저항 스탯의 영향을 받음. 각 핸들러가 저항을 검사하면 3곳에 같은 코드가 생김.
효과가 실제로 붙는 `AddEffect()` 한 곳에서만 굴림.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/DungeonUnit.cs#L194-L228

`RESIST_MAP`에 없는 타입은 저항 대상이 아니므로 항상 통과함.
`Random.value`가 1.0을 포함하기 때문에 저항 100%에서 완전 면역이 되도록 비교를 따로 처리함.

<br></br>

### 스탯 모디파이어

장비·버프·디버프·패시브가 같은 스탯에 동시에 붙음.
"공격력 +10" 과 "공격력 +20%" 의 적용 순서에 따라 결과가 달라지므로 순서를 규격으로 고정해야 했음.

**1. 계산 순서를 enum 값으로 고정**

`Flat → Percent → FixedFlat → FixedPercent` 순서를 `EStatValueType` 선언 순서가 그대로 결정함.
모디파이어는 추가 시점에 `CaculateOrder` 오름차순 위치로 삽입되므로, 붙은 순서와 무관하게 결과가 같음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Character/Stat/FlatStat.cs#L7-L18

같은 order끼리는 삽입 순서를 유지해야 `FixedPercent` 곱 순서가 흔들리지 않으므로,
`>` 비교로 멈춰서 안정 삽입(stable insert)이 되게 함.

**2. Percent를 구간으로 묶어서 합산**

연속된 `Percent`는 각각 곱하는 게 아니라 합산 후 한 번만 곱해야 함(+10%, +20% → ×1.3).
구간이 끊기는 지점에서 누적합을 적용함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Character/Stat/FlatStat.cs#L30-L74

**3. 기여도를 역산으로 구함**

장비 상세 화면에서 "이 장비가 공격력을 얼마나 올렸는가"를 표시해야 함.
`Percent`는 base에 곱해지므로 모디파이어 값을 단순 합산하면 실제 기여분과 다름.

해당 모디파이어를 **뺀 상태로 다시 계산**해서 차이를 구하는 방식으로 해결함.
계산식이 아무리 복잡해져도 기여도 산출은 그대로 동작함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Character/Stat/Stat.cs#L91-L98

제외 계산에서도 `Percent` 구간이 끊기는 지점을 놓치지 않도록,
다음 유효 항목을 미리 확인하는 `HasNextPercent()`를 둠.

**4. 캐싱과 소스 단위 제거**

`ResultValue`는 `m_IsDirty`일 때만 재계산함. 전투 중 매 프레임 조회해도 비용이 없음.
해제는 `Source` 참조로 일괄 제거하므로, 장비를 벗기거나 버프가 끝날 때 어떤 모디파이어를 붙였는지 기억할 필요가 없음.

```csharp
public bool RemoveAllModifiersFromSource(object source)
{
    m_SourceToRemove = source;
    int removeCnt = m_StatModifiers.RemoveAll(m_Predicate);
    m_SourceToRemove = null;
    // ...
}
```

`Predicate`를 생성자에서 한 번만 만들어 필드로 재사용함 (`RemoveAll` 호출마다 델리게이트 할당이 생기지 않게).

<br></br>

### 클라이언트 - 서버 검증 분리

모바일 게임이라 메모리 조작·패킷 위조를 전제해야 함.
재화·아이템이 걸린 처리는 클라이언트가 결과를 정하지 않고, 서버 펑션(AWS Lambda)이 검증 후 확정함.

**1. 소비는 요청, 확정은 서버 응답**

아이템 사용 시 클라이언트는 "이걸 쓰겠다"만 보냄. 보유 수량 검사와 차감은 전부 서버에서 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/ServerFunctions/ConsumeItemFunction/Function.cs#L84-L128

보유량보다 많이 요청하거나 갖고 있지 않은 장비 인스턴스를 지목하면 `Suspect`로 응답함.
같은 아이템이 요청에 여러 번 들어오는 경우를 대비해 먼저 합산한 뒤 한 번에 검사함
(개별 검사하면 각각은 통과하지만 합계는 보유량을 넘는 상황이 생김).

**2. 검증 실패를 두 등급으로 나눔**

정상 플레이로는 나올 수 없는 요청과, 단순 오류를 구분함.

| 응답 | 사용 시점 | 동작 |
|---|---|---|
| `Error(code, detail)` | 서버 오류, 유효성 실패 | 클라이언트에 코드 전달 |
| `Suspect(param)` | 정상 플레이로 불가능한 요청 | **GameLog에 자동 기록** 후 차단 |

`Suspect`는 응답 생성과 로그 적재가 한 함수에 묶여 있어, 호출부가 로그를 빠뜨릴 수 없음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/ServerFunctions/BackendSharedLib/ReturnObject.cs#L10-L17

**3. 여러 테이블 갱신을 트랜잭션으로 묶음**

아이템 소비는 `Inventory_Stack`과 `Inventory_Equip` 두 테이블에 걸림.
따로 쓰면 중간에 실패했을 때 한쪽만 반영된 상태가 남음.

쓰기를 큐에 모았다가 `Flush()`에서 2건 이상이면 `TransactionWriteV2`로 묶음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/ServerFunctions/BackendSharedLib/WriteBatcher.cs#L18-L41

단일 테이블도 같은 API로 호출하므로, 나중에 테이블이 추가돼도 호출부를 고치지 않음.

**4. 가챠 확률과 천장을 서버에서만 처리**

확률·천장 카운트·중복 마일리지를 클라이언트가 알면 조작 대상이 됨.
난수 생성과 천장 판정 전부 Lambda에서 하고, 클라이언트는 결과만 받아 연출함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/ServerFunctions/CharacterGachaFunction/GachaDraw.cs#L48-L93

천장 카운트는 유저 데이터에 저장되므로 앱을 껐다 켜도 유지되고, 클라이언트가 초기화할 수 없음.

**5. 응답 해석을 한 곳으로 모음**

`suspect` / `error` / `success` 판별을 호출부마다 하면 분기를 빠뜨림.
모든 BFunc 응답이 한 함수를 거치게 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/BFuncResponseHandler.cs#L6-L37

`error`는 구형(문자열)과 신형(`{code, detail}`) 두 형태를 모두 받음.
서버와 클라이언트의 배포 시점이 다르고 구버전 빌드가 스토어에 남아 있어서, 양쪽을 지원해야 했음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/BFuncResponseHandler.cs#L49-L68

<br></br>

### 던전 진행 복원

모바일이라 전투 중 앱이 강제 종료되는 상황이 상시 발생함.
던전을 처음부터 다시 시작하게 하면 이탈로 이어지므로, 전투 중간 상태까지 복원해야 했음.

**1. 복원에 필요한 최소 상태만 저장**

`DungeonProgress`가 진행 정보를, `BattleProgress`가 전투 상태를 나눠 보관함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/DungeonProgress.cs#L1-L34

맵은 통째로 저장하지 않고 **시드만** 저장함. 같은 시드로 다시 생성하면 같은 맵이 나옴.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/RandomMapGenerator.cs#L8-L30

노드 GUID도 `{seed}_{row}_{col}` 로 생성하므로, 재생성해도 클리어 기록이 그대로 대응됨.

**2. 효과는 스택 수까지 복원**

버프가 3턴 남았으면 3턴으로 돌아와야 함. 효과 목록과 스택을 기록했다가 그대로 되살림.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/Battle/BattleController.cs#L333-L343

이때 스탯 보정은 **스택 수와 무관하게 한 번만** 적용해야 함.
`Stack`은 지속 턴 수이지 중첩 배수가 아니라서, 스택만큼 반복하면 3턴 남은 버프가 3배로 걸림.

**3. 저장 시점을 턴 시작으로 고정**

턴 도중에 저장하면 스킬 연출 중간 상태가 남아 복원이 애매해짐.
행동 순서를 정한 직후, 스킬 실행 전에만 기록함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/Battle/BattleController.cs#L385-L420

복원 지점이 항상 "턴 시작"이라 한 가지 경우만 검증하면 됨.

**4. 턴 순서 동점 처리를 규칙으로 고정**

속도가 같을 때 순서가 실행마다 달라지면 저장·복원 결과가 어긋남.
`속도 내림차순 → 아군 우선 → 원래 인덱스 오름차순`으로 완전 순서를 만듦.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/Battle/BattleController.cs#L344-L371

`List.Sort`가 불안정 정렬이라 동점 시 순서를 보장하지 않으므로,
원래 인덱스를 마지막 비교 기준으로 넣어 결정적(deterministic)으로 만듦.

**5. 노드 타입별 진입 처리를 딕셔너리로 분기**

던전 노드가 전투·상점·이야기·카드선택 등으로 늘어나는데, `switch`로 분기하면 타입 추가마다 수정해야 함.
타입 → 실행기 딕셔너리로 두고, 공통 처리(진행 기록·저장)는 base가 담당함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/GamePlay/Dungeon/NodeExecutor/StageNodeExecutorRegistry.cs#L1-L23

미등록 타입은 기본 실행기로 떨어지므로, 노드를 추가해도 진행 기록이 누락되지 않음.

<br></br>

### UI 프레임워크

화면(Page)과 팝업(Popup)의 생명주기가 다름.
화면은 히스토리를 쌓고 뒤로가기가 있으며 한 번에 하나만 보임.
팝업은 여러 개가 겹칠 수 있고 순서대로 떠야 함.

**1. Page는 히스토리 리스트, Popup은 스택 + 큐**

화면 전환은 `GoAsync` / `Back`으로 히스토리를 관리하고, 이전 화면은 비활성만 시켜 재생성 비용을 없앰.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/UI/Page/PageFactory.cs#L27-L70

팝업은 요청을 큐에 넣고 `OnUpdate`에서 한 프레임에 하나씩 생성함.
같은 프레임에 팝업 3개가 요청돼도 순서대로 뜨고, 서로의 생성 타이밍이 겹치지 않음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/UI/Popup/PopupFactory.cs#L19-L36

**2. Page 생명주기를 6단계로 규격화**

`OnCreate → OnLoad → OnTransitionStart → OnResume → OnPause → OnFinish`.
데이터 로딩은 `OnLoad`, 연출은 `OnTransitionStart`에 두어 로딩 중 애니메이션이 튀지 않게 함.

`OnEnable`에서 `Animator`를 꺼두고 전환 시점에만 켜는데,
프리팹 생성 즉시 애니메이터가 1프레임 재생돼 화면이 깜빡이는 문제가 있었음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/UI/Page/PageBehaviour.cs#L12-L41

**3. 화면 간 데이터 전달을 쿼리 문자열로**

화면마다 전용 파라미터 클래스를 만들면 화면 수만큼 클래스가 늘어남.
URL 쿼리 형식(`key=value&key2=value2`)을 리플렉션으로 필드에 주입함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/UI/Page/PageQuery.cs#L7-L27

`UIManager`에 인스펙터로 경로와 쿼리를 넣고 바로 진입하는 테스트 버튼을 둬서,
특정 화면을 확인할 때 타이틀부터 거치지 않아도 됨 (에디터 전용).

<br></br>

### 튜토리얼

특정 UI를 강조하고 그것만 누를 수 있게 막는 형태.
튜토리얼이 UI를 직접 참조하면 화면이 바뀔 때마다 튜토리얼 코드를 고쳐야 하고,
아직 생성되지 않은 UI를 가리키면 예외가 남.

**1. 대상을 직접 참조하지 않고 앵커 ID로 지목**

강조할 UI에 `TutorialAnchor`를 붙이고 ID를 부여함.
튜토리얼 데이터는 ID만 들고 있어서, 대상 UI의 위치·계층이 바뀌어도 튜토리얼은 그대로 동작함.

앵커는 `OnEnable`에서 자신을 정적 레지스트리에 등록하고 `OnDisable`에서 제거함.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialAnchor.cs#L18-L37

용도에 따라 둘로 나눔.

| 클래스 | 역할 |
|---|---|
| `TutorialAnchor` | 대기 신호만 보냄. 화면 루트처럼 강조·클릭이 필요 없는 대상 |
| `TutorialClickAnchor` | rect 좌표까지 등록하고 클릭 이벤트 발행. 강조 + 클릭 진행이 필요한 대상 |

클릭이 필요 없는 앵커까지 `RectTransform`과 `IPointerClickHandler`를 갖게 하면
쓰지 않는 요구사항이 붙으므로, 상속으로 필요한 쪽만 확장함.

**2. 대상이 아직 없으면 등록될 때까지 대기**

가장 문제가 됐던 부분. 화면 전환 직후 튜토리얼이 시작되면 대상 UI가 아직 생성되지 않음.
"몇 프레임 기다린다" 같은 처리는 기기 성능에 따라 깨짐.

앵커가 등록되어 있으면 바로 진행하고, 없으면 **등록 이벤트를 구독하고 멈춤**.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialContents.cs#L184-L204

기다리던 앵커가 등록되는 순간 구독을 해제하고 이어감.
대기 시간이 아니라 실제 등록 시점을 기준으로 하므로, 로딩이 느린 기기에서도 순서가 어긋나지 않음.

**3. 진행 조건을 스텝 데이터가 결정**

스텝마다 강조 여부·설명 페이지·클릭 영역을 `TutorialStep`에 두어, 연출 조합을 데이터로 바꿈.

```csharp
public class TutorialStep
{
    public bool UseHoleMesh;          // 대상 강조 + 그 부분만 클릭 통과
    public ETutorialAnchorID AnchorID;
    public bool UseCenterPage;        // 중앙 설명 패널
    public string[] Pages;            // 여러 장이면 순서대로
    public bool UseCornerText;
    public bool UseAdvanceArea;       // 아무 곳이나 눌러 넘기기
}
```

`UseHoleMesh`(대상을 눌러야 진행)와 `UseCenterPage`(설명을 읽고 넘김)는 진행 방식이 충돌하므로,
`OnValidate`에서 동시 활성화를 에디터 단계에서 잡아냄.

```csharp
if (UseHoleMesh && UseCenterPage)
    Debug.LogError("[TutorialStep] UseHoleMesh와 UseCenterPage를 동시에 활성화할 수 없습니다.");
```

페이지가 여러 장인 스텝은 `AdvancePage()`가 페이지를 넘기다가, 마지막에서 다음 스텝으로 넘어감.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialContents.cs#L154-L182

**4. 강조 구멍을 메시로 직접 생성**

화면을 어둡게 덮되 대상만 뚫려 보여야 함.
이미지 마스크로 하면 대상 크기마다 이미지가 필요하므로, 링 형태 메시를 코드로 생성함.

구멍 둘레를 48등분해 안쪽 원과 바깥 사각형 사이를 삼각형으로 채움.
안쪽에는 정점을 두지 않으므로 그 부분이 그려지지 않아 구멍이 됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialHoleMesh.cs#L85-L117

구멍 위치·크기는 앵커의 `GetWorldCorners`로 계산하므로, 대상 크기가 달라도 자동으로 맞음.
스텝이 바뀔 때는 `Tween.Custom`으로 이전 구멍에서 새 위치로 이동시켜, 끊기지 않고 이어짐
(첫 스텝만 즉시 배치 — 이전 위치가 없어 화면 중앙에서 날아오는 것처럼 보이므로).

**5. 클릭 통과 판정을 원이 아닌 rect로**

구멍은 원인데 버튼은 사각형이라, 원 기준으로 판정하면 버튼 모서리가 눌리지 않음.
`ICanvasRaycastFilter`를 구현해 **강조는 원, 클릭 판정은 대상 rect**로 분리함.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialHoleMesh.cs#L52-L63

`IsRaycastLocationValid`가 rect 안이면 `false`를 반환해 오버레이가 클릭을 받지 않고,
그 아래 실제 버튼으로 입력이 내려감. 나머지 영역은 오버레이가 전부 막음.

**6. 시작 조건을 게임 이벤트에 연결**

튜토리얼 시작을 각 기능 코드에서 호출하면, 기능마다 튜토리얼을 아는 코드가 박힘.
게임 이벤트(던전 입장, NPC 대화 등)를 구독해 조건이 맞는 튜토리얼을 찾아 실행함.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialContents.cs#L64-L71

`TutorialSO`에 트리거 타입과 파라미터를 두어, 시작 조건 변경이 에셋 수정으로 끝남.
기능 쪽은 `GameEventReporter.Report()`만 부르고 튜토리얼의 존재를 모름.

**7. 본 튜토리얼은 서버에 기록**

이미 본 튜토리얼을 다시 띄우면 안 되고, 기기를 바꿔도 유지돼야 함.
본 목록을 `HashSet`으로 들고 있다가 저장 시점에 클라우드로 보냄.
https://github.com/hyeon0316/Project-Code-Repository/blob/5fbf1012452506895e258c432aa244cff8b6437a/UPCOMING-RELEASES/Scripts/GamePlay/Tutorial/TutorialContents.cs#L230-L246

매번 저장하지 않고 `m_IsDataChanged`가 켜졌을 때만 보냄.
저장 시점은 주기적 저장·앱 종료·백그라운드 전환 세 곳에 걸어, 강제 종료에도 기록이 남게 함.

`ETutorialID`는 int로 직렬화되므로 **enum 순서를 바꾸면 기존 유저의 기록이 어긋남**.
주석으로 명시해두고 새 항목은 뒤에만 추가함.

<br></br>

### 데이터 테이블 파이프라인

기획 수치가 코드에 있으면 밸런스 수정마다 빌드가 필요함.
수치를 전부 엑셀에 두고, 배포 없이 바꿀 수 있게 함.

테이블이 **서버 검증에 쓰이는지**에 따라 경로가 갈림.
서버가 읽어야 하는 테이블은 뒤끝 콘솔에 엑셀을 그대로 등록하고,
클라이언트만 쓰는 테이블은 JSON으로 변환해 앱에 같이 실음.

```
[서버 검증 대상] 기획 엑셀(.xlsx)
    ↓ 뒤끝 콘솔에 엑셀 직접 등록 (변환 없음)
서버 CDN ────────────────┐
                         │
[클라 전용]   기획 엑셀(.xlsx)
    ↓ ExcelToJsonConverter (에디터 툴)
JSON → Addressables ─────┤
                         ↓ 게임 시작 시
              BDatabase.Init() → JsonDispatcher → 각 테이블 클래스
```

서버 CDN 테이블은 콘솔에서 수정하면 **앱 배포 없이 즉시 반영**됨.
클라 전용은 Addressables에 포함되므로 리소스 갱신이 필요하지만, 대신 서버 왕복이 없음.

**1. 클라 전용 테이블을 JSON으로 변환**

뒤끝 콘솔을 거치지 않는 테이블은 에디터 툴로 직접 변환함.
시트를 `DataTable`로 읽어 그대로 직렬화함.
전부 빈 열은 제거해서, 기획자가 작업 중 남긴 빈 칸이 JSON에 들어가지 않게 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/ExcelToJsonConverter.cs#L176-L199

`~$`로 시작하는 엑셀 임시 파일은 정규식으로 걸러냄 (파일을 열어둔 채 변환하면 잡히는 문제).

**2. 테이블 로딩 경로를 둘로 나눔**

서버 검증에 쓰이는 테이블은 CDN에서, 클라이언트 전용(연출·표기)은 Addressables에서 받음.
클라 전용은 CDN 왕복이 없으므로 로딩이 빠르고, 서버 검증 대상만 서버와 동기화하면 됨.

기준은 **서버가 그 값을 알아야 하는가**임.
아이템 가격·보상 수량처럼 서버가 검증에 쓰는 값은 CDN에 둬야 클라이언트와 같은 수치를 봄.
스킬 설명 문구·이펙트 연출값처럼 서버가 쓰지 않는 것은 굳이 CDN에 올리지 않음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/BDatabase.cs#L14-L29

로컬 테이블 9종은 `UniTask.WhenAll`로 동시에 로드함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/BDatabase.cs#L102-L124

`ItemTable.Build`가 `EffectDic`을 참조하므로, 로딩이 `BuildData()`보다 먼저 끝나야 하는 순서 의존이 있음.

**3. 테이블명 → 파서 매핑을 딕셔너리로**

테이블이 35종인데 `switch`로 분기하면 길이가 계속 늘어남.
이름과 파서를 짝지은 딕셔너리 하나로 두고, 등록되지 않은 키는 경고만 남기고 넘어감.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/FrameWork/JsonDispatcher.cs#L9-L45

파싱 실패는 잡아서 **어느 테이블인지 로그로 남기고 다시 던짐**.
테이블 하나가 잘못되면 게임이 시작되면 안 되지만, 35개 중 어느 것인지는 알아야 하기 때문.

**4. 로딩 실패를 예외로 처리**

CDN 실패·로컬 테이블 누락은 전부 `throw`함.
데이터 없이 진행하면 이후에 엉뚱한 지점에서 `NullReference`가 나서 원인 추적이 어려워짐.

<br></br>

### 에디터 툴

작업 결과를 눈으로 확인할 수 없으면 실수가 늘어남.
숫자와 리스트로 편집하던 두 가지를 시각 편집 도구로 만듦.

#### 던전 맵 편집기

던전 맵이 노드 그래프(전투 → 분기 → 상점 → 보스) 구조인데,
인스펙터에서 노드와 연결을 리스트로 편집하면 형태를 볼 수 없어 실수가 잦았음.

**1. GraphView로 시각 편집기 구성**

`EditorWindow` + `GraphView`(UIElements)로 노드를 드래그하고 선으로 연결하는 창을 만듦.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/DungeonMapEditor.cs#L26-L67

행·열 수를 넣고 생성 버튼을 누르면 격자 형태로 노드를 자동 배치하고,
`.asset`으로 저장해 런타임에서 그대로 읽음.

**2. 노드 종류로 포트를 제한**

시작 노드에 입력 포트가 있거나 종료 노드에 출력 포트가 있으면 순환이 생김.
포트 생성 시점에 타입으로 막음.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/DungeonMap/NodeView.cs#L31-L57

`GetCompatiblePorts`로 자기 자신과 같은 방향 포트도 연결 후보에서 제외함.

**3. 노드 위치를 에셋에 반영**

`SetPosition` 오버라이드로 드래그한 좌표를 SO에 바로 기록함.
창을 닫았다 열어도 배치가 유지되고, 런타임 맵 UI가 같은 좌표를 그대로 사용함.

#### 레이아웃 편집 인스펙터 (UIMultiViewInspector)

`UIMultiView`는 자식들의 배치를 View 이름별로 들고 있는데,
기본 인스펙터로 편집하면 리스트 안의 좌표 숫자를 직접 고쳐야 해서 결과를 볼 수 없음.

씬에서 **눈으로 보면서 옮긴 배치가 그대로 저장되는** 인스펙터를 만듦.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/UIMultiViewInspector.cs#L1-L144

**1. 저장 버튼을 누르지 않아도 반영되게 함**

`OnEnable` / `OnDisable`에서 현재 배치를 저장함.
다른 오브젝트를 선택하는 순간 인스펙터가 닫히면서 저장되므로,
씬에서 위치를 옮기고 저장을 잊는 경우가 없어짐.

```csharp
public void OnDisable()
{
    UIMultiView multiView = target as UIMultiView;
    bool canSave = multiView.Save();
    if (canSave)
    {
        EditorUtility.SetDirty(multiView);
    }
}
```

`Save()`의 반환값을 확인해 성공했을 때만 `SetDirty`를 호출함.
비활성 오브젝트는 자식 배치를 읽을 수 없어 `Save()`가 `false`를 반환하는데,
이때 `SetDirty`까지 부르면 **빈 배치가 저장된 것으로 표시되어 기존 데이터가 날아감**.

**2. View 전환 시 두 번 저장**

전환에서 가장 까다로웠던 부분. 이전 View의 편집 내용을 지키면서 새 View를 불러와야 함.

```csharp
int newSelectIndex = EditorGUILayout.Popup(prevSelectIndex, keyNames);
if (prevSelectIndex != newSelectIndex)
{
    //원래 자리 복원해주기
    multiView.Save();                        // ① 이전 View 편집분 보존

    multiView.SetSelectView(keyNames[newSelectIndex]);  // ② 새 View 적용
    multiView.Save();                        // ③ 새 View 기준으로 다시 기록

    EditorUtility.SetDirty(multiView);
}
```

①을 빠뜨리면 방금 옮긴 배치가 전환과 동시에 사라짐.
③이 필요한 이유는 `SetSelectView`가 자식들의 실제 Transform을 새 View 값으로 덮어쓰기 때문.
이 시점의 씬 상태가 곧 새 View의 내용이므로, 다시 저장해 둘을 일치시킴.

**3. 인스펙터에서 수정하면 씬에 즉시 반영**

위치·크기·활성 상태를 필드로 그리고, 값을 받은 직후 `Apply()`를 호출함.
입력하는 동안 씬 뷰가 실시간으로 따라오므로 숫자를 감으로 맞추지 않아도 됨.

```csharp
widget.AnchoredPosition = EditorGUILayout.Vector2Field("Position", widget.AnchoredPosition);
widget.SizeDelta = EditorGUILayout.Vector2Field("Size", widget.SizeDelta);
widget.Active = EditorGUILayout.Toggle("Active", widget.Active);
widget.Apply();
```

각 자식은 `GUILayout.BeginVertical(widget.Object.name, guiStyle)`로 오브젝트 이름을 단 박스에 묶어,
자식이 많아도 어느 것을 편집 중인지 구분됨.

**4. 삭제된 자식과 사라진 View 처리**

`OnInspectorGUI` 진입 시 `RemoveDeletedChildrens()`를 먼저 호출해 파괴된 오브젝트 참조를 정리함.
GUI를 그리는 도중에 null을 만나면 인스펙터 전체가 예외로 멈추기 때문에, 그리기 전에 걸러냄.

선택 중이던 View가 목록에 없으면(다른 사람이 지웠거나 이름이 바뀐 경우) 첫 번째 View로 되돌림.

```csharp
if (!isContains)
{
    multiView.SetSelectView(keyNames[0]);
}
```

View 추가는 중복 키를 막고, 실패 사유를 다이얼로그로 알림
(빈 이름 / 이미 존재하는 키를 구분해서 표시).

<br></br>

### 빌드 - 배포 자동화

빌드마다 버전 올리고, Addressables 굽고, AAB 만들고, S3 올리고, Play Console에 올리는 과정을
수동으로 하면 순서를 빠뜨리기 쉬움. GitHub Actions로 묶음.

**1. 워크플로를 단계별로 분리**

`release.yml`이 세 개의 하위 워크플로를 순서대로 호출함.

```
release.yml (workflow_dispatch)
├── _build-addressables.yml  : Addressables 빌드 → S3 업로드
├── _build-aab.yml           : 버전 증가 → AAB 빌드 → 서명
└── _upload-play.yml         : Play Console 내부 테스트 트랙 업로드
```

각 단계를 입력값으로 건너뛸 수 있게 해서, Addressables만 갱신하거나 빌드만 확인하는 경우를 나눔.
`dry_run`으로 S3·Play 업로드 없이 빌드만 돌려볼 수 있음.

**2. 이전 단계 실패 시 다음 단계 차단**

`needs`와 `if` 조건으로, 앞 단계가 성공했거나 건너뛴 경우에만 진행함.

```yaml
if: ${{ !cancelled() && !inputs.skip_aab &&
        (needs.build-addressables.result == 'success' ||
         needs.build-addressables.result == 'skipped') }}
```

Addressables 빌드가 실패했는데 AAB만 올라가면 리소스와 앱 버전이 어긋나므로 막아야 했음.

**3. 버전 증가를 빌드 스크립트에서 처리**

`patch / minor / major / none`을 인자로 받아 `bundleVersion`을 올림.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/CIBuild.cs#L12-L60

키스토어 정보도 인자로 받으므로 저장소에 서명 정보가 남지 않음 (Actions Secrets 사용).

**4. Addressables 중복 빌드 차단**

`BuildPlayerContent`를 별도 단계에서 이미 실행했는데,
플레이어 빌드가 또 굽는 설정이면 시간이 두 배로 들고 결과가 덮임.
플레이어 빌드 직전에 옵션을 꺼둠.

```csharp
var settings = AddressableAssetSettingsDefaultObject.Settings;
if (settings != null)
    settings.BuildAddressablesWithPlayerBuild =
        AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
```

**5. 실패 시 종료 코드 반환**

`EditorApplication.Exit(1)`로 CI가 실패를 인지하게 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Editor/AddressableAutomation.cs#L36-L57

이걸 넣기 전에는 Unity 배치 모드가 에러 로그만 남기고 0으로 끝나서, 실패한 빌드가 업로드되는 일이 있었음.

<br></br>

### 이벤트 버스

로직(Contents)이 UI를 직접 참조하면 UI가 없는 상태(씬 전환 중, 팝업 미생성)에서 예외가 남.
단방향 이벤트 버스로 로직 → UI 방향만 허용함.

**1. 제네릭 하나로 모든 콘텐츠가 재사용**

`GlobalEvent<T>`가 등록·해제·전송을 담당하고, 각 Contents가 자기 파라미터 타입으로 인스턴스를 가짐.
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Shared/GlobalEvent.cs#L9-L45

`where T : struct` 제약으로 파라미터가 값 타입임을 보장해, 전송 후 발신자가 내용을 바꿀 수 없게 함.

**2. 즉시 호출이 아니라 큐에 쌓음**

`Send`는 큐에 넣기만 하고 `OnUpdate`에서 꺼내 전달함.
로직 처리 도중에 UI 갱신이 끼어들어 컬렉션이 변경되는 상황을 막음.

**3. 파괴된 핸들러를 순회 중 제거**

UI가 파괴됐는데 해제를 빠뜨리면 다음 이벤트에서 예외가 남.
역순 순회로 발견 즉시 제거함 (순회 중 제거해도 인덱스가 밀리지 않음).
https://github.com/hyeon0316/Project-Code-Repository/blob/dacf77958b6edda7465dbbfa53ae62879ee8301f/UPCOMING-RELEASES/Scripts/Shared/GlobalEvent.cs#L47-L70

Unity 오브젝트는 `== null` 오버로딩 때문에 일반 null 검사로 파괴 여부를 알 수 없어,
`is Object unityObj && unityObj == null` 로 따로 확인함.
