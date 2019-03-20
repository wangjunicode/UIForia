using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIForia.Util;

namespace UIForia.DataSource {

    public class DataSource<T> where T : class, IRecord {

        private Task config;
        private readonly Adapter<T> adapter;
        private readonly IRecordStore<T> recordStore;

        public event Action<T> onRecordAdded;
        public event Action<T> onRecordChanged;
        public event Action<T> onRecordRemoved;

        public DataSource(Adapter<T> adapter = null, IRecordStore<T> store = null) {
            this.adapter = adapter ?? new Adapter<T>();
            this.recordStore = store ?? new ListRecordStore<T>();
            this.config = this.adapter.Configure();
        }

        public int RecordCount => recordStore.Count;

        public void LoadRecords(Action<List<T>> onComplete) {
            // adapter.LoadRecords();
        }

        public async Task<T> AddRecord(T record) {
            if (record == null) {
                return null;
            }

            if (config != null) {
                await config;
                config = null;
            }

            T result = await adapter.AddRecord(record);

            if (result == null) {
                return null;
            }
            else {
                recordStore.SetRecord(record);
                this.onRecordAdded?.Invoke(record);
                return record;
            }
        }

        public async Task<T> SetRecord(int id, T record) {

            if (record == null) {
                return await RemoveRecord(id);
            }

            if (config != null) {
                await config;
                config = null;
            }

            T localRecord = recordStore.GetRecord(id);

            T newRecord = await adapter.SetRecord(id, record, localRecord);

            // something might have changed since we issued an await
            localRecord = recordStore.GetRecord(id);

            if (newRecord == null) {
                T current = recordStore.RemoveRecord(record.Id);
                if (current != null) {
                    onRecordRemoved?.Invoke(current);
                }
            }
            else if (localRecord == null) {
                recordStore.SetRecord(record);
                this.onRecordAdded?.Invoke(record);
            }
            else {
                recordStore.SetRecord(record);
                if (adapter.RecordChanged(newRecord, localRecord)) {
                    onRecordChanged?.Invoke(record);
                }
            }

            return newRecord;
        }

        public async Task<T> RemoveRecord(int id) {
            if (config != null) {
                await config;
                config = null;
            }
            T localRecord = recordStore.GetRecord(id);
            T returnedRecord = await adapter.RemoveRecord(id, localRecord);
            localRecord = recordStore.RemoveRecord(id);
            if (localRecord != null) {
                onRecordRemoved?.Invoke(returnedRecord);
            }
            return returnedRecord;
        }

        public async Task<T> RemoveRecord(T record) {
            return await RemoveRecord(record.Id);
        }

        public async Task<T> UpsertRecord(T record) {
            if (record == null) return null;
            return await SetRecord(record.Id, record);
        }

        public async Task<T> GetRecord(int id) {
            T returnedRecord = await adapter.GetRecord(id, recordStore.GetRecord(id));

            if (returnedRecord == null) {
                T removed = recordStore.RemoveRecord(id);
                if (removed != null) {
                    onRecordRemoved?.Invoke(removed);
                }

                return null;
            }

            T localRecord = recordStore.GetRecord(id);

            if (localRecord == null) {
                onRecordAdded?.Invoke(returnedRecord);
            }
            else {
                if (adapter.RecordChanged(localRecord, returnedRecord)) {
                    onRecordChanged?.Invoke(returnedRecord);
                }
            }

            recordStore.SetRecord(returnedRecord);
            return localRecord;
        }

    }

}