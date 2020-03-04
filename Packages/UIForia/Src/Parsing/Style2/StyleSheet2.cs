using System;
using UIForia.Util;
using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Selectors;
using UIForia.Style;

namespace UIForia.Style2 {

    public struct Style {

        public int id;
        public CharSpan name;

        public Style(CharSpan name, int id) {
            this.id = id;
            this.name = name;
        }

    }

    // parser can check xml for for </Style> and create file for it if needed without actually parsing the content

    // parsing != building
    // parsing totally done in parallel because no building is needed
    // building totally done in parallel because all data needed for building is present
    // building probably able to run in jobs
    // if path not found should be reported at parse time
    // if variable or style not found should reported after parse before first build?

    // crunching style sheet -> new implicit sheet = more memory usage (probably configurable)

    // xml style files? xml parser probably needs to either call parser in a 1 off fashion or we parse all xml files, add pseudo style files, parse those too

    // 2nd pass to build style parse dependencies? or encounter as needed?

    public class StyleSheet2 {

        public readonly Module module;
        public readonly string filePath;

        internal StructList<Style> styles;
        internal StructList<Mixin> mixins;
        internal StructList<Selector> selectors;
        internal StructList<StyleBodyPart> parts;
        internal StructList<RunCommand> commands;
        internal StructList<PendingConstant> constants;

        internal StructList<StyleProperty2> properties;
        // animations
        // spritesheets
        // sounds
        // cursors

        internal StyleSheet2(Module module, string filePath) {
            this.module = module;
            this.filePath = filePath;
            this.parts = new StructList<StyleBodyPart>(128);
        }

        public RuntimeStyleSheet Build(DisplayConfiguration configuration) {
            // import all references -> need to build if not built already
            // resolve all constants 
            // resolve all mixins
            // resolve all base classes 
            // resolve all animations
            // resolve all sounds
            // resolve all cursors
            // resolve all selectors
            // resolve all style groups

            return default;
        }

        public Style GetStyle(string name) {
            return default;
        }

        public string GetConstant(string s, IList<bool> results = null) {
            if (results == null) {
                for (int i = 0; i < constants.size; i++) {
                    PendingConstant constant = constants.array[i];
                    if (constant.name == s) {
                        if (constant.conditions == null) {
                            return constant.defaultValue.ToString();
                        }
                    }
                }

                return null;
            }

            for (int i = 0; i < constants.size; i++) {
                PendingConstant constant = constants.array[i];
                if (constant.name == s) {
                    if (constant.conditions == null) {
                        return constant.defaultValue.ToString();
                    }
                    else {
                        for (int j = 0; j < constant.conditions.Length; j++) {
                            if (results[constant.conditions[j].conditionId]) {
                                return constant.conditions[j].value.ToString();
                            }
                        }

                        return constant.defaultValue.ToString();
                    }
                }
            }

            return null;
        }


        internal bool AddConstant(in PendingConstant constant) {
            constants = constants ?? new StructList<PendingConstant>();
            for (int i = 0; i < constants.size; i++) {
                if (constants.array[i].name == constant.name) {
                    return false;
                }
            }

            constants.Add(constant);
            return true;
        }

        // same as style but data wont contain substates
        private struct _Mixin {

            

        }

        // seperate list?
        private struct _Selector { }

        private struct _Style {

            public int baseId;
            public int partStart;
            public int partCount;
            public int normalStart;
            public int hoverStart;
            public int activeStart;
            public int focusStart;
            // commands & selectors? reverse map?
            public CharSpan name;

        }

        internal void Build() {
            
            for (int i = 0; i < parts.size; i++) {
                
                ref StyleBodyPart part = ref parts.array[i];
                
                switch (part.type) {

                    case BodyPartType.Property:
                        break;

                    case BodyPartType.RunCommand:
                        break;

                    case BodyPartType.Selector:
                        break;

                    case BodyPartType.Mixin:
                        // ResolveMixin(id, data); resolve circular
                        // foreach property in mixin 
                        break;

                    case BodyPartType.ConditionPush:
                        // if condition failed
                        // continue until matching condition pop
                        // if !conditionValid -> throw
                        break;

                    case BodyPartType.ConditionPop:
                        break;

                    case BodyPartType.BeginStyle:
                        break;
                    
                    case BodyPartType.ExtendBaseStyle:
                        // resolve style, check for circular dep
                        // if already built and local, use it
                        // if already built and not local, use it
                        // add / upsert properties to each state
                        // add selectors
                        // add commands
                        break;
                    
                    case BodyPartType.EndStyle:
                        break;
                    
                    case BodyPartType.StyleStatePop:
                        // set current array to normal style array if not end
                        break;

                    case BodyPartType.StyleStatePush:
                       break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }

        internal void BeginCondition(int conditionId) {
            parts.Add(new StyleBodyPart(BodyPartType.ConditionPush, conditionId));
        }

        internal void EndCondition() {
            parts.Add(new StyleBodyPart(BodyPartType.ConditionPop, -1));
        }

        internal int AddStyle(CharSpan name) {
            styles = styles ?? new StructList<Style>();
            for (int i = 0; i < styles.size; i++) {
                if (styles[i].name == name) {
                    throw new ParseException("Style " + name + " was already declared in " + filePath + ". You have redefined it on line " + name.GetLineNumber());
                }
            }


            int retn = styles.size;
            styles.Add(new Style(name, retn));
            parts.Add(new StyleBodyPart(BodyPartType.StyleStatePush, retn));
            return retn;
        }

        internal void BeginStyleBody(int styleId) { }

        internal void EndStyleBody() {
            parts.Add(new StyleBodyPart(BodyPartType.StyleStatePop, -1));
        }

        internal void AddProperty(in StyleProperty2 property) {
            parts.Add(new StyleBodyPart(BodyPartType.Property, properties.Count));
            properties = properties ?? new StructList<StyleProperty2>(64);
            properties.Add(property);
        }

        internal void AddRunCommand(in RunCommand cmd) {
            throw new NotImplementedException();
        }

        internal void ApplyMixin(int mixinId, int mixinData = -1) {
            parts.Add(new StyleBodyPart(BodyPartType.Mixin, mixinId, mixinData));
        }

        // internal StructList<Style> styles;
        // internal StructList<Mixin> mixins;
        // internal StructList<Selector> selectors;
        // internal StructList<RunCommand> commands;
        // internal StructList<PendingConstant> constants;
        // internal StructList<StyleProperty2> properties;
        internal struct StyleBodyPart {

            public readonly int id;
            public readonly int dataId;
            public readonly BodyPartType type;

            public StyleBodyPart(BodyPartType type, int id, int dataId = -1) {
                this.id = id;
                this.type = type;
                this.dataId = dataId;
            }

        }

        internal enum BodyPartType {

            Property,
            RunCommand,
            Selector,
            Mixin,
            ConditionPush,
            ConditionPop,

            StyleStatePop,

            StyleStatePush,

            EndStyle,

            BeginStyle,

            ExtendBaseStyle

        }

    }

}