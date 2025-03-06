using System;
using System.Collections;
using System.Collections.Generic;

namespace DevEn.Xrm.Observables.Models
{
    internal class Delegates(IEnumerable<Delegate> delegates)
        : IEnumerable<Delegate>
    {
        private readonly List<Delegate> _delegates = [..delegates];

        public static implicit operator Delegates(Delegate[] lst)
            => new(lst);

        public IEnumerator<Delegate> GetEnumerator()
            => _delegates.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
