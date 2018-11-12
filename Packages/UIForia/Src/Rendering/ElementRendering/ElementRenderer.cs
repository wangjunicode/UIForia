using Src.Rendering;
using UIForia.Rendering;
using UIForia.Rendering.ElementRendering;
using UnityEngine;

namespace UIForia.Systems {

    public abstract class ElementRenderer {

        public readonly int id;

        private static int s_IdGenerator;

        protected ElementRenderer() {
            id = s_IdGenerator++;
        }

        public static readonly ElementRenderer DefaultNonInstanced = new StandardRenderer();
        public static readonly ElementRenderer DefaultInstanced = new StandardInstancedRenderer();
        public static readonly ElementRenderer DefaultText = new StandardTextRenderer();
        public static readonly ElementRenderer DefaultScrollbar = new ScrollbarRenderer();
        
        public abstract void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera);

    }

}