namespace UIForia.Style {

    [AssertSize(16)]
    public struct TextureAssetHandle {

        // 
        /// <summary>
        /// The id of the texture this refers to
        /// </summary>
        public int textureId;
        
        /// <summary>
        /// the application id that this property belongs to. This is used to support
        /// multiple applications running at the same time so we don't cross reference assets
        /// </summary>
        public int systemId;
        
        // the uv box for this texture (in texels), used to compute bw & bh sizes and sprite sheets
        public ushort uvTop;
        public ushort uvRight;
        public ushort uvBottom;
        public ushort uvLeft;

    }

}