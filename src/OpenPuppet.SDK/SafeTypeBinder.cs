using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Silk.NET.Core.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public class SafeTypeBinder : ISerializationBinder
    {
        public List<Assembly> AllowedAssemblies { get; } = [Assembly.GetExecutingAssembly()];
        private readonly ConcurrentDictionary<string, Type> _nameToTypeCache = new();

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = GetReadableName(serializedType);
            _nameToTypeCache.TryAdd(typeName.ToLowerInvariant(), serializedType);
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            var key = typeName.ToLowerInvariant();

            if (_nameToTypeCache.TryGetValue(key, out var cached))
                return cached;

            var resolved = ResolveType(typeName);
            _nameToTypeCache[key] = resolved;
            return resolved;
        }

        private static string GetReadableName(Type type)
        {
            var defName = type.Name;
            if (type.BaseType is not null && type.BaseType.IsInterface)
                defName = $"{type.BaseType.Name}.{defName}";

            if (!type.IsGenericType) return defName;

            var tickIndex = defName.IndexOf('`');
            if (tickIndex >= 0)
                defName = defName[..tickIndex];

            var args = type.GetGenericArguments().Select(GetReadableName);
            return $"{defName}<{string.Join(",", args)}>";
        }

        private Type ResolveType(string typeName)
        {
            var genericStart = typeName.IndexOf('<');

            if (genericStart < 0)
                return FindType(typeName, arity: 0);

            var baseName = typeName[..genericStart];
            var argsSection = typeName[(genericStart + 1)..^1];
            var argNames = SplitTopLevel(argsSection);

            var argTypes = argNames.Select(ResolveType).ToArray();
            var openType = FindType(baseName, arity: argTypes.Length);

            return openType.MakeGenericType(argTypes);
        }

        private static List<string> SplitTopLevel(string s)
        {
            var parts = new List<string>();
            int depth = 0, start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '<') depth++;
                else if (s[i] == '>') depth--;
                else if (s[i] == ',' && depth == 0)
                {
                    parts.Add(s[start..i]);
                    start = i + 1;
                }
            }
            parts.Add(s[start..]);
            return parts;
        }

        private Type FindType(string baseName, int arity)
        {
            var key = baseName.ToLowerInvariant();

            var matches = AllowedAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                {
                    var name = t.Name;

                    if (t.BaseType is not null && t.BaseType.IsInterface)
                        name = $"{t.BaseType.Name}.{name}";

                    if (t.IsGenericTypeDefinition)
                    {
                        var tick = name.IndexOf('`');
                        if (tick >= 0) name = name[..tick];
                        return t.GetGenericArguments().Length == arity && name.Equals(baseName, StringComparison.OrdinalIgnoreCase);
                    }
                    return arity == 0 && name.Equals(baseName, StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();

            if (matches.Length == 0)
                throw new JsonSerializationException($"Type '{baseName}' (arity {arity}) not found in allowed assemblies.");

            if (matches.Length > 1)
                throw new JsonSerializationException($"Type name '{baseName}' is ambiguous across: {string.Join(", ", matches.Select(t => t.FullName))}");

            return matches[0];
        }
    }
}
