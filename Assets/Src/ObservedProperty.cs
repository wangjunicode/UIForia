public class ObservedProperty {

    public readonly string name;
    public readonly UIElement owner;
 
    public ObservedProperty(string name, UIElement owner) {
        this.owner = owner;
    }
    
}

public class ObservedProperty<T> : ObservedProperty {
    private T value;

    public T Value {
        get { return value; }
        set {
            if (this.value.Equals(value)) return;
            this.value = value;
            if (!owner.dirtyProperties.Contains(this)) {
                owner.dirtyProperties.Add(this);
            }

            if (owner.dirtyProperties.Count > 0) {
                owner.MarkDirty();
            }
        }
    }
    
    public ObservedProperty(UIElement owner) : base(owner) { }

    public static implicit operator T(ObservedProperty<T> property) {
        return property.value;
    }

}