using System;
using System.Collections.Generic;
using System.Reflection;

namespace REPOLib.Extensions;

internal static class TypeExtensions
{
    public static IEnumerable<MethodInfo?> SafeGetMethods(this Type type)
    {
        try
        {
            // Return methods safely
            return type.GetMethods();
        }
        catch (Exception ex)
        {
            // Log and return null if there's an error
            // Console.WriteLine($"Error retrieving methods for type {type.FullName}: {ex.Message}");
            return null;
        }
    }
}
