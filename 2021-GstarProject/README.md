# 2021 GSTAR Project - 용사(?)의 모험

## 프로젝트 정보
- 기간 : 2021/09/27 ~ 2021/11/16
- 장르 : RPG(3D, Polygon)
- 플랫폼 : PC
- 도구 : Unity, C#
- 인원 : 3인 (프로그래머 2, 그래픽/사운드 1)

## 코드

### 생명체 추상 클래스 설계

원본 구조는 플레이어와 적이 각자 체력 변수와 사망 처리를 따로 들고 있었음.
적을 근접형 / 원거리형 / 보스 3종으로 늘리면서, 체력·피격·사망 처리가 클래스마다 복사되는 상태가 됨.
공통 뼈대를 만들어 파생 클래스가 연출만 얹도록 구조를 다시 잡음.

`LivingEntity`를 4종(`Player`, `Enemy`, `Enemy_Far`, `Boss`)의 공통 부모로 두고, 체력 관련 상태를 전부 여기에 모음.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/LivingEntity.cs#L8-L44
      
- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `MonoBehaviour` 상속 | 체력·사망이 게임오브젝트에 묶여 단위 테스트 불가 | 스탯을 순수 C# 클래스로 분리, 씬 오브젝트는 표현만 담당 |
| `Die()` 단일 책임 과다 | 콜라이더 비활성·AI 정지·드랍 생성·진행도 갱신을 한 메서드에서 처리 | 정리(콜라이더·AI)와 후속(드랍·진행도)을 분리, 후자는 이벤트 구독자에게 위임 |
| 사망 알림 부재 | 사망 사실을 외부가 알려면 매 프레임 `dead`를 확인해야 함 | 사망 이벤트 발행 |

### 인벤토리 

`Slot` 하나가 Unity `EventSystem` 인터페이스 7종을 구현해 클릭·드래그·드롭·툴팁을 모두 처리.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L6

**1. 드래그 중개자를 출처별로 분리**

`DragSlot`(인벤토리), `DragSlot_Equip`(장비창), `DragSlot_Used`(퀵슬롯) 3종을 두고, 드롭받는 쪽이 어느 중개자에 값이 들어있는지로 분기.
드롭받는 슬롯이 "어디서 왔는지"만 판단하면 되므로, 슬롯 종류가 늘어도 받는 쪽에 케이스 하나만 추가하면 됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L232-L247

**2. 백업 후 되쓰기로 교환 처리**

목적지 아이템을 먼저 복사해두고 덮어쓴 뒤, 백업본을 원본 슬롯에 되쓰는 순서.
자기 자신에게 드롭해도 데이터가 유실되지 않고, 빈 슬롯 이동 / 동일 소모품 합치기 / 스왑 세 경우를 한 흐름에서 처리함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Slot.cs#L309-L330

**3. 아이템 타입에 따라 획득 방식을 나눔**

장비는 개별 인스턴스라 스택이 불가능하고, 소모품과 재료만 개수가 합쳐져야 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Inventory/Inventory.cs#L74-L107

**4. 장착한 장비가 원래 자리로 복귀**

장비를 장착하면 인벤토리 슬롯이 비고 기존 장착 장비가 되돌아오는데, 획득 로직이 첫 번째 빈 슬롯을 찾으므로 엉뚱한 자리로 가는 문제가 있었음.
장착 직전에 슬롯 인덱스를 기억해 그 자리로 되돌림.

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

**5. 장비 착용이 스탯과 UI 변동으로 이어짐**

드래그로 장비를 옮기면 착용·해제 처리가 호출되고, 변경된 최대 체력이 체력바에 반영됨.
드래그 → 스탯 변동 → UI 갱신이 한 줄기로 연결됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Player/Player.cs#L944-L962


- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `Slot` 단일 클래스 378줄 | UI 표시·데이터 보유·교환 규칙을 모두 담당 | 데이터와 뷰를 분리, 슬롯은 표시와 입력만 담당 |
| 교환 규칙 분산 | 슬롯 3종 × 출처 3종 규칙이 각 클래스에 흩어져 있음 | `CanAccept(item)` 하나로 위임해 조합 폭발 제거 |
| `static pMemory` / `pNumber` | 전역 상태라 다른 획득 경로(퀘스트 보상 등)가 기억된 자리를 가로챌 수 있음 | 인자로 전달하거나, 장착을 제거+추가가 아닌 교체 단일 연산으로 처리 |
| 드롭 영역이 하드코딩 좌표 | `position.x > 1390 && < 1830` 형태, 해상도 변경 시 판정 붕괴. 같은 조건식이 `OnEndDrag` 안에 4회 중복 | `RectTransformUtility.RectangleContainsScreenPoint` + 영역 리스트 순회 |
| 더블클릭 직접 구현 | `Time.time` 차이로 판정 | `eventData.clickCount` 사용 |
| 스택 상한 없음 | 소모품이 무한히 쌓임 | 아이템 데이터에 상한을 두고 초과분은 다음 슬롯으로 |
| 장비 착용/해제 중복 | 부호만 반대인 코드가 두 벌, 한쪽만 수정하면 스탯이 영구히 어긋남 | 출처 기반 스탯 수정자 등록/해제 |

### 코루틴 기반 보스 패턴

https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Boss.cs#L148-L187
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2021-GstarProject/Scripts/Enemy/Boss.cs#L197-L243

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `new WaitForSeconds(0.25f)` | 루프마다 새로 생성해 보스전 내내 힙 할당 발생 | 필드로 캐싱 |
| 밸런스 수치가 매직 넘버 | 확률 `Random.Range(0, 30)`, 페이즈 체력 `15000`, 대기 시간 `1.25f` 등이 코드에 고정 | 패턴 조건·가중치·모션·후딜을 SO 테이블로 분리 |
| 스킬 대기 시간 하드코딩 | 애니메이션 길이와 손으로 맞춰져 있어 모션 교체 시 조용히 어긋남 | 애니메이션 이벤트로 종료를 통지받음 |
| 불리언 플래그 다발 | `AllStop`, `_IsInFirstSkill`, `_StunOn`, `NextPageOn` 조합이 늘수록 검증 불가 | 상태 `enum` 하나로 압축 |
| 코루틴 중복 실행 가드 없음 | 확률이 연속으로 맞으면 같은 스킬 코루틴이 겹쳐 실행될 수 있음 | 실행 중인 코루틴 핸들 보관 후 중복 차단 |
| 판정 시 범위 재검사 없음 | `OnDamageEvent()`가 거리를 다시 확인하지 않아, 모션 시작 후 벗어나도 피격됨 | 판정 프레임에서 범위 재검사 |
| 장판 로직 분산 | 추적은 `Update`, 발동 시퀀스는 코루틴에 있어 한 스킬을 두 곳에서 읽어야 함 | 장판 오브젝트가 자기 추적과 발동을 스스로 관리 |
| `Player.inst` 직접 참조 | 보스가 플레이어 UI를 직접 켬 | 페이즈 전환을 이벤트로 발행 |
