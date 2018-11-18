using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.Util;

namespace UIForia.Compilers {

    public class AssignmentCallbackBinding_Root_Field<T> : Binding {

        private readonly FieldInfo fieldInfo;
        private readonly EventInfo evtInfo;
        
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public AssignmentCallbackBinding_Root_Field(FieldInfo fieldInfo, EventInfo eventInfo) : base(eventInfo.Name) {
            this.fieldInfo = fieldInfo;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, fieldInfo), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly IExpressionContextProvider target;
            private readonly FieldInfo fieldInfo;
            
            public Handler(UITemplateContext ctx, IExpressionContextProvider target, FieldInfo fieldInfo) {
                this.ctx = ctx;
                this.target = target;
                this.fieldInfo = fieldInfo;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                ctx.current = target;

                object oldValue = fieldInfo.GetValue(ctx.rootElement);
                fieldInfo.SetValue(ctx.rootElement, evtArg0);

                IPropertyChangedHandler changedHandler = ctx.rootElement as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(fieldInfo.Name, oldValue);
             
            }

        }

    }
    
    public class AssignmentCallbackBinding_Root_Field_WithCallbacks<T, U> : Binding {

        private readonly FieldInfo fieldInfo;
        private readonly EventInfo evtInfo;
        private readonly Action<U, string>[] callbacks;
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public AssignmentCallbackBinding_Root_Field_WithCallbacks(FieldInfo fieldInfo, EventInfo eventInfo, LightList<object> callbacks) : base(eventInfo.Name) {
            this.fieldInfo = fieldInfo;
            this.evtInfo = eventInfo;
            this.callbacks = new Action<U, string>[callbacks.Count];
            for (int i = 0; i < callbacks.Count; i++) {
                this.callbacks[i] = (Action<U, string>) callbacks[i];
            }
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, fieldInfo, callbacks), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly IExpressionContextProvider target;
            private readonly FieldInfo fieldInfo;
            private readonly Action<U, string>[] callbacks;

            public Handler(UITemplateContext ctx, IExpressionContextProvider target, FieldInfo fieldInfo, Action<U, string>[] callbacks) {
                this.ctx = ctx;
                this.target = target;
                this.fieldInfo = fieldInfo;
                this.callbacks = callbacks;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                ctx.current = target;

                object oldValue = fieldInfo.GetValue(ctx.rootElement);
                fieldInfo.SetValue(ctx.rootElement, evtArg0);

                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke((U)(object)ctx.rootElement, fieldInfo.Name);
                }
                
                IPropertyChangedHandler changedHandler = ctx.rootElement as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(fieldInfo.Name, oldValue);
             
            }

        }

    }
    
    public class AssignmentCallbackBinding_Root_Property<T> : Binding {

        private readonly PropertyInfo property;
        private readonly EventInfo evtInfo;
        
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public AssignmentCallbackBinding_Root_Property(PropertyInfo property, EventInfo eventInfo) : base(eventInfo.Name) {
            this.property = property;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, property), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly IExpressionContextProvider target;
            private readonly PropertyInfo property;
            
            public Handler(UITemplateContext ctx, IExpressionContextProvider target, PropertyInfo property) {
                this.ctx = ctx;
                this.target = target;
                this.property = property;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                ctx.current = target;

                object oldValue = property.GetValue(ctx.rootElement);
                property.SetValue(ctx.rootElement, evtArg0);

                IPropertyChangedHandler changedHandler = ctx.rootElement as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(property.Name, oldValue);
             
            }

        }

    }
    
    public class AssignmentCallbackBinding_Root_Property_WithCallbacks<T, U> : Binding {

        private readonly PropertyInfo propertyInfo;
        private readonly EventInfo evtInfo;
        private readonly Action<U, string>[] callbacks;
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public AssignmentCallbackBinding_Root_Property_WithCallbacks(PropertyInfo propertyInfo, EventInfo eventInfo, LightList<object> callbacks) : base(eventInfo.Name) {
            this.propertyInfo = propertyInfo;
            this.evtInfo = eventInfo;
            this.callbacks = new Action<U, string>[callbacks.Count];
            for (int i = 0; i < callbacks.Count; i++) {
                this.callbacks[i] = (Action<U, string>) callbacks[i];
            }
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, propertyInfo, callbacks), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly IExpressionContextProvider target;
            private readonly PropertyInfo propertyInfo;
            private readonly Action<U, string>[] callbacks;

            public Handler(UITemplateContext ctx, IExpressionContextProvider target, PropertyInfo propertyInfo, Action<U, string>[] callbacks) {
                this.ctx = ctx;
                this.target = target;
                this.propertyInfo = propertyInfo;
                this.callbacks = callbacks;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                ctx.current = target;

                object oldValue = propertyInfo.GetValue(ctx.rootElement);
                propertyInfo.SetValue(ctx.rootElement, evtArg0);

                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke((U)(object)ctx.rootElement, propertyInfo.Name);
                }
                
                IPropertyChangedHandler changedHandler = ctx.rootElement as IPropertyChangedHandler;
                changedHandler?.OnPropertyChanged(propertyInfo.Name, oldValue);
             
            }

        }

    }
    
    
    public class CallbackBinding : Binding {

        private readonly EventInfo evtInfo;
        private readonly Expression<Terminal> expression;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public CallbackBinding(Expression<Terminal> expression, EventInfo eventInfo) : base(eventInfo.Name) {
            this.expression = expression;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        // todo -- can this be a struct?
        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly IExpressionContextProvider target;

            public Handler(Expression<Terminal> expression, UITemplateContext ctx, IExpressionContextProvider target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run() {
                ctx.current = target;
                expression.EvaluateTyped(ctx);
            }

        }

    }

    public class CallbackBinding<T> : Binding {

        private readonly EventInfo evtInfo;
        private readonly Expression<Terminal> expression;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public CallbackBinding(Expression<Terminal> expression, EventInfo eventInfo) : base(eventInfo.Name) {
            this.expression = expression;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly IExpressionContextProvider target;

            public Handler(Expression<Terminal> expression, UITemplateContext ctx, IExpressionContextProvider target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                ctx.current = target;
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgDefaultName, evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[0], evtArg0);

                expression.EvaluateTyped(ctx);

                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgDefaultName);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[0]);
            }

        }

    }

    public class CallbackBinding<T, U> : Binding {

        private readonly EventInfo evtInfo;
        private readonly Expression<Terminal> expression;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public CallbackBinding(Expression<Terminal> expression, EventInfo eventInfo) : base(eventInfo.Name) {
            this.expression = expression;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly IExpressionContextProvider target;

            public Handler(Expression<Terminal> expression, UITemplateContext ctx, IExpressionContextProvider target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1) {
                ctx.current = target;
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgDefaultName, evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[0], evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[1], evtArg1);
                expression.EvaluateTyped(ctx);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgDefaultName);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[0]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[1]);
            }

        }

    }

    public class CallbackBinding<T, U, V> : Binding {

        private readonly EventInfo evtInfo;
        private readonly Expression<Terminal> expression;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public CallbackBinding(Expression<Terminal> expression, EventInfo eventInfo) : base(eventInfo.Name) {
            this.expression = expression;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly IExpressionContextProvider target;

            public Handler(Expression<Terminal> expression, UITemplateContext ctx, IExpressionContextProvider target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1, V evtArg2) {
                ctx.current = target;
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgDefaultName, evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[0], evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[1], evtArg1);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[2], evtArg2);
                expression.EvaluateTyped(ctx);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgDefaultName);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[0]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[1]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[2]);
            }

        }

    }

    public class CallbackBinding<T, U, V, W> : Binding {

        private readonly EventInfo evtInfo;
        private readonly Expression<Terminal> expression;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static MethodInfo runInfo;

        public CallbackBinding(Expression<Terminal> expression, EventInfo eventInfo) : base(eventInfo.Name) {
            this.expression = expression;
            this.evtInfo = eventInfo;
            runInfo = runInfo ?? typeof(Handler).GetMethod(nameof(Handler.Run));
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly UITemplateContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly IExpressionContextProvider target;

            public Handler(Expression<Terminal> expression, UITemplateContext ctx, IExpressionContextProvider target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1, V evtArg2, W evtArg3) {
                ctx.current = target;
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgDefaultName, evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[0], evtArg0);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[1], evtArg1);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[2], evtArg2);
                ctx.SetContextValue(target, PropertyBindingCompiler.EvtArgNames[3], evtArg3);
                expression.EvaluateTyped(ctx);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgDefaultName);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[0]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[1]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[2]);
                ctx.RemoveContextValue<T>(target, PropertyBindingCompiler.EvtArgNames[3]);
            }

        }

    }

}