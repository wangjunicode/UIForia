using System;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Style {

    internal unsafe partial struct CustomPropertyInfo : IDisposable {

        public void* solver;
        public PropertyId propertyId;
        public PropertyType propertyType;

        public int GetSize() {
            int size = 0;
            GetSizeGenerated(ref size);
            return size;
        }

        partial void CreateSolverGenerated(byte * memory);
        
        partial void GetSizeGenerated(ref int size);
        
        partial void SolveGenerated(ref SolverParameters parameters, ref PropertyTables propertyTables, ref CustomSolverInfo customSolverInfo, BumpAllocator* tempAllocator);

        partial void DisposeGenerated();

        public void Dispose() {
            DisposeGenerated();
        }

        private static void DisposeSolver<T>(ref void* solver) where T : unmanaged, ICustomPropertySolver {
            ((T*) solver)->Dispose();
            solver = default;
        }

        public void Solve(ref SolverParameters parameters, ref PropertyTables propertyTables, ref CustomSolverInfo sharedSolverInfo, BumpAllocator* tempAllocator) {
            SolveGenerated(ref parameters, ref propertyTables, ref sharedSolverInfo, tempAllocator);
        }

        public void CreateSolver(byte* buffer) {
            CreateSolverGenerated(buffer);            
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveCustomProperties : IJob {

        // todo -- should be a parallel batch job

        public SolverParameters solverParameters;
        public DataList<CustomPropertyInfo> customProperties;
        
        [NativeDisableUnsafePtrRestriction, NoAlias]
        public TempBumpAllocatorPool* tempAllocatorPool;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public PropertyTables * propertyTables;
        
        public CheckedArray<ValueVariable> valueVariables;

        private void Solve<T>(CustomPropertyInfo info, ref CustomSolverInfo sharedSolverInfo, BumpAllocator* allocator) where T : unmanaged, ICustomPropertySolver {
            T* solver = (T*) info.solver;
            solver->Solve(info.propertyId, ref solverParameters, ref *propertyTables, ref sharedSolverInfo, allocator);
        }

        public void Execute() {

            BumpAllocator* bumpAllocator = tempAllocatorPool->Get();

            int longsPerElementMap = solverParameters.longsPerElementMap;
            
            const int k_BufferCount = 4;
            const int k_BufferClearCount = 3; // don't clear the invalidOrRebuiltMap map 
            
            ulong* mapBuffer = TypedUnsafe.MallocCleared<ulong>(longsPerElementMap * k_BufferCount, Allocator.Temp);
            
            ElementMap definitionMap = new ElementMap(mapBuffer + (longsPerElementMap * 0), longsPerElementMap);
            ElementMap animationMap = new ElementMap(mapBuffer + (longsPerElementMap * 1), longsPerElementMap);
            ElementMap scratchMap = new ElementMap(mapBuffer + (longsPerElementMap * 2), longsPerElementMap);
            ElementMap invalidOrRebuiltMap = new ElementMap(mapBuffer + (longsPerElementMap * 3), longsPerElementMap); // not cleared!!!
            
            CustomSolverInfo sharedSolverInfo = new CustomSolverInfo();
            
            sharedSolverInfo.definitionMap = definitionMap;
            sharedSolverInfo.scratchMap = scratchMap;
            sharedSolverInfo.animationMap = animationMap;
            sharedSolverInfo.invalidOrRebuiltMap = invalidOrRebuiltMap;
            sharedSolverInfo.valueVariableList = valueVariables;
            
            invalidOrRebuiltMap.Copy(solverParameters.invalidatedElementMap);
            invalidOrRebuiltMap.Combine(solverParameters.rebuildBlocksMap);

            for (int i = 0; i < customProperties.size; i++) {

                CustomPropertyInfo customProperty = customProperties[i];

                TypedUnsafe.MemClear(mapBuffer, longsPerElementMap * k_BufferClearCount);

                customProperty.Solve(ref solverParameters, ref *propertyTables, ref sharedSolverInfo, bumpAllocator);
                
                // switch (customProperty.propertyType) {
                //     case PropertyType.Float: {
                //         // Solve<CustomPropertySolver_Float>(customProperty, ref sharedSolverInfo, bumpAllocator);
                //         break;
                //     }
                //
                //     default:
                //         Debug.Log("Unhandled property: " + customProperty.propertyId);
                //         break;
                // }
            }

            tempAllocatorPool->Release(bumpAllocator);

        }

    }

}