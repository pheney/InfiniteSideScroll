using UnityEngine;
using System.Collections;

namespace InfiniteSideScroll {
    public class TreeDemoManager : MonoBehaviour
    {
        #region Inspector assigned

        [Tooltip("Inspector assigned. Trees to be managed.")]
        public TreeGenerator[] treeGenerators;

        [Tooltip("Insector assigned. The simulation control object.")]
        public GameObject simControlUI;

        #endregion

        #region Data
        private int waitingCount;

        #endregion

        #region Unity API

        void Start()
        {
            waitingCount = treeGenerators.Length;

            foreach (TreeGenerator tree in treeGenerators)
            {
                tree.GenerationComplete += new CompletedEventHandler(OnTreeReady);
            }
        }

        #endregion
        #region public API

        public void RecycleAll()
        {
            foreach (TreeGenerator tree in treeGenerators)
            {
                tree.Recycle();
            }
            foreach (TreeGenerator tree in treeGenerators)
            {
                tree.Regenerate();
                waitingCount++;
            }
            simControlUI.SetActive(false);
        }

        #endregion

        #region Message Listeners

        public static string TREE_IS_READY = "OnTreeReady";
        public void OnTreeReady(object source)
        {
            waitingCount--;
            if (waitingCount == 0) simControlUI.SetActive(true);
        }
        #endregion
    }
}