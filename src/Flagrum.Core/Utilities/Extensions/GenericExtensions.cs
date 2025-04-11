using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Flagrum.Core.Utilities.Extensions;

public static class GenericExtensions
{
    /// <summary>
    /// Creates a new object of the same type as the source, and copies the property values over.
    /// WARNING: May not cater to all property types
    /// </summary>
    public static T DeepClone<T>(this T @object) where T : new()
    {
        var typeSource = @object.GetType();
        var clone = Activator.CreateInstance(typeSource);

        var propertyInfo =
            typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var property in propertyInfo)
        {
            if (property.CanWrite && !property.GetCustomAttributes().Any(a => a.GetType().Name == "FactoryInjectAttribute"))
            {
                if (property.PropertyType.IsValueType || property.PropertyType.IsEnum ||
                    property.PropertyType == typeof(string))
                {
                    property.SetValue(clone, property.GetValue(@object));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var itemType = property.PropertyType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(i => i.GetGenericArguments()[0])
                        .FirstOrDefault()!;

                    var countMethodInfo = typeof(Enumerable).GetMethods().Single(method =>
                        method.Name == "Count" && method.IsStatic && method.IsGenericMethod &&
                        method.GetParameters().Length == 1);
                    var elementAtMethodInfo = typeof(Enumerable).GetMethods().Single(method =>
                        method.Name == "ElementAt" && method.IsStatic && method.IsGenericMethod &&
                        method.GetParameters().Length == 2 &&
                        method.GetParameters()[1].ParameterType == typeof(int));
                    var enumerable = property.GetValue(@object) as IEnumerable;

                    var countMethod = countMethodInfo.MakeGenericMethod(itemType);
                    var elementAtMethod = elementAtMethodInfo.MakeGenericMethod(itemType);
                    var count = (int)countMethod.Invoke(enumerable, new object[] {enumerable})!;
                    var cloneArray = Array.CreateInstance(itemType, count);

                    for (var i = 0; i < count; i++)
                    {
                        cloneArray.SetValue(
                            elementAtMethod.Invoke(enumerable, new object[] {enumerable, i}).DeepClone(), i);
                    }

                    // var type = typeof(IEnumerable<>).MakeGenericType(itemType);
                    // var constructor = property.PropertyType.GetConstructor(new[] {type})!;
                    // var enumerableClone = constructor.Invoke((object[])cloneArray);

                    if (property.PropertyType.IsAssignableTo(typeof(IList)))
                    {
                        var toListMethodInfo = typeof(Enumerable).GetMethods().Single(method =>
                            method.Name == "ToList" && method.IsStatic && method.IsGenericMethod &&
                            method.GetParameters().Length == 1);
                        var toListMethod = toListMethodInfo.MakeGenericMethod(itemType);
                        property.SetValue(clone, toListMethod.Invoke(cloneArray, new object[] {cloneArray}));
                    }
                    else
                    {
                        property.SetValue(clone, cloneArray);
                    }
                }
                else
                {
                    var objPropertyValue = property.GetValue(@object);
                    property.SetValue(clone, objPropertyValue?.DeepClone());
                }
            }
        }

        return (T)clone;
    }

    /// <summary>
    /// Compares the property values of two objects of the same type.
    /// WARNING: May not cater to all property types
    /// </summary>
    public static bool DeepCompare(this object left, object right)
    {
        if (left == null || right == null)
        {
            return true;
        }

        var properties = left.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.PropertyType.IsValueType || property.PropertyType.IsEnum ||
                property.PropertyType == typeof(string))
            {
                var leftValue = property.GetValue(left);
                var rightValue = property.GetValue(right);

                if (!(leftValue == null && rightValue == null) &&
                    (leftValue == null || rightValue == null || !leftValue.Equals(rightValue)))
                {
                    return false;
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                var itemType = property.PropertyType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault()!;

                var countMethodInfo = typeof(Enumerable).GetMethods().Single(method =>
                    method.Name == "Count" && method.IsStatic && method.IsGenericMethod &&
                    method.GetParameters().Length == 1);
                var elementAtMethodInfo = typeof(Enumerable).GetMethods().Single(method =>
                    method.Name == "ElementAt" && method.IsStatic && method.IsGenericMethod &&
                    method.GetParameters().Length == 2 &&
                    method.GetParameters()[1].ParameterType == typeof(int));
                var leftEnumerable = property.GetValue(left) as IEnumerable;
                var rightEnumerable = property.GetValue(right) as IEnumerable;

                if ((leftEnumerable == null && rightEnumerable != null) ||
                    (leftEnumerable != null && rightEnumerable == null))
                {
                    return false;
                }

                if (leftEnumerable != null && rightEnumerable != null)
                {
                    var countMethod = countMethodInfo.MakeGenericMethod(itemType);
                    var elementAtMethod = elementAtMethodInfo.MakeGenericMethod(itemType);
                    var leftCount = (int)countMethod.Invoke(leftEnumerable, new object[] {leftEnumerable})!;
                    var rightCount = (int)countMethod.Invoke(rightEnumerable, new object[] {rightEnumerable})!;

                    if (leftCount != rightCount)
                    {
                        return false;
                    }

                    for (var i = 0; i < leftCount; i++)
                    {
                        var leftItem = elementAtMethod.Invoke(leftEnumerable, new object[] {leftEnumerable, i});
                        var rightItem = elementAtMethod.Invoke(rightEnumerable, new object[] {rightEnumerable, i});

                        if (!leftItem.DeepCompare(rightItem))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (!property.GetValue(left).DeepCompare(property.GetValue(right)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool HashCompare(this byte[] left, byte[] right)
    {
        var leftSha256 = SHA256.Create();
        var rightSha256 = SHA256.Create();
        var leftHash = leftSha256.ComputeHash(left);
        var rightHash = rightSha256.ComputeHash(right);
        return leftHash.SequenceEqual(rightHash);
    }
}