using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TodManager : MonoBehaviour {

	float changeDelay = 0.1f; //How often in seconds the lighting will update
	float nextChangeTime = 0;

	float lastUpdateTime = 0; //Time of the last frame update

	private int cycleLength = 36000;

	private int currentTime;

	public bool cycleDaytime;

	public int startTime;

	public float timeFactor; // 1 real second = how many integer time ticks?

	public GameObject sunLightObj;
	public GameObject moonLightObj;

	private Light sunLight;
	private Light moonLight;

	//top col, horizon col, bottom col, ambient col, fog col, sun col
	private static int numCols = 6;
	public Color[] daytimeCol = new Color[numCols];
	public Color[] middayCol = new Color[numCols];
	public Color[] sunsetCol = new Color[numCols];
	public Color[] nighttimeCol = new Color[numCols];
	public Color[] midnightCol = new Color[numCols];
	public Color[] sunriseCol = new Color[numCols];

	public Material skyMaterial;

	//private Color[] finalCol = new Color[3];
	private Color[] finalCol = {new Color(92f/255f, 173f/255f, 255f/255f), new Color(171f/255f, 227f/255f, 255f/255f), new Color(122f/255f, 211f/255f, 255f/255f), new Color(198f/255f, 209f/255f, 216f/255f), new Color(133f/255f, 202f/255f, 255f/255f), new Color(255f/255f, 252f/255f, 222f/255f)};

	// Use this for initialization
	void Start () {
		sunLight = sunLightObj.GetComponent<Light> ();
		moonLight = moonLightObj.GetComponent<Light> ();

		currentTime = startTime;

		float tod = (float)currentTime / (float)cycleLength;

		calculateSkyChange (tod, finalCol);
		setSkyCol (finalCol);
		setLightRotation (tod);
		setSunVector (tod);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate(){
		if (cycleDaytime) {
			currentTime += (int)((Time.time - lastUpdateTime)*timeFactor);
			lastUpdateTime = Time.time;

			if (currentTime > cycleLength)
				currentTime -= cycleLength;

			float tod = (float)currentTime / (float)cycleLength;

			if (Time.time > nextChangeTime) { //Obly update the heavy stuff occasionally
				nextChangeTime += changeDelay;

				calculateSkyChange (tod, finalCol);

				setLightRotation (tod);
			}

			setSunVector (tod);
		}
	}

	public void calculateSkyChange(float tod, Color[] col){
		

		if (tod <= 0.2f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (middayCol[i], daytimeCol[i], tod/0.2f);
			}
			sunLight.intensity = 1;
			moonLight.intensity = 0;
		}
		else if (tod <= 0.25f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (daytimeCol[i], sunsetCol[i], (tod-0.2f)/0.05f);
			}
			sunLight.intensity = 1-((tod-0.2f)/0.1f);
			moonLight.intensity = ((tod - 0.2f) / 0.1f) * 0.1f;
		}
		else if (tod <= 0.3f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (sunsetCol[i], nighttimeCol[i], (tod-0.25f)/0.05f);
			}
			sunLight.intensity = 1-((tod-0.2f)/0.1f);
			moonLight.intensity = ((tod - 0.2f) / 0.1f) * 0.1f;
		}
		else if (tod <= 0.5f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (nighttimeCol[i], midnightCol[i], (tod-0.3f)/0.2f);
			}
			sunLight.intensity = 0;
			moonLight.intensity = 0.1f;
		}
		else if (tod <= 0.7f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (midnightCol[i], nighttimeCol[i], (tod-0.5f)/0.2f);
			}
			sunLight.intensity = 0;
			moonLight.intensity = 0.1f;
		}
		else if (tod <= 0.75f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (nighttimeCol[i], sunriseCol[i], (tod-0.7f)/0.05f);
			}
			sunLight.intensity = (tod-0.7f)/0.1f;
			moonLight.intensity = (1-((tod-0.7f)/0.1f))*0.1f;
		}
		else if (tod <= 0.8f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (sunriseCol[i], daytimeCol[i], (tod-0.75f)/0.05f);
			}
			sunLight.intensity = (tod-0.7f)/0.1f;
			moonLight.intensity = (1-((tod-0.7f)/0.1f))*0.1f;
		}
		else if (tod <= 1.0f) {
			for (int i = 0; i < finalCol.Length; i++) {
				finalCol [i] = Color.Lerp (daytimeCol[i], middayCol[i], (tod-0.8f)/0.2f);
			}
			sunLight.intensity = 1;
			moonLight.intensity = 0;
		}

		setSkyCol (finalCol);
	
	}

	public void setSkyCol(Color[] col){
		skyMaterial.SetColor ("_SkyColor1" , col[0]);
		skyMaterial.SetColor ("_SkyColor2" , col[1]);
		skyMaterial.SetColor ("_SkyColor3" , col[2]);
		skyMaterial.SetColor ("_SunColor", col[5]);
		RenderSettings.ambientLight = col [3];
		RenderSettings.fogColor = col [4];
	}

	public void setLightRotation(float tod){
		sunLightObj.transform.eulerAngles = new Vector3(90 + tod * 360, 90, 0);
		moonLightObj.transform.eulerAngles = new Vector3(270 + tod * 360, 90, 0);
	}

	public void setSunVector(float tod){
		skyMaterial.SetVector ("_SunVector", new Vector4(Mathf.Sin(tod*2f*Mathf.PI), Mathf.Cos(tod*2f*Mathf.PI), 0, 0));
	}
}
