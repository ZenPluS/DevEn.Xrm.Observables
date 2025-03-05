using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables;

/// <summary>
/// Represents an observable entity that tracks changes to its attributes and allows subscribing to attribute changes.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class ObservableEntity<TEntity>
    where TEntity : Entity
{
    private readonly HashSet<string> _trackedKeys = new(new List<string>(), StringComparer.OrdinalIgnoreCase);
    private readonly IDictionary<string, Delegate> _delegateOnTrackUpdate = new Dictionary<string, Delegate>();
    private readonly TEntity _entity;

    /// <summary>
    /// Gets or sets the value of the specified attribute.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The value of the attribute.</returns>
    public object this[string attributeName]
    {
        get => _entity.Contains(attributeName) ? _entity[attributeName] : null;
        set
        {
            var isContains = _trackedKeys.Contains(attributeName);
            var currentDelegate = isContains ? _delegateOnTrackUpdate[attributeName] : null;
            _entity[attributeName] = value;

            if (isContains)
            {
                currentDelegate?.DynamicInvoke();
            }
        }
    }

    public static implicit operator Entity(ObservableEntity<TEntity> observableEntity)
        => observableEntity.GetEntity();

    public static implicit operator ObservableEntity<TEntity>(TEntity entity)
        => Create(entity);

    /// <summary>
    /// Invokes all subscribed delegates for the tracked attributes.
    /// </summary>
    public void InvokeAll()
    {
        var entityKeys = _entity.Attributes.Keys.ToList();
        entityKeys.ForEach(key =>
        {
            var isContains = _trackedKeys.Contains(key);
            var currentDelegate = isContains ? _delegateOnTrackUpdate[key] : null;
            if (isContains)
            {
                currentDelegate?.DynamicInvoke();
            }
        });
    }

    /// <summary>
    /// Invokes specific subscribed delegate.
    /// </summary>
    public void Invoke(string key)
    {
        var isContains = _trackedKeys.Contains(key);
        var currentDelegate = isContains ? _delegateOnTrackUpdate[key] : null;
        if (isContains)
        {
            currentDelegate?.DynamicInvoke();
        }
    }

    /// <summary>
    /// Gets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <returns>The value of the attribute.</returns>
    public T GetValue<T>(string key)
        => (T)_entity[key];

    /// <summary>
    /// Sets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The current instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public ObservableEntity<TEntity> SetValue<T>(string key, T value)
    {
        var isContains = _trackedKeys.Contains(key);
        var currentDelegate = isContains ? _delegateOnTrackUpdate[key] : null;
        _entity[key] = value;

        if (isContains)
        {
            currentDelegate?.DynamicInvoke();
        }
        return this;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified entity.
    /// </summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<TEntity> Create(TEntity entity) => new(entity);

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified entity.
    /// </summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<Entity> Create(Entity entity) => new(entity);

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified logical name.
    /// </summary>
    /// <param name="logicalName">The logical name of the entity.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<Entity> Create(string logicalName) => new(logicalName);

    private ObservableEntity(TEntity entity)
    {
        _entity = (TEntity)(entity ?? new Entity());
    }

    private ObservableEntity(Entity entity)
    {
        _entity = (TEntity)(entity ?? new Entity());
    }

    private ObservableEntity(string logicalName)
    {
        _entity = (TEntity)new Entity(logicalName);
    }

    /// <summary>
    /// Gets the underlying entity.
    /// </summary>
    /// <returns>The underlying entity.</returns>
    public Entity GetEntity() => _entity;

    /// <summary>
    /// Subscribes to changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="onChange">The delegate to invoke when the attribute changes.</param>
    public void Subscribe(string key, Delegate onChange)
    {
        if (key == null || onChange == null)
            return;

        _trackedKeys.Add(key);
        _delegateOnTrackUpdate.Add(key, onChange);
    }

    /// <summary>
    /// Unsubscribes from changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    public void UnSubscribe(string key)
    {
        _trackedKeys.Remove(key);
        if (_delegateOnTrackUpdate.ContainsKey(key))
            _delegateOnTrackUpdate.Remove(key);
    }
}
