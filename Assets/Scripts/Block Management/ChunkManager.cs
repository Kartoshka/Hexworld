using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;
using NoiseTest;

public class ChunkManager : MonoBehaviour {

    //Along x axis
    private float xDistanceBlocks = 0.866f * 2f;
    //Along z axis, distance between blocks
    private float zDistanceBlocks = 1.5f;

    public GameObject trackedObject;

    //public GameObject hexObj;
    public GameObject block_stone;
    public GameObject block_dirt;
    public GameObject block_grass;

	public Material[] blockMaterials;

    public AnimationCurve curve;
    public AnimationCurve curve2;

    //For future generation implementation
    public AnimationCurve densityCurve_plains;
    public AnimationCurve densityCurve_mountains;
    public AnimationCurve densityCurve_caves;
    public AnimationCurve densityMixFactor;

    public int size = 16;
    public int maxNumBlocks = 512;
    public float blockSize = 0.25f;
    public long gameSeed;

	public float[] octaveZoom =  { 1f, 5f, 10f};
    public float[] octaveWeights = { 1, 2, 3};
    public long[] octaveSeeds = { 57131, 16447, 486132};

	public OpenSimplexNoise[] noise;

    public float pScale = 0.02f;
    public float pScale2 = 0.008f;

    public float pScale_octaves = 0.01f;
    public float pScale_height = 0.008f;
    public float pScale_mix = 0.0075f;
    public float pOff_height = 6969f;
    public float pOff_mix = 1337f;
    

    private float pOff = 100000f;

    [SerializeField]
    private List<Chunk> chunks;

    Dictionary<Vector2, Chunk> loadedChunks;
	Dictionary<Vector2, bool> generatingChunks;

    private Vector2 currentChunkPos;

    public AbChunkModifier[] chunkModifiers;

    // Use this for initialization
    void Start () {
		noise = new OpenSimplexNoise[octaveZoom.Length];
		for(int n=0;n<noise.Length;n++){
			noise[n] = new OpenSimplexNoise (octaveSeeds[n]*gameSeed);
		}

        loadedChunks = new Dictionary<Vector2, Chunk>();
		generatingChunks = new Dictionary<Vector2, bool> ();
        
		if (chunkModifiers == null)
        {
            chunkModifiers = new AbChunkModifier[0];
        }

        currentChunkPos = findCurrentChunk();


        foreach (AbChunkModifier cMod in chunkModifiers)
        {
            cMod.OnChunkManagerStart(this);
        }

       
    }
	
	// Update is called once per frame
	void LateUpdate () {
        Vector2 pos = findCurrentChunk();
        bool moved = !(currentChunkPos.Equals(pos));

        if(moved)
        {
									//This line is to commemorate Eeloo
            currentChunkPos = pos;
        }

        foreach (AbChunkModifier cMod in chunkModifiers)
        {
            if (moved)
            {
                cMod.OnMoveChunks(this);
            }
            cMod.OnChunkManagerUpdate(this);
        }
			
    }


	void OnApplicationQuit(){
		foreach (Chunk c in loadedChunks.Values) {
			foreach(GameObject gObj in c.hexObjs){
				Destroy (gObj);
			}
			foreach (Transform child in c.mainHolder.transform) {
				Destroy (child.gameObject);
			}
			Destroy (c.mainHolder);
		}
	}

