using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;
using PLib.Pooling;
using PLib.General;
using PLib.Math;

namespace InfiniteSideScroll
{
    /// <summary>
    /// 2016-5-10
    ///	Iss brick GameObject manager. Responsible for managing the game objects
    ///	that comprise the level. Interfaces directly with IssBrickDataManager.
    /// </summary>
    public class IssBrickObjectManager : MonoBehaviour
    {
        #region Inspector assigned

        [Tooltip("Number of seconds it takes to generate the initial level.")]
        [Range(0, 10)]
        public float growDuration = 3;

        [Tooltip("How many pillars span the width of the screen.")]
        [Range(10, 30)]
        public int screenWidthByBricks = 22;

        [Tooltip("Inspector Assigned. The default brick")]
        public GameObject greenBrick;

        [Tooltip("Inspector Assigned. Pillar container.")]
        public GameObject brickHolder;

        #endregion
        #region Data

        private IssBrickDataManager dataManager;
        private List<PillarData> pillars;
        private List<PillarData> recycledPillars;

        /// <summary>
        /// Represents a column of bricks extending down from
        /// the "main" brick. The "main" brick is the top brick,
        /// which correlates to the data in BrickData in IssBrickDataManager.
        /// Columns maintain a list of "depth" bricks, which are
        /// the bricks beneith the "main" brick.
        /// Columns also maintain pointers to their neighbor bricks.
        /// The neighbor brick positions, and the depth bricks are used
        /// to ensure continutity between neighbors of different heights.
        /// </summary>
        private class PillarData
        {
            #region Navigation fields

            public GameObject mainBrick;
            public PillarData left, right;
            public List<GameObject> depthBricks;

            #endregion
            #region Data fields

            //  The altitude of the main brick
            public int height;

            //  The number of bricks below the main brick
            public int depth
            {
                get
                {
                    return depthBricks.Count;
                }
            }

            #endregion
            #region Constructors

            public PillarData(int height, GameObject brick)
            {
                this.depthBricks = new List<GameObject>();
                Reconstruct(height, brick);
            }

            #endregion
            #region Methods

            public void Reconstruct(int height, GameObject brick)
            {
                this.height = height;
                this.mainBrick = brick;
                this.depthBricks.Clear();
                this.left = null;
                this.right = null;
            }

            /// <summary>
            /// Sets the left neighbor pointer to the brick provided.
            /// </summary>
            public void SetLeftNeighbor(PillarData leftNeighbor)
            {
                this.left = leftNeighbor;
            }

            /// <summary>
            /// Sets the right neighbor pointer to the brick provided.
            /// </summary>
            public void SetRightNeighbor(PillarData rightNeighbor)
            {
                this.right = rightNeighbor;
            }

            /// <summary>
            /// Sets the neighbor brick as the right neighbor for the
            /// main brick.
            /// Uses the position of the neighbor brick to calculate
            /// the main brick's position. The main brick position is
            /// one unit left of the neighbor brick. The main brick's
            /// height information is used instead of the neighbor's.
            /// </summary>
            public void PositionLeftOf(PillarData neighbor)
            {
                if (this.mainBrick == null
                    || neighbor.mainBrick == null)
                {
                    throw new MissingReferenceException(
                        "Either Main brick or neighbor is null. " +
                        "Cannot position pillar.");
                }
                SetRightNeighbor(neighbor);

                //  This brick's position is based on
                //  the neighbor's position.
                Vector3 position = neighbor.mainBrick.transform.position;

                //  Move this brick 1 unit to the left of
                //  the neighbor's position.
                position += Vector3.left;

                //  Replace the altitude data with this brick's
                //  height data.
                position.y = height;

                //  Set this brick's position
                this.mainBrick.transform.position = position;
            }

            /// <summary>
            /// Sets the neighbor brick as the left neighbor for the
            /// main brick.
            /// Uses the position of the neighbor brick to calculate
            /// the main brick's position. The main brick position is
            /// one unit right of the neighbor brick. The main brick's
            /// height information is used instead of the neighbor's.
            /// </summary>
            public void PositionRightOf(PillarData neighbor)
            {
                if (this.mainBrick == null
                    || neighbor.mainBrick == null)
                {
                    throw new MissingReferenceException(
                        "Either Main brick or neighbor is null. " +
                        "Cannot position pillar.");
                }
                SetLeftNeighbor(neighbor);

                //  This brick's position is based on
                //  the neighbor's position.
                Vector3 position = neighbor.mainBrick.transform.position;

                //  Move this brick 1 unit to the left of
                //  the neighbor's position.
                position += Vector3.right;

                //  Replace the altitude data with this brick's
                //  height data.
                position.y = height;

                //  Set this brick's position
                this.mainBrick.transform.position = position;
            }

            #endregion
        }

        #endregion
        #region Unity API

