# DefanceGameBase

## 🛡️ 프로젝트 소개

이 레포지토리는 디펜스 게임(Defense Game) 개발을 위한 기본적인 구조와 베이스 코드를 제공합니다. 어떤 디펜스 게임 프로젝트에서든 공통적으로 활용할 수 있도록 모듈화하여 설계되었습니다.

  * **목표**: 디펜스 게임의 핵심 요소(매니저, 캐릭터 컨트롤, 데이터, 타일 시스템 등)를 미리 구성하여 빠른 개발 시작을 지원합니다.
  * **주요 기술**: C\#

## 🛠️ 주요 구성 요소 (프로젝트 구조)

프로젝트는 디펜스 게임 개발에 필요한 핵심 기능들을 다음과 같이 모듈화된 폴더로 나누어 관리합니다.

| 폴더명 | 설명 |
| :--- | :--- |
| **Manager** | 게임의 전반적인 시스템 관리 로직 (예: 씬 관리, 리소스 관리, 게임 상태 관리 등) |
| **CharacterController** | 게임 내 유닛 및 캐릭터의 움직임, 공격, 행동 등을 담당하는 로직 |
| **Data** | 캐릭터 스탯, 아이템 정보 등 게임 데이터 관련 스크립트 및 구조체 |
| **Tile** | 맵 타일 관련 로직 및 타일 배치 처리 로직 |
| **MapEditor** | 맵을 제작하거나 수정하는 에디터 기능 관련 요소 (Unity Editor 확장 등) |
| **UI** | 사용자 인터페이스(HP 바, 메뉴, 인벤토리 등) 관련 스크립트 |
| **Util** | 공통으로 사용되는 범용 유틸리티 기능 모음 (확장 메소드, 헬퍼 클래스 등) |

### 핵심 파일

  * `Config.cs`: 게임의 전역 설정 값들을 정의하는 스크립트
  * `MapData.cs`: 맵의 구조나 경로 정보를 정의하는 데이터 스크립트
  * `AwakeScene.cs`: 게임 시작 시 초기 설정을 담당하는 스크립트

UI 에디터
<img width="2547" height="1015" alt="image" src="https://github.com/user-attachments/assets/af708302-6b00-42f8-bdb5-bde5f380934b" />

Map 에디터
<img width="1775" height="940" alt="image" src="https://github.com/user-attachments/assets/07b49eef-cc93-4eaf-9804-6060226b0fb6" />
<img width="1772" height="938" alt="image" src="https://github.com/user-attachments/assets/d96237a4-5e0b-4080-a317-8f1c802d54aa" />

