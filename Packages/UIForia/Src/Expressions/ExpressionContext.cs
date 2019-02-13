namespace UIForia {

    public class ExpressionContext {

        public object rootObject;
        public readonly object currentObject;
        public object aux;

        public ExpressionContext(object rootObject, object currentObject = null, object aux = null) {
            this.rootObject = rootObject;
            this.currentObject = currentObject ?? rootObject;
            this.aux = aux ?? rootObject;
        }

    }

}