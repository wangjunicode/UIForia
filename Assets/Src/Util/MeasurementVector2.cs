using Src;

public struct MeasurementVector2 {

    public UIMeasurement x;
    public UIMeasurement y;

    public MeasurementVector2(UIMeasurement x, UIMeasurement y) {
        this.x = x;
        this.y = y;
    }
    
    public static bool operator ==(MeasurementVector2 self, MeasurementVector2 other) {
        return self.x == other.x && self.y == other.y;
    }

    public static bool operator !=(MeasurementVector2 self, MeasurementVector2 other) {
        return !(self == other);
    }

    public bool IsDefined() {
        return x.IsDefined() && y.IsDefined();
    }

}