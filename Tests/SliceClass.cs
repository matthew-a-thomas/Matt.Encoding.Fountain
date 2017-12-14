namespace Tests
{
    using Matt.Encoding.Fountain;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceClass
    {
        [TestMethod]
        public void ConstructorDoesNotPassAlongArrayReferences()
        {
            var coefficients = new bool[0];
            var data = new byte[0];
            var slice = new Slice(coefficients: coefficients, data: data);
            Assert.AreNotSame(coefficients, slice.Coefficients);
            Assert.AreNotSame(data, slice.Data);
        }
    }
}