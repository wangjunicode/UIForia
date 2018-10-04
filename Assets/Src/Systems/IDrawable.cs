using System;
using Rendering;
using UnityEngine;

namespace Src.Systems {

    public interface IDrawable {

        void OnAllocatedSizeChanged();
        void OnStylePropertyChanged(StyleProperty property);
        
        Mesh GetMesh();
        Material GetMaterial();
        
        event Action<IDrawable> onMeshDirty;
        event Action<IDrawable> onMaterialDirty;

        int Id { get; }
        bool IsMaterialDirty { get; }
        bool IsGeometryDirty { get; }
    }
    
 
}