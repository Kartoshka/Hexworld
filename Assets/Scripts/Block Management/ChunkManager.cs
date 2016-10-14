using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Consts;
using NoiseTest;

public class ChunkManager : MonoBehaviour {

	private Vector3 originalPos;
	private Vector3 originalScale;
	private Quaternion originalRotation;
	private Transform originalParent;

    //Along x axis
    private float xDistanceBlocks = 0.866f * 2f;
    //Along z axis, distance between blocks
    private float zDistanceBlocks = 1.5f;

    public GameObject trackedObject;

    //public GameObject hexObj;
	private GameObject block_stone;
	private GameObject block_dirt;
	private GameObject block_grass;
	private GameObject block_stoneTile;
	private GameObject block_crystalO;
	private GameObject block_lampO;


	//Probably temporary, the light object for lampO block
	public GameObject light_lampO;

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

	private OpenSimplexNoise crystalONoise;

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

	//In-game persistence
	Dictionary<Vector2, Chunk> persistentChunks;

    private Vector2 currentChunkPos;

    public AbChunkModifier[] chunkModifiers;

    // Use this for initialization
    void Start () {

		block_stone = ConstsClass.getPrefab (BLOCKID.Stone);
		block_grass = ConstsClass.getPrefab (BLOCKID.Grass);
		block_dirt = ConstsClass.getPrefab (BLOCKID.Dirt);
		block_stoneTile = ConstsClass.getPrefab (BLOCKID.StoneTile);
		block_crystalO = ConstsClass.getPrefab (BLOCKID.CrystalO);
		block_lampO = ConstsClass.getPrefab (BLOCKID.LampO);

		originalPos = this.transform.position;
		originalScale = this.transform.localScale;
		originalRotation = this.transform.rotation;
		originalParent = this.transform.parent;

		noise = new OpenSimplexNoise[octaveZoom.Length];
		for(int n=0;n<noise.Length;n++){
			noise[n] = new OpenSimplexNoise (octaveSeeds[n]*gameSeed);
		}

		crystalONoise = new OpenSimplexNoise(42);

        loadedChunks = new Dictionary<Vector2, Chunk>();
		generatingChunks = new Dictionary<Vector2, bool> ();
		loadedChunks = new Dictionary<Vector2, Chunk>();

		persistentChunks = new Dictionary<Vector2, Chunk> ();
        
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

		Chunk result = new Chunk ();

		if (persistentChunks.ContainsKey (cPos)) {
			
			Chunk pChunk = persistentChunks[cPos];
			persistentChunks.Remove (cPos);

			result.blocks = pChunk.blocks;
			result.blockTypes = pChunk.blockTypes;
		} else {

			short[,,] blockValues = new short[size, size, maxNumBlocks];

			int chunkX = Mathf.FloorToInt (cPos.x);
			int chunkZ = Mathf.FloorToInt (cPos.y);


			float deltaX = (float)chunkX * (size * xDistanceBlocks);
			float deltaZ = (float)chunkZ * (size * zDistanceBlocks);

			List<Block> blocks = new List<Block> ();

			AnimationCurve densityMixFactor_copy = new AnimationCurve (densityMixFactor.keys);
			AnimationCurve densityCurve_mountains_copy = new AnimationCurve (densityCurve_mountains.keys);
			AnimationCurve densityCurve_plains_copy = new AnimationCurve (densityCurve_plains.keys);
			AnimationCurve densityCurve_caves_copy = new AnimationCurve (densityCurve_caves.keys);


			//First pass for main stone generation
			for (int i = 0; i < size; i++) {
				for (int j = 0; j < size; j++) {

					float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f + deltaX;
					float blockZ = j * 1.5f + deltaZ;

					short prev = (short)BLOCKID.Air;
					int dirtCount = 0;

					int blockStart = 0;

					for (int k = maxNumBlocks - 1; k >= 0; k--) {
                    
						float heightFactor = (float)k / (float)maxNumBlocks;

						float mixValue = densityMixFactor_copy.Evaluate (Mathf.PerlinNoise (blockX * pScale_mix + pOff_mix + pOff, blockZ * pScale_mix + pOff_mix + pOff));
						float heightOffset = 0.3f * Mathf.PerlinNoise (blockX * pScale_height + pOff_height + pOff, blockZ * pScale_height + pOff_height + pOff);

						float mountainValue = densityCurve_mountains_copy.Evaluate (heightFactor - heightOffset);
						float plainsValue = densityCurve_plains_copy.Evaluate (heightFactor - heightOffset);
						float cavesValue = densityCurve_caves_copy.Evaluate (heightFactor);

						float biomeCombineValue = ((1 - mixValue) * mountainValue) + ((mixValue) * plainsValue);
				

						double chanceOfBlock = 0.0f;
						double weightSum = 0;

						for (int n = 0; n < noise.Length; n++) {
							double newWeight = octaveWeights [n] * 0.5f + octaveWeights [n] * Mathf.PerlinNoise (blockX * pScale_octaves + (float)octaveSeeds [n] + pOff, blockZ * pScale_octaves + (float)octaveSeeds [n] + pOff);
							chanceOfBlock += noise [n].Evaluate (blockX * (1f / octaveZoom [n]), (float)k * blockSize * (1f / octaveZoom [n]), blockZ * (1f / octaveZoom [n])) * newWeight;
							weightSum += newWeight;
						}

						chanceOfBlock = chanceOfBlock / weightSum;

						int grassCutoff = maxNumBlocks / 2 - 30;
                    

						if (chanceOfBlock < biomeCombineValue && chanceOfBlock < cavesValue) {

							if (prev == (short)BLOCKID.Air && k > grassCutoff) {
								blockValues [i, j, k] = (short)BLOCKID.Grass;
							} else if (prev == (short)BLOCKID.Grass || (prev == (short)BLOCKID.Dirt && dirtCount < 5)) {
								blockValues [i, j, k] = (short)BLOCKID.Dirt;
								dirtCount++;
							} else {
								if (crystalONoise.Evaluate (blockX * 0.2f, (float)k * blockSize * 0.2f, blockZ * 0.2f) < 0.6)
									blockValues [i, j, k] = (short)BLOCKID.Stone;
								else
									blockValues [i, j, k] = (short)BLOCKID.CrystalO;
							
								dirtCount = 0;
							}
						} else {
							blockValues [i, j, k] = (short)BLOCKID.Air;
							dirtCount = 0;
						}


						if (blockValues [i, j, k] != prev || k == 0) {
							if (prev != (short)BLOCKID.Air) {
								blocks.Add (new Block (new Vector3 (blockX, k * blockSize, blockZ), (blockStart - k) * blockSize, prev));
							}

							blockStart = k;
						}


						prev = blockValues [i, j, k];
					}
				}
			}


			result.blocks = blocks;
			result.blockTypes = blockValues;

		}

		result.size = size;
		result.pos = cPos;
        

        return result;
    }

