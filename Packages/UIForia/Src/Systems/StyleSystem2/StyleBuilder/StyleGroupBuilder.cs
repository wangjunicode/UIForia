using System;
using UIForia.Util;

namespace UIForia {

    public class StyleGroupBuilder : EffectBuilder {

        internal LightList<SelectorBuilder> selectorBuilders;
        
        public void Selector(string selectorName, Action<SelectorBuilder> action) {
            
            SelectorBuilder builder = new SelectorBuilder(selectorName);

            action.Invoke(builder);
            
            selectorBuilders = selectorBuilders ?? new LightList<SelectorBuilder>();
            selectorBuilders.Add(builder);

        }

    }

}