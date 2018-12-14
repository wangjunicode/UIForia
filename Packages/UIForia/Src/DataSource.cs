using System;
using UIForia.Util;

namespace UIForia.Data {

    public abstract class Record {

        public readonly int id;

        internal static int s_IdGenerator = 0;
        
        protected Record() {
            this.id = s_IdGenerator++;
        }

    }

    public class User : Record {

        public int playerId;
        public string username;
        public DateTime lastLogin;

    }

    public class RecordCollection<T> where T : Record {

        public Func<T, bool> filter;
        private LightList<T> m_Records;
        
        public event Action<T> onRecordAdded;
        public event Action<T> onRecordUpdated;
        public event Action<T> onRecordRemoved;
        
        public void AddRecord(T record) {
            if (filter == null || filter.Invoke(record)) {
                m_Records.Add(record);
                onRecordAdded?.Invoke(record);
            }
        }

        public void HasRecord(int id) { }
        
        public void HasRecord(T record) { }
        
        public void HasRecord(Func<T, bool> fn) { }

    }

    public class DefaultAdapter<T> : Adapter<T> where T : Record {

        
        public override void AddRecord(T Record) {
            
        }

    }

    public class AsyncElement : UIElement {

        //[Inject] public IDataSource<User> users;
        
        public virtual void OnCreate2() {
            //Injector.Inject(this);
        }

    }

    public class Adapter<T> where T : Record {

        public virtual void Serialize() { }
        
        public virtual void AddRecord(T Record) {
            
        }

        public virtual T GetRecord(int id) {
            return default;
        }

    }

    public class DataSource<T> where T : Record {

        private Adapter<T> adapter;

        public DataSource(Adapter<T> adapter = null) {
            
        }

        public void FetchRecord() { }
        
        public void GetRecord() { }

        public void SetRecord(int id, T record) {
            
        }

        public void UpdateRecord(T record) {
            
        }

        public void AddRecord(T record) {
            
        }

//        public ResultSet<T> Query() { }
//
//        public LiveResultSet<T> LiveQuery(Predicate<T> fn) {
//            
//        }

    }

}