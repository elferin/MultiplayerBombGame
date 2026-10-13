using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class HostLobbySettingsTests {

        private HostLobbySettings _settings;

        [SetUp]
        public void SetUp() {
            _settings = new HostLobbySettings();
        }

        [Test]
        public void StartsWithFourPlayers() {
            Assert.AreEqual("4", _settings.GetPlayerCountText());
            Assert.AreEqual(4, _settings.GetPlayerCount());
            Assert.IsTrue(_settings.IsPlayerCountValid());
        }

        [Test]
        public void AllowsTwoToEightPlayers() {
            Assert.AreEqual(2, _settings.GetMinPlayers());
            Assert.AreEqual(8, _settings.GetMaxPlayers());
        }

        [TestCase("2", 2)]
        [TestCase("8", 8)]
        [TestCase("5", 5)]
        public void AcceptsCountInRange(string text, int expected) {
            _settings.SetPlayerCount(text);

            Assert.IsTrue(_settings.IsPlayerCountValid());
            Assert.AreEqual(expected, _settings.GetPlayerCount());
        }

        [TestCase("1")]
        [TestCase("9")]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase(null)]
        public void RejectsInvalidCount(string text) {
            _settings.SetPlayerCount(text);

            Assert.IsFalse(_settings.IsPlayerCountValid());
            Assert.AreEqual(0, _settings.GetPlayerCount());
        }

        [Test]
        public void KeepsInvalidTextForTheInputField() {
            _settings.SetPlayerCount("12");

            Assert.AreEqual("12", _settings.GetPlayerCountText());
        }

        [Test]
        public void StoresNullTextAsEmpty() {
            _settings.SetPlayerCount(null);

            Assert.AreEqual(string.Empty, _settings.GetPlayerCountText());
        }
    }
}
