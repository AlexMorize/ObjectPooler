using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SR.ObjectPooler
{
    public interface IReleaseFromPoolHandler
    {
        void OnReleaseFromPool();
    }
}
