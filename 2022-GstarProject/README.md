# 2022 Gstar프로젝트 - EnforceFor

## 프로젝트 소개
프로그래밍 전공 학생들 끼리 모여 Gstar에 참여하기 위해 진행된 프로젝트
- 개발기간 : 2022/09/19 ~ 2022/11/15
- 장르 : 미니RPG
- 그래픽 : 3D / Polygon
- 플랫폼 : Mobile
- 개발엔진 : 유니티 / 2021.3.5f1 

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 프로그래머
- ehdura485(김동겸) : 프로그래머, 기획

## 소개 영상
- https://youtu.be/1-v3--n1AEo

## 프로젝트 인원 상세 역할

- ### hyeon0316(김현진)
    #### 주요 시도 및 구현 목록 
    - `시네머신 카메라`를 사용하여 다른 오브젝트에 가려지지 않는 3인칭 시점 구현
    - `오클루전 컬링` 사용으로 필요없는 부분 렌더링 방지
    - `추상 함수 및 가상 함수`의 재정의를 통하여 함수의 재사용 및 확장성 고려
    - `오브젝트 풀링`을 사용하여 불필요한 GC 호출 방지
    - `ScriptableObject`를 사용하여 캐릭터의 스탯 관리
    - `NavMesh`를 사용하여 플레이어 및 적 기능 구현
    - `코루틴 및 재귀함수`를 사용하여 플레이어의 자동사냥 구현 및 적 패턴 구현
    - `라인 렌더러` 를 응용하여 플레이어 스킬 구현
    - 적 스포너 구현
    #### 리팩토링 및 성능 개선 시도 목록
    - 게임 특징상 많은 적이 한 씬에 한번에 있는 경우가 있기 때문에 기존 적 패턴 구현에 사용했던 코루틴에서 GC생성을 방지하고자 `UniTask`로 변경
    - 캐릭터의 체력관리를 `UniRx`로 변경하여 체력 변동 될 때 개체마다 다른 반응 구현(의존성 제거, 코드 가독성 향상)
    - 움직이지 않는 구조물에 대해 `정적 배칭`하여 드로우콜 감소 
    - 애니메이터를 파라미터에서 코드로 관리하여 유지보수 하기 용이하게 마련
    - 오브젝트 풀링 매니저 로직 분리(단일 책임 원칙)
    
    #### 코드 목록
    - 오브젝트 풀링(생성, 사용, 반환)
    https://github.com/hyeon0316/Project-Code-Repository/blob/aa7af714b54886cd58cb75cb74233ee37693d886/2022-GstarProject/Scripts/Manager/ObjectPool/ObjectPoolManager.cs#L6-L31
    https://github.com/hyeon0316/Project-Code-Repository/blob/aa7af714b54886cd58cb75cb74233ee37693d886/2022-GstarProject/Scripts/Manager/ObjectPool/PoolContainer.cs#L5-L52
    https://github.com/hyeon0316/Project-Code-Repository/blob/aa7af714b54886cd58cb75cb74233ee37693d886/2022-GstarProject/Scripts/Manager/ObjectPool/IPoolable.cs#L1-L4
    
    
    - 모든 캐릭터의 부모 클래스 설계
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Creature.cs#L9-L105
    
    - 플레이어 오토 모드(적 탐색, 자동사냥, 자동이동)
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L88-L136
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L283-L309
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L359-L386
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Player.cs#L402-L439

    - 적 패턴 구현(대기 모드, 추적 모드, 공격, 일정지역 벗어나면 복귀)
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Characters/Enemy.cs#L86-L206
  
    - 라인 렌더러를 응용한 체인 라이트닝 구현
    https://github.com/hyeon0316/Project-Code-Repository/blob/f238bdcd8ae6670aad59e1cc850fa2164ec4f9b0/2022-GstarProject/Scripts/InGame/Characters/Player/Mage/ChainLightningLine.cs#L8-L269
    
    - 모든 캐릭터가 사용하는 Stat(Hp의 경우 UniRx로 개체마다 다른 반응을 구현)
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Stat/Stat.cs#L6-L37
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Stat/EnemyStat.cs#L4-L23
    https://github.com/hyeon0316/Project-Code-Repository/blob/b9d6e73dd0f21e766865767f4d92d27c02f8166a/2022-GstarProject/Scripts/InGame/Stat/PlayerStat.cs#L4-L61
    
    - 적 스폰 관리
    https://github.com/hyeon0316/Project-Code-Repository/blob/aa7af714b54886cd58cb75cb74233ee37693d886/2022-GstarProject/Scripts/InGame/Characters/Enemy/EnemySpawnArea.cs#L7-L125
    
    
    