	#region ChunkGeneration and Instantiation
    public Chunk getNewChunkData(Vector2 cPos, int size, int maxNumBlocks)
    {
		generatingChunks.Add (cPos, true);

        short[,,] blockValues = new short[size, size, maxNumBlocks];

        int chunkX = Mathf.FloorToInt(cPos.x);
        int chunkZ = Mathf.FloorToInt(cPos.y);


        float deltaX = (float)chunkX * (size * xDistanceBlocks);
        float deltaZ = (float)chunkZ * (size * zDistanceBlocks);

		List<Block> blocks = new List<Block> ();

		AnimationCurve densityMixFactor_copy = new AnimationCurve(densityMixFactor.keys);
		AnimationCurve densityCurve_mountains_copy = new AnimationCurve(densityCurve_mountains.keys);
		AnimationCurve densityCurve_plains_copy = new AnimationCurve(densityCurve_plains.keys);
		AnimationCurve densityCurve_caves_copy = new AnimationCurve(densityCurve_caves.keys);

        //First pass for main stone generation
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {

                float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f + deltaX;
                float blockZ = j * 1.5f + deltaZ;

                short prev = (short)BLOCKID.Air;
                int dirtCount = 0;

				int blockStart = 0;

                for (int k = maxNumBlocks - 1; k >= 0; k--)
                {
                    
                    float heightFactor = (float)k / (float)maxNumBlocks;

                    float mixValue = densityMixFactor_copy.Evaluate(Mathf.PerlinNoise(blockX*pScale_mix + pOff_mix + pOff, blockZ*pScale_mix + pOff_mix + pOff));
                    float heightOffset = 0.3f*Mathf.PerlinNoise(blockX * pScale_height + pOff_height + pOff, blockZ * pScale_height + pOff_height + pOff);

                    float mountainValue = densityCurve_mountains_copy.Evaluate(heightFactor - heightOffset);
                    float plainsValue = densityCurve_plains_copy.Evaluate(heightFactor - heightOffset);
                    float cavesValue = densityCurve_caves_copy.Evaluate(heightFactor);

                    float biomeCombineValue = ((1-mixValue) * mountainValue) + ((mixValue) * plainsValue);
				

					double chanceOfBlock = 0.0f;
					double weightSum = 0;

					for (int n = 0; n < noise.Length; n++)
                    {
						double newWeight = octaveWeights[n]*0.5f + octaveWeights[n]*Mathf.PerlinNoise(blockX*pScale_octaves + (float)octaveSeeds[n] + pOff, blockZ*pScale_octaves + (float)octaveSeeds[n] + pOff);
						chanceOfBlock += noise[n].Evaluate(blockX*(1f/octaveZoom[n]), (float)k * blockSize*(1f/octaveZoom[n]), blockZ*(1f/octaveZoom[n])) * newWeight;
                        weightSum += newWeight;
                    }

                    chanceOfBlock = chanceOfBlock / weightSum;

                    int grassCutoff = maxNumBlocks / 2 - 30;
                    

                    if (chanceOfBlock < biomeCombineValue && chanceOfBlock < cavesValue)
                    {

                        if (prev == (short)BLOCKID.Air && k > grassCutoff)
                        {
                            blockValues[i, j, k] = (short)BLOCKID.Grass;
                        }
                        else if (prev == (short)BLOCKID.Grass || (prev == (short)BLOCKID.Dirt && dirtCount < 5))
                        {
                            blockValues[i, j, k] = (short)BLOCKID.Dirt;
                            dirtCount++;
                        }
                        else
                        {
                            blockValues[i, j, k] = (short)BLOCKID.Stone;
                            dirtCount = 0;
                        }
                    }
                    else
                    {
                        blockValues[i, j, k] = (short)BLOCKID.Air;
                    }


					if (blockValues[i, j, k] != prev || k == 0)
					{
						if (prev != (short)BLOCKID.Air)
						{
							blocks.Add(new Block(new Vector3(blockX, k * blockSize, blockZ), (blockStart-k) * blockSize, prev));
						}

						blockStart = k;
					}


                    prev = blockValues[i, j, k];
                }
            }
        }

		Chunk result = new Chunk();
		result.blocks = blocks;
        result.blockTypes = blockValues;
        result.size = size;
        result.pos = cPos;

        //loadedChunks.Add(cPos, result);

