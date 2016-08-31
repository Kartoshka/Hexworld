using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NoiseTest;

public class Generate : MonoBehaviour {

	public GameObject hexObj;
	public AnimationCurve curve;

	//public int interpolationDistance=8;
	//TrilinearInterpolation interpolator;

	public int size;
	public int maxNumBlocks;

	public long gameSeed;

	readonly int smallestInterpolation = 4;
	readonly int levels = 6;
	private long[] octaveSeeds = {57131, 16447, 25999, 40591, 38557, 63629 };
	readonly float[] octaveWeights = {1f, 1.5f, 4f, 6f, 9f, 15f};
	private TrilinearInterpolation[] interpolators = new TrilinearInterpolation[6];

	public float pScale;


	List<GameObject> temp;
	// Use this for initialization
	public void Populate () {
		Clear ();
		//int size = 16;
		float blockSize = 0.25f;
		//maxNumBlocks = 64;
		//float spawnThresh = 0.35f;
		float sum = 0;
		foreach (float f in octaveWeights) {
			sum += f;
		}
		for (int d = 0; d < interpolators.Length; d++) {
			interpolators [d] = new TrilinearInterpolation (gameSeed*octaveSeeds[d], (int)(smallestInterpolation*Mathf.Pow(2f, (float)d)));
		}

		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {

				bool contBlock = false;
				int numBlocks = 0;
				int blockHeight = 0;
				GameObject hObj;
				for (int b = 0; b < maxNumBlocks; b++) {
					//Get interpolated sample value, random number and threshold function temporary
					//float chanceOfBlock = Random.Range(0, 1.0f);

					float chanceOfBlock = 0.0f;

					for (int oct = 0; oct < interpolators.Length; oct++) {
						chanceOfBlock += interpolators [oct].trilinearInterpolation(i * 0.866f * 2.0f + (j % 2) * 0.866f, (float)b*blockSize, j * 1.5f)*octaveWeights[oct];
					}

					chanceOfBlock = chanceOfBlock / (sum);

					//float chanceOfBlock = interpolator.trilinearInterpolation(i * 0.866f * 2.0f + (j % 2) * 0.866f, (float)b*blockSize, j * 1.5f);
					//if (chanceOfBlock < spawnThresh) {

					float x = i * 0.866f * 2.0f + (j % 2) * 0.866f;
					float y = j * 1.5f;
					//float pScale = 0.025f;

					//if (b*blockSize < (Mathf.PerlinNoise(x*pScale , y*pScale)*30f)) {
					if (chanceOfBlock < curve.Evaluate((float)b/(float)maxNumBlocks)* (Mathf.PerlinNoise(x*pScale , y*pScale))) {
						if (contBlock) {
							//increment curent block height
							numBlocks++;
						} else {
							//start new block
							contBlock = true;
							blockHeight = b;
						}
					} else {
						if (contBlock && numBlocks != 0) {
							//spawn block

							hObj = (GameObject)Instantiate (hexObj, new Vector3 (0, 0, 0), hexObj.transform.rotation);
							temp.Add(hObj);

							hObj.transform.localScale = new Vector3 (1, 1, numBlocks * blockSize);
							hObj.transform.localPosition = new Vector3 (i * 0.866f * 2.0f + (j % 2) * 0.866f, blockHeight * blockSize, j * 1.5f);

							contBlock = false;
							numBlocks = 0;
						} else {
							//nothing to be done
						}
					}
				}

				if (contBlock && numBlocks != 0) {
					hObj = (GameObject)Instantiate (hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
					temp.Add(hObj);

					hObj.transform.localScale = new Vector3(1, 1, numBlocks*blockSize);
					hObj.transform.localPosition = new Vector3 (i*0.866f*2.0f + (j%2)*0.866f, blockHeight*blockSize, j * 1.5f);
				}

				//GameObject hObj = (GameObject)Instantiate (hexObj, new Vector3(0, 0, 0), hexObj.transform.rotation);
				//hObj.transform.localScale = new Vector3(1, 1, height);
				//hObj.transform.localPosition = new Vector3 (i*0.866f*2.0f + (j%2)*0.866f, 0, j * 1.5f);
			}
		}
	
	}
	
	// Update is called once per frame
	public void Clear () {
		if (temp != null) {
			foreach (GameObject c in temp) {
				DestroyImmediate (c);
			}
		}
		temp = new List<GameObject> ();
		//interpolator = new TrilinearInterpolation(interpolationDistance);
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
