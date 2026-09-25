namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
internal sealed class RequiresPreviewFeaturesAttribute : Attribute
{
	public string? Message { get; }

	public string? Url { get; set; }

	public RequiresPreviewFeaturesAttribute()
	{
	}

	public RequiresPreviewFeaturesAttribute(string? message)
	{
		Message = message;
	}
}
