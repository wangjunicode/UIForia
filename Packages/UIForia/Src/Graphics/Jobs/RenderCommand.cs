namespace UIForia.Graphics {

    public unsafe struct RenderCommand {

        public RenderCommandType type;
        public int batchIndex;
        public int meshIndex;
        public void* data; // context dependent. for batches this is float4 list for clipping 
        public int sortIdx;

    }

}