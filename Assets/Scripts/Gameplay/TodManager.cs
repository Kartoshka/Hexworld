using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TodManager : MonoBehaviour {

	private int cycleLength = 36000;

	private int currentTime;

	public bool cycleDaytime;

	public int startTime;

	public int timePerFrame;

	public GameObject sunLight;
	public GameObject moonLight;

	public Color[] daytimeCol = new Color[3];
	public Color[] middayCol = new Color[3];
	public Color[] nighttimeCol = new Color[3];

	public Material skyMaterial;

	//private Color[] finalCol = new Color[3];
	private Color[] finalCol = {new Color(92f/255f, 173f/255f, 255f/255f), new Color(171f/255f, 227f/255f, 255f/255f), new Color(122f/255f, 211f/255f, 255f/255f)};

	// Use this for initialization
	void Start () {
		currentTime = startTime;

		calculateSkyCol (finalCol);
		setSkyCol (finalCol);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate(){
		if (cycleDaytime) {
			currentTime += timePerFrame;

			if (currentTime > cycleLength)
				currentTime -= cycleLength;

			calculateSkyCol (finalCol);
			setSkyCol (finalCol);
		}
	}

	public void calculateSkyCol(Color[] col){
		float tod = (float)currentTime / (float)cycleLength;

		if (tod <= 0.25f) {
			for (int i = 0; i < 3; i++) {
				finalCol [i] = Color.Lerp (daytimeCol[i], middayCol[i], tod/0.25f);
			}
		}
		else if (tod <= 0.5f) {
			for (int i = 0; i < 3; i++) {
				finalCol [i] = Color.Lerp (middayCol[i], daytimeCol[i], (tod-0.25f)/0.25f);
			}
		}
		else if (tod <= 0.55f) {
			for (int i = 0; i < 3; i++) {
				finalCol [i] = Color.Lerp (daytimeCol[i], nighttimeCol[i], (tod-0.5f)/0.05f);
			}
		}
		else if (tod <= 1f) {
			for (int i = 0; i < 3; i++) {
				finalCol [i] = Color.Lerp (nighttimeCol[i], daytimeCol[i], (tod-0.95f)/0.05f);
			}
		}
	
	}

	public void setSkyCol(Color[] col){
		skyMaterial.SetColor ("_SkyColor1" , col[0]);
		skyMaterial.SetColor ("_SkyColor2" , col[1]);
		skyMaterial.SetColor ("_SkyColor3" , col[2]);
	}
}
