using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class TypeResolverTests {

    private class Thing {

        public class SubThing { }

        public enum NestedEnum { }

    }
    
    
    [Test]
    public void ResolveArrayType() {
        TypeLookup lookup = new TypeLookup();
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.isArray = true;
        Type t = TypeResolver.Default.ResolveType(lookup);
        Assert.AreEqual(typeof(string[]), t);
    }

    [Test]
    public void ResolveWithNamespaceNoOrigin() {
        Type t = TypeResolver.Default.ResolveType("Color", new List<string>() {"UnityEngine"});
        Assert.AreEqual(typeof(Color), t);

        Type t1 = TypeResolver.Default.ResolveType("SortedList", new List<string>() {"System.Collections"});
        Assert.AreEqual(typeof(SortedList), t1);
        
    }

    [Test]
    public void ResolveTypeFromTypeLookup() {
        TypeLookup lookup = new TypeLookup();
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.generics = null;
        Type t0 = TypeResolver.Default.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(string), t0);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "string"
            }
        };
        Type t3 = TypeResolver.Default.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t3);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "String",
                namespaceName = "System"
            }
        };
        Type t4 = TypeResolver.Default.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t4);

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                namespaceName = "System.Collections.Generic",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                        namespaceName = "UnityEngine"
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        namespaceName = "System.Collections.Generic",
                        generics = new[] {
                            new TypeLookup() {
                                typeName = "float"
                            },
                            new TypeLookup() {
                                typeName = "int"
                            }
                        }
                    }
                }
            }
        };
        Type t5 = TypeResolver.Default.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t5);

        lookup.typeName = "List";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        generics = new[] {
                            new TypeLookup() {
                                typeName = "float"
                            },
                            new TypeLookup() {
                                typeName = "int"
                            }
                        }
                    }
                }
            }
        };
        
        Type t6 = TypeResolver.Default.ResolveType(lookup, new List<string>() {"System.Collections.Generic", "UnityEngine"});
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t6);
    }

   

}