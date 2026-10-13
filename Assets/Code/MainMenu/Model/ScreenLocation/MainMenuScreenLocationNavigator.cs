using JetBrains.Annotations;

namespace Code.MainMenu {

    [UsedImplicitly]
    public class MainMenuScreenLocationNavigator : IMainMenuScreenLocation {

        private MainMenuScreen _mainMenuScreen;

        public MainMenuScreenLocationNavigator() {
            _mainMenuScreen = MainMenuScreen.MainMenu;
        }

        public void SetLocation(MainMenuScreen currentScreen) {
            _mainMenuScreen = currentScreen;
        }

        public void ResetLocation() {
            _mainMenuScreen = MainMenuScreen.MainMenu;
        }

        public MainMenuScreen GetCurrentScreen() {
            return _mainMenuScreen;
        }
    }
}
