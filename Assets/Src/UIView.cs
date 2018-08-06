using System.Collections.Generic;
using Src;
using UnityEngine;

public abstract class UIView<T> : MonoBehaviour where T : UIElement {

    public readonly List<UIElement> dirtyElements;
    public readonly List<TemplateContext> dirtyContexts;
    private readonly List<UIElement> renderQueue;
    public UIElement rootElement;
    
//    public static UIView<T> Create(Canvas canvas, List<PropBinding> props) {
//        UIElementTemplate template = TemplateParser.GetParsedTemplate<T>();
//        UIView<T> view = null;//template.CreateView<T>(props);
//        view.transform.SetParent(canvas.transform);
//        return view;
//    }
//
//    public virtual List<PropBinding> GetProps() {
//        return new List<PropBinding>();
//    }

    public void FlushChanges() {

        for (int i = 0; i < dirtyElements.Count; i++) {
            UIElement element = dirtyElements[i];
            FlushDirtyProperties(element);
        }

    }

    public void UpdateRendering() {
        /*
         * for each dirty element
         *     if needs creation -> push to create queue
         *     if needs layout
         *     
         */

        // hide
        // show
        // create
        // destroy
        // style
        // layout
    }

    public void UpdateLayout() { }

    private void FlushDirtyProperties(UIElement element) {
        TemplateContext context = element.providedContext;

        for (int i = 0; i < context.dirtyBindings.Count; i++) {
            string name = context.dirtyBindings[i];
            //List<TemplateContext.Binding> dirtyBindings = context.GetBoundElements(name);
            // for each dirty element
            // update dirty prop values via reflection
            // invoke on props updated handle
            // send to render system for layout / styling / painting

        }

    }

}