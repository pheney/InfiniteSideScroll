using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PLib;
using PLib.Pooling;
using Random = UnityEngine.Random;
using PLib.General;
using PLib.Rand;
using PLib.Math;

namespace InfiniteSideScroll
{
    /// <summary>
    /// Manages tree objects in the procedural scrolling
    /// level. Since levels are deterministic, the heuristic
    /// for when trees appear will be as follows:
    /// When the total number of bricks falls below a certain
    /// value, generate a tree.
    /// 
    /// Reasoning: Lots of bricks means a busy level, players
    /// will focus on navigating the level. Fewer bricks means
    /// an easy stretch, players will have more mental resources
    /// available to notice background elements.
    /// </summary>
    class IssTreeManager : MonoBehaviour
    {
        #region Inspector Assigned

        [Header("Debug")]

        public bool disableInput = true;

        [Header("Inspector Assigned")]

        [Tooltip("Adjusts the frequency of"
            +" tree generation")]
        public int treeFrequency = 0;

        [Tooltip("The object that holds"
            + " the moving bricks and trees.")]
        public GameObject sceneHolder;

        [Tooltip("Tag that identifies bricks as bricks.")]
        public string brickTag;

        [Header("Prefabs (Inspector Assigned)")]

        [Tooltip("Small trees. Seen most often")]
        public GameObject[] smallTreePrefabs;

        [Tooltip("Large trees. Seen less often")]
        public GameObject[] largeTreePrefabs;

        #endregion
        #region Data

        private IssBrickObjectManager brickManager;

        /// <summary>
        /// When the number of onscreen bricks falls 
        /// below a value equal to brickThreshold, a
        /// tree is spawned.
        /// 
        /// brickThreshold =  screenWidthByBricks x (1 + thresholdMultiple)
        /// </summary>
        private int brickThreshold = 0;

        /// <summary>
        /// This value is used to determine brickThreshold.
        ///
        /// brickThreshold =  screenWidthByBricks x (1 + thresholdMultiple)
        ///
        /// This value is linearly adjusted by the variable
        /// "treeFrequency"
        /// </summary>
        private float thresholdMultiple = 0.1f;

        private List<GameObject> liveSmallTrees;
        private List<GameObject> liveLargeTrees;

        #endregion
        #region Unity API

        void Awake()
        {
            brickManager = GetComponent<IssBrickObjectManager>();
        }

        void Start()
        {
            liveSmallTrees = new List<GameObject>();
            liveLargeTrees = new List<GameObject>();
            brickThreshold = Mathf.RoundToInt(brickManager.screenWidthByBricks * (1 + thresholdMultiple) + treeFrequency);
        }

        #endregion
        #region External API

        public void RecycleTree(GameObject treeToRecycle)
        {
            GameObject tree = treeToRecycle;
            tree.Unparent();
            TreeGenerator gen = tree.GetComponent<TreeGenerator>();
            gen.RecycleComplete += new CompletedEventHandler(RemoveTreeFromScene);
        }

        public void RemoveTreeFromScene(object source)
        {
            TreeGenerator tree = (TreeGenerator)source;
            tree.RecycleComplete -= new CompletedEventHandler(RemoveTreeFromScene);
            tree.GenerationComplete -= new CompletedEventHandler(AddTreeToScene);
            PPool.Return(tree.gameObject);
        }

        #endregion
        #region Internal Methods

        /// <summary>
        /// Determines if a new tree should be created, 
        /// and creates one if needed.
        /// 
        /// Nomally, the seed value is the total distance 
        /// the player has travelled in either direction.
        /// 
        /// The more trees that exist, the less likely new
        /// trees should be generated.
        /// 
        /// An edge case to consider: When the player moves
        /// rapidly back-and-forth across the generation
        /// threshold, e.g., at +5m, SpawnTree() is called,
        /// and what if the player changes direction rapidly
        /// right at the +5m mark. Prevent multiple trees
        /// spawning.
        /// </summary>
        private void SpawnTree(float seed)
        {
            //  Chance of a tree is based on the ratio
            //  of bricks on screen to the threshold
            int screenWidthByBricks = brickManager.screenWidthByBricks;
            int threshold = Mathf.Max(0, brickThreshold - screenWidthByBricks);

            //  count the bricks in the scene
            int currentBricks = 0;
            Transform[] bricks = sceneHolder.FindChildrenWithTag(brickTag);
            foreach (Transform brick in bricks) {
                currentBricks += 1 + brick.gameObject.GetComponentsInChildren<Transform>().Length;
            }
            //  higher overage reduces chances
            int overage = Mathf.Max(0,currentBricks - screenWidthByBricks);

            //  1. prevents division by zero
            //  2. reduces severity of curvature
            float smoothingConstant = 2;

            //  probability of creating a tree
            float ratio = Mathf.Pow(threshold + smoothingConstant, 2) / Mathf.Pow(overage + smoothingConstant, 2);
            float probability = Mathf.Log10(1 + ratio);

            //  The more bricks on screen, the less likely
            //  to create a tree
            if (Random.value < probability) return;

            //  The more bricks on screen, the more likely
            //  the tree is to be small
            bool small = Random.value < probability;

            //  Create the tree

            GameObject prefab = null;
            GameObject tree = null;

            if (small)
            {
                //  make small tree
                prefab = smallTreePrefabs.GetRandom();
                tree = PPool.Get(prefab);
                liveSmallTrees.Add(tree);
            }
            else
            {
                //  make big tree
                prefab = largeTreePrefabs.GetRandom();
                tree = PPool.Get(prefab);
                liveLargeTrees.Add(tree);
            }
            
            //  start the generation process
            tree.SendMessage("Regenerate", SendMessageOptions.RequireReceiver);

            //  when generation is complete,
            //  use the callback to parent tree to 
            //  scrollHolder
            tree.GetComponent<TreeGenerator>().GenerationComplete += new CompletedEventHandler(AddTreeToScene);
        }

        /// <summary>
        /// Parent the newly created tree to the "sceneHolder",
        /// which is the same object that holds the bricks
        /// the player is running across.
        /// </summary>
        public void AddTreeToScene(object source)
        {
            if (sceneHolder == null) throw new NullReferenceException();

            TreeGenerator tree = (TreeGenerator)source;
            GameObject treeObject = tree.gameObject;
            Transform furthestBrick = sceneHolder.FindChildrenWithTag(brickTag).GetLast();
            treeObject.transform.position = furthestBrick.position + Vector3.forward * 2;
            treeObject.SetParent(sceneHolder);
        }

        #endregion
        #region AxisInputReceivable.IAxisInputReceivable

        [SerializeField]
        private float cumulativeInput;

        private const int checkFrequency = 5;

        public void OnAxisInput(Vector2 inputVector)
        {
            if (disableInput) return;
            if (inputVector.Equals(Vector2.zero)) return;

            cumulativeInput += inputVector.x;
            bool isZero = Mathf.Approximately(cumulativeInput,0);
            if (!isZero && Mathf.Abs(Mathf.FloorToInt(cumulativeInput)) % checkFrequency == 0)
            {
                if (cumulativeInput.IsPositive())
                {
                    cumulativeInput -= checkFrequency;
                }
                if (cumulativeInput.IsNegative())
                {
                    cumulativeInput += checkFrequency;
                }
                SpawnTree(cumulativeInput);
            }
        }

        public void OnInputEnabled(bool enableInput) { }

        #endregion
        #region Debugging
        #endregion
    }
}
