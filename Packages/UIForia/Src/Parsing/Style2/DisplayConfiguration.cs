using UnityEngine;

namespace UIForia.Style2 {

    public struct DisplayConfiguration {

        public readonly float dpi;
        public readonly float screenWidth;
        public readonly float screenHeight;
        public readonly ScreenOrientation screenOrientation;
        public readonly DeviceOrientation deviceOrientation;

        public DisplayConfiguration(float screenWidth, float screenHeight, float dpi, ScreenOrientation screenOrientation = ScreenOrientation.Landscape, DeviceOrientation deviceOrientation = DeviceOrientation.Unknown) {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.dpi = dpi;
            this.screenOrientation = screenOrientation;
            this.deviceOrientation = deviceOrientation;
        }

    }

}