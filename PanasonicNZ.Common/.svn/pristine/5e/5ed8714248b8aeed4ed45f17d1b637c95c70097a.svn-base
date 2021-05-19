using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PanasonicNZ.Common
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
        where T: class
    {
        Func<T, T, int> comparer;
        IList<T> originalList;

        public SortedObservableCollection()
            : base()
        {
            this.comparer = DefaultComparer;
            this.originalList = new List<T>();
        }

        public SortedObservableCollection(IList<T> list)
            : this(list, null)
        {
        }

        public SortedObservableCollection(IList<T> list, Func<T, T, int> comparer)
            : base()
        {
            this.originalList = list;
            this.comparer = comparer ?? DefaultComparer;

            List<T> sorted = new List<T>(list);
            sorted.Sort((x, y) => this.comparer(x, y));

            foreach (var item in sorted)
                base.InsertItem(Count, item);
        }

        protected override void SetItem(int index, T item)
        {
            if(index > Count)
            {
                // Adding to the list, need to call InsertItem
                this.InsertItem(index, item);
            }
            else if (comparer(this[index], item) == 0)
            {
                var originalIndex = originalList.IndexOf(this[index]);
                
                // No change to sort order
                base.SetItem(index, item);

                // If the object instance has changed, then replace the item in the original list
                if (originalIndex < 0)
                    originalList.Add(item);
                else if(originalList[originalIndex] != item)
                    originalList[originalIndex] = item;
            }
            else
            {
                // Sort order has changed, so remove and re-insert the item
                base.RemoveAt(index);
                this.InsertItem(0, item);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            // Default location is at the end of the list
            index = Count;
            
            // Ignore the given index and determine where it should go, according to IComparible results
            for (int i = 0; i < Count; i++)
            {
                if (this.comparer(item, this[i]) <= 0)
                {
                    index = i;
                    break;
                }
            }

            base.InsertItem(index, item);
            originalList.Add(item);
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            base.RemoveItem(index);

            index = originalList.IndexOf(item);
            if(index >= 0)
                originalList.RemoveAt(index);
        }

        public static int DefaultComparer(T a, T b)
        {
            return Comparer<T>.Default.Compare(a, b);
        }

    }
}
