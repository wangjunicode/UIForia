using System;
using System.Collections.Generic;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UIForia {

    public sealed class ResourceManager : IDisposable {

        internal struct AssetEntry<T> where T : Object {

            public T asset;
            public int id;
            public int linkedId;

        }

        internal readonly IntMap_Deprecated<AssetEntry<Texture2D>> textureMap;
        internal readonly IntMap_Deprecated<AssetEntry<SpriteAtlas>> spriteAtlasMap;
        internal readonly Dictionary<string, FontAsset> fontMap;
        internal readonly IntMap_Deprecated<AssetEntry<AudioClip>> audioMap;
        internal readonly Dictionary<string, StylePainterDefinition> stylePainters;

        public ResourceManager() {
            stylePainters = new Dictionary<string, StylePainterDefinition>();
            textureMap = new IntMap_Deprecated<AssetEntry<Texture2D>>();
            spriteAtlasMap = new IntMap_Deprecated<AssetEntry<SpriteAtlas>>();
            fontMap = new Dictionary<string, FontAsset>();
            audioMap = new IntMap_Deprecated<AssetEntry<AudioClip>>();
        }

        public void Reset() {
            textureMap.Clear();
            spriteAtlasMap.Clear();
            fontMap.Clear();
            audioMap.Clear();
            stylePainters.Clear();
        }

        public Texture2D AddTexture(string path, Texture2D texture) {
            return AddResource(path, texture, textureMap);
        }

        public Texture2D AddTexture(Texture2D texture) {
            return AddResource(texture, textureMap);
        }

        public AudioClip AddAudioClip(AudioClip clip) {
            return AddResource(clip, audioMap);
        }

        public Texture2D GetTexture(string path) {
            return GetResource(path, textureMap);
        }

        internal DataList<FontAssetInfo>.Shared fontAssetMap;
        
        private static readonly PainterVariableDeclaration[] s_EmptyVariables = {};

        internal void Initialize() {
            fontAssetMap = new DataList<FontAssetInfo>.Shared(16, Allocator.Persistent);
            FontAsset font = FontAsset.defaultFontAsset;
            font.id = 0;
            fontAssetMap.Add(font.GetFontInfo());
        }

        public FontAsset GetFont(string path, bool tryReloading = false) {
            // will be present & null if loaded but not resolved
            if (fontMap.TryGetValue(path, out FontAsset fontAsset)) {
                if (fontAsset != null) {
                    return fontAsset;
                }

                if (!tryReloading) {
                    return null;
                }
            }

            FontAsset retn = Resources.Load<FontAsset>(path);

            if (retn == null) {
                fontMap.Add(path, null);
                return null;
            }

            retn.id = fontAssetMap.size;
            fontAssetMap.Add(retn.GetFontInfo());
            fontMap.Add(path, retn);
            retn.id = fontMap.Count;
            return retn;
        }

        public AudioClip GetAudioClip(string path) {
            return GetResource(path, audioMap);
        }

        private T AddResource<T>(string path, T resource, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            if (resource == null || path == null) {
                return null;
            }

            int pathId = path.GetHashCode();
            int id = resource.GetHashCode();

            AssetEntry<T> pathEntry;
            AssetEntry<T> idEntry;

            if (map.TryGetValue(pathId, out pathEntry)) {
                return resource;
            }

            idEntry.id = id;
            idEntry.linkedId = pathId;
            idEntry.asset = resource;
            pathEntry.id = id;
            pathEntry.linkedId = id;
            pathEntry.asset = resource;
            map.Add(pathId, pathEntry);
            map.Add(id, idEntry);
            return resource;
        }

        private T AddResource<T>(T resource, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                return resource;
            }

            entry.id = id;
            entry.linkedId = -1;
            entry.asset = resource;
            map.Add(id, entry);
            return resource;
        }

        private T GetResource<T>(int id, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            AssetEntry<T> entry;
            map.TryGetValue(id, out entry);
            return entry.asset;
        }

        private T GetResource<T>(string path, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            T resource;
            if (path == null) {
                return null;
            }

            AssetEntry<T> pathEntry;
            int pathId = path.GetHashCode();
            if (map.TryGetValue(pathId, out pathEntry)) {
                return pathEntry.asset;
            }
            else {
                // this might be null, but we want to mark the map to show that we tried to load it
                // during the lifecycle of an application we can expect Resources not to be updated
                resource = Resources.Load<T>(path);
                pathEntry.id = pathId;
                pathEntry.asset = resource;
                pathEntry.linkedId = -1;
                if (resource != null) {
                    // see if we already have it loaded by id and update linkedId accordingly
                    int resourceId = resource.GetHashCode();
                    AssetEntry<T> idEntry;
                    idEntry.id = resourceId;
                    idEntry.linkedId = pathId;
                    idEntry.asset = resource;
                    map[idEntry.id] = idEntry;

                    pathEntry.linkedId = idEntry.id;
                }

                map.Add(pathId, pathEntry);
            }

            return resource;
        }

        private void RemoveResource<T>(T resource, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            if (resource == null) return;
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                map.Remove(entry.linkedId);
                map.Remove(id);
            }
        }

        private void RemoveResource<T>(string path, IntMap_Deprecated<AssetEntry<T>> map) where T : Object {
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            int pathId = path.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(pathId, out entry)) {
                map.Remove(entry.linkedId);
                map.Remove(pathId);
            }
        }

        public void Dispose() {
            fontAssetMap.Dispose();
        }

        public void AddStylePainter(string painterName, StylePainterDefinition stylePainterDefinition) {
            if (Application.HasCustomPainter(painterName)) {
                throw new CompileException("Painter with name: " + painterName + " was already registered");
            }

            if (stylePainters.TryGetValue(painterName, out StylePainterDefinition existing)) {
                throw new CompileException("Painter with name " + painterName + " was already registered from " + existing.fileName);
            }

            stylePainterDefinition.definedVariables = stylePainterDefinition.definedVariables ?? s_EmptyVariables;
            stylePainters.Add(painterName, stylePainterDefinition);

        }

        public bool TryGetStylePainter(string customPainter, out StylePainterDefinition painterDefinition) {
            return stylePainters.TryGetValue(customPainter, out painterDefinition);
        }

    }

}