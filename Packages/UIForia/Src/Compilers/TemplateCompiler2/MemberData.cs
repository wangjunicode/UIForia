using System;
using System.Reflection;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class MemberData {
        
        public static readonly ConstructorInfo InputHandlerGroup_Ctor = typeof(InputHandlerGroup).GetConstructor(Type.EmptyTypes);

        public static readonly MethodInfo ElementSystem_InitializeElement = typeof(ElementSystem).GetMethod(nameof(ElementSystem.InitializeElement));
        public static readonly MethodInfo ElementSystem_InitializeEntryPoint = typeof(ElementSystem).GetMethod(nameof(ElementSystem.InitializeEntryPoint));
        public static readonly MethodInfo ElementSystem_InitializeStaticAttribute = typeof(ElementSystem).GetMethod(nameof(ElementSystem.InitializeStaticAttribute));
        public static readonly MethodInfo ElementSystem_InitializeDynamicAttribute = typeof(ElementSystem).GetMethod(nameof(ElementSystem.InitializeDynamicAttribute));
        public static readonly MethodInfo ElementSystem_AddChild = typeof(ElementSystem).GetMethod(nameof(ElementSystem.AddChild));
        public static readonly MethodInfo ElementSystem_AddSlotChild = typeof(ElementSystem).GetMethod(nameof(ElementSystem.AddSlotChild));
        public static readonly MethodInfo ElementSystem_AddHydratedChild = typeof(ElementSystem).GetMethod(nameof(ElementSystem.AddHydratedChild));
        public static readonly MethodInfo ElementSystem_GetTemplateData = typeof(ElementSystem).GetMethod(nameof(ElementSystem.GetTemplateData));
        public static readonly MethodInfo ElementSystem_HydrateElement = typeof(ElementSystem).GetMethod(nameof(ElementSystem.HydrateElement));
        public static readonly MethodInfo ElementSystem_HydrateEntryPoint = typeof(ElementSystem).GetMethod(nameof(ElementSystem.HydrateEntryPoint));
        public static readonly MethodInfo ElementSystem_OverrideSlot = typeof(ElementSystem).GetMethod(nameof(ElementSystem.OverrideSlot));
        public static readonly MethodInfo ElementSystem_ForwardSlot = typeof(ElementSystem).GetMethod(nameof(ElementSystem.ForwardSlot));

        public static readonly FieldInfo Element_InputHandlers = typeof(UIElement).GetField(nameof(UIElement.inputHandlers));
        
        public static readonly MethodInfo Element_SetEnabledInternal = typeof(UIElement).GetMethod(nameof(UIElement.internal__dontcallmeplease_SetEnabledIfBinding));

        public static readonly FieldInfo BindingNode_Element = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.element));
        public static readonly FieldInfo BindingNode_Root = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.root));

        public static readonly FieldInfo TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo TextElement_SetText = typeof(UITextElement).GetMethod(nameof(UITextElement.SetText), BindingFlags.Instance | BindingFlags.Public);

        public static readonly MethodInfo StringBuilder_AppendString = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(string)});
        public static readonly MethodInfo StringBuilder_AppendInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(short)});
        public static readonly MethodInfo StringBuilder_AppendInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(int)});
        public static readonly MethodInfo StringBuilder_AppendInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(long)});
        public static readonly MethodInfo StringBuilder_AppendUInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ushort)});
        public static readonly MethodInfo StringBuilder_AppendUInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(uint)});
        public static readonly MethodInfo StringBuilder_AppendUInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ulong)});
        public static readonly MethodInfo StringBuilder_AppendFloat = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(float)});
        public static readonly MethodInfo StringBuilder_AppendDouble = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(double)});
        public static readonly MethodInfo StringBuilder_AppendDecimal = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(decimal)});
        public static readonly MethodInfo StringBuilder_AppendByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(byte)});
        public static readonly MethodInfo StringBuilder_AppendSByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(sbyte)});
        public static readonly MethodInfo StringBuilder_AppendBool = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(bool)});
        public static readonly MethodInfo StringBuilder_AppendChar = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(char)});

    }

}