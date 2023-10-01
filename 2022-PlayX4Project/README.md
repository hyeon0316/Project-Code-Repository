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
    - 유니티 패키지에 포함된 `시네머신`을 활용하여 중간 스토리 진행씬 및 엔딩씬 제작
    - `json`데이터 파싱을 활용하여 게임 전시 중에도 실시간으로 게임의 전체적인 벨런스 조절 가능하도록 조성
    - `오브젝트 풀링`을 사용하여 불필요한 GC 호출 방지
    - `코루틴 및 재귀함수`를 사용한 적들의 랜덤 패턴 구현 
    - 게임의 퀄리티를 상승 시키기 위해 유니티의 `포스트 프로세싱` 사용
    - 현재 캐릭터가 존재하지 않는 구역의 경우 렌더링 작업을 비활성화(큰 맵 방지)
    - 자료구조 `Queue`를 사용하여 NPC 대화 관리
    - 전체적인 상호작용 구조물(NPC, 상자, 문, 포탈 등)을 `추상 클래스`로 작성하여 재정의
    - `인터페이스`를 사용하여 다중상속 지원
    - `ScriptableObject`를 사용하여 아이템 데이터 관리 

    #### 코드 목록
    - json 데이터 파싱
      https://github.com/hyeon0316/Project-Code-Repository/blob/cd8f4c55166b0f1af90b5fd4822ef1ce2dd77efb/2022-PlayX4Project/Scripts/Manager/DataManager.cs#L8-L71
      <br/>
    - 스토리 진행 관리
      https://github.com/hyeon0316/Project-Code-Repository/blob/cd8f4c55166b0f1af90b5fd4822ef1ce2dd77efb/2022-PlayX4Project/Scripts/Manager/DialogueManager.cs#L99-L151
      <br/>
    - 상호작용 구조물 추상클래스 작성
      https://github.com/hyeon0316/Project-Code-Repository/blob/cd8f4c55166b0f1af90b5fd4822ef1ce2dd77efb/2022-PlayX4Project/Scripts/Interaction/Interaction.cs#L6-L41
      <br/>
    - 오브젝트 풀(자료구조 Queue로 관리)
      https://github.com/hyeon0316/Project-Code-Repository/blob/cd8f4c55166b0f1af90b5fd4822ef1ce2dd77efb/2022-PlayX4Project/Scripts/Life/Enemy/Demon.cs#L93-L153
      <br/>
    - 재귀함수를 사용한 랜덤 패턴
      https://github.com/hyeon0316/Project-Code-Repository/blob/cd8f4c55166b0f1af90b5fd4822ef1ce2dd77efb/2022-PlayX4Project/Scripts/Life/Enemy/Necromancer.cs#L80-L116
      <br/>
   
      
      
