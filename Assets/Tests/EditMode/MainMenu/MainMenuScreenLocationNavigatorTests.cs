using Code.MainMenu;
using NUnit.Framework;

namespace Code.Tests {

    public class MainMenuScreenLocationNavigatorTests {

        [Test]
        public void StartsOnMainMenu() {
            var navigator = new MainMenuScreenLocationNavigator();

            Assert.AreEqual(MainMenuScreen.MainMenu, navigator.GetCurrentScreen());
        }

        [Test]
        public void SetsLocation() {
            var navigator = new MainMenuScreenLocationNavigator();

            navigator.SetLocation(MainMenuScreen.JoinLobby);

            Assert.AreEqual(MainMenuScreen.JoinLobby, navigator.GetCurrentScreen());
        }

        [Test]
        public void ResetGoesBackToMainMenu() {
            var navigator = new MainMenuScreenLocationNavigator();
            navigator.SetLocation(MainMenuScreen.HostLobby);

            navigator.ResetLocation();

            Assert.AreEqual(MainMenuScreen.MainMenu, navigator.GetCurrentScreen());
        }
    }
}
