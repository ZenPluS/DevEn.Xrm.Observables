using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DevEn.Xrm.Observables
{
    /// <summary>
    /// Represents an observable entity that extends the functionality of the <see cref="Entity"/> class
    /// and provides mechanisms to observe changes to its attributes.
    /// </summary>
    public sealed class ObservableEntityAttributes : Entity
    {
        private readonly Dictionary<string, BehaviorSubject<object>> _subjects = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableEntityAttributes"/> class with the specified entity name.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        private ObservableEntityAttributes(string entityName)
            : base(entityName)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableEntityAttributes"/> class from an existing <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The existing entity.</param>
        private ObservableEntityAttributes(Entity entity) : base(entity.LogicalName, entity.Id)
        {
            EntityState = entity.EntityState;
            FormattedValues = entity.FormattedValues;
            RelatedEntities = entity.RelatedEntities;
            RowVersion = entity.RowVersion;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ObservableEntityAttributes"/> from an existing <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The existing entity.</param>
        /// <returns>A new instance of <see cref="ObservableEntityAttributes"/>.</returns>
        public static ObservableEntityAttributes Create(Entity entity) => new(entity);

        /// <summary>
        /// Creates a new instance of <see cref="ObservableEntityAttributes"/> with the specified entity name.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <returns>A new instance of <see cref="ObservableEntityAttributes"/>.</returns>
        public static ObservableEntityAttributes Create(string entityName) => new(entityName);

        /// <summary>
        /// Gets or sets the value of the attribute with the specified key.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <returns>The value of the attribute.</returns>
        public new object this[string key]
        {
            get => Attributes[key];
            set
            {
                Attributes[key] = value;
                Notify(key, value);
            }
        }

        /// <summary>
        /// Adds a new attribute with the specified key and value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public void Add<T>(string key, T value)
            => this[key] = value;

        /// <summary>
        /// Notifies observers of a change to the attribute with the specified key.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The new value of the attribute.</param>
        private void Notify(string key, object value)
        {
            if (_subjects.TryGetValue(key, out var observable))
            {
                observable.OnNext(value);
            }
            else
            {
                _subjects[key] = new BehaviorSubject<object>(value);
            }
        }

        /// <summary>
        /// Observes changes to the attribute with the specified key.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <returns>An observable sequence of changes to the attribute.</returns>
        public IObservable<object> Observe(string key)
        {
            if (_subjects.TryGetValue(key, out var subject))
            {
                return subject is IObservable<object> observable
                    ? observable.Where(x => x != null).Cast<object>()
                    : Observable.Empty<object>();
            }

            var newSubject = new BehaviorSubject<object>(null);
            _subjects[key] = newSubject;
            return newSubject.Where(x => x != null).Cast<object>();
        }

        /// <summary>
        /// Gets the value of the attribute with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the attribute.</param>
        /// <returns>The value of the attribute.</returns>
        public override T GetAttributeValue<T>(string key)
            => (T)this[key];

        /// <summary>
        /// Sets the value of the attribute with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public void SetAttributeValue<T>(string key, T value)
        {
            this[key] = value;
        }
    }
}
