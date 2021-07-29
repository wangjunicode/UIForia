using System;
using UIForia.Util.Unsafe;

namespace UIForia.Style {

    // todo -- just generate these 

    internal unsafe partial struct PropertySolverGroup_TextMeasurement : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_LayoutBehaviorTypeFontSize : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_HorizontalSpacing : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_VerticalSpacing : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_LayoutSizes : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_LayoutSetup : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_Rendering : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_LayoutAndText : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

    }

    internal unsafe partial struct PropertySolverGroup_ClippingAndTransformation : IDisposable {

        public PropertySolverContext context;

        public void Initialize() {
            int propertyCount = 0;
            InitializeSolvers();
            GetPropertyCount(ref propertyCount);
            context = PropertySolverContext.Create(propertyCount);
        }

        public void Dispose() {
            context.Dispose();
        }

        public void Invoke(SolverParameters solverParameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator) {
            SolveProperties(solverParameters, styleTables, propertyTables, bumpAllocator);
        }

        partial void InitializeSolvers();

        partial void GetPropertyCount(ref int propertyCount);

        partial void SolveProperties(in SolverParameters parameters, StyleTables* styleTables, PropertyTables* propertyTables, BumpAllocator* bumpAllocator);

        partial void GetLocalPropertyIndex(int propertyId, ref int outputIdx);

        public ElementMap GetDefinitionMap(PropertyId propertyId) {
            int outputIdx = -1;
            
            GetLocalPropertyIndex(propertyId.index, ref outputIdx);
            
            if (outputIdx == -1) {
                return default;
            }

            return context.GetDefinitionMap(outputIdx);
        }

    }

}