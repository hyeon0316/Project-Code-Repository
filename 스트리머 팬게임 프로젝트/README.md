# 스트리머 팬게임 프로젝트 - TreasureWak

## 프로젝트 소개
- 개발기간 : 2022/07 ~ 2022/12
- 장르 : 멀티 오토배틀러(2D)
- 플랫폼 : PC
- 도구 : Unity, C#
- 인원 : 13인 (클라이언트 1, 서버 1, 기획 2, 그래픽 8)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 클라이언트
- 그 외 서버 프로그래머 1명, 기획자 2명, 그래픽 8명

## 담당 범위
**클라이언트 전체** — 아이템 시스템 / 전투 화면 / UI / 연출 / 서버 연동

네트워크 계층(`NetworkManager`, 패킷 구조체, 직렬화 스트림)은 서버 개발자가 작성했고,
본인은 이를 호출해 게임 로직과 화면에 반영하는 부분을 담당함.

> 이 문서는 **서버가 계산한 결과를 클라이언트가 어떻게 받아 게임으로 만들었는가**에 대한 기록.
> 통신 계층 자체의 구현이 아니라, 그 위에서 내린 설계 판단을 다룸.

## 프로젝트 구조
서버가 전투 결과를 전부 계산하고 클라이언트는 그 결과를 재생만 하는 구조.
클라이언트에 판정 로직이 없으므로 8인 전원이 같은 결과를 보고, 클라이언트 조작으로 결과를 바꿀 수 없음.

```
[서버 개발자 담당]
NetworkManager ── TCP 소켓, 패킷 송수신
Packet ────────── 패킷 구조체 30여 종
Read/WriteMemoryStream ── 직렬화

        ↕ 요청 전송 / 결과 수신

[본인 담당 — 클라이언트 전체]
PlayerManager   : 8인 Player 데이터, 조회·사망·체력 갱신
WindowManager   : Title / Lobby / Select / InGame 화면 전환
DataManager     : 프리팹·스프라이트 캐싱, 씬 전환에도 유지
SoundManager    : BGM / 효과음
ToolTipManager  : 아이템·라운드 툴팁

InGame
├── Ready  : 아이템 뽑기 / 배치 / 조합 (드래그 앤 드롭), 46종 아이템
└── Battle : 전장 8개 동시 구성, 아이템 순차 발동 연출, 상태이상 UI
```

## 코드

### 서버 연동 설계

**1. 조작은 요청만, 확정은 서버 결과로**

드래그로 아이템을 옮겨도 클라이언트가 슬롯을 바꾸지 않음.
이동·업그레이드 **요청**을 보내고, 슬롯이 실제로 바뀌는 것은 서버의 인벤토리 정보 패킷을 받은 뒤.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L290-L400

메모리를 조작해도 서버 상태가 바뀌지 않고, 다음 인벤토리 패킷이 오면 원래대로 덮어써짐.
응답을 기다리는 동안 아이템 알파를 0으로 낮춰 처리 중임을 표시함.
사용자가 "먹통"으로 느끼지 않으면서도, 클라이언트가 결과를 미리 확정하지 않는 상태를 유지함.

**2. 인벤토리를 부분 갱신하지 않고 전체 재구성**

아이템 이동·업그레이드·조합은 결과가 여러 슬롯에 동시에 걸림.
개별 갱신은 처리 순서에 따라 결과가 달라져서, 패킷이 뒤섞이면 클라이언트 상태가 서버와 벌어짐.

서버가 인벤토리 전체를 내려주면 슬롯 16개를 모두 비우고 다시 만드는 방식으로 통일.
어떤 순서로 도착해도 마지막 패킷 기준으로 수렴하므로 멱등하게 동작함.

재구성 직후 `UpdateEquipEffect()`로 장착 효과를 다시 계산해, 아이템 배치와 스탯이 항상 같은 시점에 맞춰지도록 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Player.cs#L106-L120

**3. 서버가 보내지 않는 값을 상태 차이로 유도**

서버는 데미지 수치를 따로 보내지 않고 갱신된 체력만 보냄.
직전 체력을 보관해뒀다가 차이를 구해 플로팅 텍스트로 띄움.
https://github.com/hyeon0316/Project-Code-Repository/blob/77274518d5c242d73161f9012db9fe19715d5f29/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/BattleCharacter.cs#L132-L138

