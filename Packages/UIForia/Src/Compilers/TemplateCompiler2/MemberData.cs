using System;
using System.Reflection;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class MemberData {
        
        public static readonly ConstructorInfo InputHandlerGroup_Ctor = typeof(InputHandlerGroup).GetConstructor(Type.EmptyTypes);
        public static readonly MethodInfo InputHandlerGroup_AddMouseEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddMouseEvent));
        public static readonly MethodInfo InputHandlerGroup_AddKeyboardEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddKeyboardEvent));
        public static readonly MethodInfo InputHandlerGroup_AddDragCreator = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragCreator));
        public static readonly MethodInfo InputHandlerGroup_AddDragEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragEvent));

        public static readonly MethodInfo TemplateSystem_InitializeElement = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeElement));
        public static readonly MethodInfo TemplateSystem_InitializeHydratedElement = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeHydratedElement));
        public static readonly MethodInfo TemplateSystem_InitializeSlotElement = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeSlotElement));
        public static readonly MethodInfo TemplateSystem_InitializeEntryPoint = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeEntryPoint));
        public static readonly MethodInfo TemplateSystem_InitializeStaticAttribute = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeStaticAttribute));
        public static readonly MethodInfo TemplateSystem_InitializeDynamicAttribute = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.InitializeDynamicAttribute));
        public static readonly MethodInfo TemplateSystem_AddChild = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddChild));
        public static readonly MethodInfo TemplateSystem_AddSlotChild = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddSlotChild));
        public static readonly MethodInfo TemplateSystem_HydrateElement = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.HydrateElement));
        public static readonly MethodInfo TemplateSystem_HydrateEntryPoint = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.HydrateEntryPoint));
        public static readonly MethodInfo TemplateSystem_OverrideSlot = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.OverrideSlot));
        public static readonly MethodInfo TemplateSystem_ForwardSlot = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.ForwardSlot));
        public static readonly MethodInfo TemplateSystem_SetText = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.SetText));
        public static readonly MethodInfo TemplateSystem_SetBindings = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.SetBindings));
        public static readonly MethodInfo TemplateSystem_CreateBindingVariable = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.CreateBindingVariable));
        public static readonly MethodInfo TemplateSystem_ReferenceBindingVariable = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.ReferenceBindingVariable));
        public static readonly MethodInfo TemplateSystem_AddMouseHandler = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddMouseEventHandler));
        public static readonly MethodInfo TemplateSystem_AddKeyboardHandler = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddKeyboardEventHandler));
        public static readonly MethodInfo TemplateSystem_AddDragCreateHandler = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddDragCreateHandler));
        public static readonly MethodInfo TemplateSystem_AddDragEventHandler = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.AddDragEventHandler));
        public static readonly MethodInfo TemplateSystem_RegisterForKeyboardEvents = typeof(TemplateSystem).GetMethod(nameof(TemplateSystem.RegisterForKeyboardEvents));

        public static readonly FieldInfo Element_InputHandlers = typeof(UIElement).GetField(nameof(UIElement.inputHandlers));
        public static readonly MethodInfo Element_SetAttribute = typeof(UIElement).GetMethod(nameof(UIElement.SetAttribute));
        public static readonly MethodInfo Element_SetEnabledInternal = typeof(UIElement).GetMethod(nameof(UIElement.internal__dontcallmeplease_SetEnabledIfBinding));

        public static readonly FieldInfo BindingNode_Element = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.element));
        public static readonly FieldInfo BindingNode_Root = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.root));
        public static readonly FieldInfo BindingNode_ReferencedContexts = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.referencedContexts));
        public static readonly MethodInfo BindingNode_SetBindingVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.SetBindingVariable));
        public static readonly MethodInfo BindingNode_GetBindingVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetBindingVariable));
        public static readonly MethodInfo BindingNode_InvokeUpdate = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.InvokeUpdate));

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
        public static readonly MethodInfo EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));
        
        public static readonly FieldInfo InputEventHolder_MouseInputEvent = typeof(InputEventHolder).GetField(nameof(InputEventHolder.mouseEvent));
        public static readonly FieldInfo InputEventHolder_KeyboardInputEvent = typeof(InputEventHolder).GetField(nameof(InputEventHolder.keyEvent));
        public static readonly FieldInfo InputEventHolder_DragEvent = typeof(InputEventHolder).GetField(nameof(InputEventHolder.dragEvent));
        public static readonly FieldInfo InputEventHolder_DragCreateResult = typeof(InputEventHolder).GetField(nameof(InputEventHolder.dragCreateResult));

    }

}