using System;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public class RenderContextInfo : IDisposable {

        internal LightList<Mesh> meshList;
        internal LightList<Material> materialList;

        internal DataList<int>.Shared textureIds;
        internal DataList<float4x4>.Shared transformList;
        internal DataList<DrawInfo>.Shared drawList;
        internal DataList<ShapeInfo>.Shared shapeInfoList;
        internal DataList<MaterialPropertyOverride>.Shared propertyOverrides;

        public RenderContextInfo() {
            this.meshList = new LightList<Mesh>();
            this.materialList = new LightList<Material>();
            this.textureIds = new DataList<int>.Shared(16, Allocator.Persistent);
            this.drawList = new DataList<DrawInfo>.Shared(32, Allocator.Persistent);
            this.shapeInfoList = new DataList<ShapeInfo>.Shared(32, Allocator.Persistent);
            this.propertyOverrides = new DataList<MaterialPropertyOverride>.Shared(32, Allocator.Persistent);
            this.transformList = new DataList<float4x4>.Shared(16, Allocator.Persistent);
        }

        public void Dispose() {
            textureIds.Dispose();
            transformList.Dispose();
            drawList.Dispose();
            shapeInfoList.Dispose();
            propertyOverrides.Dispose();
        }

        protected internal void Clear() {
            textureIds.size = 0;
            transformList.size = 0;
            drawList.size = 0;
            shapeInfoList.size = 0;
            propertyOverrides.size = 0;

            meshList.Clear();
            materialList.Clear();
        }

    }

}