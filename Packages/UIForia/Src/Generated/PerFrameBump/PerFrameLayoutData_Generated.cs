namespace UIForia {

    internal unsafe partial struct PerFrameLayoutData {
        
        partial void AllocatePerFrameBump(LockedBumpAllocator* allocator, int count) {

            emTable = allocator->Allocate<float>(count);
            lineHeightTable = allocator->Allocate<float>(count);
            solvedWidths = allocator->Allocate<UIForia.Layout.SolvedSize>(count);
            solvedHeights = allocator->Allocate<UIForia.Layout.SolvedSize>(count);
            minWidths = allocator->Allocate<UIForia.Layout.SolvedConstraint>(count);
            maxWidths = allocator->Allocate<UIForia.Layout.SolvedConstraint>(count);
            minHeights = allocator->Allocate<UIForia.Layout.SolvedConstraint>(count);
            maxHeights = allocator->Allocate<UIForia.Layout.SolvedConstraint>(count);
            borderTops = allocator->Allocate<float>(count);
            borderRights = allocator->Allocate<float>(count);
            borderBottoms = allocator->Allocate<float>(count);
            borderLefts = allocator->Allocate<float>(count);
            marginTops = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            marginRights = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            marginBottoms = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            marginLefts = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            paddingTops = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            paddingRights = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            paddingBottoms = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            paddingLefts = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            spaceBetweenHorizontal = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            spaceBetweenVertical = allocator->Allocate<UIForia.Layout.ResolvedSpacerSize>(count);
            spaceCollapseHorizontal = allocator->Allocate<UIForia.SpaceCollapse>(count);
            spaceCollapseVertical = allocator->Allocate<UIForia.SpaceCollapse>(count);

        }
    }
}