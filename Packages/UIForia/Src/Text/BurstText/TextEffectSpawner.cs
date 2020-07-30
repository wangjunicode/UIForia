using UIForia.Util;

namespace UIForia.Text {

    public interface ITextEffectSpawner {

        TextEffect Spawn();

        void Despawn(TextEffect effect);

    }

    public class TextEffectSpawner<T, U> : ITextEffectSpawner where T : TextEffect<U>, new() {

        private LightList<T> pool;
        private U parameters;
        private int maxPoolSize;

        public TextEffectSpawner(U parameters, int maxPoolSize = 16) {
            this.parameters = parameters;
            this.maxPoolSize = maxPoolSize;
        }

        public TextEffect Spawn() {
            T retn;
            if (pool != null && pool.size > 0) {
                retn = pool.array[--pool.size];
                pool.array[pool.size] = null;
            }
            else {
                retn = new T();
            }

            retn.SetParameters(parameters);

            return retn;
        }

        public void Despawn(TextEffect effect) {
            if (!(effect is T typedEffect)) {
                return;
            }

            if (pool == null) {
                pool = new LightList<T>();
                pool.Add(typedEffect);
            }
            else {
                if (pool.size < maxPoolSize) {
                    pool.Add(typedEffect);
                }
            }
        }

    }

    public class TextEffectSpawner<T> : ITextEffectSpawner where T : TextEffect, new() {

        private LightList<T> pool;
        private int maxPoolSize;

        public TextEffectSpawner(int maxPoolSize = 16) {
            this.maxPoolSize = maxPoolSize;
        }

        public TextEffect Spawn() {
            T retn;
            if (pool != null && pool.size > 0) {
                retn = pool.array[--pool.size];
                pool.array[pool.size] = null;
            }
            else {
                retn = new T();
            }

            return retn;
        }

        public void Despawn(TextEffect effect) {

            if (!(effect is T typedEffect)) {
                return;
            }

            if (pool == null) {
                pool = new LightList<T>();
                pool.Add(typedEffect);
            }
            else {
                if (pool.size < maxPoolSize) {
                    pool.Add(typedEffect);
                }
            }
        }

    }

}