        return result;
    }

    public Chunk getNewChunkData(Vector2 cPos)
    {
        return this.getNewChunkData(cPos, this.size, this.maxNumBlocks);
    }

    //public Chunk instantiateChunk(Vector2 cPos, int size, int maxNumBlocks, short[,,] blockValues)
	public Chunk instantiateChunk(Chunk c)
    {
		
		GameObject holder = new GameObject("Holder of chunk ( " + c.pos.x + " , " + c.pos.z + " ) " + "of size " + size);
		c.mainHolder = holder;

		GameObject stoneHolder = new GameObject ("StoneHolder");
		GameObject dirtHolder = new GameObject ("DirtHolder");
		GameObject grassHolder = new GameObject ("GrassHolder");

		stoneHolder.transform.SetParent (holder.transform);
		dirtHolder.transform.SetParent (holder.transform);
		grassHolder.transform.SetParent (holder.transform);

		int t = 0;
		foreach (Transform child in holder.transform) {
			child.gameObject.AddComponent<MeshCollider>();
			MeshFilter filter = child.gameObject.AddComponent<MeshFilter>();
			filter.sharedMesh = new Mesh ();
			MeshRenderer renderer = child.gameObject.AddComponent<MeshRenderer>();
			renderer.material = blockMaterials [t];

			t++;
		}

        if (chunks == null){
            chunks = new List<Chunk>();
        }

		c.hexObjs = new List<GameObject>();

		foreach (Block b in c.blocks) {
			addBlock (b, c, true);
		}

        chunks.Add(c);
        loadedChunks.Add(c.pos, c);
		generatingChunks.Remove(c.pos);
        return c;
    }

	#endregion

	#region ChunkModification

	public bool addBlock(Block b, Chunk c, bool deleteOriginal){

		if(b.pos.y < 0 || b.pos.y + b.vertScale > maxNumBlocks*blockSize){
			return false;
		}
		
		GameObject hObj = null;
		GameObject subChunk = null;

		if (b.blockType == (short)BLOCKID.Stone)
		{
			hObj = (GameObject)Instantiate(block_stone, new Vector3(0, 0, 0), block_stone.transform.rotation);
			subChunk = c.mainHolder.transform.FindChild("StoneHolder").gameObject;
		}
		else if (b.blockType == (short)BLOCKID.Dirt)
		{
			hObj = (GameObject)Instantiate(block_dirt, new Vector3(0, 0, 0), block_dirt.transform.rotation);
			subChunk = c.mainHolder.transform.FindChild("DirtHolder").gameObject;
		}
		else if (b.blockType == (short)BLOCKID.Grass)
		{
			hObj = (GameObject)Instantiate(block_grass, new Vector3(0, 0, 0), block_grass.transform.rotation);
			subChunk = c.mainHolder.transform.FindChild("GrassHolder").gameObject;
		}

		GameObject[] twoObjects = {hObj, subChunk};

		if (hObj != null) {
			//c.hexObjs.Add(hObj);
			hObj.transform.SetParent (subChunk.transform);

			hObj.transform.localScale = new Vector3 (1, 1, b.vertScale);
			hObj.transform.localPosition = b.pos;
			scaleUV (hObj);


			Mesh finalMesh = new Mesh ();

			CombineInstance[] combiners = new CombineInstance[2];

			for (int i = 0; i < 2; i++) {
				combiners [i].subMeshIndex = 0;
				combiners [i].mesh = twoObjects [i].GetComponent<MeshFilter> ().sharedMesh;
				combiners [i].transform = twoObjects [i].GetComponent<MeshFilter> ().transform.localToWorldMatrix;
			}

			finalMesh.CombineMeshes (combiners);

			subChunk.GetComponent<MeshFilter> ().sharedMesh = finalMesh;
			subChunk.GetComponent<MeshCollider> ().sharedMesh = finalMesh;

			c.hexObjs.Add (hObj);
		} else {
			return false;
		}


		if (deleteOriginal) {
			Destroy (hObj.GetComponent<MeshFilter> ().sharedMesh);
			Destroy (hObj);

		} else {
			hObj.SetActive (false);
		}

		return true;
	}


	#endregion


	#region UVScaling
    //Scale the UV coordinates of a block's mesh
    private void scaleUV(GameObject obj)
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
            else
            {
                uvs[i][1] = mesh.uv[i][1];
            }
        }

        mesh.uv = uvs;

    }

	#endregion

	#region Accessors
    public Vector2[] getChunkPositions() {
        Vector2[] result = new Vector2[loadedChunks.Count];
        loadedChunks.Keys.CopyTo(result, 0);
        return result;
    }

    public Chunk[] getLoadedChunks() {
        Chunk[] result = new Chunk[loadedChunks.Count];
        loadedChunks.Values.CopyTo(result, 0);
        return result;
    }

    public bool DestroyChunk(Vector2 pos)
    {
        if (loadedChunks.ContainsKey(pos)) {
            Chunk c = loadedChunks[pos];
            if (c.hexObjs != null)
            {
                foreach(GameObject obj in c.hexObjs)
                {
					if (obj != null) {
						MeshFilter filter = obj.GetComponent<MeshFilter> ();
						if (filter != null) {
							Destroy (filter.sharedMesh);
						}
						Destroy (obj);
					}
                }
            }
			foreach (Transform child in c.mainHolder.transform) {
				MeshFilter filter = child.GetComponent<MeshFilter> ();
				if (filter != null) {
					Destroy (filter.sharedMesh);
				}
				Destroy (child.gameObject);
			}
			Destroy (c.mainHolder);
        }
        return loadedChunks.Remove(pos);
    }


    public Chunk getLoadedChunk(Vector2 cpos)
    {
        return loadedChunks[cpos];
    }

    public bool chunkIsLoaded(Vector2 cPos)
    {
        return loadedChunks.ContainsKey(cPos);
    }

	public bool chunkIsGenerating(Vector2 cPos){
		return generatingChunks.ContainsKey (cPos);
	}

	public int numChunksGenerating(){
		return generatingChunks.Count;
	}

    public Vector2 findCurrentChunk()
    {
        int x = Mathf.FloorToInt(trackedObject.transform.position.x / ((float)size * xDistanceBlocks));
        int z = Mathf.FloorToInt(trackedObject.transform.position.z / ((float)size * zDistanceBlocks));

        return new Vector2(x, z);
    }


	public Chunk getChunkAtPos(Vector3 position){

		int x = Mathf.FloorToInt(position.x / ((float)size * xDistanceBlocks));
		int z = Mathf.FloorToInt(position.z / ((float)size * zDistanceBlocks));

		Chunk result;
		bool success = loadedChunks.TryGetValue (new Vector2 (x, z), out result);

		if (!success) {
			new UnityException ("Chunk not laoded, cannot retrieve");
		}

		return result;
		//First snap position to grid

	}
	#endregion

    [System.Serializable]
    public struct Chunk
    {
        public int size;
        public Vector3 pos;
		public GameObject mainHolder;
        public List<GameObject> hexObjs;
        public short[,,] blockTypes;
		public List<Block> blocks;
    }
}
