using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NoiseTest;
using CielaSpike;


public class Generate : MonoBehaviour {

	private List<Chunk> awaitingInstantiation;
    //Along x axis
    private float xDistanceBlocks = 0.866f * 2f;
    //Along z axis, distance between blocks
    private float zDistanceBlocks = 1.5f;
    #region Generation Information
    public GameObject source;

    //public GameObject hexObj;
    public GameObject block_stone;
    public GameObject block_dirt;
    public GameObject block_grass;

    public AnimationCurve curve;
    public AnimationCurve curve2;

    public Vector2 chunkPosition;
    public int size;
    public int maxNumBlocks;
    public float blockSize = 0.25f;
    public long gameSeed;

    public int[] octaveDistances;
    public float[] octaveWeights;
    public long[] octaveSeeds;

    private TrilinearInterpolation[] interpolators;

    private enum BLOCKID : short {Air=0, Stone=1, Dirt=2, Grass=3 };

    public float pScale;
    public float pScale2;

    [SerializeField]
    List<GameObject> temp;
    [SerializeField]
    private List<Chunk> chunks;

    
	public bool optimization =false;
    public /*short[,,]*/Chunk genChunk(Vector2 cPos, int size, int maxNumBlocks)
    {
		Debug.Log ("GenChunk");
        short[,,] blockValues = new short[size, size, maxNumBlocks];

        int chunkX = Mathf.FloorToInt(cPos.x);
        int chunkZ = Mathf.FloorToInt(cPos.y);


        float deltaX = (float)chunkX * (size * xDistanceBlocks);
        float deltaZ = (float)chunkZ * (size * zDistanceBlocks);

        float sum = 0;
        foreach (float f in octaveWeights)
        {
            sum += f;
        }

//        interpolators = new TrilinearInterpolation[octaveDistances.Length];
//
//        for (int d = 0; d < interpolators.Length; d++)
//        {
//            interpolators[d] = new TrilinearInterpolation(gameSeed * octaveSeeds[d], octaveDistances[d]);
//        }

        //First pass for main stone generation
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {

                float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f + deltaX;
                float blockZ = j * 1.5f + deltaZ;

				short prev = (short)BLOCKID.Air;
				int dirtCount = 0;

				for (int k = maxNumBlocks-1; k >=0; k--) {

                    float chanceOfBlock = 0.0f;

					float perlinValue = Mathf.PerlinNoise(blockX * pScale, blockZ * pScale);
					float perlinValue2 = Mathf.PerlinNoise((blockX + 456456) * pScale2, (blockZ + 12123) * pScale2); //random numbers to offset the noise so not same as perlinValue
					float heightFactor = ((float)k / maxNumBlocks) - (perlinValue2 * 0.25f); //Not sure what this 0.25f is, may or may not be min block height, should be looked into
					float combinedCurveValue = (perlinValue * curve.Evaluate(heightFactor)) + ((1.0f - perlinValue) * curve2.Evaluate(heightFactor)); //Mixing two curves based on noise

					if (combinedCurveValue <= 0) {
						blockValues[i, j, k] = (short)BLOCKID.Air;
						continue;
					}

                    for (int oct = 0; oct < interpolators.Length; oct++)
                    {
                        chanceOfBlock += interpolators[oct].trilinearInterpolation(blockX, (float)k * blockSize, blockZ) * octaveWeights[oct];
                    }

                    chanceOfBlock = chanceOfBlock / (sum);

                    

                    if (chanceOfBlock < combinedCurveValue)
                    {

						if (prev == (short)BLOCKID.Air){
							blockValues[i, j, k] = (short)BLOCKID.Grass;
						}else if(prev == (short)BLOCKID.Grass || (prev == (short)BLOCKID.Dirt && dirtCount < 5))
						{
							blockValues[i, j, k] = (short)BLOCKID.Dirt;
							dirtCount++;
						}
						else {
							blockValues[i, j, k] = (short)BLOCKID.Stone;
							dirtCount = 0;
						}
                    }
                    else {
                        blockValues[i, j, k] = (short)BLOCKID.Air;

                    }
                    
                    prev = blockValues[i, j, k];

                }
            }
        }
		Chunk result = new Chunk ();
		result.blockTypes = blockValues;
		result.size = size;
		result.pos = cPos;

