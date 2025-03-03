using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace DevEn.Xrm.Observables
{
    public class ObservableEntityAttributes
        : Entity
    {
        private readonly Dictionary<string, BehaviorSubject<object>> _subjects = new();

        public void Add<T>(string key, T value)
        {
            if (!_subjects.ContainsKey(key))
            {
                _subjects[key] = new BehaviorSubject<object>(value);
            }
            Attributes[key] = value;
        }

        public new object this[string key]
        {
            get => base[key];
            set => SetAndNotify(key, value);
        }

        public void Update<T>(string key, T value)
        {
            if (_subjects.TryGetValue(key, out var subject))
            {
                ((BehaviorSubject<object>)subject).OnNext(value); // Notify observers of change
            }
            Attributes[key] = value;
        }

        private void SetAndNotify(string key, object value)
        {
            base[key] = value;

            if (_subjects.TryGetValue(key, out var observable))
            {
                ((BehaviorSubject<object>)observable).OnNext(value);
            }
            else
            {
                _subjects[key] = new BehaviorSubject<object>(value);
            }
        }

        public IObservable<object> Observe<T>(string key)
        {
            if (!_subjects.TryGetValue(key, out var observe))
            {
                return null;
            }

            return observe as IObservable<object>;
        }

        public override T GetAttributeValue<T>(string key)
        {
            if (Attributes.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default!;
        }

        public void SetAttributeValue<T>(string key, T value)
        {
            SetAndNotify(key, value!);
        }

        public Entity ToEntity() => this;
    }
}
