# vArchiveHelper 0.2.0

**캡쳐 오류가 있는 사람들을 위한 v-archive 클라이언트 보조 프로그램입니다.**

**「전체 화면 최적화 사용 중지」를 켠 채로** v-archive에 올리는 입니다.  

단축키 → 모니터 캡처(DXGI) → 클립보드 → v-archive 모드1 인식(Alt+Insert).

[v-archive] exe 파일을 필수로 합니다.
---
https://gall.dcinside.com/mgallery/board/view/?id=djmaxrespect&no=2135759&page=1
---
## 설치

1. Releases의 `vArchiveHelper.zip` 압축 해제 (또는 이 폴더에서 빌드)
2. `vArchiveHelper.exe` 실행 → v-archive 경로 지정
3. vArchiveHelper.exe 및 동일 경로의 dll 파일들은 **같은 폴더**에 있어야 합니다

---

## 개발

```powershell
.\scripts\build.ps1
.\vArchiveHelper\bin\Release\net472\vArchiveHelper.exe
```

자세히: [DEVELOP.md](DEVELOP.md) · 변경 이력: [CHANGELOG.md](CHANGELOG.md)
