using System;

namespace vProto.Internals
{
    internal class Allocator<T>
        where T : class
    {
        internal T[] cont = new T[10];
        object syncer = new object();

        int count = 0;

        private void _DoubleContainer()
        {
            lock (syncer)
            {
                T[] n = new T[cont.Length * 2];

                cont.CopyTo(n, 0);

                cont = n;
            }
        }

        public int Add(T h)
        {
            if (h == null)
                throw new ArgumentNullException("Cannot add a null object.", "h");

            int pos = -1;

            lock (syncer)
                for (int i = 0; i < cont.Length; i++)
                    if (cont[i] == null)
                    {
                        pos = i;
                        break;
                    }

            if (pos == -1)
            {
                _DoubleContainer();

                return Add(h);
            }
            else
            {
                lock (syncer)
                {
                    cont[pos] = h;
                    count++;
                }

                if (h is IHazID) (h as IHazID).ID = pos;

                return /*h.ID =*/ pos;
            }
        }

        public int Remove(T h)
        {
            lock (syncer)
                if (h is IHazID)
                {
                    IHazID g = (h as IHazID);

                    if (g.ID > -1 && g.ID < cont.Length)
                    {
                        cont[g.ID] = null;
                    }
                }
                else
                    for (int i = 0; i < cont.Length; i++)
                        if (cont[i].Equals(h))
                        {
                            cont[i] = null;
                            count--;
                            return i;
                        }

            return -1;
        }

        public T RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must be non-negative.", "index");

            T ret;

            lock (syncer)
            {
                if (index >= cont.Length)
                    return null;

                ret = cont[index];
                cont[index] = null;
            }

            return ret;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentException("Index must be non-negative.", "index");

                lock (syncer)
                {
                    if (index >= cont.Length)
                        return null;

                    return cont[index];
                }
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }
    }
}
