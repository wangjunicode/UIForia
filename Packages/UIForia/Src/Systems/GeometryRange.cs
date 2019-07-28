namespace UIForia.Rendering {

    public struct GeometryRange {

        public int vertexStart;
        public int vertexEnd;
        public int triangleStart;
        public int triangleEnd;

        public GeometryRange(int vertexStart, int vertexEnd, int triangleStart, int triangleEnd) {
            this.vertexStart = vertexStart;
            this.vertexEnd = vertexEnd;
            this.triangleStart = triangleStart;
            this.triangleEnd = triangleEnd;
        }

    }

}