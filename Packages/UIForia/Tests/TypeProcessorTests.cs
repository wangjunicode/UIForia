using System;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia;
using UIForia.Parsing;
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
    public void ResolveSubType() {
        Type t = TypeProcessor.ResolveType(typeof(Thing), "SubThing", new List<string>());
        Assert.AreEqual(typeof(Thing.SubThing), t);
    }
    
    [Test]
    public void ResolveSubEnumType() {
        Type t = TypeProcessor.ResolveType(typeof(Thing), "NestedEnum", new List<string>());
        Assert.AreEqual(typeof(Thing.NestedEnum), t);
    }

}