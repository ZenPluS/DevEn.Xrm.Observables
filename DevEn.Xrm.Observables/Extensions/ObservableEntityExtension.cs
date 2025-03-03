using System;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables.Extensions;

public static class ObservableEntityExtension
{
    public static ObservableEntity ToObservable(this Entity entity)
    {
        return ObservableEntity.Create(entity);
    }

    public static Entity ToEntity(this ObservableEntity observableEntity)
    {
        if (observableEntity == null)
            return new Entity();

        var entity = new Entity(observableEntity?.LogicalName);

        foreach (var attribute in observableEntity?.Attributes)
        {
            entity[attribute.Key] = attribute.Value;
        }

        if (observableEntity?.Id != Guid.Empty)
        {
            entity.Id = observableEntity.Id;
        }

        return entity;
    }

    internal static TValue TryGetOrAdd<TValue>(this Entity entity, string key, TValue valueToAdd)
    {
        if (entity?.Attributes == null)
            return default;

        var exists = entity.TryGetAttributeValue<TValue>(key, out var valueOut);

        if (!exists)
            entity[key] = valueToAdd;

        return valueOut;
    }

    internal static TValue TryGetOrAdd<TValue>(this Entity entity, string key, Func<string, TValue> valueFactory)
    {
        if (entity?.Attributes == null)
            return default;

        var exists = entity.TryGetAttributeValue<TValue>(key, out var valueOut);

        if (!exists)
            entity[key] = valueFactory(key);
        return valueOut;
    }

    internal static bool TryUpdate<TValue>(this Entity entity, string key, TValue valueToUpdate)
    {
        if (entity?.Attributes == null)
            return false;

        var exists = entity.TryGetAttributeValue<TValue>(key, out _);

        if (!exists)
            return false;

        entity[key] = valueToUpdate;

        return true;
    }

    internal static bool TryUpdate<TValue>(this Entity entity, string key, Func<string, TValue> updateFactory)
    {
        if (entity?.Attributes == null)
            return false;

        var exists = entity.TryGetAttributeValue<TValue>(key, out _);

        if (!exists)
            return false;

        entity[key] = updateFactory(key);

        return true;
    }

    internal static bool TryAddOrUpdate<TValue>(this Entity entity, string key, TValue valueToAddOrUpdate)
    {
        if (entity?.Attributes == null)
            return false;

        var exists = entity.TryGetAttributeValue<TValue>(key, out _);

        entity[key] = valueToAddOrUpdate;

        return !exists; // "true" if added, "false" if updated
    }

    internal static bool TryAddOrUpdate<TValue>(this Entity entity, string key, Func<string, TValue> valueFactory)
    {
        if (entity?.Attributes == null)
            return false;

        var exists = entity.TryGetAttributeValue<TValue>(key, out _);

        entity[key] = valueFactory(key);

        return !exists; // "true" if added, "false" if updated
    }

    internal static bool TryDelete<TValue>(this Entity entity, string key, out TValue value)
    {
        switch (entity)
        {
            case null:
                value = default;
                return false;

            default:
                return entity.Attributes.Remove(key, out value);
        }
    }

    private static bool Remove<TValue>(this AttributeCollection attributes, string key, out TValue value)
    {
        if (attributes.Contains(key))
        {
            value = (TValue)attributes[key];
            attributes.Remove(key);
            return true;
        }
        value = default;
        return false;
    }

    internal static bool TryDelete(this Entity entity, string key)
        => entity?.Attributes.Remove(key) == true;
}