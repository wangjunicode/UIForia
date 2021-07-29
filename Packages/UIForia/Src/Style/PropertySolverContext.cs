using System;
using UIForia.ListTypes;
using UIForia.Unsafe;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Style {

    // parameters = variable per frame
    // context | sharedSolverInfo = shared data for all solvers -> listAllocator, inheritMap, definitionMap, maybe thats it
    // solverInfo = data for this solver 

    internal unsafe struct PropertySolverContext : IDisposable {

        private int mapCapacity;
        private int handledPropertyCount;

        // owned
        public ListAllocatorSized * listAllocator; // persistent across frames 
        public DataList<PropertyContainer> variableResolveList;
        
        // persistent maps 
        private const int k_MapCount = 2;
        [NoAlias] public ulong* inheritMap;
        [NoAlias] public ulong* definitionMap;

        private int longsPerElementMap;
        public void Setup(in SolverParameters parameters) {
            longsPerElementMap = parameters.longsPerElementMap;
            
            if (mapCapacity >= parameters.longsPerElementMap) {
                return;
            }

            int prevCapacity = mapCapacity;
            mapCapacity = parameters.longsPerElementMap; //  + 8; // buffer a bit by leaving space for 512 more elements  
            
            // re-allocate, need to know property counts and copy in accordingly 
            // clear whole buffer, then memcpy prev data in once per handled property
            long bytes = sizeof(ulong) * handledPropertyCount * mapCapacity * k_MapCount;
            ulong* ptr = (ulong*) UnsafeUtility.Malloc(bytes, UnsafeUtility.AlignOf<ulong>(), Allocator.Persistent);
            UnsafeUtility.MemClear(ptr, bytes);

            if (prevCapacity != 0) {

                for (int i = 0; i < handledPropertyCount; i++) {
                    TypedUnsafe.MemCpy(ptr + (i * mapCapacity), definitionMap + (i * prevCapacity), prevCapacity);
                }

                ptr += mapCapacity * handledPropertyCount;

                for (int i = 0; i < handledPropertyCount; i++) {
                    TypedUnsafe.MemCpy(ptr + (i * mapCapacity), inheritMap + (i * prevCapacity), prevCapacity);
                }

                TypedUnsafe.Dispose(definitionMap, Allocator.Persistent);

            }

            definitionMap = ptr;
            inheritMap = definitionMap + mapCapacity * handledPropertyCount;

        }

        public ElementMap GetDefinitionMap(int propertyIndex) {
            return new ElementMap(definitionMap + (longsPerElementMap * propertyIndex), longsPerElementMap);
        }

        public static PropertySolverContext Create(int handledPropertyCount) {
            return new PropertySolverContext() {
                handledPropertyCount = handledPropertyCount,
                variableResolveList = new DataList<PropertyContainer>(8, Allocator.Persistent),
                listAllocator = TypedUnsafe.Malloc(ListAllocatorSized.CreateAllocator(128, 4096), Allocator.Persistent)
            };
        }

        public void Dispose() {
            listAllocator->Dispose();
            variableResolveList.Dispose();
            TypedUnsafe.Dispose(listAllocator, Allocator.Persistent);
            this = default;
        }

        public ElementMap GetDefinitionMap(PropertyId alignmentBoundaryX) {
            throw new NotImplementedException();
        }

    }

}