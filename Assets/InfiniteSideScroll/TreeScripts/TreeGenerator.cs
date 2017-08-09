using UnityEngine;
using System.Collections;
using PLib.Pooling;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using PLib.Math;
using PLib.Rand;
using PLib.General;

namespace InfiniteSideScroll
{
    public delegate void CompletedEventHandler(object sender);
    public delegate void BeginEventHandler(object sender, EventArgs e);
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Add this to an empty game object to create a tree.
    /// Prefabs for trunk and leaves must be provided.
    /// </summary>
    public class TreeGenerator : MonoBehaviour
    {
        #region Inspector assigned
        public bool generateTrunk = true;
        public bool generateBranches = true;
        public bool generateLeaves = true;

        [Tooltip("(sec) Number of seconds for the entire tree to appear.")]
        [Range(0, 8)]
        public float growDuration = 5;
        private float growDelay;

        [Header("Trunk Data")]

        [Tooltip("Segements in the tree trunk")]
        public int segments;

        [Tooltip("Larger values amplify the curvature of branches and trunk")]
        [Range(0, 30)]
        public int curvature = 10;

        [Tooltip("Size of the base of the tree. 0 smallest, 1 is largest")]
        [Range(0, 1)]
        public float thickness = 0.15f;

        [Tooltip("Inspector assigned. Prefabs for the trunk and branches.")]
        public GameObject[] TrunkPrefabs;

        [Header("Limb Data")]

        [Tooltip("Number of limbs. 0 fewer, 1 more")]
        [Range(0, 1)]
        public float limbQuantity;

        [Tooltip("Limb length. 0 shorter, 1 longer")]
        [Range(0, 1)]
        public float limbLength = 0.5f;

        [Tooltip("Minimum hight where limbs can appear. 0 lower, 1 higher")]
        [Range(0, 1)]
        public float limbHeight = 0.5f;

        [Header("Leaf Data")]

        [Tooltip("Amount of leaves. 0 less, 1 more")]
        [Range(0, 1)]
        public float leafFill = 0.65f;

        [Tooltip("Inspector assigned. Prefabs for the leaves.")]
        public GameObject[] LeafPrefabs;

        [Tooltip("Inspector assigned. Scene Controller tag.")]
        public string sceneControllerTag = "SceneController";

        #endregion
        #region Data

        //  container name for leaves
        private string leafGroupName = "leafGroup";

        /// <summary>
        /// Data representation of the tree. Consists of 
        /// a lot of Vector4 objects, which all represent
        /// individual nodes of the tree (trunk, branch,
        /// leaves). The Vector4 have xyz values that 
        /// represent local position of the node. The w
        /// value represents the node size (scale 
        /// dimension).
        /// </summary>
        [SerializeField]
        private TreeData treeData;

        /// <summary>
        /// This data structure is a group of lists that
        /// contain position and size values for every 
        /// node in the tree.
        /// </summary>
        [System.Serializable]
        private class TreeData
        {
            /// <summary>
            /// Returns the a Vector3 position of the world
            /// space location of the trunk's root node.
            /// In the scene hierarchy, this will be the top
            /// most node in the GameObject group.
            /// </summary>
            public Vector3 position
            {
                get
                {
                    return new Vector3(trunk[0].x, trunk[0].y, trunk[0].z);
                }
            }

            /// <summary>
            /// This returns the total number of nodes in
            /// the tree.
            /// </summary>
            public int size;

            /// <summary>
            /// The nodes that make up the tree trunk.
            /// Node 0 is the bottom of the tree.
            /// </summary>
            public List<Vector4> trunk;

            /// <summary>
            /// The nodes that make up the individual
            /// branches of the tree. There is no relation
            /// information regarding where the branches
            /// connect. Although, the base node of each
            /// branch (index 0) will share the same xyz
            /// coordinates of one of the trunk nodes.
            /// </summary>
            public List<List<Vector4>> branches;

            /// <summary>
            /// The nodes that make up the leaf-groups.
            ///	Represented by 3 values. The first is the branch
            ///	index, the second is the node index on the
            ///	branch, the third is the scale of the leaf.
            /// </summary>
            public List<Vector3Int> leaves;

            /// <summary>
            /// Constructor
            /// </summary>
            public TreeData(List<Vector4> trunk, List<List<Vector4>> branches, List<Vector3Int> leaves)
            {
                this.trunk = trunk;
                this.branches = branches;
                this.leaves = leaves;
                this.size = trunk.Count + leaves.Count;
                foreach (var b in branches)
                {
                    this.size += b.Count;
                }
            }

