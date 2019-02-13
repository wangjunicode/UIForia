using TMPro;
using UnityEngine;

namespace UIForia {

    public static class ResourceManager {

        private struct AssetEntry<T> where T : UnityEngine.Object {

            public T asset;
            public int id;
            public int linkedId;

        }

        // todo -- add cursors / animations / maybe style sheets
        private static readonly IntMap<AssetEntry<Texture2D>> s_TextureMap;
        private static readonly IntMap<AssetEntry<TMP_FontAsset>> s_FontMap;
        private static readonly IntMap<AssetEntry<AudioClip>> s_AudioMap;

        static ResourceManager() {
            s_TextureMap = new IntMap<AssetEntry<Texture2D>>();
            s_FontMap = new IntMap<AssetEntry<TMP_FontAsset>>();
            s_AudioMap = new IntMap<AssetEntry<AudioClip>>();
        }

        public static void Reset() {
            s_TextureMap.Clear();
            s_FontMap.Clear();
            s_AudioMap.Clear();
        }
        
        public static Texture2D AddTexture(string path, Texture2D texture) {
            return AddResource(path, texture, s_TextureMap);
        }
        
        public static Texture2D AddTexture(Texture2D texture) {
            return AddResource(texture, s_TextureMap);
        }

        public static TMP_FontAsset AddFont(TMP_FontAsset font) {
            return AddResource(font, s_FontMap);
        }
        
        public static TMP_FontAsset AddFont(string path, TMP_FontAsset font) {
            return AddResource(path, font, s_FontMap);
        }

        public static AudioClip AddAudioClip(AudioClip clip) {
            return AddResource(clip, s_AudioMap);
        }

        public static Texture2D GetTexture(string path) {
            return GetResource(path, s_TextureMap);
        }

        public static TMP_FontAsset GetFont(string path) {
            return GetResource(path, s_FontMap);
        }

        public static AudioClip GetAudioClip(string path) {
            return GetResource(path, s_AudioMap);
        }
        
        public static void RemoveTexture(string path) {
            RemoveResource(path, s_TextureMap);
        }

        public static void RemoveTexture(Texture2D texture) {
            s_TextureMap.Remove(texture.GetHashCode());
        }

        public static void RemoveFont(string path) {
            RemoveResource(path, s_FontMap);
        }

        public static void RemoveFont(TMP_FontAsset font) {
            RemoveResource(font, s_FontMap);
        }

        public static void RemoveAudioClip(AudioClip audioClip) {
            RemoveResource(audioClip, s_AudioMap);
        }

        public static void RemoveAudioClip(string path) {
            RemoveResource(path, s_AudioMap);
        }
  
        //todo
//        public static Texture2D GetEditorTexture() {
//            // load using EditorGUIUtility.Load()
//            // hook into AssetModificationProcessor to watch for changes
//        }

        private static T AddResource<T>(string path, T resource, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
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
        
        private static T AddResource<T>(T resource, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
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

        private static T GetResource<T>(int id, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
            AssetEntry<T> entry;
            map.TryGetValue(id, out entry);
            return entry.asset;
        }

        private static T GetResource<T>(string path, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
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

        private static void RemoveResource<T>(T resource, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
            if (resource == null) return;
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                map.Remove(entry.linkedId);
                map.Remove(id);
            }
        }

        private static void RemoveResource<T>(string path, IntMap<AssetEntry<T>> map) where T : UnityEngine.Object {
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

    }

}