using System;

namespace TextEditor.Attributes
{
    /// <summary>
    /// Attribute to indicate not null value
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.ReturnValue)]
    public class NotNullAttribute : Attribute
    {
    }
}