    public Chunk getNewChunkData(Vector2 cPos)
    {
        return this.getNewChunkData(cPos, this.size, this.maxNumBlocks);
    }

    //public Chunk instantiateChunk(Vector2 cPos, int size, int maxNumBlocks, short[,,] blockValues)
	public Chunk instantiateChunk(Chunk c)
    {
		
		GameObject holder = new GameObject("Holder of chunk ( " + c.pos.x + " , " + c.pos.y + " ) " + "of size " + size);
		c.mainHolder = holder;

		GameObject stoneHolder = new GameObject ("StoneHolder");
		GameObject dirtHolder = new GameObject ("DirtHolder");
		GameObject grassHolder = new GameObject ("GrassHolder");
		GameObject stoneTileHolder = new GameObject ("StoneTileHolder");
		GameObject crystalOHolder = new GameObject ("CrystalOHolder");
		GameObject lampOHolder = new GameObject ("LampOHolder");

		stoneHolder.transform.SetParent (holder.transform);
		dirtHolder.transform.SetParent (holder.transform);
		grassHolder.transform.SetParent (holder.transform);
		stoneTileHolder.transform.SetParent (holder.transform);
		crystalOHolder.transform.SetParent (holder.transform);
		lampOHolder.transform.SetParent (holder.transform);

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

//		foreach (Block b in c.blocks) {
//			addBlockToMesh (b, c, true,false);
//		}

		for (int i = 0; i < c.blocks.Count; i++) {
			addBlockToMesh (c.blocks[i], c, true,false);
		}

		//Update the colliders. In reality we want to do this a little as possible because we're using meshcolliders and those do not like being changed.
		stoneHolder.GetComponent<MeshCollider> ().sharedMesh = stoneHolder.GetComponent<MeshFilter> ().sharedMesh;
		grassHolder.GetComponent<MeshCollider> ().sharedMesh = grassHolder.GetComponent<MeshFilter> ().sharedMesh;
		dirtHolder.GetComponent<MeshCollider> ().sharedMesh = dirtHolder.GetComponent<MeshFilter> ().sharedMesh;
		stoneTileHolder.GetComponent<MeshCollider> ().sharedMesh = stoneTileHolder.GetComponent<MeshFilter> ().sharedMesh;
		crystalOHolder.GetComponent<MeshCollider> ().sharedMesh = crystalOHolder.GetComponent<MeshFilter> ().sharedMesh;
		lampOHolder.GetComponent<MeshCollider> ().sharedMesh = lampOHolder.GetComponent<MeshFilter> ().sharedMesh;


        chunks.Add(c);
        loadedChunks.Add(c.pos, c);
		generatingChunks.Remove(c.pos);
        return c;
    }

