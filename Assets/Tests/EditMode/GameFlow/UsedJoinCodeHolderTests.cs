using Code.GameFlow;
using NUnit.Framework;

namespace Code.Tests.GameFlow {

    public class UsedJoinCodeHolderTests {

        private UsedJoinCodeHolder _holder;

        [SetUp]
        public void SetUp() {
            _holder = new UsedJoinCodeHolder();
        }

        [Test]
        public void StartsEmpty() {
            Assert.AreEqual(string.Empty, _holder.GetUsedJoinCode());
        }

        [Test]
        public void StoresJoinCode() {
            _holder.SetUsedJoinCode("RELAY1");

            Assert.AreEqual("RELAY1", _holder.GetUsedJoinCode());
        }

        [Test]
        public void StoresNullAsEmpty() {
            _holder.SetUsedJoinCode(null);

            Assert.AreEqual(string.Empty, _holder.GetUsedJoinCode());
        }
    }
}
