using System;
using UIForia.Parsing;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Style {

    internal struct ResolvedImport {

        public StyleFileShell shell;
        public RangeInt alias;

    }

    internal unsafe class StyleFileShell {

        public string filePath;
        public DateTime lastWriteTime;
        public bool checkedTimestamp;
        public bool hasParseErrors;
        public bool isCompiled;
        public bool isValid;

        public ImportNode[] imports;
        public StyleNode[] styles;
        public ParseBlockNode[] blocks;
        public StyleDeclaration[] propertyDefinitions;
        public ConstantNode[] constants;
        public MixinNode[] mixins;
        public MixinVariable[] mixinVariables;
        public TransitionDeclaration[] transitions;
        public AnimationNode[] animations;
        public CustomPropertyDefinition[] customProperties;
        
        public char* charBuffer;
        internal int charBufferSize;

        public ResolvedImport[] resolvedImports;
        private SizedArray<StringLookup> resolvedStrings;
        public UIModule module;

        internal StyleFileShell(string locationFilePath) {
            this.filePath = locationFilePath;
        }

        ~StyleFileShell() {
            // todo -- when created via parse cache we might not have individual allocations for char data
            TypedUnsafe.Dispose(charBuffer, Allocator.Persistent);
        }

        public void Serialize(ref ManagedByteBuffer buffer) {
            throw new NotImplementedException();
        }

        public void Deserialize(ref ManagedByteBuffer byteBuffer) {
            throw new NotImplementedException();
        }

        public bool TryGetLocalConstant(CharSpan identifier, out int idx) {
            if (charBuffer == null || charBufferSize <= 0) {
                idx = -1;
                return false;
            }

            for (int i = 0; i < constants.Length; i++) {
                RangeInt range = constants[i].identifier;
                CharSpan constantName = new CharSpan(charBuffer, range.start, range.end);
                if (constantName == identifier) {
                    idx = i;
                    return true;
                }
            }

            idx = -1;
            return false;
        }

        public bool TryResolveAlias(CharSpan aliasName, out StyleFileShell alias) {

            if (resolvedImports == null) {
                alias = null;
                return false;
            }

            for (int i = 0; i < resolvedImports.Length; i++) {
                RangeInt range = resolvedImports[i].alias;
                CharSpan nameSpan = new CharSpan(charBuffer, range.start, range.end);
                if (nameSpan == aliasName) {
                    alias = resolvedImports[i].shell;
                    return true;
                }
            }

            alias = null;
            return false;
        }

        internal string GetString(RangeInt charRange) {
            if (charRange.length == 0) return null;
            for (int i = 0; i < resolvedStrings.size; i++) {
                RangeInt checkRange = resolvedStrings.array[i].range;
                if (checkRange.start == charRange.start && checkRange.length == charRange.length) {
                    return resolvedStrings.array[i].stringVal;
                }
            }

            string s = new string(charBuffer, charRange.start, charRange.length);

            resolvedStrings.Add(new StringLookup() {
                range = charRange,
                stringVal = s
            });

            return s;
        }

        private struct StringLookup {

            public RangeInt range;
            public string stringVal;

        }

        public CharSpan GetCharSpan(int start, int end) {
            return new CharSpan(charBuffer, start, end);
        }

        public CharSpan GetCharSpan(RangeInt range) {
            return new CharSpan(charBuffer, range.start, range.end);
        }

        public bool TryGetMixin(CharSpan target, out MixinNode mixinNode) {
            for (int i = 0; i < mixins.Length; i++) {
                CharSpan mixinName = GetCharSpan(mixins[i].nameRange);
                if (mixinName == target) {
                    mixinNode = mixins[i];
                    return true;
                }
            }

            mixinNode = default;
            return false;
        }

        public bool TryGetAnimation(CharSpan target, out AnimationNode animationNode) {
            for (int i = 0; i < animations.Length; i++) {
                CharSpan animationName = GetCharSpan(animations[i].nameRange);
                if (animationName == target) {
                    animationNode = animations[i];
                    return true;
                }
            }

            animationNode = default;
            return false;
        }

        public string ProjectRelativePath() {
            return PathUtil.ProjectRelativePath(filePath);
        }

    }

}