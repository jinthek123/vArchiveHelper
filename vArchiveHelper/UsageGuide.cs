using System.Collections.Generic;

namespace vArchiveHelper;

internal static class UsageGuide
{
	public const int ContentWidth = 600;

	public static IReadOnlyList<UsageGuideSection> Sections { get; } = new UsageGuideSection[4]
	{
		new UsageGuideSection
		{
			Title = "누가 쓰면 좋을까요?",
			Lines = new string[3]
			{
				" DJMAX RESPECT V 를 플레이하는 유저 중에서",
				"'전체 화면 최적화 사용 중지' 옵션을 켜면",
				"캡쳐 인식이 제대로 되지 않는 경우 실험적으로 사용을 하는 것을 권장합니다."
			}
		},
		new UsageGuideSection
		{
			Title = "사용 순서",
			Lines = new string[4]
			{
				"★ v-archive.exe 에서 캡쳐 단축키 설정에서 그림처럼 설정되었는지 확인해주세요!",
				"1. v-archive 경로, 모니터, 그리고 해상도를 맞춰서 저장하세요.",
				"2. 결과 화면에서 캡처 단축키를 눌러 캡처하세요.",
				"3. 상태 줄에서 완료 여부를 확인하세요."
			}
		},
		new UsageGuideSection
		{
			Title = "단축키",
			Lines = new string[3]
			{
				"지정한 키는 바로 저장되며, Esc로 취소할 수 있습니다.",
				"★ 인식 키(Alt+Insert)와는 별개입니다!",
				"「캡처 동작 실행」으로 키 없이 캡처 기능을 확인할 수 있습니다."
			}
		},
		new UsageGuideSection
		{
			Title = "창",
			Lines = new string[2]
			{
				"창을 닫으면 종료됩니다. 트레이에서 실행 중이라면 트레이 메뉴에서 종료할 수 있습니다.",
				"「트레이에서 실행」을 켜고 창을 닫으면 숨김 처리되며, 종료는 트레이 메뉴에서 가능합니다."
			}
		}
	};
}
