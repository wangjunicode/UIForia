public struct ExpressionAlias<T> {

    public readonly T value;
    public readonly string name;

    public ExpressionAlias(string name, T value) {
        this.name = name;
        this.value = value;
    }

}