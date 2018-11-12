using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace UIForia.Compilers {

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