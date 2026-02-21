using System;
using System.Collections;
using System.Collections.Generic;

namespace TownOfUsStatsExporter.Reflection;

/// <summary>
/// Helper for converting IL2CPP types to managed types.
/// </summary>
public static class IL2CPPHelper
{
    /// <summary>
    /// Convert IL2CPP list/collection to managed List.
    /// </summary>
    /// <param name="il2cppCollection">The IL2CPP collection to convert.</param>
    /// <returns>A managed list of objects.</returns>
    public static List<object> ConvertToManagedList(object il2cppCollection)
    {
        var result = new List<object>();

        try
        {
            // Try as IEnumerable
            if (il2cppCollection is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }

            // Try as Il2CppSystem.Collections.Generic.List<T>
            var listType = il2cppCollection.GetType();
            var countProperty = listType.GetProperty("Count");

            if (countProperty != null)
            {
                var count = (int)countProperty.GetValue(il2cppCollection)!;
                var getItemMethod = listType.GetMethod("get_Item") ?? listType.GetMethod("Get");

                if (getItemMethod != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var item = getItemMethod.Invoke(il2cppCollection, new object[] { i });
                        if (item != null)
                        {
                            result.Add(item);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error converting IL2CPP collection: {ex}");
        }

        return result;
    }

    /// <summary>
    /// Convert IL2CPP dictionary to managed Dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="il2cppDictionary">The IL2CPP dictionary to convert.</param>
    /// <returns>A managed dictionary.</returns>
    public static Dictionary<TKey, TValue> ConvertToManagedDictionary<TKey, TValue>(object il2cppDictionary)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();

        try
        {
            if (il2cppDictionary is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (entry.Key is TKey key && entry.Value is TValue value)
                    {
                        result[key] = value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TownOfUsStatsPlugin.Logger.LogError($"Error converting IL2CPP dictionary: {ex}");
        }

        return result;
    }
}