            /// <summary>
            /// Clears data from all nodes. Returns the
            /// number of nodes that were removed.
            /// </summary>
            /// <returns></returns>
            public int Recycle()
            {
                int cleared = 0;
                if (trunk != null)
                {
                    cleared = trunk.Count;
                    trunk.Clear();
                }

                if (branches != null)
                {
                    for (int i = branches.Count - 1; i > 0; i--)
                    {
                        var b = branches[i];
                        cleared += b.Count;
                        b.Clear();
                    }
                    branches.Clear();
                }

                if (leaves != null)
                {
                    cleared += leaves.Count;
                    leaves.Clear();
                }

                return cleared;
            }
        }

        private GameObject sceneManager;

        #endregion
        #region Property aliases for long method calls

        /// <summary>
        /// Property alias for long method call
        /// </summary>
        private int RndAngle
        {
            get { return PRand.RandomRightAngle(); }
        }

        /// <summary>
        /// Returns a random quaternion rotation, where all angles are
        /// multiples of 90 degrees
        /// </summary>
        private Quaternion RndRightAngleRotation
        {
            get
            {
                return Quaternion.Euler(RndAngle, RndAngle, RndAngle);
            }
        }

        #endregion

        #region Unity API

        void Awake()
        {
            GameObject manager = GameObject.FindGameObjectWithTag(sceneControllerTag);
            if (manager != null) sceneManager = manager;
        }

        void Start()
        {
            foreach (var p in TrunkPrefabs) PPool.SetLimit(p, -1);
            foreach (var p in LeafPrefabs) PPool.SetLimit(p, -1);
            Regenerate();
        }

        #endregion
        #region External API

        /// <summary>
        /// Called when generation is complete. A tree cannot
        /// be moved during generation because it's nodes are positioned
        /// in world coordinates, and positioning is spread over many
        /// frames.
        /// </summary>
        public event CompletedEventHandler GenerationComplete;

        private void OnGenerationComplete()
        {
            if (GenerationComplete != null) GenerationComplete(this);
        }

        /// <summary>
        /// Called when recycling of all child elements
        /// is complete. At which point the game object
        /// itself can be recycled.
        /// </summary>
        public event CompletedEventHandler RecycleComplete;

        private void OnRecycleComplete()
        {
            if (RecycleComplete != null) RecycleComplete(this);
        }

        /// <summary>
        /// Delegate for listening for tree "Recycle Complete"
        /// events.
        /// </summary>
        public delegate void RecycleCompleteEventHandler(object sender, EventArgs e);

        public void RecycleAndRegenerate()
        {
            Recycle();
            Regenerate();
        }

        /// <summary>
        /// Recycles the current tree, if there is one.
        /// Re-generates tree data.
        /// Starts the coroutine that builds the tree
        /// out of GameObjects.
        /// </summary>
        public void Regenerate()
        {
            //	generate new data
            treeData = GenerateTreeData(segments);

            //  calculate growth time
            growDelay = growDuration / treeData.size;

            //  begin the growth process
            StartCoroutine("CreateTreeGameObjects");
        }

        /// <summary>
        /// Systematically goes through the game objects 
        /// that make up the tree and returns each one to 
        /// the pooled resources object. TreeData remains
        /// intact.
        /// </summary>
        public void Recycle()
        {
            //  delete the data structure
            if (treeData != null) treeData.Recycle();

            //	remove the leaf group
            Transform leafGroup = transform.Find(leafGroupName);
            if (leafGroup != null)
            {
                for (int i = leafGroup.childCount - 1; i > 0; i--)
                {
                    Transform leaf = leafGroup.GetChild(0);
                    leaf.gameObject.Unparent();
                    PPool.Return(leaf.gameObject);
                }
            }

            //	remove all the remaining nodes

            //	start at the top node
            Transform current = transform;

            while (current.childCount > 0 || !current.Equals(transform))
            {
                if (current.childCount > 0)
                {
                    //	descend
                    current = current.GetChild(0);
                }
                else
                {
                    //	ascend
                    current = current.parent;

                    //	detatch and recycle the first child
                    Transform child = current.GetChild(0);
                    child.gameObject.Unparent();
                    PPool.Return(child.gameObject);
                }
            }

            OnRecycleComplete();
        }

        #endregion
        #region Data Generation

