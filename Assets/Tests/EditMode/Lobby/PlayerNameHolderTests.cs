using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class PlayerNameHolderTests {

        private FakePlayerNameStorage _storage;
        private PlayerNameHolder _holder;

        [SetUp]
        public void SetUp() {
            _storage = new FakePlayerNameStorage("Saved");
            _holder = new PlayerNameHolder(_storage);
        }

        [Test]
        public void StartsEmpty() {
            Assert.AreEqual(string.Empty, _holder.GetPlayerName());
            Assert.IsFalse(_holder.HasPlayerName());
        }

        [Test]
        public void KeepsNameAsTyped() {
            _holder.SetPlayerName("  Luka ");

            Assert.AreEqual("  Luka ", _holder.GetPlayerName());
            Assert.AreEqual("Luka", _holder.GetTrimmedPlayerName());
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void BlankNameIsNotAName(string name) {
            _holder.SetPlayerName(name);

            Assert.IsFalse(_holder.HasPlayerName());
        }

        [Test]
        public void LoadsSavedName() {
            _holder.LoadPlayerName();

            Assert.AreEqual("Saved", _holder.GetPlayerName());
        }

        [Test]
        public void SavesTrimmedName() {
            _holder.SetPlayerName("  Luka ");

            _holder.SavePlayerName();

            Assert.AreEqual("Luka", _storage.SavedName);
        }

        [Test]
        public void SavingBlankNameKeepsPreviousSave() {
            _holder.SetPlayerName("   ");

            _holder.SavePlayerName();

            Assert.AreEqual("Saved", _storage.SavedName);
        }
    }
}
