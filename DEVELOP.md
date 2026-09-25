# 개발자 안내 — vArchiveHelper 0.2.0

## 제품 목적

**전체화면 DJMAX 플레이를 유지한 채 v-archive 캡처·인식을 살리는 것.**

「전체 화면 최적화 사용 중지」는 켜 둔 채 업로드하는 것이 목적이다.
우선순위는 DXGI 캡처와, 사용 매뉴얼에 맞춘 단축키 안내이다.
트레이 상주·동시실행 등은 부가이며, 트레이는 **기본 꺼진 선택 옵션**이다.

첫 실행 문구는 `OnboardingForm.cs`, 매뉴얼 본문은 `UsageGuide.cs`, 매뉴얼 제목은 `UsageManualForm.cs`에 있습니다.

## 폴더

```
vArchiveHelper0.2.0/
├── vArchiveHelper/     # 소스 + csproj (Version 0.2.0)
├── icon.jpg
├── scripts/
└── README.md / CHANGELOG.md / DEVELOP.md / INSTALL.txt
```

## 빌드

```powershell
.\scripts\build.ps1
# → vArchiveHelper\bin\Release\net472\vArchiveHelper.exe
```

버전은 `vArchiveHelper.csproj`의 `<Version>`만 올리면 창 제목(`Application.ProductName`)에 반영됩니다.

## 미션과 맞닿은 코드

| 파일 | 역할 |
|------|------|
| `OnboardingForm.cs` | 첫 실행 문구 · 문제 → 전체화면 캡처 → 경로/모니터 |
| `UsageGuide.cs` | 매뉴얼: 왜 필요한가 → 트러블슈팅 우선 |
| `SettingsForm.cs` | 설정 맨 위 = 전체화면 캡처 |
| `CapturePipeline.cs` / `ScreenCapture.cs` | DXGI·실패 안내 |

## GitHub에 올릴 때

저장소에는 소스만 두고, 받는 파일은 Releases의 `vArchiveHelper.zip` 하나로 둡니다.  
`scripts/build-release.ps1`는 그 zip만 만듭니다. main 루트에 exe와 dll을 커밋하지 않습니다.
