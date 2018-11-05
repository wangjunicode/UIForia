using Src;

public struct MeasurementPair {

    public UIMeasurement x;
    public UIMeasurement y;

    public MeasurementPair(UIMeasurement x, UIMeasurement y) {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(MeasurementPair self, MeasurementPair other) {
        return self.x == other.x && self.y == other.y;
    }

    public static bool operator !=(MeasurementPair self, MeasurementPair other) {
        return !(self == other);
    }

    public bool IsDefined() {
        return x.IsDefined() && y.IsDefined();
    }

}