**4. 상태 스냅샷에서 이벤트를 추출**

폭탄 아이템은 "설치 → 유지 → 폭발"로 이어지지만, 서버는 폭발 시점을 알리지 않고 설치 플래그만 내려줌.
효과음 재생에는 순간이 필요했는데, 패킷 규격이 이미 확정된 상태라 이벤트 패킷을 추가할 수 없었음.

상태만 내려주는 프로토콜과 이벤트가 필요한 연출 사이를 클라이언트에서 메운 방식.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/BattleCharacter.cs#L162-L184

**5. 이탈자를 기존 사망 경로로 환원**

8인이 붙는 게임이라 중도 이탈이 상시 발생.
이탈자를 그대로 두면 전투 상대 배정이 어긋나고, 나간 플레이어의 슬롯을 참조해 예외가 남.

서버가 이탈 알림을 보내면 `UpdateHp(-999)`로 즉시 사망시킴.
사망 경로(`Die()`)가 이미 현황판 갱신·패배 애니메이션·전장 정리를 담당하고 있으므로,
이탈 전용 처리를 새로 만들지 않고 기존 경로를 그대로 태움.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Player.cs#L200-L228

`IsDisconnect` 플래그로 중복을 막아, 같은 알림이 두 번 와도 사망이 두 번 실행되지 않게 함.

**6. 홀수 인원 대응 규약을 화면에 반영**

인원이 홀수가 되면 상대가 없는 플레이어가 생김.
서버가 상대 ID를 비트 반전(`~id`)한 음수로 내려주기로 규약을 정하고,
클라이언트는 음수를 유령으로 판정해 원래 ID를 복원함.

유령은 화면에 정상적으로 보이되 실제 체력 계산에서 제외되므로, 부전승 없이 매 라운드 전투가 성립함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/Battle.cs#L103-L133

<br></br>

### 아이템 드래그 앤 드롭

준비 단계에서 아이템을 배치·합성·판매하는데, 조작이 전부 드래그 하나로 이루어짐.
놓는 위치(장착 슬롯 / 보관 슬롯 / 판매 영역 / 조합 슬롯)와 대상 슬롯 상태(비었는지, 같은 아이템인지, 업그레이드 가능한지)의
조합마다 결과가 달라 분기가 많았음.

**1. 놓은 위치를 레이어로 판별**

드롭 지점에서 `Physics2D.Raycast`를 쏘고 레이어 이름으로 목적지를 구분.
UI 좌표를 직접 비교하지 않으므로 해상도나 레이아웃이 바뀌어도 판정이 유지됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L253-L287

레이캐스트 마스크에 세 레이어만 넣어 후보를 먼저 좁히고, 아무것도 맞지 않으면 원위치로 되돌림.

**2. 드래그 중 아이템의 레이캐스트를 끔**

드래그 중인 아이템이 커서를 따라다니므로, 그대로 두면 자기 자신이 먼저 맞아 대상 슬롯을 알 수 없음.
`raycastTarget`을 끄고 드래그하고 종료 시 되돌림.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L206-L214

부모를 최상위 `DragItem`으로 옮겨, 드래그 중인 아이템이 다른 UI에 가려지지 않게 함.

**3. 슬롯 인덱스를 하나의 체계로 통일**

장착 슬롯 0~5, 보관 슬롯 6~15로 인덱스를 통일해 서버 요청에 그대로 실음.
슬롯이 자기 인덱스를 계층 구조에서 산출하므로 호출부가 소속을 신경 쓰지 않아도 됨.

```csharp
public int GetSlotIndex()
{
    return transform.GetSiblingIndex() + (transform.parent.parent.name.Equals("UnUsingInventory") ? 6 : 0);
}
```

**4. 조합 슬롯과 인벤토리의 동기화**

재조합 슬롯에 올린 아이템이 인벤토리에서 이동·업그레이드되면 조합 슬롯의 참조가 깨짐.
슬롯을 옮길 때마다 `CheckDuplicationIndex()`로 조합 슬롯에 같은 인덱스가 있는지 확인하고 제거함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Ready/ReCombination.cs#L33-L45