		return result;//blockValues;
    }
	public IEnumerator TraverseList(){
		if (awaitingInstantiation == null) {
			yield return null;
		}
		else {
			Chunk[] listToParse = awaitingInstantiation.ToArray ();
			//awaitingInstantiation.Clear ();
			foreach (Chunk c in listToParse) {
				loadedChunks.Add (c.pos,instantiateChunk (c.pos, c.size, maxNumBlocks, c.blockTypes));
				awaitingInstantiation.Remove (c);
			}
		}
		awaitingInstantiation.Clear ();
		yield return null;
	}
    public Chunk instantiateChunk(Vector2 cPos, int size, int maxNumBlocks, short[,,] blockValues) {
        GameObject holder = new GameObject("Holder of chunk of size " + size);
        if (chunks == null)
        {
            chunks = new List<Chunk>();
        }


        int chunkX = Mathf.FloorToInt(cPos.x);
        int chunkZ = Mathf.FloorToInt(cPos.y);


        float deltaX = (float)chunkX * (size * xDistanceBlocks);
        float deltaZ = (float)chunkZ * (size * zDistanceBlocks);

        //Initialize our chunk
        Chunk result = new Chunk();
        result.hexObjs = new List<GameObject>();
        result.pos = new Vector2(chunkX, chunkZ);
        result.size = size;
        //result.blockTypes = blockValues;


        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {

                float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f + deltaX;

                float blockZ = j * 1.5f + deltaZ;

                short previousID = (short)BLOCKID.Air;
                int blockStart = 0;

                for (int k = 0; k < maxNumBlocks; k++) {
                    short id = blockValues[i, j, k];

                    if (id != previousID || k == maxNumBlocks)
                    {
                        //instantiate block if not air
                        if (previousID != (short)BLOCKID.Air) {
                            GameObject hObj = null;
							if (previousID == (short)BLOCKID.Stone) {
								hObj = (GameObject)Instantiate (block_stone, new Vector3 (0, 0, 0), block_stone.transform.rotation);
							} else if (previousID == (short)BLOCKID.Dirt) {
								hObj = (GameObject)Instantiate (block_dirt, new Vector3 (0, 0, 0), block_dirt.transform.rotation);
							} else if (previousID == (short)BLOCKID.Grass) {
								hObj = (GameObject)Instantiate (block_grass, new Vector3 (0, 0, 0), block_grass.transform.rotation);
							}

                            result.hexObjs.Add(hObj);
                            hObj.transform.SetParent(holder.transform);

                            hObj.transform.localScale = new Vector3(1, 1, (k-blockStart) * blockSize);
                            hObj.transform.localPosition = new Vector3(blockX, blockStart * blockSize, blockZ);
                            scaleUV(hObj);
                        }

                        blockStart = k;
                        previousID = id;
                    }
                    else {
                        //nothing to do
                    }
                }
            }
        }


        chunks.Add(result);
        temp.Add(holder);
        return result;
    }

    //Generate and immediately instantiate a chunk
	public IEnumerator getChunkAtPos(Vector2 cPos, int size, int maxNumBlocks) {
		Chunk blockValues = genChunk(cPos, size, maxNumBlocks);
		awaitingInstantiation.Add (blockValues);
		yield return null;//
		//return blockValues;//instantiateChunk(cPos, size, maxNumBlocks, blockValues.blockTypes);
    }

    //Scale the UV coordinates of a block's mesh
    public void scaleUV(GameObject obj)
    {

        obj.GetComponent<MeshFilter>().sharedMesh = Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
        //obj.GetComponent<MeshFilter>().sharedMesh = (Mesh)Instantiate(obj.GetComponent<MeshFilter>().sharedMesh);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        //Debug.Log("--Next Mesh--");

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i][0] = mesh.uv[i][0];

            if (mesh.uv[i][1] == 0.5)
            {
                uvs[i][1] = 0.5f * obj.transform.localScale[2];
            }
            else {
                uvs[i][1] = mesh.uv[i][1];
            }
        }

        mesh.uv = uvs;
        
    }

    public void Clear()
    {
        if (temp != null)
        {
            foreach (GameObject c in temp)
            {
                DestroyImmediate(c);
            }
        }
        temp = new List<GameObject>();

        if (chunks != null) {
            foreach (Chunk c in chunks)
            {
                if (c.hexObjs == null)
                {
                    continue;
                }
                foreach (GameObject hex in c.hexObjs) {
                    DestroyImmediate(hex);
                }
            }
        }
        chunks = new List<Chunk>();

		awaitingInstantiation.Clear ();
    }

    [System.Serializable]
    public struct Chunk {
        public int size;
        public Vector3 pos;
        public List<GameObject> hexObjs;
		public short[,,] blockTypes;
    }


    private class TrilinearInterpolation {
        [SerializeField]
		private ConcurrentDictionary<Vector3, float> interpolators;
        [SerializeField]
        private OpenSimplexNoise noise;
        [SerializeField]
        public int interpolationDistance;


        public TrilinearInterpolation(int distanceInterpolation) : this(System.DateTime.Now.Ticks, distanceInterpolation) {

        }
        public TrilinearInterpolation(long seed, int distanceInterpolation) {
            noise = new OpenSimplexNoise(seed);
            this.interpolationDistance = distanceInterpolation;
			interpolators = new ConcurrentDictionary<Vector3, float>();
        }


        private float getSimplexNoise(int x, int y, int z) {
            if (interpolators == null) {
				interpolators = new ConcurrentDictionary<Vector3, float>();
            }
            if (noise == null) {
                noise = new OpenSimplexNoise();
            }
//            float result;
//            if (!interpolators.TryGetValue(new Vector3(x, y, z), out result)) {
//                result = (float)noise.Evaluate(x, y, z);
//				if(!interpolators.ContainsKey(new Vector3(x,y,z))){
//					interpolators.Add(new Vector3(x, y, z), result);
//				}
//            }

			return (float)noise.Evaluate(x, y, z);
        }

        //http://paulbourke.net/miscellaneous/interpolation/
        public float trilinearInterpolation(float x, float y, float z) {

            if (x % interpolationDistance == 0 && y % interpolationDistance == 0 && z % interpolationDistance == 0) {
                return getSimplexNoise((int)x, (int)y, (int)z);
            }

            int minX = Mathf.FloorToInt((Mathf.Floor(x) / (float)interpolationDistance)) * interpolationDistance;
            int minY = Mathf.FloorToInt((Mathf.Floor(y) / (float)interpolationDistance)) * interpolationDistance;
            int minZ = Mathf.FloorToInt((Mathf.Floor(z) / (float)interpolationDistance)) * interpolationDistance;

            int maxX = minX + interpolationDistance;
            int maxY = minY + interpolationDistance;
            int maxZ = minZ + interpolationDistance;

            float V000 = getSimplexNoise(minX, minY, minZ);
            float V100 = getSimplexNoise(maxX, minY, minZ);
            float V010 = getSimplexNoise(minX, maxY, minZ);
            float V001 = getSimplexNoise(minX, minY, maxZ);
            float V101 = getSimplexNoise(maxX, minY, maxZ);
            float V011 = getSimplexNoise(minX, maxY, maxZ);
            float V110 = getSimplexNoise(maxX, maxY, minZ);
            float V111 = getSimplexNoise(maxX, maxY, maxZ);

            float localX = (x - minX) / interpolationDistance;
            float localY = (y - minY) / interpolationDistance;
            float localZ = (z - minZ) / interpolationDistance;


            return V000 * (1 - localX) * (1 - localY) * (1 - localZ) +
                V100 * localX * (1 - localY) * (1 - localZ) +
                V010 * (1 - localX) * localY * (1 - localZ) +
                V001 * (1 - localX) * (1 - localY) * localZ +
                V101 * localX * (1 - localY) * localZ +
                V011 * (1 - localX) * localY * localZ +
                V110 * localX * localY * (1 - localZ) +
                V111 * localX * localY * localZ;
        }

    }
    #endregion

    #region Runtime chunk management
    [Tooltip("Number of blocks away from center loaded. Total number of chunks loaded is equal to (2*radius +1)^2")]
    public int radius;

    Dictionary<Vector2, Chunk> loadedChunks;
    private Vector2 currentChunk;

    //Called whenever object is enabled on runtime
    public void Start()
    {
        loadedChunks = new Dictionary<Vector2, Chunk>();
		awaitingInstantiation = new List<Chunk> ();

		interpolators = new TrilinearInterpolation[octaveDistances.Length];

		for (int d = 0; d < interpolators.Length; d++)
		{
			interpolators[d] = new TrilinearInterpolation(gameSeed * octaveSeeds[d], octaveDistances[d]);
		}

		StartCoroutine(getChunkAtPos(findCurrentChunk(source.transform.position), size, maxNumBlocks));
		StartCoroutine(TraverseList ());
		verifySurroundings();
		StartCoroutine(TraverseList ());

		currentChunk.x = findCurrentChunk (source.transform.position).x;
		currentChunk.y = findCurrentChunk (source.transform.position).y;
		Debug.Log (awaitingInstantiation.Count);
       // loadedChunks.Add(currentChunk, firstChunk);
        
        

    }

    public void LateUpdate()
    {

        Vector2 cc = findCurrentChunk(source.transform.position);
        int x = (int)cc.x;
        int z = (int)cc.y;

        if (x != currentChunk.x || z != currentChunk.y)
        {
            currentChunk.x = x;
            currentChunk.y = z;

			verifySurroundings();
        }
		StartCoroutine(TraverseList ());
    }



    public Vector2 findCurrentChunk(Vector3 sourcePos) {
        int x = Mathf.FloorToInt(source.transform.position.x / ((float)size * xDistanceBlocks));
        int z = Mathf.FloorToInt(source.transform.position.z / ((float)size * zDistanceBlocks));

        return new Vector2(x, z);
    }

	private void verifySurroundings() {
        int startX = (int)this.currentChunk.x - (radius);
        int startZ = (int)this.currentChunk.y - (radius);

        int squareSize = 2 * radius + 1;

        for (int i = 0; i < squareSize; i++)
        {
            for (int k = 0; k < squareSize; k++)
            {
                if (!loadedChunks.ContainsKey(new Vector2(startX + i, startZ + k))) {
					this.StartCoroutineAsync (getChunkAtPos (new Vector2 (startX + i, startZ + k), size, maxNumBlocks));
                    //loadedChunks.Add(new Vector2(startX + i, startZ + k), newC);

                }
            }
        }
		//yield return null;
    }
    #endregion
}
