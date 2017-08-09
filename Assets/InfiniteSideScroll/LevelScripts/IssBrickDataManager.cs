using UnityEngine;
using System.Collections.Generic;
using System.Text;
using PLib.Rand;
using PLib.General;
using PLib.Math;

namespace InfiniteSideScroll
{
    /// <summary>
    /// 2016-5-10
    ///	Iss brick data manager. Responsible for managing the data 
    ///	representation of the level.
    /// </summary>
    public class IssBrickDataManager : MonoBehaviour
    {
        #region Inspector assigned
        [Header("Temporary for Development")]

        //	this is the horizontal "spread" of the noise
        [Tooltip("0 indicates perfectly flat. Higher values indicate larger steps across the Perlin noise.")]
        [Range(0, 10)]
        public float smoothness = 1;

        //	this is the change-per-brick of the horizontal "spread" of the noise
        [Tooltip("0 indicates no change per-brick. Higher values indicate larger increases in smoothness, per-brick")]
        [Range(0, 10)]
        public float smoothDelta = 1;

        //	this is the vertical amplification of the noise value
        [Tooltip("This is the multiplier of the height value. Larger values indicate steeper hills. ")]
        [Range(0, 1)]
        public float jagginess = 0.5f;

        //	this is the change-per-brick of the vertical amplification of the noise value
        [Tooltip("0 indicates no change per-brick. Higher values indicate larger increases in jagginess, per-brick")]
        [Range(-20, 20)]
        public float jaggiDelta = 1;

        //	this is the number of vertical bricks to apply the noise over
        [Tooltip("This is the maximum hill size (in bricks).")]
        [Range(3, 8)]
        public int maxVerticalBricks = 5;

        #endregion
        #region Data

        /// <summary>
        ///	One-dimensional representation of the player's progress
        ///	through the level. Serialized for debugging.
        ///	This represents the progress across the Perlin noise.
        /// </summary>
        [SerializeField]
        private int playerPosition;

        private List<BrickData> brickData;
        private List<BrickData> recycledBrickData;

        /// <summary>
        /// Data for the top brick of each 'column' of bricks along the level.
        ///	This also contains the delta values for how smoothness and scale
        ///	are changing at each point in the level.
        ///	smoothness and jagginess delta values indicate how the values are changing when heading
        ///	to the right, e.g., 0.27 means, an increase of 0.27 per block to the right.
        /// </summary>
        public class BrickData
        {
            public Vector2 noisePosition;
            public int brickHeight;
            public float smoothness, smoothDelta;
            public float jagginess, jaggiDelta;
            public int maxVerticalBricks;

            //  constructor
            public BrickData(Vector2 noisePosition, int brickHeight, float smoothness,
                float smoothDelta, float jagginess, float jaggiDelta, int maxVerticalBricks)
            {
                Reconstruct(noisePosition, brickHeight, smoothness,
                smoothDelta, jagginess, jaggiDelta, maxVerticalBricks);
            }

            //  methods
            public void Reconstruct(Vector2 noisePosition, int brickHeight, float smoothness,
                float smoothDelta, float jagginess, float jaggiDelta, int maxVerticalBricks)
            {
                this.noisePosition = noisePosition;
                this.brickHeight = brickHeight;
                this.smoothness = smoothness;
                this.smoothDelta = smoothDelta;
                this.jagginess = jagginess;
                this.jaggiDelta = jaggiDelta;
                this.maxVerticalBricks = maxVerticalBricks;
            }

            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                b.Append("\n[BrickData: ");
                b.Append("noise (" + noisePosition.ToString() + "), ");
                b.Append("height (" + brickHeight + "), ");
                b.Append("smooth (" + smoothness + "/" + smoothDelta + "), ");
                b.Append("jagg (" + jagginess + "/" + jaggiDelta + "), ");
                b.Append("max stack (" + maxVerticalBricks + ")");
                b.Append("]");
                return b.ToString();
            }
        }

        #endregion
        #region External API

        /// <summary>
        /// Returns the height value of the left-most data.
        /// </summary>
        public int GetLeftHeight()
        {
            return brickData[0].brickHeight;
        }

        /// <summary>
        /// Returns the height value of the right-most data.
        /// </summary>
        public int GetRightHeight()
        {
            return brickData[brickData.Count - 1].brickHeight;
        }

