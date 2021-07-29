using UIForia.Util.Unsafe;

namespace UIForia.Style {

    internal unsafe struct CustomSolverInfo {

        public CheckedArray<ValueVariable> valueVariableList;
        public ElementMap definitionMap;
        public ElementMap scratchMap;
        public ElementMap animationMap;
        public ElementMap invalidOrRebuiltMap;
        public DataList<PropertyContainer> variableResolveList;

        public CheckedArray<TVARIABLETYPE> GetVariableList_TVARIABLETYPE() {
            throw new System.NotImplementedException();
        }

        public CheckedArray<ValueVariable> GetVariableList_ValueVariable() {
            return new CheckedArray<ValueVariable>(valueVariableList.array, valueVariableList.size);
        }

        public CheckedArray<ColorVariable> GetVariableList_ColorVariable() {
            return default;
        }

        public CheckedArray<TextureVariable> GetVariableList_TextureVariable() {
            return default;
        }

        public CheckedArray<VectorVariable> GetVariableList_VectorVariable() {
            return default;
        }

    }

}