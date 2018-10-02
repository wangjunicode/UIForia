using System;
using Src.Systems;

namespace Tests.Mocks {

    public class MockView : UIView {

        public readonly ILayoutSystem layoutSystem;
        public readonly MockTextSizeCalculator textSizeCalculator;
        public readonly MockInputSystem inputSystem;
        
        public MockView(Type elementType, string template = null) : base(elementType, template) {
            textSizeCalculator = new MockTextSizeCalculator();
            layoutSystem = new MockLayoutSystem(textSizeCalculator, styleSystem);
            inputSystem = new MockInputSystem(layoutSystem, styleSystem);
            systems.Add(layoutSystem);
            systems.Add(inputSystem);
        }

    }


}