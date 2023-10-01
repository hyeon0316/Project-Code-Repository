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
     - `ScriptableObject`를 사용하여 캐릭터의 스텟 관리
     - `NavMesh`를 사용하여 플레이어의 오토모드에 사용 될 이동 관련 구현
     - `코루틴 및 재귀함수`를 사용하여 플레이어의 자동사냥 구현 및 적 패턴 구현
     - `라인 렌더러` 를 응용하여 플레이어 스킬 구현
     - 지속적인 데이터 관리를 위한 `싱글톤` 사용
     - 프레임 향상을 위해 플레이어 이동 지역마다 해당 적들만 활성화
     - `딕셔너리`를 사용하여 원하는 몬스터 종류(Key)와 스폰 수(Value)를 인스펙터에서 조정 가능하도록 함
    
    
    #### 코드 목록
    - 오브젝트 풀링(생성, 사용, 반환)
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/Manager/ObjectPoolManager.cs#L68-L141
    
    - 모든 캐릭터의 부모가 되는 추상 클래스
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Creature.cs#L15-L145
    
    - 플레이어의 오토 모드(적 탐색, 자동사냥, 자동이동)
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Player.cs#L310-L339
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Player.cs#L113-L165
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Player.cs#L432-L469
    
    -라인 렌더러를 응용한 체인 라이트닝 구현
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Player/Mage/ChainLightningLine.cs#L8-L269
    
    -모든 캐릭터가 가지는 일반 공격 or 스킬 공격에 대한 추상 클래스
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Attack/Attack.cs#L5-L25
    
    
    -모든 적의 부모가 되는 추상 클래스(패턴 구현)
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Enemy.cs#L11-L378
    
    -모든 캐릭터가 사용하는 Stat(ScriptableObject로 저장된 데이터를 불러와서 사용)
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Stat/Stat.cs#L8-L67
    
    -적 스폰 관리
    https://github.com/hyeon0316/GstarProject2022/blob/23a0f8ca646b42472d29e5f466cd33621d112bec/Assets/Scripts/InGame/Characters/Enemy/EnemySpawnArea.cs#L9-L145
    
    
    
