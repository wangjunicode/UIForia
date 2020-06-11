namespace ThisOtherThing.UI {

    public struct EdgeGradientData {

        public bool isActive;
        public float innerScale;
        public float shadowOffset;
        public float sizeAdd;

        public void SetActiveData(float innerScale, float shadowOffset, float sizeAdd) {
            isActive = true;
            this.innerScale = innerScale;
            this.shadowOffset = shadowOffset;
            this.sizeAdd = sizeAdd;
        }

        public void Reset() {
            isActive = false;
            innerScale = 1.0f;
            shadowOffset = 0.0f;
            sizeAdd = 0.0f;
        }

    }

}