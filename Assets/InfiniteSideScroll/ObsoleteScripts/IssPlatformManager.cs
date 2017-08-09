using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;
using PLib.Pooling;
using PLib.Math;

namespace InfiniteSideScroll
{
    namespace Obsolete
    {
        public class IssPlatformManager : MonoBehaviour//, AxisInputReceivable.IAxisInputReceivable
        {
            #region Inspector assigned

            [Tooltip("Number of full tiles that fit across the screen.")]
            public float visibleTiles = 5;

            [Tooltip("Maximum number of tiles up and down")]
            public float scale = 5;

            [Tooltip("Rockiness of terrain (smaller is smoother)")]
            [Range(0, 1)]
            public float smoothness = 1;
            public float jagginess = 0.5f;

            [Tooltip("Inspector Assigned")]
            public GameObject greenBrick;

            [Tooltip("Inspector Assigned")]
            public GameObject brickHolder;

            #endregion

            //	the index of tile the player is standing on
            private float playerPosition;
            private float positionProgress;

            //	the index of the left-most and right-most tiles on screen
            //	these are updated when the player moves
            private int rightIndex;

            private List<BrickData_Old> brickData;

            public void Awake()
            {
                DontDestroyOnLoad(gameObject);
                brickData = new List<BrickData_Old>();
            }

            public void Start()
            {
                playerPosition = (int)visibleTiles / 2 + 1;

                PPool.SetLimit(greenBrick, 300);
                for (rightIndex = 0; rightIndex < visibleTiles; rightIndex++)
                {
                    AddBrickOnRight();
                }
            }

            /// <summary>
            /// Adds a brick object on right of the current brick set.
            ///	Uses checks the data representation of the board.
            ///	Uses existing data if it is found, generates new data otherwise.
            /// </summary>
            public void AddBrickOnRight()
            {
                #region get or generate brick data

                BrickData_Old current;

                //	check for existing data (meaning this is not the first time the player has been here)
                if (rightIndex == brickData.Count)
                {
                    //	first time here
                    Vector2 noisePosition = Vector2.zero;

                    //	get the perlin noise data for the previous brick
                    if (rightIndex > 0)
                    {
                        noisePosition = brickData[rightIndex - 1].noisePosition;
                    }

                    //	calculate the perlin noise position for this brick
                    noisePosition += smoothness * Vector2.right;// + jagginess * Vector2.up;
                    //	save the data for this brick
                    int height = Mathf.RoundToInt(scale * Mathf.PerlinNoise(noisePosition.x, noisePosition.y));
                    current = new BrickData_Old(height);
                    current.noisePosition = noisePosition;
                    brickData.Add(current);
                }
                else
                {
                    //	return visit
                    current = brickData[rightIndex];
                }

                #endregion

                #region create and position brick object

                GameObject brick = GetBrick();
                current.brick = brick;

                //	get the previous brick so we can use it's horizontal position
                Vector3 position = Vector3.zero;
                GameObject leftBrick = null;
                if (rightIndex > 0)
                {
                    BrickData_Old leftData = brickData[rightIndex - 1];
                    leftBrick = leftData.brick;

                    //	set the new brick's horizontal position based on the left brick's position
                    position = leftBrick.transform.position + Vector3.right;
                }

                //	add the brick object to the game object hierarchy
                position.y = current.height;
                brick.transform.position = position;
                brick.transform.parent = brickHolder.transform;
                #endregion

                #region connect brick to neighbors

                //	intelligently connect neighbors
                if (rightIndex == 0) return;

                int leftY = (int)leftBrick.transform.position.y;
                int rightY = (int)current.brick.transform.position.y;
                if (PMath.Approx(leftY, rightY, 2)) return;
                if (leftY > rightY)
                {
                    //	went down
                    FillBricksDownFrom(brickData[rightIndex - 1], brickData[rightIndex]);
                }
                if (leftY < rightY)
                {
                    //	went up
                    FillBricksDownFrom(brickData[rightIndex], brickData[rightIndex - 1]);
                }

                #endregion
            }

            /// <summary>
            /// Fills the bricks down from provided brick, down a number of bricks equal to
            ///	distance.
            /// </summary>
            /// <param name="brick">Brick.</param>
            /// <param name="depth">Depth.</param>
            public void FillBricksDownFrom(BrickData_Old leftData, BrickData_Old rightData)
            {
                float distance = leftData.brick.transform.position.y - rightData.brick.transform.position.y;
                if (distance < leftData.subLayers) return;

                GameObject brick = leftData.brick;
                Vector3 position = brick.transform.position;

                for (int i = leftData.subLayers; i < distance; i++)
                {
                    GameObject b = GetBrick();
                    Vector3 p = position + Vector3.down * (i + 1);
                    b.transform.position = p;
                    b.transform.parent = brickHolder.transform;
                    leftData.subBricks.Add(b);
                }
                leftData.subLayers = (int)distance;
            }

            /// <summary>
            /// Gets a brick from the object pool. Rotates it randomly.
            /// </summary>
            /// <returns>The brick.</returns>
            private GameObject GetBrick()
            {
                //	get a brick from the object pool
                GameObject brick = PPool.Get(greenBrick);

                brick.transform.position = Vector3.zero;
                brick.transform.rotation = Quaternion.identity;

                //	randomize the rotation about the z-axis
                brick.transform.Rotate(Vector3.forward, Random.Range(0, 3) * 90);

                return brick;
            }

            #region AxisInputReceivable.IAxisInputReceivable

            public void OnAxisInput(Vector2 inputVector)
            {

                positionProgress += inputVector.x;

                while (positionProgress > 1)
                {
                    positionProgress--;
                    AddBrickOnRight();
                    rightIndex++;
                }

            }

            public void OnInputEnabled(bool enableInput) { }

            #endregion
        }

        /// <summary>
        /// represents a column of bricks in the world
        /// </summary>
        public class BrickData_Old
        {
            //	the y-position in world space of the top brick
            public int height;
            public int subLayers;
            public GameObject brick;
            public Vector2 noisePosition;

            //	list of brick IDs
            //	first one is this brick
            //	any other bricks are the ones directly under this one
            public List<GameObject> subBricks;

            public BrickData_Old(int height)
            {
                this.height = height;
                this.subLayers = 0;
                subBricks = new List<GameObject>();
            }
        }
    }
}