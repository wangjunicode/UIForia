
using UIForia.Util.Unsafe;

namespace UIForia.Style {
    internal unsafe partial struct CustomPropertyInfo {
        
        partial void CreateSolverGenerated(byte * memory) { 
            switch(propertyType) {
                case PropertyType.float2: {
                    CustomPropertySolver_float2* pSolver = (CustomPropertySolver_float2*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.Single: {
                    CustomPropertySolver_Single* pSolver = (CustomPropertySolver_Single*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.half: {
                    CustomPropertySolver_half* pSolver = (CustomPropertySolver_half*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.Byte: {
                    CustomPropertySolver_Byte* pSolver = (CustomPropertySolver_Byte*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.UInt16: {
                    CustomPropertySolver_UInt16* pSolver = (CustomPropertySolver_UInt16*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.Int32: {
                    CustomPropertySolver_Int32* pSolver = (CustomPropertySolver_Int32*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.TextureInfo: {
                    CustomPropertySolver_TextureInfo* pSolver = (CustomPropertySolver_TextureInfo*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.UIColor: {
                    CustomPropertySolver_UIColor* pSolver = (CustomPropertySolver_UIColor*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }
                case PropertyType.Enum: {
                    CustomPropertySolver_EnumValue* pSolver = (CustomPropertySolver_EnumValue*) memory;
                    *pSolver = default;
                    pSolver->Initialize();
                    solver = pSolver;
                    break;
                }

            }
        }

        partial void SolveGenerated(ref SolverParameters parameters, ref PropertyTables propertyTables, ref CustomSolverInfo sharedSolverInfo, BumpAllocator* bumpAllocator) { 
            switch(propertyType) {
                case PropertyType.float2: {
                    CustomPropertySolver_float2* pSolver = (CustomPropertySolver_float2*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.Single: {
                    CustomPropertySolver_Single* pSolver = (CustomPropertySolver_Single*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.half: {
                    CustomPropertySolver_half* pSolver = (CustomPropertySolver_half*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.Byte: {
                    CustomPropertySolver_Byte* pSolver = (CustomPropertySolver_Byte*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.UInt16: {
                    CustomPropertySolver_UInt16* pSolver = (CustomPropertySolver_UInt16*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.Int32: {
                    CustomPropertySolver_Int32* pSolver = (CustomPropertySolver_Int32*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.TextureInfo: {
                    CustomPropertySolver_TextureInfo* pSolver = (CustomPropertySolver_TextureInfo*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.UIColor: {
                    CustomPropertySolver_UIColor* pSolver = (CustomPropertySolver_UIColor*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }
                case PropertyType.Enum: {
                    CustomPropertySolver_EnumValue* pSolver = (CustomPropertySolver_EnumValue*) solver;
                    pSolver->Solve(propertyId, ref parameters, ref propertyTables, ref sharedSolverInfo, bumpAllocator);
                    break;
                }

            }
        }

        partial void GetSizeGenerated(ref int size) { 
            switch(propertyType) {
                case PropertyType.float2:
                    size = sizeof(CustomPropertySolver_float2);
                    break;
                case PropertyType.Single:
                    size = sizeof(CustomPropertySolver_Single);
                    break;
                case PropertyType.half:
                    size = sizeof(CustomPropertySolver_half);
                    break;
                case PropertyType.Byte:
                    size = sizeof(CustomPropertySolver_Byte);
                    break;
                case PropertyType.UInt16:
                    size = sizeof(CustomPropertySolver_UInt16);
                    break;
                case PropertyType.Int32:
                    size = sizeof(CustomPropertySolver_Int32);
                    break;
                case PropertyType.TextureInfo:
                    size = sizeof(CustomPropertySolver_TextureInfo);
                    break;
                case PropertyType.UIColor:
                    size = sizeof(CustomPropertySolver_UIColor);
                    break;
                case PropertyType.Enum:
                    size = sizeof(CustomPropertySolver_EnumValue);
                    break;

            }
        }

        partial void DisposeGenerated() {
            switch(propertyType) {
                case PropertyType.float2: {
                    CustomPropertySolver_float2* pSolver = (CustomPropertySolver_float2*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.Single: {
                    CustomPropertySolver_Single* pSolver = (CustomPropertySolver_Single*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.half: {
                    CustomPropertySolver_half* pSolver = (CustomPropertySolver_half*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.Byte: {
                    CustomPropertySolver_Byte* pSolver = (CustomPropertySolver_Byte*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.UInt16: {
                    CustomPropertySolver_UInt16* pSolver = (CustomPropertySolver_UInt16*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.Int32: {
                    CustomPropertySolver_Int32* pSolver = (CustomPropertySolver_Int32*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.TextureInfo: {
                    CustomPropertySolver_TextureInfo* pSolver = (CustomPropertySolver_TextureInfo*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.UIColor: {
                    CustomPropertySolver_UIColor* pSolver = (CustomPropertySolver_UIColor*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
                case PropertyType.Enum: {
                    CustomPropertySolver_EnumValue* pSolver = (CustomPropertySolver_EnumValue*) solver;
                    pSolver->Dispose();
                    *pSolver = default;
                    solver = default;
                    break;
                }
        
            }
        }

    }
}
