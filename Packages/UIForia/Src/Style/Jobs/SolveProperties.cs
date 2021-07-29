using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveLayoutSetupProperties : IJob {

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertySolverGroup_LayoutSetup* solverGroup;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables* propertyTables;

        public void Execute() {
            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, bumpAllocator);
            tempAllocatorPool->Release(bumpAllocator);
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveLayoutAndTextProperties : IJob {

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertySolverGroup_LayoutAndText* solverGroup;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables* propertyTables;

        public void Execute() {
            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, bumpAllocator);
            tempAllocatorPool->Release(bumpAllocator);
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveClippingTransformProperties : IJob {

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertySolverGroup_ClippingAndTransformation* solverGroup;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables* propertyTables;

        public void Execute() {
            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, bumpAllocator);
            tempAllocatorPool->Release(bumpAllocator);
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveRenderingProperties : IJob {

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertySolverGroup_Rendering* solverGroup;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables* propertyTables;

        public void Execute() {
            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, bumpAllocator);
            tempAllocatorPool->Release(bumpAllocator);
        }

    }
    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveTextMeasurementProperties : IJob {

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertySolverGroup_TextMeasurement* solverGroup;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public SolverParameters solverParameters;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables* propertyTables;

        public void Execute() {
            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();
            solverGroup->Invoke(solverParameters, styleTables, propertyTables, bumpAllocator);
            tempAllocatorPool->Release(bumpAllocator);
        }

    }
}