        /// <summary>
        /// Get references to any components that will be used via
        /// direct-access.
        /// </summary>
        void Awake()
        {
            dataManager = GetComponent<IssBrickDataManager>();
        }

        /// <summary>
        /// Ensure all the direct-references have been acquired.
        /// Generate the brickHolder object if none has been provided.
        /// Initialize the pool lists for the pillar data.
        /// Initialize the pool size for the brick game objects.
        /// Generate the first level.
        /// </summary>
        void Start()
        {
            if (dataManager == null)
            {
                throw new MissingComponentException("IssBrickObjectManager requires an IssBrickDataManager on the same GameObject");
            }
            if (brickHolder == null)
            {
                brickHolder = new GameObject("_BrickHolder (autogenerated)");
            }

            pillars = new List<PillarData>();
            recycledPillars = new List<PillarData>();

            //  set the pool to 'unlimited' maximum size
            PPool.SetLimit(greenBrick, -1);

            StartCoroutine("GenerateFirstLevel");
        }

        #endregion
        #region Internal methods

        /// <summary>
        /// Creates the inital data and generates the level from that
        /// data.
        /// </summary>
        private IEnumerator GenerateFirstLevel()
        {
            IssPlayerSpawner playerSpawner = GetComponent<IssPlayerSpawner>();
            if (playerSpawner != null) playerSpawner.enabled = false;

            float delay = growDuration / screenWidthByBricks;
            yield return new WaitForSeconds(delay);

            //  create the inital level
            dataManager.AddFirstBrick();
            AddFirstPillar();

            yield return new WaitForSeconds(delay);

            //  expand the level to fill the screen
            for (int i = 0; i < screenWidthByBricks / 2; i++)
            {
                dataManager.ExpandQueue();
                AddPillarOnLeft();
                yield return new WaitForSeconds(delay);
                AddPillarOnRight();
                yield return new WaitForSeconds(delay);
            }

			if (playerSpawner != null) playerSpawner.enabled = true;
        }

        /// <summary>
        /// Adds the first brick of the level. Also initalizes
        /// the column data, so if there is any existing level data
        /// when this is called, it will be destroyed.
        /// </summary>
        private void AddFirstPillar()
        {
            GameObject brick = GetBrick();
            int height = dataManager.GetLeftHeight();
            PillarData pillar = GetPillar(height, brick);

            if (pillars == null) pillars = new List<PillarData>();
            pillars.Clear();
            pillars.Add(pillar);
            Vector3 position = Vector3.up * pillar.height;
            pillar.mainBrick.transform.position = position;
        }

        /// <summary>
        /// Adds a complete column of bricks on the left side
        /// of the map. This generates the top brick using the
        /// data provided by the IssBrickDataManager. The brick
        /// is positioned correctly, and the Pillar object is
        /// inserted into the pillar list. The underlying bricks
        /// are also generated and connected to the top brick.
        /// </summary>
        private void AddPillarOnLeft()
        {
            GameObject brick = GetBrick();
            int height = dataManager.GetLeftHeight();
            PillarData pillar = GetPillar(height, brick);
            PillarData neighbor = pillars[0];

            pillars.Insert(0, pillar);
            pillar.PositionLeftOf(neighbor);

            int neighborHeight = neighbor.height;
            if (height < neighborHeight)
            {
                //  height dropped (moving left)
                FillBricksDownFrom(neighbor, pillar);
            }
            else
            {
                //  height increased (moving left)
                FillBricksDownFrom(pillar, neighbor);
            }
        }

        /// <summary>
        /// Adds a complete column of bricks on the right side
        /// of the map. This generates the top brick using the
        /// data provided by the IssBrickDataManager. The brick
        /// is positioned correctly, and the Pillar object is
        /// inserted into the pillar list. The underlying bricks
        /// are also generated and connected to the top brick.
        /// </summary>
        private void AddPillarOnRight()
        {
            GameObject brick = GetBrick();
            int height = dataManager.GetRightHeight();
            PillarData pillar = GetPillar(height, brick);
            PillarData neighbor = pillars[pillars.Count - 1];

            pillars.Add(pillar);
            pillar.PositionRightOf(neighbor);
            FillBricksDownFrom(neighbor, pillar);

            int neighborHeight = neighbor.height;
            if (height < neighborHeight)
            {
                //  height dropped
                //  fill sub-bricks under the neighbor
                FillBricksDownFrom(neighbor, pillar);
            }
            else
            {
                //  height increased
                //  fill sub-bricks under the new pillar
                FillBricksDownFrom(pillar, neighbor);
            }
        }

