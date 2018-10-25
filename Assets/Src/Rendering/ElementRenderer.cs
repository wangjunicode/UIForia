using Src.Rendering;
using UnityEngine;

namespace Src.Systems {

    public abstract class ElementRenderer {

        public readonly int id;

        private static int s_IdGenerator;

        protected ElementRenderer() {
            id = s_IdGenerator++;
        }

        public static readonly ElementRenderer DefaultNonInstanced = new StandardRenderer();
        public static readonly ElementRenderer DefaultInstanced = new StandardInstancedRenderer();

        public virtual void SetupFrame() { }

        public abstract void Render(RenderData[] drawList, int start, int end, Vector3 origin, Camera camera);

        public virtual void TeardownFrame() { }

    }

}