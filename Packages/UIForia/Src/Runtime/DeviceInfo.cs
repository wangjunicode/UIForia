using UnityEngine;

namespace UIForia {
    public struct DeviceInfo {


        
        public float dpiScale;

        public DeviceInfo (float dpiScale) {
            this.dpiScale = dpiScale;
        }

        public float GetScaledScreenWidth() {
            return Screen.width;
        }
        
        public float GetScaledScreenHeight() {
            return Screen.height;
        }
    }
}