	#endregion

	#region ChunkModification

	private bool addBlockToMesh(Block b, Chunk c, bool deleteOriginal,bool updateCollider){

		if(b.pos.y < 0 || b.pos.y + b.vertScale > maxNumBlocks*blockSize){
			return false;
		}

		GameObject hObj = null;
		Mesh hMesh =null;
        Mesh finalMesh;
        Transform transform = null;
		GameObject subChunk = null;
		GameObject blockLight = null;

		if (b.blockType == (short)BLOCKID.Stone) {
			hObj = block_stone;
			hMesh = (Mesh)Instantiate (block_stone.GetComponent<MeshFilter> ().sharedMesh);
			subChunk = c.mainHolder.transform.FindChild ("StoneHolder").gameObject;
		} 
		else if (b.blockType == (short)BLOCKID.Dirt) {
			hObj = block_dirt;
			hMesh = (Mesh)Instantiate (block_dirt.GetComponent<MeshFilter> ().sharedMesh);

			subChunk = c.mainHolder.transform.FindChild ("DirtHolder").gameObject;
		} 
		else if (b.blockType == (short)BLOCKID.Grass) {
			hObj = block_grass;
			hMesh = (Mesh)Instantiate (block_grass.GetComponent<MeshFilter> ().sharedMesh);

			subChunk = c.mainHolder.transform.FindChild ("GrassHolder").gameObject;
		} 
		else if (b.blockType == (short)BLOCKID.StoneTile) {
			hObj = block_stoneTile;
			hMesh = (Mesh)Instantiate (block_stoneTile.GetComponent<MeshFilter> ().sharedMesh);

			subChunk = c.mainHolder.transform.FindChild ("StoneTileHolder").gameObject;
		}
		else if (b.blockType == (short)BLOCKID.CrystalO) {
			hObj = block_crystalO;
			hMesh = (Mesh)Instantiate (block_crystalO.GetComponent<MeshFilter> ().sharedMesh);

			subChunk = c.mainHolder.transform.FindChild ("CrystalOHolder").gameObject;
		}
		else if (b.blockType == (short)BLOCKID.LampO) {
			hObj = block_lampO;
			hMesh = (Mesh)Instantiate (block_lampO.GetComponent<MeshFilter> ().sharedMesh);

			blockLight = light_lampO;

			subChunk = c.mainHolder.transform.FindChild ("LampOHolder").gameObject;
		}

		Mesh[] twoMeshes = { hMesh, subChunk.GetComponent<MeshFilter> ().sharedMesh };



		if (hMesh != null) {
			
			transform = this.gameObject.transform;

			transform.rotation = hObj.transform.rotation;

			transform.parent = subChunk.transform;
			transform.localScale = new Vector3 (1, 1, b.vertScale);
			transform.localPosition = b.pos;

			if (blockLight != null) {
				for (int i = 0; i < (int)(b.vertScale * 4); i++) {
					GameObject bl = (GameObject)Instantiate (blockLight);
					bl.transform.localPosition = b.pos + new Vector3 (0, 0.125f + i*(0.25f), 0); //Offset light by half the block height
					bl.transform.parent = subChunk.transform;
				}
			}

			int yPos = (int)(b.pos.y * 4);

			scaleUV (hMesh, transform.localScale, yPos);


			finalMesh = new Mesh ();
			Transform[] transforms = { transform, subChunk.transform };

			CombineInstance[] combiners = new CombineInstance[2];

			for (int i = 0; i < 2; i++) {
				combiners [i].subMeshIndex = 0;
				combiners [i].mesh = twoMeshes [i];
				combiners [i].transform = transforms [i].localToWorldMatrix;
			}

			finalMesh.CombineMeshes (combiners);

			subChunk.GetComponent<MeshFilter> ().sharedMesh = finalMesh;
			if (updateCollider) {
				subChunk.GetComponent<MeshCollider> ().sharedMesh = finalMesh;
			}
		} else {
			return false;
		}

		this.gameObject.transform.localScale = originalScale;
		this.gameObject.transform.position = originalPos;
		this.gameObject.transform.rotation = originalRotation;
		this.gameObject.transform.SetParent (originalParent);


		if (deleteOriginal) {
			DestroyImmediate (hMesh);
        } else {
			hObj.SetActive (false);
		}
		return true;
	}

