namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
	public const string RefStructs = "RefStructs";

	public const string RequiredMembers = "RequiredMembers";

	public string FeatureName { get; }

	public bool IsOptional { get; set; }

	public CompilerFeatureRequiredAttribute(string featureName)
	{
		FeatureName = featureName;
	}
}
