using UIForia.Compilers;
using System;
using System.Collections.Generic;
// ReSharper disable PossibleNullReferenceException

namespace UIForia.Generated {

    public partial class Generated_TestApp : TemplateLoader {

        public Generated_TestApp() {
            templateData = new[] {
                template_28f7eb09_98c3_46bf_82b6_1bce7cdb8b64,
                template_8fac8c10_45fd_444e_bf24_a63d3682813a
            };

            templateDataMap = new Dictionary<Type, TemplateData>() {
                { typeof(TemplateCompiler2Test.TestTemplateCompiler2.SimpleTemplate), template_28f7eb09_98c3_46bf_82b6_1bce7cdb8b64},
                { typeof(TemplateCompiler2Test.TestTemplateCompiler2.ExpandedTemplate), template_8fac8c10_45fd_444e_bf24_a63d3682813a}
            };
        }

    }

}