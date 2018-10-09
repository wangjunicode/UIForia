using System;
using Src.Systems;

namespace Tests.Mocks {

    public class MockView : UIView {

        public readonly ILayoutSystem layoutSystem;
        public readonly MockInputSystem inputSystem;
        
        public MockView(Type elementType, string template = null) : base(elementType, template) {
            layoutSystem = new MockLayoutSystem(styleSystem);
            inputSystem = new MockInputSystem(layoutSystem, styleSystem);
            systems.Add(layoutSystem);
            systems.Add(inputSystem);
        }

    }


}