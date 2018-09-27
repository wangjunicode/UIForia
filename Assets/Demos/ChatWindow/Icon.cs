using Src;
using UnityEngine;


public class Icon : UIElement {

    public AssetPointer<Texture2D> src;

    public override void OnCreate() {
        Debug.Log("src: " + src.asset);
    }

}