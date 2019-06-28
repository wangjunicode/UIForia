using System;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia;
using UIForia.Parsing.Expression;
using UIForia.Parsing.Expression.AstNodes;
using UnityEngine;

[TestFixture]
public class TypeProcessorTests {

//    [Test]
//    public void ResolveATypeWithNamespace() {
//        
////        Type type = TypeProcessor.ResolveType("UnityEngine.Vector3", new List<string>());
////        Assert.AreEqual(typeof(UnityEngine.Vector3), type);
////        
////        type = TypeProcessor.ResolveType("UnityEngine.MeshTopology.Lines", new List<string>());
////        Assert.AreEqual(typeof(UnityEngine.MeshTopology), type);
//        Type type;
//        
//        TypePath typePath = new TypePath();
//        
//        // main problem: where to draw the line w/ whats a namespace and whats a type
//        
//        typePath.path = new List<string>(){"System", "Collections", "Generic", "Dictionary"};
//        typePath.genericArguments = new List<TypePath>() {
//            new TypePath() {
//                path = new List<string>() {
//                    "System", "Single"
//                }
//            },
//            new TypePath() {
//                path = new List<string>() {
//                    "System", "Single"
//                }
//            }
//        };
//        
//        // Thing<float>.OtherThing<string>.Value
//        
//        Debug.Log("path: " + typePath.GetConstructedPath());
//        Type t = Type.GetType(typePath.GetConstructedPath());
//        Debug.Log(typeof(Dictionary<float, float>) == t);
//        type = TypeProcessor.ResolveType("System.Collections.Generic.List`1[System.Single]", new List<string>());
//        Assert.AreEqual(typeof(List<float>), type);
//        
//    }

    private class Thing {

        public class SubThing { }

        public enum NestedEnum { }

    }

    [Test]
    public void ResolveWithNamespace() {
        Type t = TypeProcessor.ResolveType(typeof(Thing), "Vector3", new List<string>() {"UnityEngine"});
        Assert.AreEqual(typeof(Vector3), t);
    }

    [Test]
    public void ResolveWithNamespaceNoOrigin() {
        Type t = TypeProcessor.ResolveType("Color", new List<string>() {"UnityEngine"});
        Assert.AreEqual(typeof(Color), t);

        Type t1 = TypeProcessor.ResolveType("SortedList", new List<string>() {"System.Collections"});
        Assert.AreEqual(typeof(System.Collections.SortedList), t1);
        
    }

    [Test]
    public void ResolveSubType() {
        Type t = TypeProcessor.ResolveType(typeof(Thing), "SubThing", new List<string>());
        Assert.AreEqual(typeof(Thing.SubThing), t);
    }

    [Test]
    public void ResolveTypeFromTypeLookup() {
        TypeLookup lookup = new TypeLookup();
//        lookup.typeName = "AnimationCurve";
//        Type t = TypeProcessor.ResolveType(lookup, new List<string>() { "UnityEngine"});
//        Assert.AreEqual(typeof(AnimationCurve), t);
//        lookup.typeName = "AnimationCurve";
//        lookup.namespaceName = "UnityEngine";
//        Type t2 = TypeProcessor.ResolveType(lookup, new List<string>());
//        Assert.AreEqual(typeof(AnimationCurve), t2);
//        
        lookup.typeName = "String";
        lookup.namespaceName = "System";
        lookup.generics = null;
        Type t0 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(string), t0);

        lookup.typeName = "List`1";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "string"
            }
        };
        Type t3 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t3);

        lookup.typeName = "List`1";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "String",
                namespaceName = "System"
            }
        };
        Type t4 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<string>), t4);

        lookup.typeName = "List`1";
        lookup.namespaceName = "System.Collections.Generic";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary`2",
                namespaceName = "System.Collections.Generic",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                        namespaceName = "UnityEngine"
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair`2",
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
        Type t5 = TypeProcessor.ResolveType(lookup, new List<string>());
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t5);

        lookup.typeName = "List`1";
        lookup.generics = new[] {
            new TypeLookup() {
                typeName = "Dictionary`2",
                generics = new[] {
                    new TypeLookup() {
                        typeName = "Color",
                    },
                    new TypeLookup() {
                        typeName = "KeyValuePair`2",
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
        
        Type t6 = TypeProcessor.ResolveType(lookup, new List<string>() {"System.Collections.Generic", "UnityEngine"});
        Assert.AreEqual(typeof(List<Dictionary<Color, KeyValuePair<float, int>>>), t6);
    }

    [Test]
    public void ResolveSubEnumType() {
        Type t = TypeProcessor.ResolveType(typeof(Thing), "NestedEnum", new List<string>());
        Assert.AreEqual(typeof(Thing.NestedEnum), t);
    }

}