using UIForia.Util.Unsafe;

namespace UIForia.Style {

    internal unsafe interface ICustomPropertySolver {

        void Initialize();

        void Solve(PropertyId propertyId, ref SolverParameters parameters, ref PropertyTables propertyTables, ref CustomSolverInfo sharedSolverInfo, BumpAllocator* tempAllocator);

        void Dispose();

    }

}