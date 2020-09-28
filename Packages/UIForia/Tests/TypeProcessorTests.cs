using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class TypeProcessorTests {

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
        Type t = TypeResolver.ResolveType(lookup);
        Assert.AreEqual(typeof(string[]), t);

        Assert.IsTrue(TypeParser.TryParseTypeName("System.String[]", out lookup));
        Assert.AreEqual(typeof(string[]), TypeResolver.ResolveType(lookup));
    }

    [Test]
    public void ResolveWithNamespaceNoOrigin() {
        Type t = TypeResolver.ResolveType("Color", new List<string>() {"UnityEngine"});
        Assert.AreEqual(typeof(Color), t);

        Assert.IsTrue(TypeParser.TryParseTypeName("Color", out TypeLookup lookup));
        Assert.AreEqual(typeof(Color), TypeResolver.ResolveType(lookup, new List<string>() {"UnityEngine"}));

        Type t1 = TypeResolver.ResolveType("SortedList", new List<string>() {"System.Collections"});
        Assert.AreEqual(typeof(SortedList), t1);

        Assert.IsTrue(TypeParser.TryParseTypeName("SortedList", out lookup));
        Assert.AreEqual(typeof(SortedList), TypeResolver.ResolveType(lookup, new List<string>() {"System.Collections"}));

    }


    [Test]
    public void ResolveTypeFromTypeLookup() {
        TypeLookup lookup = new TypeLookup();
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.generics = default;
        Type t0 = TypeResolver.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(string), t0);

        Assert.IsTrue(TypeParser.TryParseTypeName("System.String", out lookup));
        Assert.AreEqual(typeof(string), TypeResolver.ResolveType(lookup, new List<string>()));

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new SizedArray<TypeLookup>(new[] {
            new TypeLookup() {
                typeName = "string"
            }
        });
        Type t3 = TypeResolver.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t3);

        Assert.IsTrue(TypeParser.TryParseTypeName("System.Collections.Generic.List<string>", out lookup));
        Assert.AreEqual(typeof(List<string>), TypeResolver.ResolveType(lookup, new List<string>()));

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new SizedArray<TypeLookup>(new[] {
            new TypeLookup() {
                typeName = "String",
                namespaceName = "System"
            }
        });
        Type t4 = TypeResolver.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t4);

        Assert.IsTrue(TypeParser.TryParseTypeName("System.Collections.Generic.List<System.String>", out lookup));
        Assert.AreEqual(typeof(List<string>), TypeResolver.ResolveType(lookup, new List<string>()));

        lookup.typeName = "List";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new SizedArray<TypeLookup>(new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                namespaceName = "System.Collections.Generic",
                generics = new SizedArray<TypeLookup>(new[] {
                    new TypeLookup() {
                        typeName = "Color",
                        namespaceName = "UnityEngine"
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        namespaceName = "System.Collections.Generic",
                        generics = new SizedArray<TypeLookup>(
                            new[] {
                                new TypeLookup() {
                                    typeName = "float"
                                },
                                new TypeLookup() {
                                    typeName = "int"
                                }
                            }
                        )
                    }
                })
            }

        });
        Type t5 = TypeResolver.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t5);

        Assert.IsTrue(TypeParser.TryParseTypeName("System.Collections.Generic.List<System.Collections.Generic.Dictionary<UnityEngine.Color, System.Collections.Generic.KeyValuePair<float, int>>>", out lookup));
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), TypeResolver.ResolveType(lookup, new List<string>()));

        lookup.typeName = "List";
        lookup.generics = new SizedArray<TypeLookup>(new[] {
            new TypeLookup() {
                typeName = "Dictionary",
                generics = new SizedArray<TypeLookup>(new[] {
                    new TypeLookup() {
                        typeName = "Color",
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair",
                        generics = new SizedArray<TypeLookup>(new[] {
                            new TypeLookup() {
                                typeName = "float"
                            },
                            new TypeLookup() {
                                typeName = "int"
                            }
                        })
                    }
                })
            }
        });

        Type t6 = TypeResolver.ResolveType(lookup, new List<string>() {"System.Collections.Generic", "UnityEngine"});
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t6);
        
        Assert.IsTrue(TypeParser.TryParseTypeName("List<Dictionary<Color, KeyValuePair<float, int>>>", out lookup));
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), TypeResolver.ResolveType(lookup, new List<string>() {"System.Collections.Generic", "UnityEngine"}));
    }

   

}