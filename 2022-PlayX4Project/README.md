# 2022 PlayX4프로젝트 - Treasure Of Dungeon

## 프로젝트 소개
프로그래밍 전공 학생들 끼리 모여 PlayX4에 참여하고, 보완하여 인디게임공모전에 참여한 팀 프로젝트 
- 개발기간 : 2022/03/18 ~ 2022/05/12
- 장르 : 액션 어드벤처
- 그래픽 : 2.5D / Pixel, Voxel
- 플랫폼 : PC
- 개발엔진 : 유니티 / 2019.4.30f1 

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 팀장, 프로그래머, 기획
- kimehunsu(김은수) :  그래픽, 사운드
- kjcy(주찬영) : 프로그래머

## 소개 영상
- https://youtu.be/45aYV0KyuEQ

## 프로젝트 인원 상세 역할

- ### hyeon0316(김현진)
    #### 주요 시도 및 구현 목록 
    - 게임 시연 중 빠른 벨런스 패치를 위한 `json` 데이터 관리
    - `코루틴 및 재귀함수`를 사용한 적 패턴 구현 
    - 플레이어와 적, 상호작용 오브젝트 `상속` 및 `인터페이스` 활용 구조 설계
    - Npc 대화 매니저, 사운드 매니저, 페이드 매니저 구현
    - `ScriptableObject`를 사용하여 아이템 데이터 관리 
    - 유니티 `시네머신`을 활용하여 중간 스토리 컷씬 및 엔딩씬 제작
    - 유니티 `포스트 프로세싱` 사용
      
    #### 리팩토링 목록
    - 무분별한 GetComponent, FindObjectType, GameObject.Find 사용 수정
    - 상속 구조 개선, 코드 중복 최소화
    - 비동기 처리 및 코드 가독성 향상을 위한 `UniTask` 사용
    - 클래스 간 결합도를 낮추기 위한 `event` 사용


    #### 코드 목록
    - 대화 진행 관리
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L44-L64
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L72-L110
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L179-L194
      <br/>
    - json 데이터 관리
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Manager/JsonToDataManager.cs#L16-L50
    - Fade 시스템
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Manager/FadeManager.cs#L16-L66
      <br/>
    - 캐릭터 구조
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Life/Life.cs#L19-L84
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Life/Enemy/ILifePattern.cs#L2-L10
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Life/Enemy/EnemyAttack.cs#L17-L21
      <br/>
    - 상호작용 오브젝트 구조
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Interaction/Interaction.cs#L6-L53
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Interaction/Npc.cs#L6-L73
      <br/>
    - 적 패턴 구현
      https://github.com/hyeon0316/Project-Code-Repository/blob/7d37d1991328a71d5804f28227701a2eaf16dee0/2022-PlayX4Project/Scripts/Life/Enemy/Necromancer.cs#L30-L99
      <br/>
   
      
      
