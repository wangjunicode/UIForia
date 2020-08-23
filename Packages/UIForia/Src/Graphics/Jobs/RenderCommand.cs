namespace UIForia.Graphics {

    public unsafe struct RenderCommand {

        public RenderCommandType type;
        public int batchIndex;
        public void* data;

    }

}