using System;
using System.Collections.Generic;
using System.Linq;
using DevEn.Xrm.Observables.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables;

/// <summary>
/// Represents an observable entity that tracks changes to its attributes and allows subscribing to attribute changes.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class ObservableEntity<TEntity>
    : IObservableEntity<TEntity>
    where TEntity : Entity
{
    private readonly HashSet<string> _trackedKeys = new(new List<string>(), StringComparer.OrdinalIgnoreCase);
    private readonly IDictionary<string, List<Delegate>> _delegatesOnChange = new Dictionary<string, List<Delegate>>();
    private readonly TEntity _entity;

    /// <inheritdoc />
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
            var currentDelegate = isContains ? _delegatesOnChange[attributeName] : null;
            _entity[attributeName] = value;

            if (isContains)
            {
                currentDelegate.ForEach(d => d?.DynamicInvoke());
            }
        }
    }

    public static implicit operator Entity(ObservableEntity<TEntity> observableEntity)
        => observableEntity.GetEntity();

    public static implicit operator ObservableEntity<TEntity>(TEntity entity)
        => Create(entity);

    public static implicit operator ObservableEntity<TEntity>(string logicalName)
        => (TEntity)Create(logicalName);

    /// <inheritdoc />
    /// <summary>
    /// Invokes all subscribed delegates for the tracked attributes.
    /// </summary>
    public void InvokeAllOnChange()
    {
        var entityKeys = _entity.Attributes.Keys.ToList();
        entityKeys.ForEach(key =>
        {
            var isContains = _trackedKeys.Contains(key);
            var currentDelegate = isContains ? _delegatesOnChange[key] : null;
            if (isContains)
            {
                currentDelegate.ForEach(d => d?.DynamicInvoke());
            }
        });
    }

    /// <inheritdoc />
    /// <summary>
    /// Invokes specific subscribed delegate.
    /// </summary>
    public void InvokeOnChange(string key)
    {
        var isContains = _trackedKeys.Contains(key);
        var currentDelegate = isContains ? _delegatesOnChange[key] : null;
        if (isContains)
        {
            currentDelegate.ForEach(d => d?.DynamicInvoke());
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Gets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <returns>The value of the attribute.</returns>
    public T GetValue<T>(string key)
        => (T)_entity[key];

    /// <inheritdoc />
    /// <summary>
    /// Sets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The current instance of <see cref="T:DevEn.Xrm.Observables.ObservableEntity`1" />.</returns>
    public ObservableEntity<TEntity> SetValue<T>(string key, T value)
    {
        var isContains = _trackedKeys.Contains(key);
        var currentDelegate = isContains ? _delegatesOnChange[key] : null;
        _entity[key] = value;

        if (isContains)
        {
            currentDelegate.ForEach(d => d?.DynamicInvoke());
        }
        return this;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified entity.
    /// </summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<TEntity> Create(TEntity entity)
        => new(entity);

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified entity.
    /// </summary>
    /// <param name="entity">The entity to wrap.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<Entity> Create(Entity entity)
        => new(entity);

    /// <summary>
    /// Creates a new instance of <see cref="ObservableEntity{TEntity}"/> with the specified logical name.
    /// </summary>
    /// <param name="logicalName">The logical name of the entity.</param>
    /// <returns>A new instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    public static ObservableEntity<Entity> Create(string logicalName)
        => new(logicalName);

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

    /// <inheritdoc />
    /// <summary>
    /// Gets the underlying entity.
    /// </summary>
    /// <returns>The underlying entity.</returns>
    public Entity GetEntity()
        => _entity;

    /// <inheritdoc />
    /// <summary>
    /// Subscribes to changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="onChange">The delegate to invoke when the attribute value change</param>
    public void AddOnChange(string key, params Delegate[] onChange)
    {
        if (key == null || (onChange == null || onChange.Length == 0))
            return;

        _trackedKeys.Add(key);
        _delegatesOnChange.Add(key, onChange.ToList());
    }

    /// <inheritdoc />
    /// <summary>
    /// Unsubscribes from changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    public void RemoveOnChange(string key)
    {
        _trackedKeys.Remove(key);
        if (_delegatesOnChange.ContainsKey(key))
            _delegatesOnChange.Remove(key);
    }
}
