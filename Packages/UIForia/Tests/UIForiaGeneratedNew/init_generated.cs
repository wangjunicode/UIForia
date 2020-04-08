using UIForia.Compilers;
using System;
using System.Collections.Generic;

namespace UIForia.Generated {

    public partial class Generated_TestApp : TemplateLoader {

        public Generated_TestApp() {
            templateData = new[] {
                template_d4b877d9_9092_4451_b909_208a4e34ccd4,
                template_15d71eda_bd8c_494d_b0a2_9905fbcc3584
            };

            templateDataMap = new Dictionary<Type, TemplateData>() {
                { typeof(TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate), template_d4b877d9_9092_4451_b909_208a4e34ccd4},
                { typeof(TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate), template_15d71eda_bd8c_494d_b0a2_9905fbcc3584}
            };
        }

    }

}