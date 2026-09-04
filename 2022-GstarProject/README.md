# 2022 Gstar프로젝트 - EnforceFor

## 프로젝트 소개
- 기간 : 2022/09/19 ~ 2022/11/15
- 장르 : 미니 RPG(3D)
- 플랫폼 : Mobile
- 도구 : Unity, C#
- 인원 : 2인 (프로그래머 2)

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 프로그래머
- ehdura485(김동겸) : 프로그래머, 기획

## 코드

### 프레임 저하 대응

개발 후반에 실기기에서 프레임이 눈에 띄게 떨어지는 문제가 발생함.
원인을 나눠 확인한 결과 세 가지가 겹쳐 있었음.

| 원인 | 내용 |
|---|---|
| 적 상시 활성 | 전체 맵의 모든 적이 게임 시작 시 한 번에 생성되어, 플레이어가 없는 구역에서도 렌더링과 AI 패턴이 계속 수행됨 |
| 시야 밖 렌더링 | 벽이나 지형에 가려 보이지 않는 오브젝트까지 매 프레임 그려짐 |
| 스킬 오브젝트 | 투사체와 이펙트를 사용할 때마다 생성하고 파괴해 GC가 반복 발생 |

- 원본

적 스폰 지역이 `Start`에서 적을 생성했기 때문에, 씬이 로드되는 시점에 **맵 전체의 적이 동시에 존재**하게 됨.
플레이어가 한 구역에 있어도 나머지 구역의 적이 전부 렌더링되고, 배회 패턴도 함께 수행됨.

각 적이 "플레이어가 근처에 없는지" 확인하는 처리도 개별로 돌고 있었음.
적 한 마리당 물리 검사가 하나씩 붙는 구조라, 적이 많을수록 비용이 그대로 누적됨.
```csharp
private bool IsNullPlayer()
{
    Collider[] colliders = Physics.OverlapSphere(transform.position, _attackRadius * 6, LayerMask.GetMask("Player"));
    return colliders.Length == 0;
}
```

- 수정

**1. 구역에 들어갈 때만 적을 생성**

스폰 지역의 생성 시점을 `Start`에서 `OnEnable`로 옮기고, 비활성화될 때 구역의 적 전원을 풀로 반환하도록 함.
게임 시작 시 한 번 생성되던 것이, 구역이 켜지고 꺼질 때마다 생성·반환되는 구조로 바뀜.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy/EnemySpawnArea.cs#L36-L44

**2. 플레이어 위치로 구역을 켜고 끔**

구역마다 트리거를 두고, 플레이어가 들어오면 활성화 나가면 비활성화함.
활성화된 구역의 적만 씬에 존재하므로 렌더링 대상과 AI 수행 대상이 함께 줄어듦.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/EnemySpawnController.cs#L6-L42

맵 이동으로 순간이동할 때는 트리거를 거치지 않으므로, 이동 처리에서 전 구역을 먼저 끔(`MapManager.GetSpwan`).

**3. 비활성 시 진행 중이던 패턴을 정지**

구역이 꺼져도 적이 실행 중이던 이동·복귀 처리가 남아 계속 돌던 문제가 있었음.
비활성화 시점에 내비게이션을 끄고 진행 중이던 처리를 전부 취소하도록 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L61-L64

**4. 적마다 돌던 플레이어 탐색 제거**

구역 활성화로 플레이어 근처의 적만 존재하게 되었으므로, 각 적이 플레이어를 찾던 `OverlapSphere` 호출 자체를 삭제함.
적 수만큼 발생하던 물리 연산이 사라짐.

**5. 오클루전 컬링과 정적 배칭**

지형·건물처럼 움직이지 않는 오브젝트를 Static으로 지정해 드로우콜을 줄이고,
오클루전 컬링으로 벽 뒤에 가려진 오브젝트를 렌더링에서 제외함.

**6. 스킬 오브젝트 풀링**

투사체와 이펙트를 매번 생성·파괴하지 않고 풀에서 대여·반환하도록 함(아래 항목 참고).

**7. 적 패턴의 코루틴 할당 제거**

적이 다수 존재하는 구조라 코루틴 대기마다 발생하는 할당이 누적됨. `UniTask`로 전환함(아래 항목 참고).

- 결과

| | 원본 | 수정 |
|---|---|---|
| 씬에 존재하는 적 | 맵 전체의 적 전부 | 플레이어가 있는 구역만 |
| AI 패턴 수행 | 모든 적이 상시 수행 | 활성 구역의 적만 |
| 플레이어 탐색 | 적마다 `OverlapSphere` 주기 실행 | 제거 |
| 시야 밖 오브젝트 | 상시 렌더링 | 오클루전 컬링으로 제외 |
| 스킬 오브젝트 | 사용마다 생성·파괴 | 풀 대여·반환 |
| 대기 처리 | 코루틴 할당 누적 | `UniTask` |

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| 구역 경계에서 반복 전환 | 경계에 서 있으면 트리거가 반복 발생해 스폰과 반환이 되풀이됨 | 지연 시간을 두거나 이력 처리 |
| 구역 진입 시 일괄 생성 | 적을 한 번에 꺼내 순간적인 부하가 발생할 수 있음 | 여러 프레임에 나눠 생성 |
| 컬링 기준이 구역 단위 | 같은 구역 안에서는 멀리 있는 적도 전부 갱신됨 | 거리 기준 LOD 또는 갱신 주기 분산 |
| GPU 인스턴싱 미적용 | 같은 적이 다수 등장하는 구조에 적합 | 적 렌더러에 인스턴싱 적용 |

