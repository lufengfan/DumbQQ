using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public class DataEventArgs<TData> : EventArgs
    {
        private TData data;

        public TData Data => this.data;

        public DataEventArgs(TData data)
        {
            this.data = data;
        }

        public static implicit operator DataEventArgs<TData>(TData data)
        {
            return new DataEventArgs<TData>(data);
        }

        public static implicit operator TData(DataEventArgs<TData> e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            return e.Data;
        }
    }
}
