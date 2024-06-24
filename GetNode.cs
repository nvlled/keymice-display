#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace godot_getnode;


public static class NodeExtension
{
    public static void GetAnnotatedNodes<T>(this T node) where T : Node
    {
        GetNodeAttribute.Ready(node);
    }
}

class CacheEntry(FieldInfo field, GetNodeAttribute attr)
{
    public FieldInfo field = field;
    public GetNodeAttribute attr = attr;
}

[AttributeUsage(AttributeTargets.Field)]
public class GetNodeAttribute(string? Path = null, bool AllowNull = false, bool Unique = false) : Attribute
{
    private readonly string? path = Path;
    private readonly bool allowNull = AllowNull;
    private readonly bool unique = Unique;

    static Dictionary<Guid, CacheEntry[]> cache = new();

    public static void Ready<T>(T node) where T : Node
    {

        if (node is null) return;

        var nodeType = node.GetType();
        CacheEntry[] fields;

        if (!cache.TryGetValue(nodeType.GUID, out fields))
        {
            GD.PrintT("no field cache", nodeType.Name);
            var allFields = node.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var temp = new List<CacheEntry>();
            foreach (var field in allFields)
            {
                var attr = (GetNodeAttribute?)field.GetCustomAttribute(typeof(GetNodeAttribute));
                if (attr is not null)
                {
                    temp.Add(new CacheEntry(field, attr));
                };
            }
            fields = temp.ToArray();
            cache[nodeType.GUID] = fields;
        }

        foreach (var entry in fields)
        {
            var attr = entry.attr;
            var field = entry.field;

            var path = attr.path ?? field.Name;
            if (attr.unique && path[0] != '%')
                path = "%" + path;

            var child = node.GetNode(path);
            if (child is null)
            {
                if (attr.allowNull) return;
                throw new ArgumentException($"Failed to find annotated node, GetNode(\"{path}\") returns null");
            }

            var childType = child.GetType();
            if (field.FieldType != childType && !childType.IsSubclassOf(field.FieldType))
            {
                throw new ArgumentException($"Expected GetNode(\"{path}\") to have type {field.FieldType} but got {child.GetType()}");
            }

            field.SetValue(node, child);
        }
    }
}
