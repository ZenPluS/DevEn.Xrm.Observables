using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevEn.Xrm.Observables.Models
{
    internal class Delegates : IEnumerable<Delegate>
    {
        public static implicit operator Delegates(Delegate[] lst)
            => lst;

        public IEnumerator<Delegate> GetEnumerator()
            => new List<Delegate>.Enumerator();

        IEnumerator IEnumerable.GetEnumerator()
            =>  new List<Delegate>.Enumerator();
    }
}
