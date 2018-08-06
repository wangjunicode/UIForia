using System.Collections.Generic;

public class ObservedList { }

public class ObservedList<T> : ObservedList where T : struct {

    private List<T> list;
    private bool isDirty;
    
    public T this[int index] {
        get { return list[index]; }
        set {
            list[index] = value;
            isDirty = true;
        }
    }

}