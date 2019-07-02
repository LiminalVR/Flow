// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow.Impl
{
    internal class Future<T>
        : Transient
        , IFuture<T>
    {
        public event FutureHandler<T> Arrived;

        public bool Available { get; private set; }

        public T Value
        {
            get
            {
                if (!Available)
                    throw new FutureNotSetException();

                return _value;
            }
            set
            {
                if (Available)
                    throw new FutureAlreadySetException(Name);

                _value = value;
                Available = true;

                Arrived?.Invoke(this);

                Dispose();
            }
        }

        private T _value;
    }
}
