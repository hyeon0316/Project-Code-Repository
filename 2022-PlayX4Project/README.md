# 2022 PlayX4프로젝트 - Treasure Of Dungeon

## 프로젝트 소개
- 기간 : 2022/03/18 ~ 2022/07 (2022/05 PlayX4 전시 후 보완 개발)
- 장르 : 액션 어드벤처(2.5D)
- 플랫폼 : PC
- 도구 : Unity, C#
- 인원 : 3인 (프로그래머 2, 그래픽/사운드 1)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 팀장, 프로그래머, 기획
- kimehunsu(김은수) : 그래픽, 사운드, 메뉴 UI 일부
- kjcy(주찬영) : 프로그래머

## 담당 범위
**게임 진행·연출과 보스 전투** — 대화 / 카메라 / 사운드 / 튜토리얼 / 상호작용 / 인벤토리 / 데이터 / 보스

| 영역 | 내용 |
|---|---|
| 진행·연출 | `DialogueManager`, `CameraManager`, `SoundManager`, `GameManager`, 튜토리얼, 페이드, 로비 |
| 상호작용 | 상자·문·사다리·감옥·NPC 대화·맵 이동 |
| 인벤토리 | 아이템 데이터·슬롯·쿨타임 |
| 데이터 | 스탯 JSON 외부화 |
| 보스 | 최종 보스 1페이즈 `Necromancer`·2페이즈 `Demon`, 중간 보스 `Bringer`와 각 스킬 오브젝트 (2페이즈 패턴 선택 조건은 팀원 작성) |

플레이어 조작·공격, 캐릭터 공통 클래스(`Life`, `Enemy`), 일반 몬스터는 팀원 담당.

## 코드

### 대화 시스템

대사 목록을 `Queue`에 담고 하나씩 꺼내며, 명령어면 분기하고 아니면 화면에 출력함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L87-L141

**1. 대사 목록 안의 명령어로 연출 제어**

대사 사이에 `MoveCamera` 한 줄을 넣으면 그 시점에 카메라가 이동함.
연출 순서를 바꿀 때 코드를 고치지 않고 인스펙터의 대사 목록만 수정함.

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

### 보스 전투

최종 보스는 소환형인 1페이즈(`Necromancer`)와 근접·원거리 혼합형인 2페이즈(`Demon`)로 나뉘고, 1페이즈가 중간 보스(`Bringer`)를 소환함.

**1. 페이즈를 별도 프리팹으로 분리**

1페이즈가 사망하면 전환 대사를 출력한 뒤, 같은 위치에 2페이즈 프리팹을 생성함.
페이즈마다 클래스가 따로 있어, 한 클래스 안에서 페이즈 플래그로 모든 패턴을 분기하지 않음.
전환 연출은 대화 시스템을 그대로 써서, 보스 대사도 일반 대사처럼 인스펙터에서 수정함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Life/Enemy/Necromancer.cs#L162-L187

**2. 체력 절반에서 한 번만 발동하는 패턴**

1페이즈는 체력이 절반 이하가 되면 중간 보스를 소환하고, 중간 보스는 체력이 절반 이하가 되면 추적 트랩을 깔음.
`_canSkill` 같은 플래그로 각 패턴이 전투당 한 번만 발동하도록 함.

**3. 활성화할 때마다 플레이어 위치로 이동하는 트랩**

트랩은 `OnEnable`에서 플레이어 위치로 이동하고 1.5초 뒤 스스로 꺼짐.
보스는 트랩을 2초 간격으로 다시 켜기만 하면 되므로, 추적 로직 없이 플레이어를 따라오는 장판이 됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Life/Enemy/SkillObj/BringerTrap.cs#L20-L32

2페이즈의 폭탄·화염구는 시작할 때 미리 만들어 두고 재사용하며, 2페이즈의 패턴 선택 조건(거리·축 정렬·누적 피격)은 팀원이 작성함.

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 상태 판정이 `Update`의 중첩 `if` | 거리·타이머·애니메이션 이름을 매 프레임 비교해, 패턴을 추가할 때마다 분기가 깊어짐 | 패턴을 상태 클래스로 나눈 상태 머신 |
| `GameObject.Find` 반복 호출 | 폭발·화염구 발사·페이즈 전환 때마다 이름으로 씬을 탐색 | 생성 시점에 참조 캐싱 |
| 페이즈 전환이 `Resources.Load` | 프리팹 경로 문자열이 코드에 하드코딩되어 이름을 바꾸면 런타임에 깨짐 | 인스펙터에서 다음 페이즈 프리팹 참조 |
| 풀 고갈 시 처리 없음 | 화염구·폭발 이펙트는 개수를 확인하지 않고 `Dequeue` | 고갈 시 동적 확장 |

<br></br>

### JSON 외부 데이터로 밸런스 분리

체력·공격력 같은 수치를 빌드 외부의 `setting.Json`으로 두고, 실행 시 읽어 각 캐릭터에 주입함.
수치를 바꿀 때 에디터를 열거나 다시 빌드하지 않고 파일만 고치면 됨.

파일이 없으면 인스펙터에 등록한 기본값(`StatObj`)으로 생성하므로 첫 실행도 정상 동작함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Manager/JsonToDataManager.cs#L9-L66

<br></br>

### 커스텀 에디터로 아이템 데이터 입력

아이템은 소모품과 재료 두 종류이고, 재료는 회복량과 쿨타임이 필요 없음.
하나의 클래스로 두면 재료 아이템에도 쓰지 않는 칸이 보여, 채우지 말아야 할 칸을 채워도 알아채기 어려움.

`CustomEditor`로 인스펙터를 직접 그려, 선택한 타입에 필요한 필드만 표시함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-PlayX4Project/Scripts/Life/Player/Inventory/Item.cs#L13-L50
