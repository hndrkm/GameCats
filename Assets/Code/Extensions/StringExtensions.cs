using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    using System.Runtime.CompilerServices;
    public static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasValue(this string s) 
        {
            return string.IsNullOrEmpty(s) == false;
        }
    }
}
