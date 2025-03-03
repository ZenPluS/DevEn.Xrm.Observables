using System;
using System.Collections.Generic;
using System.Text.Json;
using DevEn.Xrm.Observables.Extensions;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables;

/// <summary>
/// Represents an observable entity that notifies observers of changes to its attributes.
/// </summary>
public sealed class ObservableEntity :
    Entity,
    IObservable<KeyValuePair<string, object>>
{
    private readonly List<IObserver<KeyValuePair<string, object>>> _observers = new(1);

    private Lazy<string> LazyJson(JsonSerializerOptions options = null) =>
        new(() => JsonSerializer.Serialize(this, options));

    private void NotifyObservers(KeyValuePair<string, object> change)
    {
        foreach (var observer in _observers)
            observer.OnNext(change);
    }

    private ObservableEntity(string entityName)
        : base(entityName)
    { }

    private ObservableEntity(Entity entity)
        : base(entity.LogicalName, entity.Id)
    {
        Attributes = entity.Attributes;
        EntityState = entity.EntityState;
        FormattedValues = entity.FormattedValues;
        RelatedEntities = entity.RelatedEntities;
        RowVersion = entity.RowVersion;
    }

    /// <summary>
    /// Gets the underlying entity.
    /// </summary>
    public Entity Entity => this;

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity"/> with the specified entity name.
    /// </summary>
    /// <param name="entityName">The logical name of the entity.</param>
    /// <returns>A new instance of <see cref="ObservableEntity"/>.</returns>
    public static ObservableEntity Create(string entityName) => new(entityName);

    /// <summary>
    /// Serializes the entity to a JSON string.
    /// </summary>
    /// <returns>A JSON string representation of the entity.</returns>
    public string Json()
        => LazyJson().Value;

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity"/> from an existing entity.
    /// </summary>
    /// <param name="entity">The existing entity.</param>
    /// <returns>A new instance of <see cref="ObservableEntity"/>.</returns>
    public static ObservableEntity Create(Entity entity) => new(entity);

    /// <summary>
    /// Serializes the entity to a JSON string using the specified options.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON string representation of the entity.</returns>
    public string Json(JsonSerializerOptions options)
        => LazyJson(options).Value;

    /// <summary>
    /// Converts an existing entity to an observable entity.
    /// </summary>
    /// <param name="entity">The existing entity.</param>
    /// <returns>A new instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity ToObservable(Entity entity)
    {
        return new ObservableEntity(entity);
    }

    /// <summary>
    /// Subscribes an observer to receive notifications of attribute changes.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>A disposable object that can be used to unsubscribe the observer.</returns>
    public IDisposable Subscribe(IObserver<KeyValuePair<string, object>> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        return new InternalDelegateDisposable(() => _observers.Remove(observer));
    }

    /// <summary>
    /// Subscribes to receive notifications of attribute changes with specified callback actions.
    /// </summary>
    /// <param name="onNext">The action to invoke when a new attribute value is observed.</param>
    /// <param name="onError">The action to invoke when an error occurs during observation.</param>
    /// <param name="onCompleted">The action to invoke when the observation is completed.</param>
    /// <returns>A disposable object that can be used to unsubscribe the observer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="onNext"/> is <c>null</c>.</exception>
    public IDisposable Subscribe(Action<KeyValuePair<string, object>> onNext, Action<Exception> onError = null, Action onCompleted = null)
    {
        if (onNext is null)
            throw new ArgumentNullException(nameof(onNext));

        return Subscribe(new InternalObserver(onNext, onError, onCompleted));
    }

    private sealed class InternalDelegateDisposable(Action disposeAction) : IDisposable
    {
        private Action _disposeAction = disposeAction;

        public void Dispose()
        {
            _disposeAction?.Invoke();
            _disposeAction = null;
        }
    }

    private sealed class InternalObserver(
        Action<KeyValuePair<string, object>> onNext,
        Action<Exception> onError,
        Action onCompleted)
        : IObserver<KeyValuePair<string, object>>
    {
        public void OnNext(KeyValuePair<string, object> value)
            => onNext(value);

        public void OnError(Exception error)
            => onError?.Invoke(error);

        public void OnCompleted()
            => onCompleted?.Invoke();
    }

    private static KeyValuePair<string, object> AttributeCollectionToNotify<T>(string key, T value)
    {
        return new KeyValuePair<string, object>(key, value);
    }

    /// <summary>
    /// Tries to get or add an attribute with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueToAdd">The value to add if the key does not exist.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryGetOrAdd<T>(string key, T valueToAdd)
    {
        ObservableEntityExtension.TryGetOrAdd(this, key, valueToAdd);
        NotifyObservers(AttributeCollectionToNotify(key, valueToAdd));

        return this;
    }

    /// <summary>
    /// Tries to get or add an attribute with the specified key and value factory.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueFactoryToAdd">The factory function to create the value if the key does not exist.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryGetOrAdd<T>(string key, Func<string, T> valueFactoryToAdd)
    {
        ObservableEntityExtension.TryGetOrAdd(this, key, valueFactoryToAdd);
        NotifyObservers(AttributeCollectionToNotify(key, valueFactoryToAdd(key)));

        return this;
    }

    /// <summary>
    /// Tries to update an attribute with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueToUpdate">The value to update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryUpdate<T>(string key, T valueToUpdate)
    {
        if (ObservableEntityExtension.TryUpdate(this, key, valueToUpdate))
            NotifyObservers(AttributeCollectionToNotify(key, valueToUpdate));

        return this;
    }

    /// <summary>
    /// Tries to update an attribute with the specified key and value factory.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueFactoryToUpdate">The factory function to create the value to update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryUpdate<T>(string key, Func<string, T> valueFactoryToUpdate)
    {
        if (ObservableEntityExtension.TryUpdate(this, key, valueFactoryToUpdate))
            NotifyObservers(AttributeCollectionToNotify(key, valueFactoryToUpdate(key)));

        return this;
    }

    /// <summary>
    /// Tries to add or update an attribute with the specified key and value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueToAddOrUpdate">The value to add or update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryAddOrUpdate<T>(string key, T valueToAddOrUpdate)
    {
        ObservableEntityExtension.TryAddOrUpdate(this, key, valueToAddOrUpdate);
        NotifyObservers(AttributeCollectionToNotify(key, valueToAddOrUpdate));

        return this;
    }

    /// <summary>
    /// Tries to add or update an attribute with the specified key and value factory.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the attribute.</param>
    /// <param name="valueFactoryToAddOrUpdate">The factory function to create the value to add or update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryAddOrUpdate<T>(string key, Func<string, T> valueFactoryToAddOrUpdate)
    {
        ObservableEntityExtension.TryAddOrUpdate(this, key, valueFactoryToAddOrUpdate);
        NotifyObservers(AttributeCollectionToNotify(key, valueFactoryToAddOrUpdate(key)));

        return this;
    }

    /// <summary>
    /// Tries to delete an attribute with the specified key.
    /// </summary>
    /// <param name="key">The key of the attribute.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryDelete(string key)
    {
        if (ObservableEntityExtension.TryDelete(this, key))
            NotifyObservers(AttributeCollectionToNotify<object>(key, null));

        return this;
    }

    /// <summary>
    /// Tries to get or add multiple attributes with the specified keys and values.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valuesToAdd">The values to add if the keys do not exist.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryGetOrAdd(IEnumerable<string> keys, IEnumerable<object> valuesToAdd)
    {
        if (keys is null || valuesToAdd is null)
            return this;

        if (keys is IList<string> keyList && valuesToAdd is IList<object> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
            {
                ObservableEntityExtension.TryGetOrAdd(this, keyList[i], valueList[i]);
                NotifyObservers(AttributeCollectionToNotify(keyList[i], valueList[i]));
            }
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAdd.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                ObservableEntityExtension.TryGetOrAdd(this, keyEnumerator.Current, valueEnumerator.Current);
                NotifyObservers(AttributeCollectionToNotify(keyEnumerator.Current, valueEnumerator.Current));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to get or add multiple attributes with the specified keys and value factory.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valueFactoryToAdd">The factory function to create the values if the keys do not exist.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryGetOrAdd(IEnumerable<string> keys, Func<string, object> valueFactoryToAdd)
    {
        if (keys is null || valueFactoryToAdd is null)
            return this;

        if (keys is IList<string> keyList)
        {
            foreach (var key in keyList)
            {
                var value = valueFactoryToAdd(key);

                ObservableEntityExtension.TryGetOrAdd(this, key, value);
                NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToAdd(key);

                ObservableEntityExtension.TryGetOrAdd(this, key, value);
                NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to update multiple attributes with the specified keys and values.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valuesToUpdate">The values to update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryUpdate(IEnumerable<string> keys, IEnumerable<object> valuesToUpdate)
    {
        if (keys is null || valuesToUpdate is null)
            return this;

        if (keys is IList<string> keyList && valuesToUpdate is IList<object> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
            {
                if (ObservableEntityExtension.TryUpdate(this, keyList[i], valueList[i]))
                {
                    NotifyObservers(AttributeCollectionToNotify(keyList[i], valueList[i]));
                }
            }
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                if (ObservableEntityExtension.TryUpdate(this, keyEnumerator.Current, valueEnumerator.Current))
                    NotifyObservers(AttributeCollectionToNotify(keyEnumerator.Current, valueEnumerator.Current));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to update multiple attributes with the specified keys and value factory.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valueFactoryToUpdate">The factory function to create the values to update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryUpdate(IEnumerable<string> keys, Func<string, object> valueFactoryToUpdate)
    {
        if (keys is null || valueFactoryToUpdate is null)
            return this;

        if (keys is IList<string> keyList)
        {
            foreach (var key in keyList)
            {
                var value = valueFactoryToUpdate(key);
                if (ObservableEntityExtension.TryUpdate(this, key, value))
                    NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToUpdate(key);
                if (ObservableEntityExtension.TryUpdate(this, key, value))
                    NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to add or update multiple attributes with the specified keys and values.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valuesToAddOrUpdate">The values to add or update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryAddOrUpdate(IEnumerable<string> keys, IEnumerable<object> valuesToAddOrUpdate)
    {
        if (keys is null || valuesToAddOrUpdate is null)
            return this;

        if (keys is IList<string> keyList && valuesToAddOrUpdate is IList<object> valueList)
        {
            var count = Math.Min(keyList.Count, valueList.Count);

            for (var i = 0; i < count; i++)
            {
                if (ObservableEntityExtension.TryAddOrUpdate(this, keyList[i], valueList[i]))
                    NotifyObservers(AttributeCollectionToNotify(keyList[i], valueList[i]));
            }
        }
        else
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var valueEnumerator = valuesToAddOrUpdate.GetEnumerator();

            while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
            {
                if (ObservableEntityExtension.TryAddOrUpdate(this, keyEnumerator.Current, valueEnumerator.Current))
                    NotifyObservers(AttributeCollectionToNotify(keyEnumerator.Current, valueEnumerator.Current));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to add or update multiple attributes with the specified keys and value factory.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <param name="valueFactoryToAddOrUpdate">The factory function to create the values to add or update.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryAddOrUpdate(IEnumerable<string> keys, Func<string, object> valueFactoryToAddOrUpdate)
    {
        if (keys is null || valueFactoryToAddOrUpdate is null)
            return this;

        if (keys is IList<string> keyList)
        {
            foreach (var key in keyList)
            {
                var value = valueFactoryToAddOrUpdate(key);
                if (ObservableEntityExtension.TryAddOrUpdate(this, key, value))
                    NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }
        else
        {
            foreach (var key in keys)
            {
                var value = valueFactoryToAddOrUpdate(key);

                if (ObservableEntityExtension.TryAddOrUpdate(this, key, value))
                    NotifyObservers(AttributeCollectionToNotify(key, value));
            }
        }

        return this;
    }

    /// <summary>
    /// Tries to delete multiple attributes with the specified keys.
    /// </summary>
    /// <param name="keys">The keys of the attributes.</param>
    /// <returns>The current instance of <see cref="ObservableEntity"/>.</returns>
    public ObservableEntity TryDelete(IEnumerable<string> keys)
    {
        if (keys is null)
            return this;

        foreach (var key in keys)
        {
            if (ObservableEntityExtension.TryDelete(this, key))
                NotifyObservers(AttributeCollectionToNotify<object>(key, null));
        }

        return this;
    }
}