        /// <summary>
        /// Generates (or regenerates) the first data element.
        /// Clears all existing data (if any).
        /// Private access (public to gran access to custom editor)
        /// </summary>
        public void AddFirstBrick()
        {
            //	Generate the noise data from the first brick
            Vector2 noisePosition = 200 * (Vector2.one + PRand.RandomVector2());

            //	get the smooth and jaggi changes
            float sDelta = smoothDelta * 0.1f;
            float jDelta = jaggiDelta * 0.1f;

            //	calculate the vertical "brick space" of this brick
            int brickHeight = Mathf.RoundToInt(jagginess * Mathf.PerlinNoise(noisePosition.x, noisePosition.y));

            //	Create the queue and enqueue the brick data
            if (brickData == null)
            {
                brickData = new List<BrickData>();
            }
            if (recycledBrickData == null)
            {
                recycledBrickData = new List<BrickData>();
            }
            brickData.Clear();
            recycledBrickData.Clear();
            brickData.Add(GetBrickDataObject(noisePosition, brickHeight, smoothness, sDelta, jagginess, jDelta, maxVerticalBricks));
        }

        #endregion
        #region Internal methods

        /// <summary>
        /// Provides a BrickData object from the recycledBrickData list.
        /// This does NOT automatically add the new data object into the
        /// live brickData list. This is because objects are added to the
        /// brickData list as specific points, depending on external
        /// factors.
        /// </summary>
        private BrickData GetBrickDataObject(Vector2 noisePosition, int brickHeight,
            float smoothness, float smoothDelta, float jagginess,
            float jaggiDelta, int maxVerticalBricks)
        {
            if (recycledBrickData == null) recycledBrickData = new List<BrickData>();

            BrickData data;
            if (recycledBrickData.Count > 0)
            {
                data = recycledBrickData.RemoveFirst();
                data.Reconstruct(noisePosition, brickHeight, smoothness,
                smoothDelta, jagginess, jaggiDelta, maxVerticalBricks);
            }
            else
            {
                data = new BrickData(noisePosition, brickHeight, smoothness,
                    smoothDelta, jagginess, jaggiDelta, maxVerticalBricks);
            }
            return data;
        }

        /// <summary>
        /// This returns the data object to the recycledBrickData list. This
        /// does NOT attempt to remove the data object from the live brickData
        /// list. This is because it is handled locally and under different
        /// conditions, depending on the method.
        /// </summary>
        private void RecycleBrickData(BrickData data)
        {
            data.Reconstruct(Vector2.zero, 0, 0, 0, 0, 0, 0);
            recycledBrickData.Add(data);
        }

        /// <summary>
        /// Generates a new data element based on another data element.
        /// The "other" data element is located via the provided index.
        /// This method generates the data element that is "left" of 
        /// the provided element, so this would be used as the player is
        /// retreating through the level.
        /// </summary>
        private void AddBrickLeftOf(int index)
        {
            BrickData neighbor = brickData[index];

            //	get the noise data from the adjacent brick
            Vector2 noisePosition = neighbor.noisePosition;

            //	get the smooth and jaggi changes
            float sDelta = neighbor.smoothDelta;
            float smooth = neighbor.smoothness - sDelta;

            float jDelta = neighbor.jaggiDelta;
            float jaggi = neighbor.jagginess - jDelta;

            int maxStack = neighbor.maxVerticalBricks;

            //	calculate the noise position for this brick
            noisePosition += smooth * Vector2.left;

            //	calculate the vertical "brick space" of this brick
            int brickHeight = Mathf.RoundToInt(Mathf.Max(0, jaggi)
                * Mathf.PerlinNoise(Mathf.Max(0, noisePosition.x), Mathf.Max(0, noisePosition.y)));

            //	enqueue the data left of the index
            //	(this means insert at the given index, and the list will bump everything up)
            brickData.Insert(index, GetBrickDataObject(noisePosition, brickHeight, smooth, sDelta, jaggi, jDelta, maxStack));
        }

        /// <summary>
        /// Generates a new data element based on another data element.
        /// The "other" data element is located via the provided index.
        /// This method generates the data element that is "right" of 
        /// the provided element, so this would be used as the player is
        /// advancing through the level.
        /// </summary>
        private void AddBrickRightOf(int index)
        {
            BrickData neighbor = brickData[index];

            //	get the noise data from the adjacent brick
            Vector2 noisePosition = neighbor.noisePosition;

            //	compute the smooth and jaggi changes
            float sDelta = neighbor.smoothDelta;
            float smooth = neighbor.smoothness + sDelta;

            float jDelta = neighbor.jaggiDelta;
            float jaggi = neighbor.jagginess + jDelta;

            int maxStack = neighbor.maxVerticalBricks;

            //	calculate the noise position for this brick
            noisePosition += smooth * Vector2.right;

            //	calculate the vertical "brick space" of this brick
            int brickHeight = Mathf.RoundToInt(Mathf.Max(0, jaggi)
                * Mathf.PerlinNoise(Mathf.Max(0, noisePosition.x), Mathf.Max(0, noisePosition.y)));

            //	enqueue the data right of the index
            brickData.Insert(index + 1, GetBrickDataObject(noisePosition, brickHeight, smooth, sDelta, jaggi, jDelta, maxStack));
        }

