using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace Tests.Mocks {

    public class MockTextSizeCalculator : ITextSizeCalculator {

        private readonly Dictionary<string, Vector2> sizes;
        
        public MockTextSizeCalculator() {
            sizes = new Dictionary<string, Vector2>();    
        }

        public void SetSize(string text, Vector2 size) {
            sizes[text] = size;
        }
        
        public float CalcTextWidth(string text, UIStyleSet style) {
            if (sizes.ContainsKey(text)) return sizes[text].x;
            return 100;
        }

        public float CalcTextHeight(string text, UIStyleSet style, float width) {
            if (sizes.ContainsKey(text)) return sizes[text].y;
            return style.computedStyle.FontSize;
        }

    }


}