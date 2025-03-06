using Microsoft.Xrm.Sdk;
using System;

namespace DevEn.Xrm.Observables.Core;

public interface IObservableEntity<TEntity>
    where TEntity : Entity
{
    /// <summary>
    /// Gets or sets the value of the specified attribute.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The value of the attribute.</returns>
    object this[string attributeName] { get; set; }

    /// <summary>
    /// Invokes all subscribed delegates for the tracked attributes.
    /// </summary>
    void InvokeAllOnChange();

    /// <summary>
    /// Invokes specific subscribed delegate.
    /// </summary>
    void InvokeOnChange(string key);

    /// <summary>
    /// Gets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <returns>The value of the attribute.</returns>
    T GetValue<T>(string key);

    /// <summary>
    /// Sets the value of the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The current instance of <see cref="ObservableEntity{TEntity}"/>.</returns>
    ObservableEntity<TEntity> SetValue<T>(string key, T value);

    /// <summary>
    /// Gets the underlying entity.
    /// </summary>
    /// <returns>The underlying entity.</returns>
    Entity GetEntity();

    /// <summary>
    /// Subscribes to changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="onChange">The delegates to invoke when the attribute value change</param>
    void AddOnChange(string key, params Delegate[] onChange);

    /// <summary>
    /// Unsubscribes from changes of the specified attribute.
    /// </summary>
    /// <param name="key">The name of the attribute.</param>
    void RemoveOnChange(string key);
}