	public bool AddBlock(Block b, Chunk c, bool deleteOriginal,bool updateCollider){
		Vector3 localCoords = getLocalBlockCoords (b.pos);
		Chunk chun;

		try {
			chun = getChunkAtPos (b.pos);
		} catch (System.Exception ex) {
			return false;
		}

		//NEEDS REVISION

		if (chun.blockTypes [(int)localCoords.x, (int)localCoords.z, (int)localCoords.y] == (short)b.blockType) {
			return true;
		} else {
			if(addBlockToMesh(b,c,deleteOriginal,updateCollider)){
				c.blocks.Add (b);
				chun.blockTypes [(int)localCoords.x, (int)localCoords.z, (int)localCoords.y] = (short)b.blockType;
				return true;
			}
			else
			{
				return false;
			}
		}
	}



	#endregion


	#region UVScaling

	//Scale UV coordinates of a mesh instead of a gameobject, used for more optimied chunk generation
	private void scaleUV(Mesh mesh,Vector3 localScale, int yPos){

		float yOffset = 0.125f * (yPos % 8);

		Vector3[] vertices = mesh.vertices;
		Vector2[] uvs = new Vector2[vertices.Length];

		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i][0] = mesh.uv[i][0];

			if (mesh.uv [i] [0] <= 0.5) { //If vertices pertaining to vertical faces
				if (mesh.uv [i] [1] == 0.5) { //If top vertices
					uvs [i] [1] = 0.5f * localScale [2];
				} else {
					uvs [i] [1] = mesh.uv [i] [1];
				}

				uvs [i] [1] += yOffset; //Offset coordinates on Y axis according to block's height offset

			} else {
				uvs [i] [1] = mesh.uv [i] [1];
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
                            Destroy(filter.sharedMesh);
                            Destroy(filter);
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

			persistentChunks.Add(pos, c);
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

	//Mildly useless, should only be used for rough player position calculation
    public Vector2 findCurrentChunk()
    {
        int x = Mathf.FloorToInt(trackedObject.transform.position.x / ((float)size * xDistanceBlocks));
        int z = Mathf.FloorToInt(trackedObject.transform.position.z / ((float)size * zDistanceBlocks));

        return new Vector2(x, z);
    }


	public Chunk getChunkAtPos(Vector3 position){

		int[] pos = getGlobalBlockIndex (position);

		//Use global block index to find correct chunk
		int x = Mathf.FloorToInt ((float)pos [0] / size);
		int z = Mathf.FloorToInt ((float)pos [2] / size);

		Chunk result;
		bool success = loadedChunks.TryGetValue (new Vector2 (x, z), out result);

		if (!success) {
			new UnityException ("Chunk not loaded, cannot retrieve");
		}

		return result;
	}

     public short getBlockTypeAtAbsPos(Vector3 pos)
    {

        int[] ar = (getGlobalBlockIndex(pos));
        ChunkManager.Chunk chun = this.getChunkAtPos(pos);

        int x = (int)(ar[0] - chun.pos.x * this.size);
        int y = (int)ar[1] +1; //Changed from +2
        int z = (int)(ar[2] - chun.pos.y * this.size);

        if (x >= size)
        {
            x = (int)(ar[0] - (chun.pos.x+1) * this.size);

        }
        if (z >= size)
        {
           z = (int)(ar[2] - (chun.pos.y + 1) * this.size);
        }
        return chun.blockTypes[x,z,y];
    }

	public Vector3 getLocalBlockCoords(Vector3 pos){
		int[] ar = (getGlobalBlockIndex(pos));
		ChunkManager.Chunk chun = this.getChunkAtPos(pos);

		int x = (int)(ar[0] - chun.pos.x * this.size);
		int y = (int)ar[1] +1; //Changed from +2
		int z = (int)(ar[2] - chun.pos.y * this.size);

		return new Vector3 (x, y, z);
	}

	//Gets the actual position of a block
    public Vector3 snapCoordsToGrid(Vector3 position)
    {
		int[] gridPos = getGlobalBlockIndex(position);
        return new Vector3(gridPos[0] * 0.866f * 2f + Mathf.Abs(gridPos[2] % 2) * 0.866f, gridPos[1] * this.blockSize, gridPos[2] * 1.5f);
    }


	//Gets the global (snapped) block index for any given vector
    public int[] getGlobalBlockIndex(Vector3 position)
    {

        int zRound = Mathf.FloorToInt((position.z + 1) / 1.5f);
        float z = (float)zRound * 1.5f;

        int xRound = Mathf.FloorToInt((position.x + 0.866f + Mathf.Abs(zRound % 2) * 0.866f) / (2 * 0.866f));
        float x = xRound * 2f * 0.866f - Mathf.Abs(zRound % 2) * 0.866f;

        float zInTile = position.z + 1 - z;
        float xInTile = position.x - x;


        if (zInTile > Mathf.Abs(xInTile * (0.866f / 2f)))
        {
            xRound -= Mathf.Abs(zRound % 2);
        }
        else
        {
            //z -= 1.5f;
            zRound--;
            if (xInTile > 0)
            {
                //x += 0.866f;
                //xRound++;
            }
            else
            {
                //x -= 0.866f;
                xRound--;
            }
        }
        int[] coords = { xRound, Mathf.FloorToInt(position.y / this.blockSize), zRound };

        return coords;

    }
		
    #endregion

    [System.Serializable]
    public struct Chunk
    {
        public int size;
        public Vector2 pos;
		public GameObject mainHolder;
        public List<GameObject> hexObjs;
        public short[,,] blockTypes;
		public List<Block> blocks;
    }
}
