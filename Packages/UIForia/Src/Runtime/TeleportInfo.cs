using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct TeleportInfo {

        public string portalName;
        public TeleportFn renderFn;
        public RangeInt closureRange;
        public StructList<TeleportRenderInfo> renderInfos;

    }

}