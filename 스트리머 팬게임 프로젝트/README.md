# 스트리머 팬게임 - TreasureWak

## 프로젝트 소개
특정 스트리머를 좋아하는 팬 끼리 모여서 시작된 프로젝트 
- 개발기간 : 2022/07 ~ 2022/12
- 장르 : 멀티 오토배틀러
- 그래픽 : 2D 
- 플랫폼 : PC
- 개발엔진 : 유니티 / 2021.3.5f1 

## 프로젝트 인원 및 역할
- hyeon0316(김현진) : 클라이언트 프로그래머
- 그 외 서버 프로그래머 1명, 기획자 2명, 그래픽 8명

## 프로젝트 인원 상세 역할

- ### hyeon0316(김현진)
    #### 주요 시도 및 구현 목록
    - 전반적인 클라이언트 시스템 구현
    - 서버에서 패킷 데이터 못 받은 상황에 대한 예외처리

    #### 코드 목록
   - 서버 패킷 요청 및 데이터 적용
     https://github.com/hyeon0316/Project-Code-Repository/blob/7a1bf511f6274a40cad3951eb5ca536e28ddf5e5/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Manager/NetworkManager.cs#L224-L245
     https://github.com/hyeon0316/Project-Code-Repository/blob/7a1bf511f6274a40cad3951eb5ca536e28ddf5e5/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Manager/NetworkManager.cs#L703-L760
    
   - 플레이어 접근 및 정보 동기화 관리
     https://github.com/hyeon0316/Project-Code-Repository/blob/7a1bf511f6274a40cad3951eb5ca536e28ddf5e5/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Manager/PlayerManager.cs#L17-L87
     
   - 전투 시스템 관리
     https://github.com/hyeon0316/Project-Code-Repository/blob/7a1bf511f6274a40cad3951eb5ca536e28ddf5e5/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/InGame/Battle/Battle.cs#L7-L143

   - 아이템 드래그 처리
     https://github.com/hyeon0316/Project-Code-Repository/blob/d4697043c47264668a4b61a3a2db790b7780d81a/%EC%8A%A4%ED%8A%B8%EB%A6%AC%EB%A8%B8%20%ED%8C%AC%EA%B2%8C%EC%9E%84%20%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8/Scripts/Item/Item.cs#L253-L410
   
      
      
