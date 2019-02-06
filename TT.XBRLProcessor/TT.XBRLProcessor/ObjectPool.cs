using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Threading
{
    /// <summary>An object pool implementation, taken fom the article by Stephen Toub published at:
    /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2010/04/13/9990427.aspx">ParallelExtensionsExtras Tour - #9 - ObjectPool&lt;T&gt;</a>.
    /// </summary><remarks>
    /// This is based on the simplified implementation from Stephen's article, rather than the one from the ParallelExtensionsExtras project,
    /// but I did implement the properties and methods from the ParallelExtensionsExtras project version that I found useful.</remarks>
    public sealed class ObjectPool<T>
    {
        private readonly Func<T> generator;

        private readonly IProducerConsumerCollection<T> objects;

        public ObjectPool(Func<T> generator, IProducerConsumerCollection<T> collection)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            if (collection == null)
                throw new ArgumentNullException("collection");

            this.generator = generator;
            this.objects = collection;
        }

        public ObjectPool(Func<T> generator) :
            this(generator, new ConcurrentQueue<T>()) { }

        public int Count { get { return objects.Count; } }

        public T GetObject()
        {
            T value;
            return objects.TryTake(out value) ? value : generator();
        }

        public void PutObject(T item)
        {
            objects.TryAdd(item);
        }

        /// <summary>Clears the object pool, returning all of the data that was in the pool.</summary>
        /// <returns>An array containing all of the elements in the pool.</returns>
        public T[] ToArrayAndClear()
        {
            var items = new List<T>();
            T value;
            while (objects.TryTake(out value)) items.Add(value);
            return items.ToArray();
        }
    }
}
