using CrashKonijn.Goap.Core;
using UnityEngine;

namespace SIGGD.Goap.Interfaces
{
    public interface IInjectable
    {
        public void Inject(GoapInjector injector);
    }
}