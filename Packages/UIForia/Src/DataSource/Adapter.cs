using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIForia.DataSource {

    public class Adapter<T> where T : class, IRecord {

        public virtual Task Configure() {
            return null;
        }
        
        public virtual async Task<T> AddRecord(T record) {
            return record;
        }

        public virtual async Task<T> GetRecord(int id, T currentRecord) {
            return currentRecord;
        }

        public virtual async Task<T> SetRecord(int id, T newRecord, T oldRecord) {
            return newRecord;
        }

        public virtual async Task<IList<T>> LoadRecords(IList<T> localRecords) { 
            return localRecords ?? new List<T>();
        }

        public virtual bool RecordChanged(T recordA, T recordB) {
            return recordA != recordB;
        }

        public async Task<T> RemoveRecord(int id, T localRecord) {
            return localRecord;
        }

    }

}