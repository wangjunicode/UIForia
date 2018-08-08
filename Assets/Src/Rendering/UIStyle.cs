using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rendering {

    public struct Background {

        public Color color;
        public Texture2D texture;
        public Material material;
        public Rect uvRect;

    }
    
    public class UIStyle {
    
        private Background _background;
        private List<UIElement> elements;
        public Src.Layout.LayoutParameters layoutParameters;
        
        public Background background {
            get { return _background; }
            set {
                
                // if(element.renderState == hover)
                    // return if(hover.isBackgroundSet) else return _background
                // for each element 
                // element.view.MarkForRendering(elements);
            }
        }

        
        
    }
    
    // GetBackground(element)

}