        /// <summary>
        /// Performs the actual work of removing data from the list.
        /// This should only ever be the first or last element in the queue.
        /// </summary>
        private void ReclaimBrickAt(int index)
        {
            BrickData reclaimed = brickData[index];
            brickData.RemoveAt(index);
            RecycleBrickData(reclaimed);
        }

        /// <summary>
        /// Used to create the expand the data set from the inital brick.
        /// Also used when the screen (or visible area) gets bigger.
        /// I don't know when this would happen, but who knows?
        /// This adds a new data element at the beginning and end of the 
        /// list.
        /// Private acces (public to grant access to custom editor)
        /// </summary>
        public void ExpandQueue()
        {
            if (brickData == null) AddFirstBrick();
            AddBrickLeftOf(0);
            AddBrickRightOf(brickData.Count - 1);
        }

        /// <summary>
        /// When the screen gets smaller -- I don't know when this would
        /// happen, but who knows?
        /// This removes the first and last data element.
        /// Private acces (public to grant access to custom editor)
        /// </summary>
        public void CollapseQueue()
        {
            ReclaimBrickAt(0);
            ReclaimBrickAt(brickData.Count - 1);
        }

        /// <summary>
        ///	When player moves right, data goes left. This means
        ///	the left-most data element is removed and a new data
        ///	element is created at the rigth.
        /// Private acces (public to grant access to custom editor)
        /// </summary>
        public void ShiftQueueLeft()
        {
            AddBrickRightOf(brickData.Count - 1);
            ReclaimBrickAt(0);
        }

        /// <summary>
        ///	When player moves left, data goes right. This means
        ///	the right-most data element is removed and a new data
        ///	element is created at the left.
        /// Private acces (public to grant access to custom editor)
        /// </summary>
        public void ShiftQueueRight()
        {
            AddBrickLeftOf(0);
            ReclaimBrickAt(brickData.Count - 1);
        }

        #endregion
        #region Message receivers

        /// <summary>
        /// WARNING: Calling this will destroy all existing data.
        /// This is typically only received when a level starts.
        /// This sets the number of data elements to create.
        /// Each data element contains the parameters required to
        /// deterministically generate the position information for
        /// the top brick of each vertical slice of the board.
        /// Each data element also contains the velocity information
        /// for the parameters, so neighbor data can be generated as
        /// well.
        /// </summary>
        public void SetDataQueueSize(int numberOfElements)
        {
            int delta = numberOfElements - brickData.Count;

            for (int i = 0; i < Mathf.Abs(delta); i += 2)
            {
                if (delta.IsPositive())
                {
                    ExpandQueue();
                }
                if (delta.IsNegative() && brickData.Count > 2)
                {
                    CollapseQueue();
                }
            }
        }

        /// <summary>
        /// 2016-5-17 -- Obsolete -- IssBrickObjectManager calls
        /// ShiftQueueLeft() and ShiftQueueRight() directly.
        /// 
        ///	This is received when the player moves a full unit (m)
        ///	in either direction. This causes the data pool to "shift".
        ///	This means that the last element is removed and a new
        ///	first element is added (or vice versa, depending on the
        ///	direction the player is travelling).
        /// </summary>
        public void UpdatePlayerPosition(int playerPosition)
        {
            int delta = playerPosition - this.playerPosition;
            this.playerPosition = playerPosition;

            for (int i = 0; i < Mathf.Abs(delta); i++)
            {
                if (delta.IsPositive())
                {
                    ShiftQueueLeft();
                }

                if (delta.IsNegative())
                {
                    ShiftQueueRight();
                }
            }
        }

        #endregion
        #region Debugging & development

        /// <summary>
        /// Debug / development only
        /// Prints the contents of the list/queue to the console.
        /// Public access for the custom editor
        /// </summary>
        public void DumpToLog()
        {
            string output = System.DateTime.Now + ":";
            if (brickData == null) output += "No brickData created.";
            else output += brickData.ContentsToString(",");

            Debug.Log(output);
        }

        #endregion
    }
}