namespace Matt.Encoding.Fountain.Tests
{
    using System.Linq;
    using Fountain;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SliceExtensionsClass
    {
        [TestClass]
        public class MixMethod
        {
            [TestMethod]
            public void ClonesASingleSlice()
            {
                var slice = Slice.Create(coefficients: new bool[5], data: new byte[5]);
                var sequence = new [] { slice };
                var mixed = sequence.Mix();
                Assert.AreEqual(5, mixed.GetCoefficients().Count());
                Assert.AreEqual(5, mixed.GetData().Count());
            }
        }
        
        [TestClass]
        public class ToSlicesMethod
        {
            [TestMethod]
            public void CreatesOnlyOneSliceWhenSliceSizeIsLargerThanDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(10).ToList();
                Assert.AreEqual(slices.Count, 1);
                Assert.AreEqual(slices.Count, slices[0].GetCoefficients().Count());
                Assert.AreEqual(10, slices[0].GetData().Count());
            }
            
            [TestMethod]
            public void CreatesOnlyOneSliceWhenSliceSizeIsSameAsDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(5).ToList();
                Assert.AreEqual(slices.Count, 1);
                Assert.AreEqual(slices.Count, slices[0].GetCoefficients().Count());
                Assert.AreEqual(5, slices[0].GetData().Count());
            }
            
            [TestMethod]
            public void CreatesTwoSlicesWhenSliceSizeIsSlightlySmallerThanDataLength()
            {
                var data = new byte[5];
                var slices = data.ToSlices(4).ToList();
                Assert.AreEqual(slices.Count, 2);
                Assert.AreEqual(slices.Count, slices[0].GetCoefficients().Count());
                Assert.AreEqual(4, slices[0].GetData().Count());
                Assert.AreEqual(slices.Count, slices[1].GetCoefficients().Count());
                Assert.AreEqual(4, slices[1].GetData().Count());
            }
        }
    }
}