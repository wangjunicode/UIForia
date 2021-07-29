using System.Diagnostics;

namespace UIForia.Style {

    [DebuggerDisplay("{GetDebugDisplay()}")]
    public struct QueryId {

        public int id;

        public QueryId(int id) {
            this.id = id;
        }
        
        public string GetDebugDisplay() {
            if (id == 0) {
                return "Invalid";
            }

            // todo -- find a way to generate debug data
            
            return "Unknown";
        }
    }

}