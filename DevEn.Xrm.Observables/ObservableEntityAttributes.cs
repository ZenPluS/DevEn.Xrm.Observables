using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DevEn.Xrm.Observables
{
    public sealed class ObservableEntityAttributes : Entity
    {
        private readonly Dictionary<string, object> _subjects = new();

        // Usiamo un dizionario osservabile per intercettare le modifiche
        private readonly ObservableDictionary<string, object> _attributesWrapper;

        private ObservableEntityAttributes(string entityName) : base(entityName) 
        { 
            _attributesWrapper = new ObservableDictionary<string, object>();
            _attributesWrapper.ValueChanged += OnAttributeChanged;
        }

        private ObservableEntityAttributes(Entity entity) : base(entity.LogicalName, entity.Id)
        {
            _attributesWrapper = new ObservableDictionary<string, object>(entity.Attributes);
            _attributesWrapper.ValueChanged += OnAttributeChanged;
            EntityState = entity.EntityState;
            FormattedValues = entity.FormattedValues;
            RelatedEntities = entity.RelatedEntities;
            RowVersion = entity.RowVersion;
        }

        public static ObservableEntityAttributes Create(Entity entity) => new(entity);
        public static ObservableEntityAttributes Create(string entityName) => new(entityName);

        public override Dictionary<string, object> Attributes => _attributesWrapper;

        public new object this[string key]
        {
            get => Attributes.ContainsKey(key) ? Attributes[key] : null;
            set => SetAndNotify(key, value);
        }

        public void Add<T>(string key, T value)
        {
            SetAndNotify(key, value);
        }

        private void SetAndNotify(string key, object value)
        {
            _attributesWrapper[key] = value;
        }

        private void OnAttributeChanged(string key, object value)
        {
            if (_subjects.TryGetValue(key, out var subject) && subject is ISubject<object> typedSubject)
            {
                typedSubject.OnNext(value);
            }
            else
            {
                var newSubject = new BehaviorSubject<object>(value);
                _subjects[key] = newSubject;
            }
        }

        public IObservable<T> Observe<T>(string key)
        {
            if (!_subjects.TryGetValue(key, out var subject))
            {
                var newSubject = new BehaviorSubject<object>(default(T)!);
                _subjects[key] = newSubject;
                return newSubject.Cast<T>();
            }

            return subject is IObservable<object> observable
                ? observable.Cast<T>()
                : Observable.Empty<T>();
        }

        public override T GetAttributeValue<T>(string key)
        {
            return Attributes.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default!;
        }

        public void SetAttributeValue<T>(string key, T value)
        {
            SetAndNotify(key, value!);
        }
    }

    /// <summary>
    /// Dizionario che notifica ogni modifica ai valori
    /// </summary>
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public event Action<TKey, TValue> ValueChanged;

        public ObservableDictionary() : base() { }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                base[key] = value;
                ValueChanged?.Invoke(key, value);
            }
        }
    }
}
