using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InventoryManager;

public class InventoryUI : MonoBehaviour {

	private Inventory inv;

	public float alphaBigIcon = 0.8f;
	public float alphaSmallIcons =0.8f;

	public float horizontalOffset = 15f;
	public float verticalOffset =10;

	public int rowCount =3;

	public float sizeBigIcon = 90;
	private float sizeIcon;

	private bool initialized = false;

	public Sprite _selected;
	private Image selected;

	private List<Image> iconItems;

	private List<Sprite> imageItems;

	private Image bigIcon;

	private float screenWidth;
	private float screenHeight;


	public float textSize=35f;
	public Font textFont;
	public Vector2 textOffset;
	public Color textColor;
	private Text numItems;

	public void init(Inventory inv){

		sizeIcon = sizeBigIcon/(float)rowCount;


		screenHeight =this.gameObject.GetComponent<RectTransform> ().sizeDelta.y;
		screenWidth =this.gameObject.GetComponent<RectTransform> ().sizeDelta.x;

		GameObject bIcon = new GameObject ("Current Selected Icon");
		bIcon.transform.parent = this.transform;

		RectTransform rectBigIcon = bIcon.AddComponent<RectTransform> ();
		rectBigIcon.sizeDelta = new Vector2 (sizeBigIcon, sizeBigIcon);

		rectBigIcon.anchoredPosition = new Vector2 (screenWidth * 0.5f - sizeBigIcon * 0.5f, -(screenHeight*0.5f - sizeBigIcon * 0.5f));
		bigIcon = bIcon.AddComponent<Image> ();
		Color c = bigIcon.color;
		c.a = alphaSmallIcons;
		bigIcon.color = c;


		GameObject gText = new GameObject ("Num Items UI");
		gText.transform.parent = this.transform;
		numItems = gText.AddComponent<Text> ();




		initialized = true;

		//List for the small icons
		iconItems = new List<Image> ();
		//List for the big image of the selected image
		imageItems = new List<Sprite> ();

		this.inv = inv;

		int a = 0;
		foreach (Item i in inv.getItems()) {
			
			GameObject holder = new GameObject ("holder");
			holder.transform.parent = this.transform;

			iconItems.Add(holder.AddComponent<Image> ());
			imageItems.Add (ConstsClass.getSprite (i.blockId));
			iconItems[a].sprite = imageItems[a];
			c = iconItems [a].color;
			c.a = alphaSmallIcons;
			iconItems [a].color = c;

			RectTransform t = holder.GetComponent<RectTransform> ();	
			float x = screenWidth*0.5f -(rowCount-(a+0.5f))*sizeIcon;
			float y = -(screenHeight * 0.5f - sizeBigIcon - sizeIcon * 0.5f);


			t.anchoredPosition = new Vector2 (x,y);//new Vector2 (sizeIcon *2 * a++ + horizontalOffset, 50);

			t.sizeDelta = new Vector2 (sizeIcon, sizeIcon);

			a++;
		}


		GameObject selectedHolder = new GameObject ("I selected");
		selectedHolder.transform.parent = this.gameObject.transform;
		selected = selectedHolder.AddComponent<Image> ();
		selected.rectTransform.sizeDelta = new Vector2 (sizeIcon, sizeIcon);


		selected.sprite = _selected;
		UpdateValues ();
	}

	private void setNumItemsText(int count){
		RectTransform textTransform = numItems.GetComponent<RectTransform> ();
		textTransform.anchoredPosition = bigIcon.rectTransform.anchoredPosition + textOffset;

//		float x = count.ToString().Length *textSize;
//		float y = textSize +5;

		textTransform.sizeDelta = new Vector2 (textSize, textSize);
		numItems.resizeTextForBestFit = true;
		numItems.text = count.ToString ();
		numItems.color = textColor;
		numItems.font = textFont;
	}

	// Update is called once per frame
	public void UpdateValues () {
		if (initialized) {
			bigIcon.sprite = imageItems [inv.selectedItem];

			Vector2 anchoredDestination = iconItems [inv.selectedItem].rectTransform.anchoredPosition;
			selected.rectTransform.anchoredPosition = anchoredDestination;

			setNumItemsText (inv.getCurrentNumItems ());
			//selected.rectTransform.sizeDelta = itemImages [inv.selectedItem].rectTransform.sizeDelta;

		}
		
	}


}
