using System;

namespace ChilliCoreTemplate.Models;

[AttributeUsage(AttributeTargets.Field)]
public class GroupAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
