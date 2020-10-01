using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Extensions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia {
    
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
    
    public struct StyleCondition {

        public readonly int id;
        public readonly string name;
        public readonly Func<DisplayConfiguration, bool> fn;

        public StyleCondition(int id, string name, Func<DisplayConfiguration, bool> fn) {
            this.id = id;
            this.name = name;
            this.fn = fn;
        }

    }
    


}