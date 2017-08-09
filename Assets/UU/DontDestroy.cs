using UnityEngine;
using PLib;

namespace PLib
{
    /// <summary>
    /// 2016-5-11
    /// Don't destroy on load.
    /// </summary>
    class DontDestroy : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