<br></br>

### 오브젝트 풀링 구조 분리

첫 구현은 매니저 하나가 풀 목록·생성·대여·반환을 모두 담당했음.
풀링 대상이 20종을 넘어가면서 매니저가 모든 대상을 알아야 하는 구조가 한계로 드러남.

- 원본

`enum`으로 풀 종류를 정의하고, 매니저의 `Awake`에서 전부 나열해 초기화.
```csharp
public enum PoolType
{
    NormalAttackMissile, NormalAttackEffect, WideAreaBarrage,
    BulletRainMissile, BulletRainEffect, DamageText, SnowFootPrint,
    FrightFlyMissile, WindAttack, VolcanicSpike, Spider, FrightFly,
    ForestGolem1, ForestGolem2, ForestGolem3, SpecialGolem,
    GoblinWarrior, GoblinArcher, GoblinArcherArrow, Goblin, Boss, BossRock,
}

private void Awake()
{
    Init(PoolType.NormalAttackMissile, 4);
    Init(PoolType.NormalAttackEffect, 4);
    Init(PoolType.WideAreaBarrage, 3);
    Init(PoolType.BulletRainMissile, 10);
    //... 22줄 계속
}
```
풀링 대상을 추가할 때마다 `enum` 항목 추가 / 프리팹 배열 등록 / `Awake`에 초기화 한 줄 추가가 모두 필요했음.
매니저가 게임의 모든 풀링 대상을 알고 있는 상태라 담당 범위가 계속 커짐.

- 수정

**1. 매니저와 컨테이너를 분리**

매니저는 어떤 풀이 있는지만 관리하고, 실제 생성·대여·반환은 `PoolContainer`가 담당함.
매니저는 풀 목록만 들고 있으므로, 풀 하나의 동작을 고칠 때 매니저를 건드리지 않음.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/Manager/ObjectPool/ObjectPoolManager.cs#L6-L31
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/Manager/ObjectPool/PoolContainer.cs#L5-L52

**2. `enum` 대신 프리팹의 InstanceID를 키로 사용**

풀 종류를 미리 정의하지 않고, 필요한 쪽이 자기 프리팹으로 `Init`을 호출하면 그 시점에 풀이 만들어짐.
적 스포너는 자기 구역에 쓸 적만, 마법사는 자기 스킬 투사체만 등록함. 매니저는 대상이 무엇인지 알지 못함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player/Mage/Mage.cs#L32-L37

**3. `IPoolable`로 반환 대상을 식별**

원본은 반환할 때 어느 풀 소속인지 호출부가 알려줘야 했음(`ReturnObject(PoolType, obj)`).
생성 시점에 출처 ID를 객체에 넣어두고, 반환은 객체만 넘기면 되도록 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/Manager/ObjectPool/IPoolable.cs#L1-L3

**4. 풀 고갈 시 동적 확장**

직전 프로젝트는 풀이 비면 `null`을 반환해 이펙트가 나오지 않는 문제가 있었음.
대여 시점에 큐가 비어 있으면 그 자리에서 하나를 더 만들어 반환하도록 함(`PoolContainer.GetObject`).

풀별 부모 오브젝트를 프리팹 이름으로 자동 생성해 하이어라키를 정리함.

- 결과

| | 원본 | 수정 |
|---|---|---|
| 풀 정의 | `enum` 항목 + 매니저 `Awake` 초기화 | 사용하는 쪽이 프리팹으로 등록 |
| 매니저 책임 | 목록·생성·대여·반환 전부 | 풀 관리만, 나머지는 컨테이너 |
| 대상 추가 | 3곳 수정 | 사용처에서 `Init` 호출 1회 |
| 반환 | 호출부가 풀 종류를 지정 | 객체가 자기 출처를 보유 |
| 풀 고갈 | 대응 없음 | 동적 확장 |

- 개선점

| 항목 | 문제 | 개선 방향 |
|---|---|---|
| `GetComponent<IPoolable>()` 반환마다 호출 | 반환이 잦을수록 컴포넌트 탐색이 반복됨 | 대여 시점에 인터페이스를 캐싱해 반환에 사용 |
| 반환 시 상태 초기화 없음 | 파티클·트레일·물리 속도가 남은 채 재사용될 수 있음 | `IPoolable`에 초기화 메서드를 두고 반환 시 호출 |
| 이중 반환 방지 없음 | 같은 객체를 두 번 반환하면 큐에 중복으로 들어감 | 활성 상태 확인 또는 반환 여부 플래그 |
| 등록되지 않은 프리팹 조회 | `_poolList[id]`에 없는 키로 접근하면 예외 발생 | `TryGetValue`로 확인 후 안내 |