**5. 전투 전환 시 드래그를 강제 취소**

준비 시간이 끝났는데 아이템을 든 상태면, 아이템이 슬롯 밖에 남아 다음 단계로 넘어감.
타이머가 0이 되거나 화면 전환 키가 눌리면 드래그를 중단하고 원위치로 되돌림.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L129-L146

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `DragItemSlot()` 중첩 분기 | 슬롯 종류 × 존재 여부 × 업그레이드 가능 여부가 5단계 `if`로 겹쳐 있고, 각 분기가 거의 같은 4줄(효과음·알파·요청·중복확인)을 반복 | 판정 결과를 `enum`으로 먼저 확정하고 실행은 한 곳에서 |
| `GameObject.Find("DragItem")` | 드래그를 시작할 때마다 이름으로 씬을 탐색 | 인스펙터 참조 주입 |
| `Camera.main` 반복 호출 | 드래그 이벤트마다 호출 | `Start`에서 캐싱 |
| 슬롯 인덱스를 부모 이름으로 판별 | `parent.parent.name.Equals("UnUsingInventory")`라 오브젝트 이름을 바꾸면 런타임에 조용히 깨짐 | 슬롯이 자기 소속을 필드로 보유 |
| 아이템 존재 확인이 `childCount` | `transform.childCount == 1` 비교라, 이펙트 오브젝트가 하나라도 붙으면 판정이 무너짐 | 슬롯이 아이템 참조를 직접 보유 |
| `Update()`에서 드래그 취소 감시 | 모든 아이템 인스턴스가 매 프레임 조건을 확인 | 단계 전환 시점에 한 번만 통보 |

<br></br>

### 아이템 효과 데이터

아이템이 46종이고 각각 3단계 업그레이드를 가짐.
효과 수치와 설명 문구가 따로 관리되면 밸런스 수정 때 한쪽만 고쳐져 화면 표기와 실제 효과가 어긋남.
같은 클래스 안에서 수치와 문구를 같은 자리에 두어 함께 바뀌도록 함.

- 수정

**1. 추상 클래스로 아이템 계약을 고정**

`Item`이 발동·업그레이드 적용·설명 생성을 추상 메서드로 선언하고, 46종이 각자 구현.
드래그·툴팁·별 표시 같은 공통 동작은 부모가 전부 처리하므로 파생 클래스는 효과만 씀.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L100-L120

**2. 수치와 설명 문구를 같은 클래스에 둠**

`ApplyUpgrade()`가 등급별 수치를 정하고, `SetEquipEffectText()`가 그 수치를 문자열 보간으로 문구에 넣음.
수치를 고치면 표기가 자동으로 따라오므로 둘이 어긋날 수 없음.

```csharp
public override void ApplyUpgrade()
{
    switch (Upgrade)
    {
        case 0: _counterAttack = 5; break;
        case 1: _counterAttack = 7; break;
        case 2: _counterAttack = 10; break;
    }
}

public override void SetEquipEffectText()
{
    _effect = $"{_fixedDefense} <color=#4aa8d8>방어도</color>를 얻습니다.\n상대방이 [공격] 아이템 발동 시, " +
              $"<color=yellow>{_counterAttack}</color> 만큼 데미지를 주는 버프를 얻습니다.";
}
```

**3. 생성 경로를 한 곳으로**

`ItemSlot.AddNewItem()`이 코드로 프리팹을 찾아 생성하고, 수치 적용·문구 생성·별 표시까지 순서대로 호출.
아이템이 만들어지는 경로가 하나뿐이라 초기화 누락이 발생하지 않음.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/ItemSlot.cs#L72-L86

`EItemCode` 열거형 양 끝에 `Minimum` / `Maximun` 감시값을 두고 `Debug.Assert`로 범위를 확인해,
서버가 잘못된 코드를 보내면 개발 중에 드러나게 함.

**4. 여러 번 공격하는 아이템만 인터페이스로 분리**

연타 공격은 코루틴으로 연출해야 하므로, 해당 아이템만 `IManyAttack`을 구현.
전체 아이템에 코루틴을 강제하지 않고 필요한 것만 추가 계약을 갖게 함.

