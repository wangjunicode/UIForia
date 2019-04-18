using System;
using NUnit.Framework;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class CreateBlockTypeTests {

    [Test]
    public void CreateTypeAtRuntime() {
        
        
        Type type1 = ReflectionUtil.CreateType("type1", typeof(UIElement), new[] {
            new ReflectionUtil.FieldDefinition(typeof(int), "intField"), 
            new ReflectionUtil.FieldDefinition(typeof(Vector3), "vector3Field") 
        });
        
        Type type2 = ReflectionUtil.CreateType("type2", typeof(UIElement), new[] {
            new ReflectionUtil.FieldDefinition(typeof(int), "intField"), 
            new ReflectionUtil.FieldDefinition(typeof(Vector3), "vector3Field") 
        });

        ReflectionUtil.TryCreateInstance("type1", out UIElement element1);
        ReflectionUtil.TryCreateInstance("type2", out UIElement element2);

        Assert.IsInstanceOf<UIElement>(element1);
        Assert.IsInstanceOf<UIElement>(element2);
        
        Assert.IsNotNull(ReflectionUtil.GetFieldInfo(type1, "intField"));
        Assert.IsNotNull(ReflectionUtil.GetFieldInfo(type1, "vector3Field"));
        Assert.AreEqual(typeof(int), ReflectionUtil.GetFieldInfo(type1, "intField").FieldType);
        Assert.AreEqual(typeof(Vector3), ReflectionUtil.GetFieldInfo(type1, "vector3Field").FieldType);
        
        
    }

}