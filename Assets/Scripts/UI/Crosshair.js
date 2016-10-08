 var crosshairTexture : Texture2D;
 var position : Rect;
 static var OriginalOn = true;
 
 function Start()
 {
     position = Rect((Screen.width - crosshairTexture.width) / 2, (Screen.height - 
         crosshairTexture.height) /2, crosshairTexture.width, crosshairTexture.height);
 }
 
 function OnGUI()
 {
     if(OriginalOn == true)
     {
         GUI.DrawTexture(position, crosshairTexture);
     }
 }
