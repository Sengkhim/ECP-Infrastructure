namespace ECPLibrary.Core.Attributes;

/// <summary>
/// Custom attribute to mark a class for agent usage.
/// This attribute can be applied only to classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UseAgentAttribute : Attribute;
