using JetBrains.Annotations;

public class ObservedProperty {

    public readonly string name;
    public readonly UIElement owner;

    protected object value;
    protected bool isDirty;
    
    protected ObservedProperty(string name, UIElement owner) {
        this.name = name;
        this.owner = owner;
        this.isDirty = false;
    }

    public object RawValue {
        get { return value; }
        protected set {
            if (this.value.Equals(value)) return;
            this.value = value;
            if (!isDirty) {
                owner.providedContext.dirtyBindings.Add(name);
                isDirty = true;
            }
        }
    }

}

public sealed class ObservedProperty<T> : ObservedProperty {

    public T Value {
        get { return (T) value; }
        set { RawValue = value; }
    }

    [UsedImplicitly]
    public ObservedProperty(string name, UIElement owner) : base(name, owner) {
        this.value = default(T);
    }

    public static implicit operator T(ObservedProperty<T> property) {
        return (T) property.value;
    }

}