        /// <summary>
        /// Returns a TreeData object.
        /// </summary>
        private TreeData GenerateTreeData(int segments)
        {
            //  create the trunk
            #region trunk
            List<Vector4> trunk = new List<Vector4>();
            trunk = GenerateBranchData(segments);

            //  Adjust trunk size by adding a
            //  slight taper to the sizes.
            for (int i = 0; i < segments; i++)
            {
                //	scale the baseSize input
                float size = thickness / 5;

                //  apply the size taper
                float scale = Mathf.Pow(1 + segments - i, 1 + size) / segments;

                //  prevent the size from being too small
                scale = Mathf.Max(0.5f, scale);

                //  add a little pure randomness
                scale += Random.Range(0.95f, 1.05f);

                //  apply the size to the trunk element
                Vector4 t = trunk[i];
                t.w *= scale;
                trunk[i] = t;
            }
            #endregion

            //  create branches
            #region branches
            List<List<Vector4>> branches = new List<List<Vector4>>();

            //	determine the number of branches to create
            int numberOfBranches = Mathf.CeilToInt(limbQuantity * Mathf.Max(3, segments / 2 + Random.Range(0, 3)));

            //	create each branch data
            for (int bCount = 0; bCount < numberOfBranches; bCount++)
            {

                //  determine number of segment in branch
                int branchSegments = Mathf.RoundToInt(segments * (3 + limbLength) / 4 * Random.Range(0.35f, 0.75f));

                //	initialize a new branch
                List<Vector4> branch = new List<Vector4>();

                //	generate the data for the branch
                branch = GenerateBranchData(branchSegments);

                //	add the branch to the tree
                branches.Add(branch);
            }
            #endregion

            //  create the leaves
            #region leaves
            List<Vector3Int> leaves = new List<Vector3Int>();

            //	itterate among all the branches
            for (int bIndex = 0; bIndex < branches.Count; bIndex++)
            {
                List<Vector4> branch = branches[bIndex];

                //	itterate down the branch among each node
                for (int nIndex = 0; nIndex < branch.Count; nIndex++)
                {
                    if (Random.value < leafFill)
                    {
                        Vector4 node = branch[nIndex];

                        int quantity = Mathf.RoundToInt(leafFill * (3 + Random.Range(0, 4)));
                        for (int i = quantity; i > 0; i--)
                        {
                            //	the size of each leaf cluster is 2-3
                            //	x the branch node.
                            int size = Mathf.CeilToInt(((1 + 3 * leafFill) / 2f) * Random.Range(1, 3));

                            //	add the leave cluster to the tree
                            leaves.Add(new Vector3Int(bIndex, nIndex, size));
                        }
                    }
                }
            }
            #endregion

            return new TreeData(trunk, branches, leaves);
        }

        /// <summary>
        /// Returns a list of directions & distances.
        ///	The base node is the root node. Data is a Vector4. X, Y, Z values
        /// indicate direction, W value indicates distance.
        /// </summary>
        private List<Vector4> GenerateBranchData(int segments)
        {
            List<Vector4> positions = new List<Vector4>();
            float perlinBase = System.DateTime.Now.Millisecond + treeData.size;

            for (int i = 0; i < segments; i++)
            {
                //	initial node look direction
                Vector3 lookDirection = Vector3.forward;

                //	add noise
                float step = perlinBase + (i + 1) * 0.1f;
                Vector2 noise = new Vector2(Mathf.PerlinNoise(0, step), Mathf.PerlinNoise(step, 0));
                noise *= curvature / 30f;

                //	Convert noise to forward orientation
                //	and add to position.
                lookDirection += Vector3.right * noise.x + Vector3.up * noise.y;

                positions.Add(new Vector4(lookDirection.x, lookDirection.y, lookDirection.z, 1));
            }
            return positions;
        }

        #endregion
        #region GameObject creation

        /// <summary>
        /// Creates and parents all the game objects for a tree.
        /// </summary>
        /// <param name="tree"></param>
        private IEnumerator CreateTreeGameObjects()
        {
            if (treeData == null) throw new System.NullReferenceException();

            //  create tree trunk
            if (generateTrunk)
            {
                yield return CreateBranchGameObjects(treeData.trunk, transform.position, true);
            }

            //  create branches
            if (generateBranches)
            {
                foreach (var branch in treeData.branches)
                {
                    //	TODO weight selection so it is biased towards the top
                    //	TODO weight selection so it is biased towards non-branch nodes
                    float min = 0.1f + 0.8f * limbHeight;
                    int nodeDepth = Mathf.FloorToInt(segments * Random.Range(min, 1)) - 1;
                    Transform trunkNode = transform;

                    //	Traverse the trunk hierarchy to get
                    //	the node where the branch starts.
                    for (int i = 0; i < nodeDepth; i++) trunkNode = trunkNode.GetChild(0);

                    yield return CreateBranchGameObjects(branch, trunkNode.position);
                }
            }

            //  create leaves
            if (generateLeaves)
            {
                GameObject leafGroup = new GameObject(leafGroupName);
                foreach (var leaf in treeData.leaves)
                {
                    int branchIndex = leaf.x;
                    int nodeDepth = leaf.y;
                    int size = leaf.z;
                    Transform node = transform.GetChild(1 + branchIndex);
                    node = GetChildAtDepth(node, nodeDepth);

                    Vector3 position = node.position + PRand.OnSphere(1) + Vector3.up * 0.5f;
                    Vector3 scale = Vector3.one * size;

                    Transform t = PPool.Get(LeafPrefabs.GetRandom()).transform;
                    t.position = position;
                    t.rotation = Quaternion.Euler(0, PRand.RandomDegreeAngle(), 0);
                    t.localScale = scale;

                    t.gameObject.SetParent(leafGroup);
                    yield return new WaitForSeconds(growDelay);
                }
                leafGroup.SetParent(gameObject);
            }

            OnGenerationComplete();

            //  set message to sim control
            //if (sceneManager != null)
            //{
            //    sceneManager.BroadcastMessage(TreeDemoManager.TREE_IS_READY, SendMessageOptions.RequireReceiver);
            //}
        }

