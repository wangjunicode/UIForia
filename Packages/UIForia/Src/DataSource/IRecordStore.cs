namespace UIForia.DataSource {

    public interface IRecordStore<T> where T : class, IRecord {

        T GetRecord(int id);
        T RemoveRecord(int id);
        void SetRecord(T record);

        int Count { get; }

    }

}