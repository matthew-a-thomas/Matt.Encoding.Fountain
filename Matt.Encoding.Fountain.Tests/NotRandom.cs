namespace Matt.Encoding.Fountain.Tests
{
    using Random;

    internal sealed class NotRandom : IRandom
    {
        private readonly byte _valueToPopulateWith;

        public NotRandom(byte valueToPopulateWith)
        {
            _valueToPopulateWith = valueToPopulateWith;
        }
        
        public void Populate(
            byte[] buffer,
            int offset,
            int count)
        {
            count += offset;
            for (; offset < count; ++offset)
                buffer[offset] = _valueToPopulateWith;
        }
    }
}