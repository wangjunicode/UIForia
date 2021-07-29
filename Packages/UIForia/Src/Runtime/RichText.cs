using System;
using UIForia.Layout;
using UIForia.Prototype;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;
using TextAlignment = UIForia.Text.TextAlignment;

namespace UIForia {

    public unsafe class RichText : IDisposable {

        // todo -- use some trick for regular text not to allocate a list for symbol buffer since theres only one 
        internal DataList<char> dataBuffer;
        internal DataList<TextSymbol> symbolBuffer;
        private int nextHitRegionId;
        internal bool layoutContentChanged;

        public RichText() {
            // todo -- could allocate from a pool of these or something more clever than malloc 
            dataBuffer = new DataList<char>(16, Allocator.Persistent);
            symbolBuffer = new DataList<TextSymbol>(4, Allocator.Persistent);
        }

        ~RichText() {
            dataBuffer.Dispose();
            symbolBuffer.Dispose();
        }

        public void Dispose() {
            dataBuffer.Dispose();
            symbolBuffer.Dispose();
        }

        public void Clear() {
            nextHitRegionId = 0;
            dataBuffer.size = 0;
            symbolBuffer.size = 0;
        }

        public unsafe void EmitTextSymbols(string content) {

            symbolBuffer.Add(new TextSymbol() {
                dataLength = content.Length,
                symbolType = SymbolType.CharacterGroup
            });

            fixed (char* str = content) {
                // dataBuffer.AddRange((byte*)str, content.Length * 2);
            }

        }

        public void Append(string content) {
            if (string.IsNullOrEmpty(content)) return;

            symbolBuffer.Add(new TextSymbol() {
                dataLength = content.Length,
                symbolType = SymbolType.CharacterGroup
            });

            fixed (char* str = content) {
                dataBuffer.AddRange((char*) str, content.Length);
            }

        }

        public void AppendSpaced(string content) {
            if (string.IsNullOrEmpty(content)) return;

            int spaceCount = 0;

            for (int i = symbolBuffer.size - 1; i >= 0; i--) {

                if (symbolBuffer[i].symbolType != SymbolType.CharacterGroup) {
                    continue;
                }

                int offset = 0;

                for (int j = 0; j < i; j++) {
                    offset += symbolBuffer[j].dataLength;
                }

                char last = dataBuffer[offset + symbolBuffer[i].dataLength - 1];

                if (!char.IsWhiteSpace(last)) {
                    spaceCount = 1;
                }

                break;
            }

            symbolBuffer.Add(new TextSymbol() {
                dataLength = content.Length + spaceCount,
                symbolType = SymbolType.CharacterGroup
            });

            if (spaceCount != 0) {
                dataBuffer.Add(' ');
            }

            fixed (char* str = content) {
                dataBuffer.AddRange((char*) str, content.Length);
            }
        }

        // todo -- this should be wrapped in a lookup that resolves the font by name or by reference 
        public void PushFont(FontAssetId fontAssetId) {
            symbolBuffer.Add(new TextSymbol() {
                symbolType = SymbolType.PushFont,
                dataLength = 1
            });
            dataBuffer.Add((char) fontAssetId.id);
        }

        public void PopFont() {
            symbolBuffer.Add(new TextSymbol() {
                symbolType = SymbolType.PopFont,
                dataLength = 0
            });
        }

        public void PushFontSize(float fontSize) {
            symbolBuffer.Add(new TextSymbol(SymbolType.PushFontSize, 2));
            dataBuffer.EnsureAdditionalCapacity(2);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(float*) ptr = fontSize;
            dataBuffer.size += 2;
        }

        public void PopFontSize() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopFontSize, 0));
        }

        public void HorizontalSpace(UISpaceSize space) {
            int size = sizeof(TextSpacer) / 2;
            symbolBuffer.Add(new TextSymbol(SymbolType.HorizontalSpace, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(TextSpacer*) ptr = new TextSpacer() {
                height = 0,
                width = space.fixedValue,
                stretchParts = space.stretch
            };
            dataBuffer.size += size;
        }

        public void PushColor(Color color) {
            int size = sizeof(Color) / 2;
            symbolBuffer.Add(new TextSymbol(SymbolType.PushColor, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(Color*) ptr = color;
            dataBuffer.size += size;
        }

        public void PopColor() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopColor, 0));
        }

        public void PushAlignment(TextAlignment alignment) {
            const int size = 1; 
            symbolBuffer.Add(new TextSymbol(SymbolType.PushAlignment, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(ushort*) ptr = (ushort) alignment;
            dataBuffer.size += size;
        }

        public void PopAlignment() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopAlignment, 0));
        }

        public void PushTransform(TextTransform transform) {
            const int size = 1; 
            symbolBuffer.Add(new TextSymbol(SymbolType.PushTransform, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(ushort*) ptr = (ushort) transform;
            dataBuffer.size += size;
        }
        
        public void PopTransform() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopTransform, 0));
        }

        public void PushWhitespaceMode(WhitespaceMode whitespaceMode) {
            const int size = 1; 
            symbolBuffer.Add(new TextSymbol(SymbolType.PushWhitespaceMode, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(ushort*) ptr = (ushort) whitespaceMode;
            dataBuffer.size += size;
        }
        
        public void PopWhitespaceMode() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopWhitespaceMode, 0));
        }
        
        public void PushHitRegion(out int hitRegionId) {
            hitRegionId = nextHitRegionId++;
            symbolBuffer.Add(new TextSymbol(SymbolType.PushHitRegion, 2));
            dataBuffer.EnsureAdditionalCapacity(2);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(int*) ptr = (int) hitRegionId;
            dataBuffer.size += 2;
        }

        public void PopHitRegion() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopHitRegion, 0));
        }

        public void PushLineInset(float startInset, float endInset = 0) {
            symbolBuffer.Add(new TextSymbol(SymbolType.PushHitRegion, 4));
            dataBuffer.EnsureAdditionalCapacity(4);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(float*) ptr = (float) startInset;
            ptr += 2;
            *(float*) ptr = (float) endInset;
            dataBuffer.size += 4;
        }

        public void PopLineInset() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopLineInsets, 0));
        }

        public void InlineSpace(Size spaceSize, out int spaceId) {
            int size = sizeof(TextSpacer) / 2;
            symbolBuffer.Add(new TextSymbol(SymbolType.InlineSpace, 4));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(TextSpacer*) ptr = new TextSpacer() {
                width = spaceSize.width,
                height = spaceSize.height,
                stretchParts = 0
            };
            spaceId = 0; // todo 
            dataBuffer.size += size;
        }

        public void PushVerticalAlignment(VerticalAlignment vAlign) {
            const int size = 1;
            symbolBuffer.Add(new TextSymbol(SymbolType.PushVerticalAlignment, size));
            dataBuffer.EnsureAdditionalCapacity(size);
            char* ptr = dataBuffer.GetPointer(dataBuffer.size);
            *(VerticalAlignment*) ptr = vAlign;
            dataBuffer.size += size;
        }

        public void PopVerticalAlignment() {
            symbolBuffer.Add(new TextSymbol(SymbolType.PopVerticalAlignment, 0));
        }

        internal struct TextSpacer {

            public float width;
            public float height;
            public int stretchParts;

        }

    }

}