using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Util;

namespace UIForia.Bindings {

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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, fieldInfo), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly object target;
            private readonly FieldInfo fieldInfo;
            
            public Handler(ExpressionContext ctx, object target, FieldInfo fieldInfo) {
                this.ctx = ctx;
                this.target = target;
                this.fieldInfo = fieldInfo;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {

                object oldValue = fieldInfo.GetValue(ctx.rootObject);
                fieldInfo.SetValue(ctx.rootObject, evtArg0);

                IPropertyChangedHandler changedHandler = ctx.rootObject as IPropertyChangedHandler;
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, fieldInfo, callbacks), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly object target;
            private readonly FieldInfo fieldInfo;
            private readonly Action<U, string>[] callbacks;

            public Handler(ExpressionContext ctx, object target, FieldInfo fieldInfo, Action<U, string>[] callbacks) {
                this.ctx = ctx;
                this.target = target;
                this.fieldInfo = fieldInfo;
                this.callbacks = callbacks;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {

                object oldValue = fieldInfo.GetValue(ctx.rootObject);
                fieldInfo.SetValue(ctx.rootObject, evtArg0);

                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke((U)ctx.rootObject, fieldInfo.Name);
                }
                
                IPropertyChangedHandler changedHandler = ctx.rootObject as IPropertyChangedHandler;
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, property), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly object target;
            private readonly PropertyInfo property;
            
            public Handler(ExpressionContext ctx, object target, PropertyInfo property) {
                this.ctx = ctx;
                this.target = target;
                this.property = property;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {

                object oldValue = property.GetValue(ctx.rootObject);
                property.SetValue(ctx.rootObject, evtArg0);

                IPropertyChangedHandler changedHandler = ctx.rootObject as IPropertyChangedHandler;
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(context, element, propertyInfo, callbacks), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly object target;
            private readonly PropertyInfo propertyInfo;
            private readonly Action<U, string>[] callbacks;

            public Handler(ExpressionContext ctx, object target, PropertyInfo propertyInfo, Action<U, string>[] callbacks) {
                this.ctx = ctx;
                this.target = target;
                this.propertyInfo = propertyInfo;
                this.callbacks = callbacks;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {

                object oldValue = propertyInfo.GetValue(ctx.rootObject);
                propertyInfo.SetValue(ctx.rootObject, evtArg0);

                for (int i = 0; i < callbacks.Length; i++) {
                    callbacks[i].Invoke((U)ctx.rootObject, propertyInfo.Name);
                }
                
                IPropertyChangedHandler changedHandler = ctx.rootObject as IPropertyChangedHandler;
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context, element), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        // todo -- can this be a struct?
        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly Expression<Terminal> expression;
            private readonly object target;

            public Handler(Expression<Terminal> expression, ExpressionContext ctx, object target) {
                this.expression = expression;
                this.ctx = ctx;
                this.target = target;
            }

            [UsedImplicitly]
            public void Run() {
                expression.Evaluate(ctx);
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly Expression<Terminal> expression;

            public Handler(Expression<Terminal> expression, ExpressionContext ctx) {
                this.expression = expression;
                this.ctx = ctx;
            }

            [UsedImplicitly]
            public void Run(T evtArg0) {
                expression.Evaluate(ctx);
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly Expression<Terminal> expression;

            public Handler(Expression<Terminal> expression, ExpressionContext ctx) {
                this.expression = expression;
                this.ctx = ctx;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1) {
                expression.Evaluate(ctx);
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly Expression<Terminal> expression;

            public Handler(Expression<Terminal> expression, ExpressionContext ctx) {
                this.expression = expression;
                this.ctx = ctx;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1, V evtArg2) {
                expression.Evaluate(ctx);
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

        public override void Execute(UIElement element, ExpressionContext context) {
            evtInfo.AddEventHandler(element, Delegate.CreateDelegate(evtInfo.EventHandlerType, new Handler(expression, context), runInfo));
        }

        public override bool IsConstant() {
            return true;
        }

        private class Handler {

            private readonly ExpressionContext ctx;
            private readonly Expression<Terminal> expression;

            public Handler(Expression<Terminal> expression, ExpressionContext ctx) {
                this.expression = expression;
                this.ctx = ctx;
            }

            [UsedImplicitly]
            public void Run(T evtArg0, U evtArg1, V evtArg2, W evtArg3) {
                expression.Evaluate(ctx);
            }

        }

    }

}