using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NoiseTest;

public class Generate : MonoBehaviour {

    public GameObject hexObj;
    public AnimationCurve curve;
    public AnimationCurve curve2;

    public Vector2 centerChunk;
    public int size;
    public int maxNumBlocks;
    public float blockSize = 0.25f;
    public long gameSeed;

    public int[] octaveDistances;
    public float[] octaveWeights;
    public long[] octaveSeeds;

    private TrilinearInterpolation[] interpolators;

    public float pScale;
    public float pScale2;

    [SerializeField]
    List<GameObject> temp;
    [SerializeField]
    private List<Chunk> chunks;
    // Use this for initialization
    public void Populate() {
        Clear();

        float sum = 0;
        foreach (float f in octaveWeights) {
            sum += f;
        }

        interpolators = new TrilinearInterpolation[octaveDistances.Length];

        for (int d = 0; d < interpolators.Length; d++) {
            interpolators[d] = new TrilinearInterpolation(gameSeed * octaveSeeds[d], octaveDistances[d]);
        }

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {

                float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f;
                float blockZ = j * 1.5f;


                bool contBlock = false;
                int numBlocks = 0;
                int blockHeight = 0;
                GameObject hObj;
                for (int b = 0; b < maxNumBlocks; b++) {
                    //Get interpolated sample value, random number and threshold function temporary
                    //float chanceOfBlock = Random.Range(0, 1.0f);

                    float chanceOfBlock = 0.0f;

                    for (int oct = 0; oct < interpolators.Length; oct++) {
                        chanceOfBlock += interpolators[oct].trilinearInterpolation(blockX, (float)b * blockSize, blockZ) * octaveWeights[oct];
                    }

                    chanceOfBlock = chanceOfBlock / (sum);

                    float perlinValue = Mathf.PerlinNoise(blockX * pScale, blockZ * pScale);
                    float perlinValue2 = Mathf.PerlinNoise((blockX + 456456) * pScale2, (blockZ + 12123) * pScale2); //random numbers to offset the noise so not same as perlinValue
                    float heightFactor = ((float)b / maxNumBlocks) - (perlinValue2 * 0.25f);

                    float combinedCurveValue = (perlinValue * curve.Evaluate(heightFactor)) + ((1.0f - perlinValue) * curve2.Evaluate(heightFactor)); //Mixing two curves based on noise

                    if (chanceOfBlock < combinedCurveValue) {
                       if (contBlock)
                        {
                            //increment curent block height
                            numBlocks++;
                        }
                        else
                        {
                            //start new block
                            contBlock = true;
                            blockHeight = b;
                        }
                    }
                    else
                    {
                        if (contBlock && numBlocks != 0)
                        {
                            //spawn block

                            hObj = (GameObject)Instantiate(hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
                            temp.Add(hObj);
                            scaleUV(hObj);

                            hObj.transform.localScale = new Vector3(1, 1, numBlocks * blockSize);
                            hObj.transform.localPosition = new Vector3(i * 0.866f * 2.0f + (j % 2) * 0.866f, blockHeight * blockSize, j * 1.5f);

                            contBlock = false;
                            numBlocks = 0;
                        }
                        else
                        {
                            //nothing to be done
                        }
                    }
                }

                if (contBlock && numBlocks != 0)
                {
                    hObj = (GameObject)Instantiate(hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
                    temp.Add(hObj);
                    scaleUV(hObj);

                    hObj.transform.localScale = new Vector3(1, 1, numBlocks * blockSize);
                    hObj.transform.localPosition = new Vector3(i * 0.866f * 2.0f + (j % 2) * 0.866f, blockHeight * blockSize, j * 1.5f);
                }

                //GameObject hObj = (GameObject)Instantiate (hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
                //hObj.transform.localScale = new Vector3(1, 1, height);
                //hObj.transform.localPosition = new Vector3 (i*0.866f*2.0f + (j%2)*0.866f, 0, j * 1.5f);
            }
        }

    }

    public Chunk getChunk(Vector2 center, int size, int maxNumBlocks)
    {
        GameObject holder = new GameObject("Holder of chunk of size " + size);
        if (chunks == null)
        {
            chunks = new List<Chunk>();
        }

       
        center.x = center.x * (float)size * Mathf.Sqrt(0.75f) * 2f;
        center.y = center.y* (float)size * 1.5f;

        //Offset of a chunk centered at (0,0) with given size from our center
        float deltaX = center.x - Mathf.FloorToInt(((float)size) * 0.5f) * Mathf.Sqrt(0.75f) * 2f;
        float deltaZ = center.y - Mathf.FloorToInt(((float)size) * 0.5f) * 1.5f;

        //Initialize our chunk
        Chunk result = new Chunk();
        result.hexObjs = new List<GameObject>();
        result.center = center;
        result.size = 16;

        float sum = 0;
        foreach (float f in octaveWeights)
        {
            sum += f;
        }

        interpolators = new TrilinearInterpolation[octaveDistances.Length];

        for (int d = 0; d < interpolators.Length; d++)
        {
            interpolators[d] = new TrilinearInterpolation(gameSeed * octaveSeeds[d], octaveDistances[d]);
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {

                float blockX = i * 0.866f * 2.0f + (j % 2) * 0.866f + deltaX;

                float blockZ = j * 1.5f + deltaZ;


                bool contBlock = false;
                int numBlocks = 0;
                int blockHeight = 0;
                GameObject hObj;
                for (int b = 0; b < maxNumBlocks; b++)
                {
                    //Get interpolated sample value, random number and threshold function temporary
                    //float chanceOfBlock = Random.Range(0, 1.0f);

                    float chanceOfBlock = 0.0f;

                    for (int oct = 0; oct < interpolators.Length; oct++)
                    {
                        chanceOfBlock += interpolators[oct].trilinearInterpolation(blockX, (float)b * blockSize, blockZ) * octaveWeights[oct];
                    }

                    chanceOfBlock = chanceOfBlock / (sum);

                    float perlinValue = Mathf.PerlinNoise(blockX * pScale, blockZ * pScale);
                    float perlinValue2 = Mathf.PerlinNoise((blockX + 456456) * pScale2, (blockZ + 12123) * pScale2); //random numbers to offset the noise so not same as perlinValue
                    float heightFactor = ((float)b / maxNumBlocks) - (perlinValue2 * 0.25f);

                    float combinedCurveValue = (perlinValue * curve.Evaluate(heightFactor)) + ((1.0f - perlinValue) * curve2.Evaluate(heightFactor)); //Mixing two curves based on noise

                    if (chanceOfBlock < combinedCurveValue)
                    {
                        if (contBlock)
                        {
                            //increment curent block height
                            numBlocks++;
                        }
                        else
                        {
                            //start new block
                            contBlock = true;
                            blockHeight = b;
                        }
                    }
                    else
                    {
                        if (contBlock && numBlocks != 0)
                        {
                            //spawn block

                            hObj = (GameObject)Instantiate(hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
                            hObj.transform.SetParent(holder.transform);
                            result.hexObjs.Add(hObj);

                            hObj.transform.localScale = new Vector3(1, 1, numBlocks * blockSize);
                            hObj.transform.localPosition = new Vector3(blockX, blockHeight * blockSize, j * 1.5f + deltaZ);

                            contBlock = false;
                            numBlocks = 0;
                        }
                        else
                        {
                            //nothing to be done
                        }
                    }
                }

                if (contBlock && numBlocks != 0)
                {
                    hObj = (GameObject)Instantiate(hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
                    result.hexObjs.Add(hObj);
                    hObj.transform.SetParent(holder.transform);

                    hObj.transform.localScale = new Vector3(1, 1, numBlocks * blockSize);
                    hObj.transform.localPosition = new Vector3(blockX, blockHeight * blockSize, j * 1.5f + deltaZ);
                }
            }
        }

        chunks.Add(result);
        temp.Add(holder);
        return result;
    }

    //Scale the UV coordinates of a block's mesh, sort of works but something's weird
    public void scaleUV(GameObject obj)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        //Debug.Log("Next Mesh");

        for (int i = 0; i < uvs.Length; i++)
        {
            //Debug.Log(mesh.uv[i]);
            uvs[i][0] = mesh.uv[i][0];

            if (mesh.uv[i][1] == 0.5 && (mesh.uv[i][0] == 0.0 || mesh.uv[i][0] == 0.5))
            {
                uvs[i][1] = 0.5f * obj.transform.localScale[2];
                //uvs[i][1] = mesh.uv[i][1] * 50;
            }
            else
            {
                uvs[i][0] = mesh.uv[i][0];
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
        //interpolator = new TrilinearInterpolation(interpolationDistance);
    }

    [System.Serializable]
    public struct Chunk {
        public int size;
        public Vector3 center;
        public List<GameObject> hexObjs;
    }


	private class TrilinearInterpolation{
		[SerializeField]
		private Dictionary<Vector3,float> interpolators;
		[SerializeField]
		private OpenSimplexNoise noise;
		[SerializeField]
		public int interpolationDistance;


		public TrilinearInterpolation(int distanceInterpolation):this(System.DateTime.Now.Ticks,distanceInterpolation){
		
		}
		public TrilinearInterpolation(long seed, int distanceInterpolation){
			noise = new OpenSimplexNoise(seed);
			this.interpolationDistance = distanceInterpolation;
			interpolators = new Dictionary<Vector3, float>();
		}


		private float getInterpolation(int x, int y, int z){
			if (interpolators == null) {
				interpolators = new Dictionary<Vector3, float> ();
			}
			if (noise == null) {
				noise = new OpenSimplexNoise ();
			}
			float result;
			if (!interpolators.TryGetValue (new Vector3 (x, y, z), out result)) {
				result = (float)noise.Evaluate (x, y, z);
				interpolators.Add (new Vector3 (x, y, z), result);
			}

			return result;
		}

		//http://paulbourke.net/miscellaneous/interpolation/
		public float trilinearInterpolation(float x, float y, float z){

			if (x % interpolationDistance == 0 && y % interpolationDistance == 0 && z % interpolationDistance == 0) {
				return getInterpolation ((int)x, (int)y, (int)z);
			}

			int minX = Mathf.FloorToInt((Mathf.Floor (x) / (float)interpolationDistance))*interpolationDistance;
			int minY = Mathf.FloorToInt((Mathf.Floor (y) / (float)interpolationDistance))*interpolationDistance; 
			int minZ = Mathf.FloorToInt((Mathf.Floor (z) / (float)interpolationDistance))*interpolationDistance;

			int maxX = minX + interpolationDistance;
			int maxY = minY + interpolationDistance;
			int maxZ = minZ + interpolationDistance;

			float V000 = getInterpolation (minX, minY, minZ);
			float V100 = getInterpolation (maxX, minY, minZ);
			float V010 = getInterpolation (minX, maxY, minZ);  
			float V001 = getInterpolation (minX, minY, maxZ);
			float V101 = getInterpolation (maxX, minY, maxZ);
			float V011 = getInterpolation (minX, maxY, maxZ);
			float V110 = getInterpolation (maxX, maxY, minZ);
			float V111 = getInterpolation (maxX, maxY, maxZ);

			float localX = (x - minX)/interpolationDistance;
			float localY = (y - minY)/interpolationDistance;
			float localZ = (z - minZ)/interpolationDistance;


			return  V000 *(1 - localX) * (1 - localY)* (1 - localZ) +
				V100 * localX * (1 - localY)* (1 - localZ ) + 
				V010 *(1 - localX) *localY* (1 - localZ) + 
				V001 *(1 - localX) *(1 - localY) *localZ +
				V101 * localX * (1 - localY) *localZ + 
				V011 *(1 - localX) * localY *localZ + 
				V110 *localX* localY* (1 - localZ) + 
				V111 *localX *localY *localZ;
		}

	}
}
