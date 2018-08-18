using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Src {

    public struct ListContextDefinition {

        public readonly Type type;
        public readonly string itemAlias;
        public readonly string indexAlias;
        public readonly string lengthAlias;

        public ListContextDefinition(Type type, string itemAlias, string indexAlias, string lengthAlias) {
            this.type = type;
            this.itemAlias = itemAlias;
            this.indexAlias = indexAlias;
            this.lengthAlias = lengthAlias;
        }

    }

    public class ContextDefinition {

        private const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private List<Alias> aliases;
        public readonly ProcessedType processedType;
        public List<ListContextDefinition> listContexts;

        public ContextDefinition(Type type) {
            this.aliases = new List<Alias>();
            this.processedType = TypeProcessor.GetType(type);
            this.listContexts = new List<ListContextDefinition>();
        }

        public Type rootContextType => processedType.rawType;

        public void SetAliasToType(string alias, Type type) {

            for (int i = 0; i < aliases.Count; i++) {
                if (aliases[i].name == alias) {
                    aliases[i] = new Alias(alias, type);
                    return;
                }
            }
            aliases.Add(new Alias(alias, type));
        }

        public void RemoveAlias(string alias) {
            for (int i = 0; i < aliases.Count; i++) {
                if (aliases[i].name == alias) {
                    aliases.RemoveAt(i);
                    return;
                }
            }
        }

        public void AddListContext(Type listItemType, string itemAlias, string indexAlias, string lengthAlias) {
            if (aliases.Any((alias) => alias.name == itemAlias)) {
                throw new Exception($"Item alias {itemAlias} already used in scope. Find another name and assign via the <Repeat as='yourAlias'>");
            }
            else if (aliases.Any((alias) => alias.name == indexAlias)) {
                throw new Exception($"Index alias {indexAlias} already used in scope. Find another name and assign via the <Repeat indexAlias='yourAlias'>");
            }
            else if (aliases.Any((alias) => alias.name == lengthAlias)) {
                throw new Exception($"Length alias {lengthAlias} already used in scope. Find another name and assign via the <Repeat lengthAlias='yourAlias'>");
            }

            aliases.Add(new Alias(itemAlias, listItemType));
            aliases.Add(new Alias(indexAlias, typeof(int)));
            aliases.Add(new Alias(lengthAlias, typeof(int)));

            listContexts.Add(new ListContextDefinition(listItemType, itemAlias, indexAlias, lengthAlias));
        }

        public void RemoveListContext(string aliasName) {
            for (int i = 0; i < listContexts.Count; i++) {
                if (listContexts[i].itemAlias == aliasName) {
                    ListContextDefinition listContextDefinition = listContexts[i];
                    listContexts.RemoveAt(i);
                    aliases.RemoveAt(aliases.FindIndex((alias) => alias.name == listContextDefinition.itemAlias));
                    aliases.RemoveAt(aliases.FindIndex((alias) => alias.name == listContextDefinition.indexAlias));
                    aliases.RemoveAt(aliases.FindIndex((alias) => alias.name == listContextDefinition.lengthAlias));
                    return;
                }
            }
        }

        public Type ResolveType(string alias) {
            for (int i = 0; i < aliases.Count; i++) {
                if (aliases[i].name == alias) {
                    return aliases[i].type;
                }
            }

            FieldInfo fieldInfo = rootContextType.GetField(alias, bindFlags);
            
            if (fieldInfo != null) {
                return fieldInfo.FieldType;
            }
            
            return null;
        }

        private struct Alias {

            public readonly Type type;
            public readonly string name;

            public Alias(string name, Type type) {
                this.name = name;
                this.type = type;
            }

        }

    }

}