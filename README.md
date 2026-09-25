# vArchiveHelper 0.2.0

**전체화면으로 치고, v-archive에 올리기**

**「전체 화면 최적화 사용 중지」를 켠 채로** v-archive에 올리는 헬퍼입니다.  
그 옵션을 켜 두고, 보더리스로 바꾸지 않아도, 캡처만 이 앱이 잇습니다.

단축키 → 모니터 캡처(DXGI) → 클립보드 → v-archive 모드1 인식(Alt+Insert).

[v-archive](https://github.com/kokonohanahata/v-archive) 필요 · Windows 10/11 + .NET Framework 4.7.2

---

## 이 앱이 해결하려는 것

| 하고 싶은 것 | 이 앱 |
|--------------|--------|
| 「전체 화면 최적화 사용 중지」를 켠 채로 치기 | 옵션은 켜 둠 |
| 그 상태로 v-archive에 올리기 | DXGI 캡처 → 클립보드 → 인식 |

헬퍼에서 **DXGI · 물리 픽셀**을 켜고, 상태에 `[DXGI]`가 보이는지 확인하세요.  
v-archive 단축키는 사용 매뉴얼의 그림과 같게 맞춥니다.

---

## 설치

1. Releases의 `vArchiveHelper.zip` 압축 해제 (또는 이 폴더에서 빌드)
2. `vArchiveHelper.exe` 실행 → v-archive 경로 지정
3. exe와 dll은 **같은 폴더**에 있어야 합니다

---

## 0.2.0

- **시작하기 온보딩** — 왜 필요한지 · 전체화면 캡처 · 경로/모니터
- 매뉴얼·DXGI 실패 시 전체화면 팁
- 설정 UI — **전체화면 캡처**를 맨 위에

---

## 개발

```powershell
.\scripts\build.ps1
.\vArchiveHelper\bin\Release\net472\vArchiveHelper.exe
```

자세히: [DEVELOP.md](DEVELOP.md) · 변경 이력: [CHANGELOG.md](CHANGELOG.md)