```csharp
public interface IManyAttack
{
    IEnumerator AttackCo(BattleCharacter player, int damage);
}
```

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 아이템 하나가 클래스 하나 | 46개 파일 대부분이 `switch` 3줄과 문구 1줄뿐. 밸런스 수정에 프로그래머와 빌드가 필요 | 수치를 `ScriptableObject` 테이블로 빼고, 행동 종류만 클래스로 |
| 등급 분기가 `switch` 3케이스 반복 | 46종에 같은 형태가 복사되어 있고, 4번째 등급을 넣으려면 46곳을 고쳐야 함 | 등급별 수치를 배열로 두고 인덱싱 |
| 설명 문구가 코드에 하드코딩 | 현지화가 불가능하고 오타 수정에 빌드가 필요 | 문구 템플릿을 데이터로 분리하고 수치만 주입 |
| 수치가 서버와 이중 관리 | 실제 계산은 서버가 하고 클라이언트는 표기용 수치를 따로 들고 있어, 한쪽만 고치면 표기와 실제가 어긋남 | 서버와 클라이언트가 조회 하는 별도 데이터베이스 구성 |

<br></br>

### 전투 화면 구성

8인이 4쌍으로 나뉘어 동시에 싸우고, 플레이어는 전장을 자유롭게 둘러볼 수 있음.
전투 판정은 서버에 있으므로 클라이언트는 "누가 몇 번 슬롯 아이템을 발동했다"는 신호를 받아 연출만 재생함.

**1. 전장 8개를 미리 만들어두고 배치만 바꿈**

라운드마다 전장을 생성/파괴하지 않고, 화면 밖(`y = 1200`)으로 밀어둔 뒤 필요한 것만 끌어옴.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/BattleController.cs#L70-L126

**2. 자신의 전투를 항상 왼쪽에 고정**

같은 대결이라도 보는 사람에 따라 좌우가 달라야 함(내 캐릭터가 왼쪽).
그래서 한 쌍의 대결을 전장 2개에 서로 반대로 구성하고, 보는 사람에 맞는 쪽을 보여줌.
자신의 대결을 먼저 0번·1번 전장에 배치한 뒤 나머지를 채워, 어떤 대진이 나와도 자기 화면이 일정함.

**3. 발동 순서를 슬롯 배치와 분리**

화면상 슬롯 배치와 실제 발동 순서가 다름(위아래 지그재그).
순서 매핑 배열을 두어 서버가 보낸 논리적 인덱스를 화면 슬롯 인덱스로 변환함.

```csharp
public void ActiveItemNetwork(byte slotIndex)
{
    int[] slotIndexOrder = new int[6] { 0, 3, 1, 4, 2, 5 };
    int activeIndex = slotIndexOrder[slotIndex];
    // ...
}
```

준비 단계에서 전투 단계로 아이템을 옮길 때도 같은 방식으로 순서를 변환함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Player.cs#L251-L263

**4. 보고 있는 전장만 연출**

전장 8개가 동시에 진행되지만 화면에 보이는 것은 하나.
`IsView` 플래그로 보고 있는 전장에서만 플로팅 텍스트와 효과음을 재생해, 나머지 7개의 연출 비용을 없앰.

**5. 상태이상과 아이템 버프를 UI로 분리**

서버가 상태이상(공격력·방어도·약화·출혈·치유감소)과 아이템별 버프(반격·폭탄·피해무시 등)를
한 패킷에 담아 보내므로, 표시 담당을 `StatusUI`와 `ItemBuffUIController`로 나눔.
성격이 다른 두 종류가 같은 갱신 함수에 섞이지 않게 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/BattleCharacter.cs#L151-L184

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 매핑 배열 매 호출 생성 | `ActiveItemNetwork()`마다 `new int[6]` | `static readonly` 필드로 |
| `SetBattleRound()` 인덱스 계산 | `while` 안에서 두 인덱스를 조건부로 증가시켜, 인원 조합에 따라 배치가 어긋날 여지가 있음 | 대진 쌍을 먼저 리스트로 만들고 순회 |
| 상태 UI가 패킷 구조에 결합 | 효과가 추가될 때마다 `UpdateStatusUI` / `UpdateItemBuffUI`를 함께 고쳐야 함 | 효과를 `(타입, 수치)` 배열로 받아 순회하도록 규격 제안 |






      
      