        /// <summary>
        /// Creates and parents game objects for a branch.
        /// </summary>
        private IEnumerator CreateBranchGameObjects(List<Vector4> data, Vector3 rootPosition, bool isMainTrunk = false)
        {
            //	the last node referenced during the itteration
            GameObject parent = null;

            for (int i = 0; i < data.Count; i++)
            {
                Vector4 node = data[i];
                GameObject element = PPool.Get(TrunkPrefabs.GetRandom());

                //	first node is positioned at rootPosition
                if (i == 0)
                {

                    //	set the root node position
                    element.transform.position = rootPosition;

                    //	orient the root node depending on the type of branch
                    if (isMainTrunk)
                    {

                        //	randomly rotate tree along vertical axis
                        element.transform.Rotate(Vector3.up, PRand.RandomDegreeAngle());

                        //	main trunk points up, with a slight tilt
                        element.transform.forward = Vector3.up + PRand.InsideSphere(0.1f);

                        //	apply the scaling (before parenting)
                        //element.transform.localScale = Vector3.one * Mathf.Max(0.65f, node.w);
                        element.transform.localScale = Vector3.one * node.w * ((3 + thickness) / 3) * (data.Count - i + 12) / (data.Count + 12);
                    }
                    else
                    {
                        //	branches point out
                        Vector3 direction = PRand.OnCircle().ToVector3() + PRand.InsideSphere(0.1f) + 0.5f * Vector3.up;
                        element.transform.forward = direction;
                        element.transform.Rotate(Vector3.forward, PRand.RandomDegreeAngle());

                        //	apply the scaling (before parenting)
                        element.transform.localScale = Vector3.one * 0.75f * ((3 + thickness) / 3) * (data.Count - i + 1) / (data.Count + 1);
                    }

                    //  attach branch to the tree root
                    element.SetParent(gameObject);
                }
                else
                {
                    //	subsequent nodes

                    //	position node 1 unit in front of the parent
                    element.transform.position = parent.transform.position;
                    element.transform.Translate(parent.transform.forward);

                    //	orient the node to face away from the parent
                    element.transform.forward = parent.transform.forward;

                    //	apply the scaling (before parenting)
                    if (isMainTrunk)
                    {
                        //element.transform.localScale = Vector3.one * node.w;
                        element.transform.localScale = node.w * Vector3.one * ((3 + thickness) / 3) * (data.Count - i + 12) / (data.Count + 12);
                    }
                    else
                    {
                        element.transform.localScale = Vector3.one * 0.75f * ((3 + thickness) / 3) * (data.Count - i + 1) / (data.Count + 1);
                    }

                    //	connect to parent
                    element.SetParent(parent);
                }

                //	add the look variation
                Vector3 lookDirection = new Vector3(node.x, node.y, node.z);
                Quaternion rotation = Quaternion.LookRotation(lookDirection);
                element.transform.forward = rotation * element.transform.forward;

                //	set the "parent" reference to this node
                parent = element;

                yield return new WaitForSeconds(growDelay);
            }
        }

        /// <summary>
        /// Walks down a parented transform structure. Always chooses
        /// the first child. Returns the deepest child discovered.
        /// </summary>
        private Transform GetDeepestChild(Transform t)
        {
            while (t.childCount > 0)
            {
                t = t.GetChild(0);
            }
            return t;
        }

        private Transform GetChildAtDepth(Transform t, int depth)
        {
            Transform child = t;

            for (int currentDepth = 0; currentDepth < depth; currentDepth++)
            {
                child = child.GetChild(0);

                //	no child found
                if (child == null) break;
            }

            return child;
        }
        #endregion
    }
}