        /// <summary>
        /// Populates the bricks under the "main" brick. The intent
        /// is to fill all the bricks down to the same level as the
        /// neighbor brick, so as to prevent gaps in the ground when
        /// there is a large elevation change.
        /// </summary>
        private void FillBricksDownFrom(PillarData main, PillarData neighbor)
        {
            float distance = main.height - neighbor.height;
            if (distance < main.depth) return;

            //  get the brick to be supported
            GameObject brick = main.mainBrick;
            Vector3 position = brick.transform.position;

            //  Fill in bricks under the brick, down
            //  to the depth of the neighbor.
            for (int i = main.depth; i < distance; i++)
            {
                GameObject b = GetBrick();
                Vector3 p = position + Vector3.down * (i + 1);
                b.transform.position = p;
                b.SetParent(brick);
                main.depthBricks.Add(b);
            }
        }

        /// <summary>
        /// Recycles the left pillar of bricks.
        /// </summary>
        private void RemovePillarOnLeft()
        {
            RemovePillarAt(0);
        }

        /// <summary>
        /// Recycles the right pillar of bricks.
        /// </summary>
        private void RemovePillarOnRight()
        {
            RemovePillarAt(pillars.Count - 1);
        }

        /// <summary>
        /// Removes a pillar of bricks. Recycles the main brick, along
        /// with all sub-bricks. Ensures all bricks have no parents before
        /// they are recycled. Ensures there are no references to neighbors
        /// before recycling.
        /// </summary>
        private void RemovePillarAt(int index)
        {
            PillarData removed = pillars[index];
            pillars.RemoveAt(index);
            RecyclePillar(removed);
        }

        /// <summary>
        /// Returns a brick to the object pool. Ensures the brick
        /// is unparented before recycling.
        /// </summary>
        private void RecycleBrick(GameObject brick)
        {
            brick.Unparent();
            PPool.Return(brick);
        }

        /// <summary>
        /// Gets a brick from the object pool. Rotate it randomly.
        /// </summary>
        private GameObject GetBrick()
        {
            //	get a brick from the object pool
            GameObject brick = PPool.Get(greenBrick, Vector3.zero, Quaternion.identity);

            //	randomize the rotation about the z-axis
            brick.transform.Rotate(Vector3.forward, Random.Range(0, 4) * 90);

            //  by default, all bricks are initially parented to the 
            //  brick container. Some bricks will override this, such
            //  as the sub-bricks that are used to fill in under the
            //  top bricks.
            brick.SetParent(brickHolder);

            return brick;
        }

        /// <summary>
        /// Returns a pillar to the object pool. Ensures there are 
        /// no references to game objects or other pillars before
        /// recycling.
        /// </summary>
        private void RecyclePillar(PillarData pillar)
        {
            //  Recycle depth bricks first.
            foreach (GameObject subBrick in pillar.depthBricks)
            {
                RecycleBrick(subBrick);
            }
            pillar.depthBricks.Clear();

            //  Recycle main brick last because it may have
            //  depthbricks parented to it.
            GameObject mainBrick = pillar.mainBrick;
            pillar.mainBrick = null;
            RecycleBrick(mainBrick);

            //  Ensure there are no links to neighbors
            pillar.left = null;
            pillar.right = null;

            //  Move the pillar to the recycled list
            recycledPillars.Add(pillar);
            pillars.Remove(pillar);
        }

        /// <summary>
        /// Gets a pillar object from the object pool. Creates a 
        /// new object if there are none available.
        /// </summary>
        private PillarData GetPillar(int height, GameObject brick)
        {
            PillarData pillar = null;
            if (recycledPillars.Count > 0)
            {
                pillar = recycledPillars.RemoveFirst();
                pillar.Reconstruct(height, brick);
            }
            else
            {
                pillar = new PillarData(height, brick);
            }
            return pillar;
        }

        /// <summary>
        /// When the player moves right, pillars must be added on
        /// the right side, and recycled from the left side.
        /// </summary>
        private void ProgressRight()
        {
            dataManager.ShiftQueueLeft();
            RemovePillarOnLeft();
            AddPillarOnRight();
        }

        /// <summary>
        /// When the player moves left, pillars must be added on
        /// the left side, and recycled from the right side.
        /// </summary>
        private void ProgressLeft()
        {
            dataManager.ShiftQueueRight();
            RemovePillarOnRight();
            AddPillarOnLeft();
        }

        #endregion
        #region AxisInputReceivable.IAxisInputReceivable

        [SerializeField]
        private float cumulativeInput;

        public void OnAxisInput(Vector2 inputVector)
        {
            cumulativeInput += inputVector.x;

            while (Mathf.Abs(cumulativeInput) > 1)
            {
                if (cumulativeInput.IsPositive())
                {
                    ProgressRight();
                    cumulativeInput--;
                    continue;
                }
                if (cumulativeInput.IsNegative())
                {
                    ProgressLeft();
                    cumulativeInput++;
                    continue;
                }
            }
        }

        public void OnInputEnabled(bool enableInput) { }

        #endregion
    }
}