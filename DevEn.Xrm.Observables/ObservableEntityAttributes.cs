using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DevEn.Xrm.Observables;

public sealed class ObservableEntityAttributes : Entity
{
    private readonly Dictionary<string, BehaviorSubject<object>> _subjects = new();

    private ObservableEntityAttributes(string entityName)
        : base(entityName)
    { }

    private ObservableEntityAttributes(Entity entity) : base(entity.LogicalName, entity.Id)
    {
        EntityState = entity.EntityState;
        FormattedValues = entity.FormattedValues;
        RelatedEntities = entity.RelatedEntities;
        RowVersion = entity.RowVersion;
    }

    public AttributeCollection Attributes
    {
        get => base.Attributes;
        set
        {
            base.Attributes = value;
            foreach (var kvp in value)
            {
                Notify(kvp.Key, kvp.Value);
            }
        }
    }

    public static ObservableEntityAttributes Create(Entity entity) => new(entity);
    public static ObservableEntityAttributes Create(string entityName) => new(entityName);

    public new object this[string key]
    {
        get => base.Attributes[key];
        set
        {
            base.Attributes[key] = value;
            Notify(key, value);
        }
    }

    public void Add<T>(string key, T value)
        => this[key] = value;

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

    public override T GetAttributeValue<T>(string key)
        => (T)this[key];

    public void SetAttributeValue<T>(string key, T value)
    {
        this[key] = value;
    }
}