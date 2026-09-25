using System.Collections.Generic;

namespace vArchiveHelper;

internal sealed class UsageGuideSection
{
	public string Title { get; init; }

	public IReadOnlyList<string> Lines { get; init; }
}
