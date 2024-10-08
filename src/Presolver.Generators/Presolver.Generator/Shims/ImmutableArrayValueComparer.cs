// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Copied from https://github.com/dotnet/runtime/blob/1473deaa50785b956edd7d078e68c0581c1b4d95/src/libraries/Common/src/Roslyn/SyntaxValueProvider.ImmutableArrayValueComparer.cs

#region

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

#endregion

namespace Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

internal static partial class SyntaxValueProviderExtensions
{
    sealed class ImmutableArrayValueComparer<T> : IEqualityComparer<ImmutableArray<T>>
    {
        public static readonly IEqualityComparer<ImmutableArray<T>> Instance = new ImmutableArrayValueComparer<T>();

        public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
        {
            return x.SequenceEqual(y, EqualityComparer<T>.Default);
        }

        public int GetHashCode(ImmutableArray<T> obj)
        {
            var hashCode = 0;
            foreach (var value in obj)
                hashCode = Hash.Combine(hashCode, EqualityComparer<T>.Default.GetHashCode(value!));

            return hashCode;
        }
    }
}