using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    internal unsafe struct SolverParameters {

        public int maxElementId;
        public int deltaMS;
        public int longsPerElementMap;

        public DataList<DataList<KeyFrameResult>> animationResultBuffer;
        public DataList<DataList<InterpolatedStyleValue>> animationValueBuffer;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyUpdateSet* sharedRebuildResult;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TransitionUpdateSet* transitionUpdateResult;
        
        [NativeDisableUnsafePtrRestriction, NoAlias]
        public InstancePropertyUpdateSet* instanceRebuildResult;

        public DataList<int> defaultValueIndices;

        public CheckedArray<ElementId> elementIdToParentId;

        public ElementMap invalidatedElementMap;

        public ElementMap rebuildBlocksMap;

        public ElementMap initMap;
        
        public ElementMap activeMap;

        public CheckedArray<ColorVariable> colorVariables;
        public CheckedArray<ValueVariable> valueVariables;
        public CheckedArray<TextureVariable> textureVariables;
        
        public CheckedArray<TraversalInfo> traversalTable;
        
        public DataList<TransitionDefinition> transitionDatabase;

        public static CheckedArray<TVARIABLETYPE> GetVariableList_TVARIABLETYPE(in SolverParameters parameters) {
            return default;
        }

        public static CheckedArray<ColorVariable> GetVariableList_ColorVariable(in SolverParameters parameters) {
            return parameters.colorVariables;
        }
        
        public static CheckedArray<ValueVariable> GetVariableList_ValueVariable(in SolverParameters parameters) {
            return parameters.valueVariables;
        }
        
        public static CheckedArray<TextureVariable> GetVariableList_TextureVariable(in SolverParameters parameters) {
            return parameters.textureVariables;
        }

        
        public static CheckedArray<FontAssetVariable> GetVariableList_FontAssetVariable(in SolverParameters parameters) {
            return default;
        }

        public static CheckedArray<GridTemplateVariable> GetVariableList_GridTemplateVariable(in SolverParameters parameters) {
            return default;
        }

        public static CheckedArray<PainterVariableType> GetVariableList_PainterVariableType(in SolverParameters parameters) {
            return default;
        }

    }

}