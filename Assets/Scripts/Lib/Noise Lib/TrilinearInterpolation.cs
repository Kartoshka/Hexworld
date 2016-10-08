using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoiseTest;

public class TrilinearInterpolation
{
    [SerializeField]
    private ConcurrentDictionary<Vector3, float> interpolators;
    [SerializeField]
    private OpenSimplexNoise noise;
    [SerializeField]
    public int interpolationDistance;

	public int dicHits = 0;
	public int dicMisses = 0;

    public TrilinearInterpolation(int distanceInterpolation) : this(System.DateTime.Now.Ticks, distanceInterpolation)
    { }
    public TrilinearInterpolation(long seed, int distanceInterpolation)
    {
        noise = new OpenSimplexNoise(seed);
        this.interpolationDistance = distanceInterpolation;
        interpolators = new ConcurrentDictionary<Vector3, float>();
    }


    private float getSimplexNoise(int x, int y, int z)
    {
        if (interpolators == null)
        {
            interpolators = new ConcurrentDictionary<Vector3, float>();
        }
        if (noise == null)
        {
            noise = new OpenSimplexNoise();
        }
		float result = 0;
		Vector3 key = new Vector3 (x, y, z);
		if (interpolators.ContainsKey (key)) {
			result = interpolators [key];
			dicHits++;
		} else {
			result = (float)noise.Evaluate (x, y, z);
			dicMisses++;

			try{
				interpolators.Add(key,result);
			}catch(ArgumentException e){
				//Already in dictionnary because of other thread, oh well.
			}
		}
		return result;
    }

    //http://paulbourke.net/miscellaneous/interpolation/
    public float trilinearInterpolation(float x, float y, float z)
    {

        if (x % interpolationDistance == 0 && y % interpolationDistance == 0 && z % interpolationDistance == 0)
        {
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


