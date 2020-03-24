using System;

namespace UIForia {

    public class ModuleReference {

        private Module module;
        private readonly string alias;
        private readonly Type moduleType;

        internal ModuleReference(Type moduleType, string alias = null) {
            this.moduleType = moduleType;
            this.alias = alias;
        }

        public Type GetModuleType() {
            return moduleType;
        }

        public string GetAlias() {
            return alias ?? module.GetModuleName();
        }

        public void ResolveModule(Module module) {
            this.module = module;
            if (string.IsNullOrEmpty(alias)) {
                // alias = module.GetModuleName();
            }
        }

        public Module GetModuleInstance() {
            return module;
        }

    }

}