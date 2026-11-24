# 2021 GSTAR Project - 용사(?)의 모험

## 프로젝트 정보
- 기간 : 2021/09/27 ~ 2021/11/16
- 장르 : RPG(3D, Polygon)
- 플랫폼 : PC
- 도구 : Unity, C#
- 인원 : 3인 (프로그래머 2, 그래픽/사운드 1)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 기획, 프로그래머
- kiom485 : 프로그래머
- Maruber : 그래픽, 사운드, UI 일부

## 담당 범위
**적 AI와 인벤토리**

| 영역 | 내용 |
|---|---|
| 적 | 생명체 공통 부모 `LivingEntity`, 근접 `Enemy`, 원거리 `Enemy_Far`, 보스 `Boss`와 스킬 판정 3종, 스폰, 데미지 텍스트·HP바 |
| 인벤토리 | 인벤토리·장비창·퀵슬롯, 드래그 앤 드롭, 수량 입력, 툴팁, 스킬 쿨타임 UI |
| 기타 | 로딩 씬, 타이틀 |

플레이어는 팀원과 공동 작업, NPC·퀘스트·대화와 플레이어 스킬은 팀원 담당.

## 코드

### 적 AI

`LivingEntity`가 체력·피격·사망을 담고, 플레이어와 적 3종(`Enemy`, `Enemy_Far`, `Boss`)이 이를 상속함.
공격하는 쪽은 상대가 누구든 `LivingEntity.OnDamage()`만 호출함.
공통 부모의 형태는 유니티 입문서 예제 구조를 바탕으로 함.

**1. 원거리 적의 정지 거리를 개체마다 다르게**

원거리 적이 모두 같은 거리에서 멈추면 플레이어 주위에 한 줄로 늘어섬.
생성 시 `stoppingDistance`를 7~10 사이에서 무작위로 정해, 여러 마리가 서로 다른 거리에 흩어져 공격함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Enemy_Far.cs#L97-L100

**2. 아이템별 확률 드랍**

적마다 드랍 아이템 목록과 확률 배열을 인스펙터에서 지정하고, 사망 시 아이템마다 따로 확률을 굴림.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Enemy.cs#L266-L305

<br></br>

### 보스 패턴

**1. 거리와 페이즈에 따른 행동 선택**

사거리 안이면 기본 공격, 사거리 밖이면 추적하면서 일정 확률로 스킬을 씀.
2페이즈 스킬과 장판은 체력이 기준 이하로 내려간 뒤에만 후보에 들어감.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Boss.cs#L197-L243

**2. 2페이즈 진입 시 그로기 후 장판 예고**

2페이즈에 들어가면 보스가 5초간 기절해 플레이어에게 공격 기회를 주고, 기절이 끝나면 플레이어 머리 위에 표식을 띄워 장판 패턴을 예고함.
장판은 발동 전까지 플레이어를 따라오다가, 발동하는 순간 그 자리에 멈춰 터짐.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Boss.cs#L245-L274

**3. 스킬 판정을 공통 부모로 묶음**

스킬 판정 3종이 `BossAttackState`를 상속해 충돌 시 대미지 처리를 공유함.
한 번 맞히면 판정 오브젝트를 바로 제거해, 이펙트가 남아 있는 동안 여러 번 맞는 문제를 막음.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/BossAttackState.cs#L9-L20

기본 공격과 스킬 생성은 애니메이션 이벤트로 호출해, 휘두르는 프레임에 맞춰 대미지가 들어감.

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 불리언 플래그 다발 | `AllStop`, `_IsInFirstSkill`, `_StunOn`, `NextPageOn` 조합으로 상태를 표현해, 패턴이 늘수록 검증이 어려움 | 상태 `enum` 하나로 압축 |
| 밸런스 수치가 매직 넘버 | 확률 `Random.Range(0, 30)`, 페이즈 체력 `15000`, 대기 시간 `1.25f` 등이 코드에 고정 | 패턴 조건·가중치를 데이터로 분리 |
| 스킬 대기 시간 하드코딩 | 애니메이션 길이와 손으로 맞춰져 있어 모션 교체 시 어긋남 | 애니메이션 이벤트로 종료를 통지받음 |
| 코루틴 중복 실행 가드 없음 | 확률이 연속으로 맞으면 같은 스킬 코루틴이 겹쳐 실행될 수 있음 | 실행 중인 코루틴 핸들 보관 후 중복 차단 |

<br></br>

### 인벤토리

`Slot` 하나가 Unity `EventSystem` 인터페이스 7종을 구현해 클릭·드래그·드롭·툴팁을 모두 처리함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L6

**1. 드래그 중개자를 출처별로 분리**

`DragSlot`(인벤토리), `DragSlot_Equip`(장비창), `DragSlot_Used`(퀵슬롯) 3종을 두고, 드롭받는 쪽이 어느 중개자에 값이 들어있는지로 분기함.
드롭받는 슬롯이 "어디서 왔는지"만 판단하면 되므로, 슬롯 종류가 늘어도 받는 쪽에 케이스 하나만 추가하면 됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L232-L247

**2. 백업 후 되쓰기로 교환 처리**

목적지 아이템을 먼저 복사해두고 덮어쓴 뒤, 백업본을 원본 슬롯에 되쓰는 순서.
자기 자신에게 드롭해도 데이터가 유실되지 않고, 빈 슬롯 이동 / 동일 소모품 합치기 / 스왑 세 경우를 한 흐름에서 처리함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L309-L330

**3. 아이템 타입에 따라 획득 방식을 나눔**

장비는 개별 인스턴스라 스택이 불가능하고, 소모품과 재료만 개수가 합쳐짐.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Inventory.cs#L74-L107

**4. 장착 해제된 장비가 원래 자리로 복귀**

장비를 장착하면 인벤토리 슬롯이 비고 기존 장착 장비가 되돌아오는데, 획득 로직은 첫 번째 빈 슬롯을 찾음.
장착 직전에 슬롯 인덱스를 기억해, 교체된 장비가 방금 비운 자리로 돌아오게 함.

```csharp
//Slot.UseItem — 장착 직전 자리 기억
pMemory = true;
for (int i = 0; i < slots.Length; i++)
{
    if (slots[i].item != null && slots[i].item.Equals(item))
    {
        pNumber = i;
        break;
    }
}

//Inventory.AcquireItem — 되돌아올 때
else if (Slot.pMemory)
{
    slots[Slot.pNumber].AddItem(_item, _count);
    Slot.pMemory = false;
    return;
}
```

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `Slot` 단일 클래스 378줄 | UI 표시·데이터 보유·교환 규칙을 모두 담당 | 데이터와 뷰를 분리, 슬롯은 표시와 입력만 담당 |
| 드롭 영역이 하드코딩 좌표 | `position.x > 1390 && < 1830` 형태라 해상도가 바뀌면 판정이 깨지고, 같은 조건식이 `OnEndDrag` 안에 4회 중복 | `RectTransformUtility.RectangleContainsScreenPoint` + 영역 리스트 순회 |
| `static pMemory` / `pNumber` | 전역 상태라 다른 획득 경로(퀘스트 보상 등)가 기억된 자리를 가로챌 수 있음 | 인자로 전달하거나, 장착을 제거+추가가 아닌 교체 단일 연산으로 처리 |