<br></br>

### 플레이어 오토 모드

모바일 RPG 특성상 자동 사냥이 필요했음.
직업마다 스킬 구성이 다르고, 스킬마다 사거리와 타겟 수가 달라 **직업별로 자동 사냥을 따로 만들면 중복이 커지는 상황**이었음.
퀘스트 자동 이동(NPC에게 걸어가 대화)도 흐름이 같아 함께 처리할 필요가 있었음.

- 구조

자동 사냥이 성립하려면 네 가지가 필요했음 — **무엇을 할지 / 누구에게 / 어떻게 접근할지 / 언제 실행할지**.
각각을 분리해 부모(`Player`)가 흐름을 갖고, 직업 클래스는 자기 스킬 목록만 등록하도록 함.

```
Player (추상)
├── 행동 정의    UseActionType         스킬 하나를 함수로 표현
├── 타겟 선정    AddTarget()           범위 탐색 후 거리순 정렬
├── 접근         ActionFromDistance()  사거리 밖이면 이동 후 실행
└── 실행 순서    ExecuteAutoHunt()     쿨타임이 끝난 스킬을 큐에서 꺼내 사용
```

**1. 스킬을 델리게이트로 표현**

"무엇을 할지"를 함수 타입으로 정의해, 부모가 스킬의 내용을 몰라도 실행할 수 있게 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L51-L54

직업 클래스는 `Awake`에서 자기 스킬을 배열로 등록하기만 하면 됨.
마법사는 5종, 기사는 별도 구성을 등록하며, 부모의 자동 사냥 코드는 그대로 재사용됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player/Mage/Mage.cs#L32-L37

**2. 타겟을 거리순으로 정렬해 확보**

탐색 범위 안의 적을 가까운 순으로 정렬한 뒤, 스킬이 요구하는 수만큼만 타겟으로 지정함.
단일 타겟 스킬은 `AddTarget(1, ...)`, 다중 타겟 스킬은 필요한 수를 넘겨 같은 함수를 사용함.
타겟이 하나도 없으면 자동 사냥을 종료함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L279-L310

**3. 거리에 따라 접근 여부를 판단**

"어떻게 접근할지"를 한 곳에 모아, 사거리 밖이면 이동한 뒤 실행하고 안이면 즉시 실행함.
수행할 행동을 인자로 받으므로 **이동 로직을 스킬마다 작성하지 않음**.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L363-L376

같은 이동 로직이 NPC에도 쓰임. 대상이 NPC면 도착 판정 거리를 대화 거리로 바꿔 처리함.
공격 사거리로 접근하면 대화 범위에 닿지 않아 자동 진행이 멈추던 문제를 이 분기로 해결함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L401-L439

**4. 쿨타임이 끝난 스킬을 큐에 담아 순서대로 사용**

"언제 실행할지"는 큐로 처리함. 쿨타임이 남은 스킬은 담지 않고, 이미 담긴 스킬은 중복으로 넣지 않음.
큐가 비어 있으면 기본 공격을 하므로 "쓸 스킬이 없을 때"를 따로 분기하지 않아도 됨.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L92-L136

공격 중에는 새 행동을 넣지 않고, 공격이 끝나 `_isAttacking`이 풀리면 다음 스킬을 꺼냄.
스킬 하나가 끝날 때까지 기다리는 처리를 별도 상태 변수 없이 이 플래그 하나로 표현함.

<br></br>

### 적 패턴

**1. 재귀 호출로 패턴 순환**

대기 패턴은 "정지"와 "무작위 지점으로 이동" 중 하나를 고르고, 끝나면 자기 자신을 다시 호출해 순환함.
진행 단계를 상태 변수로 들고 있지 않고 호출 흐름만으로 반복을 표현함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L103-L118
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L120-L143

공격 패턴도 한 번의 공격이 끝나면 자신을 다시 호출해, 사거리 판정과 접근을 반복함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L199-L223

**2. `CancellationToken`으로 패턴 전환**

배회 중 피격되면 진행 중이던 이동을 즉시 끊고 추적으로 넘어가야 함.
패턴마다 토큰을 따로 두어, 전환할 때 해당 패턴만 취소하도록 함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L225-L238

사망·비활성 시점에는 모든 토큰을 취소해, 풀로 반환된 뒤 이전 패턴이 남아 도는 것을 막음.

**3. 스폰 지역 이탈 시 복귀**

플레이어가 적을 끌고 멀리 가면 원래 자리로 돌아가고 체력을 회복함.
거리 측정과 복귀를 두 단계로 나눠 연결함.
https://github.com/hyeon0316/Project-Code-Repository/blob/39abf628f085efb57039abecf69ab73d9cb1cea9/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L170-L195

