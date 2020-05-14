using System;
using UIForia.Util;

namespace UIForia {

    public struct SelectorFilterContext {

        public LightList<Func<SelectorElementWrapper, SelectorFilterContext, bool>> table_SelectorWhereFilters;
        // element info table
        // tag table
        // state table

    }

}