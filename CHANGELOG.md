# Changelog

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를 참고합니다.

## [0.2.0] — 2026-09-26

**표시 이름:** vArchiveHelper 0.2.0  
**기반:** 0.1.3 (리뉴얼)  
**포지션:** 전체화면으로 치고 ↔ v-archive에 올리기 (캡처 깨짐·같은 곡 반복의 연결고리)

### Added

- **시작하기 온보딩** — 왜 필요한지 · 전체화면 캡처 · 경로/모니터
- 사용 매뉴얼 **「왜 필요한가」** + **「전체화면에서 같은 곡만 잡힐 때」** 우선 배치
- DXGI 실패 시 옵션을 켜 두고 매뉴얼을 보라고 안내
- 앱 아이콘 갱신 (`icon.jpg` → `app.ico`)
- 첫 실행 문구는 `OnboardingForm`에, 매뉴얼 제목은 `UsageManualForm`에 둠

### Changed

- `ConfigVersion` **3**
- 설정 UI — **전체화면 캡처**를 맨 위, 테마는 맨 아래
- 기본 `VArchiveExePath` placeholder 통일, placeholder면 경로 설정 유도
- 캡처 성공 메시지 — `[DXGI]` 표기 · 「클립보드→인식」 요약
- README / DEVELOP / INSTALL — 구원(연결고리) 포지션으로 재구성
- **트레이에서 실행** (`RunInSystemTray`, 기본 OFF) — 창 닫기 시 숨김·단축키 유지, 종료는 트레이 메뉴

---

## [0.1.3] — 2026-06-08

**표시 이름:** vArchiveHelper 0.1.3  
**기반:** 0.1.2

### Added

- 앱 아이콘, 라이트/다크 테마, 사용 매뉴얼, 스크롤 가드

### Changed

- 설정 UI·캡처 성능·메모리 개선

---

## [0.1.2] — 2026-06-04

커스텀 단축키, 단일 인스턴스, 경로 설정 유도, main/source 분리 등.
