using System.Collections.Generic;

namespace UIForia.DataSource {

    public interface IRecordStore<T> where T : class, IRecord {

        T GetRecord(int id);
        T RemoveRecord(int id);
        
        void SetRecord(T record);
        ICollection<T> GetAllRecords(ICollection<T> list);
        
        int Count { get; }

    }

}