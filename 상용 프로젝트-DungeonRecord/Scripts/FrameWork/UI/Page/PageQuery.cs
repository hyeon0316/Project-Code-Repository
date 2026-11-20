using System;
using System.Diagnostics;
using System.Reflection;

public static class PageQuery
{
    public static void SetQuery(string query, Object obj)
    {
        var parts = query.Split('&');

        Type type = obj.GetType();

        foreach (var s in parts)
        {
            var pair = s.Split('=');
            FieldInfo field = type.GetField(pair[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                var value = ConvertFieldValue(field.FieldType, pair[1]);
                field.SetValue(obj, value);
            }
            else
            {
                HDebug.LogError($"Field '{pair[0]}' not found in type '{type.FullName}'");
            }
        }
    }

    private static object ConvertFieldValue(Type fieldType, string fieldValue)
    {
        try
        {
            if (fieldType.IsEnum)
            {
                return Enum.Parse(fieldType, fieldValue);
            }
            else if (fieldType == typeof(int))
            {
                return int.Parse(fieldValue);
            }
            else if (fieldType == typeof(float))
            {
                return float.Parse(fieldValue);
            }
            else if (fieldType == typeof(double))
            {
                return double.Parse(fieldValue);
            }
            else if (fieldType == typeof(bool))
            {
                return bool.Parse(fieldValue);
            }
            else if (fieldType == typeof(string))
            {
                return fieldValue;
            }
            else
            {
                UnityEngine.Debug.LogError($"Unsupported field type: {fieldType}");
                return null;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error converting '{fieldValue}' to type '{fieldType}': {ex.Message}");
            return null;
        }
    }
}

