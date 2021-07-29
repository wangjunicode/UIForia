using UIForia.Layout;

namespace UIForia {

    [PerFrameBumpAllocate]
    internal unsafe partial struct PerFrameLayoutData {

        [PerFrameBumpAllocate] public float* emTable;
        [PerFrameBumpAllocate] public float* lineHeightTable;
        [PerFrameBumpAllocate] public SolvedSize* solvedWidths;
        [PerFrameBumpAllocate] public SolvedSize* solvedHeights;

        [PerFrameBumpAllocate] public SolvedConstraint* minWidths;
        [PerFrameBumpAllocate] public SolvedConstraint* maxWidths;
        [PerFrameBumpAllocate] public SolvedConstraint* minHeights;
        [PerFrameBumpAllocate] public SolvedConstraint* maxHeights;

        [PerFrameBumpAllocate] public float* borderTops;
        [PerFrameBumpAllocate] public float* borderRights;
        [PerFrameBumpAllocate] public float* borderBottoms;
        [PerFrameBumpAllocate] public float* borderLefts;

        [PerFrameBumpAllocate] public ResolvedSpacerSize* marginTops;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* marginRights;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* marginBottoms;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* marginLefts;

        [PerFrameBumpAllocate] public ResolvedSpacerSize* paddingTops;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* paddingRights;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* paddingBottoms;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* paddingLefts;

        // todo -- I might not need to persist these 
        [PerFrameBumpAllocate] public ResolvedSpacerSize* spaceBetweenHorizontal;
        [PerFrameBumpAllocate] public ResolvedSpacerSize* spaceBetweenVertical;
        [PerFrameBumpAllocate] public SpaceCollapse* spaceCollapseHorizontal;
        [PerFrameBumpAllocate] public SpaceCollapse* spaceCollapseVertical;

        public void Allocate(LockedBumpAllocator* allocator, int count) {
            AllocatePerFrameBump(allocator, count);
        }

        partial void AllocatePerFrameBump(LockedBumpAllocator* allocator, int count);

    }

}