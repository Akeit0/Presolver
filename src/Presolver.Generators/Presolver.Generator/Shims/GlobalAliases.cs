// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Copied from https://github.com/dotnet/runtime/blob/1473deaa50785b956edd7d078e68c0581c1b4d95/src/libraries/Common/src/Roslyn/GlobalAliases.cs

#region

using System;
using System.Collections.Immutable;
using Roslyn.Utilities;

#endregion

namespace Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

/// <summary>
///     Simple wrapper class around an immutable array so we can have the value-semantics needed for the incremental
///     generator to know when a change actually happened and it should run later transform stages.
/// </summary>
internal sealed class GlobalAliases : IEquatable<GlobalAliases>
{
    public static readonly GlobalAliases Empty = new(ImmutableArray<(string aliasName, string symbolName)>.Empty);

    public readonly ImmutableArray<(string aliasName, string symbolName)> AliasAndSymbolNames;

    int _hashCode;

    GlobalAliases(ImmutableArray<(string aliasName, string symbolName)> aliasAndSymbolNames)
    {
        AliasAndSymbolNames = aliasAndSymbolNames;
    }

    public bool Equals(GlobalAliases? aliases)
    {
        if (aliases is null)
            return false;

        if (ReferenceEquals(this, aliases))
            return true;

        if (AliasAndSymbolNames == aliases.AliasAndSymbolNames)
            return true;

        return AliasAndSymbolNames.AsSpan().SequenceEqual(aliases.AliasAndSymbolNames.AsSpan());
    }

    public static GlobalAliases Create(ImmutableArray<(string aliasName, string symbolName)> aliasAndSymbolNames)
    {
        return aliasAndSymbolNames.IsEmpty ? Empty : new(aliasAndSymbolNames);
    }

    public static GlobalAliases Concat(GlobalAliases ga1, GlobalAliases ga2)
    {
        if (ga1.AliasAndSymbolNames.Length == 0)
            return ga2;

        if (ga2.AliasAndSymbolNames.Length == 0)
            return ga1;

        return new(ga1.AliasAndSymbolNames.AddRange(ga2.AliasAndSymbolNames));
    }

    public override int GetHashCode()
    {
        if (_hashCode == 0)
        {
            var hashCode = 0;
            foreach (var tuple in AliasAndSymbolNames)
                hashCode = Hash.Combine(tuple.GetHashCode(), hashCode);

            _hashCode = hashCode == 0 ? 1 : hashCode;
        }

        return _hashCode;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GlobalAliases);
    }
}