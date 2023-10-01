# 2021 Gstar프로젝트 - 용사(?)의 모험

## 프로젝트 소개
프로그래밍 전공 학생들 끼리 모여 실무와 협엽 경험을 쌓기 및 Gstar프로젝트에 참여하기 위해 진행된 팀 프로젝트 
- 개발기간 : 2021/09/27 ~ 2021/11/16
- 장르 : 미니RPG
- 그래픽 : 3D / Polygon
- 플랫폼 : PC
- 개발엔진 : 유니티 / 2019.4.30f1 

## 프로젝트 인원 및 역할
- Maruber(김동겸) :  팀장, 프로그래머, 기획
- hyeon0316(김현진) : 프로그래머, 기획 
- kiom485(김시헌) : 그래픽, 사운드

## 소개 영상
- https://youtu.be/L91Jwhi6jyY

## 프로젝트 인원 상세 역할

- ### hyeon0316(김현진)
    #### 주요 시도 및 구현 목록 
    - 유니티 패키지에 포함된 `시네머신`을 활용하여 인게임 인트로 및 엔딩씬 제작
    - `ScriptableObject`를 사용 하여 다양한 아이템, 퀘스트 제작에 있어 효율적인 데이터 관리
    - `커스텀에디터`를 사용하여 팀원 간의 원활한 작업 효율 형성
    - `추상 클래스`를 사용하여 코드 중복 최소화 및 확장성 과 유연성 고려
    - `코루틴`을 활용하여 적들의 다양한 패턴 구현
    - 유니티의 `EventSystem` 을 사용하여 아이템의 클릭, 드래그, 드롭 구현

    #### 코드 목록
    - 생명체 관련 추상 클래스 작성
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/Enemy/Scripts/LivingEntity.cs#L8-L44
      <br/>
    - 캐릭터 머리 위 HpBar 구현
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/Enemy/Scripts/EnemyHpBar.cs#L6-L37
      <br/>
    - ScriptableObject를 사용 하여 데이터 관리
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/InvenTory/Scripts/UIScripts/Item.cs#L5-L35
      <br/>
    - 아이템 획득 
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/InvenTory/Scripts/UIScripts/Inventory.cs#L74-L121
      <br/>
    - 아이템 클릭, 드래그, 드롭
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/InvenTory/Scripts/UIScripts/Slot.cs#L109-L140
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/InvenTory/Scripts/UIScripts/Slot.cs#L232-L247
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/InvenTory/Scripts/UIScripts/Slot.cs#L142-L230
      <br/>
    - 코루틴을 활용한 패턴 
      https://github.com/hyeon0316/GstarProject/blob/16ef81b3f7db8d3092750e0b99d95c0f2cd29d9e/Assets/Enemy/Scripts/Boss.cs#L148-L187
      
      
      
