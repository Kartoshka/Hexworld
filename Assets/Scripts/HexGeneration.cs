using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NoiseTest;
public class HexGeneration : MonoBehaviour {

	public GameObject hexObj;
	public int boundsX = 100;
	public int interpolationDistance =8;
	public AnimationCurve curve;
	private Dictionary<Vector3,float> interpolators;
	private OpenSimplexNoise noise;
	public float threshhold;

	// Use this for initialization
	void Start () {
		interpolators = new Dictionary<Vector3, float> ();
		OpenSimplexNoise noise = new OpenSimplexNoise ();

		for (int i = 0; i < boundsX; i++) {
			for (int j = 0; j < boundsX; j++) {
				for (float k = 0; k < (float)boundsX; k += hexObj.transform.localScale.z) {
					Vector3 position = new Vector3 (i, j, k);

					if (trilinearInterpolation(i,j,k)<curve.Evaluate(((float)j/(float)boundsX))) {
						Instantiate (hexObj, position, hexObj.transform.rotation);
					}
				}
			}
		}
	}

	//http://paulbourke.net/miscellaneous/interpolation/
	private float trilinearInterpolation(float x, float y, float z){
		
		if (x % interpolationDistance == 0 && y % interpolationDistance == 0 && z % interpolationDistance == 0) {
			return getInterpolation ((int)x, (int)y, (int)z);
		}

		int minX = Mathf.FloorToInt((Mathf.FloorToInt (x) / interpolationDistance))*interpolationDistance;
		int minY = Mathf.FloorToInt((Mathf.FloorToInt (y) / interpolationDistance))*interpolationDistance; 
		int minZ = Mathf.FloorToInt((Mathf.FloorToInt (z) / interpolationDistance))*interpolationDistance;

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


}
