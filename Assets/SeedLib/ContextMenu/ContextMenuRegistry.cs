using System;
using System.Collections.Generic;

namespace SeedLib {

    public class ContextMenuRegistry {

        private readonly List<Entry> builders;
        private ContextMenuTree activeMenuTree;

        public ContextMenuRegistry() {
            builders = new List<Entry>();
        }

        public void Register<T>(string contextMenuId, Action<T, ContextMenuBuilder> buildFn) where T : class {
            // todo -- make sure we aren't double registering
            builders.Add(new Entry<T>(contextMenuId, buildFn));
        }

        public void Show<T>(string menuId, float x, float y, T data = default) where T : class {
            
            activeMenuTree = null;
            
            for (int i = 0; i < builders.Count; i++) {
                if (builders[i].id == menuId && builders[i].type == typeof(T)) {
                    activeMenuTree = builders[i].Build(data);
                    return;
                }
            }

            throw new Exception($"Cannot find context menu with id `{menuId}` with type matching {typeof(T)}");
        }

        private abstract class Entry {

            public string id;
            public Type type;

            public abstract ContextMenuTree Build(object data);

        }

        private class Entry<T> : Entry where T : class {

            public Action<T, ContextMenuBuilder> builder;

            public Entry(string id, Action<T, ContextMenuBuilder> builder) {
                this.id = id;
                this.type = typeof(T);
                this.builder = builder;
            }

            public override ContextMenuTree Build(object data) {
                T castData = data as T;
                ContextMenuTree retn = new ContextMenuTree();
                builder.Invoke(castData, new ContextMenuBuilder(retn));
                return retn;
            }

        }

        public bool HasActiveMenu() {
            return activeMenuTree != null;
        }

        public string GetMenuHeader() {
            return activeMenuTree?.header;
        }

        public IList<IList<ContextMenuItemData>> GetMenuGroups() {
            return activeMenuTree?.groups;
        }
        
    }

}