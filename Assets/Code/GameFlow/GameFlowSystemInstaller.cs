using Code.DependancyInjection;
using VContainer;

namespace Code.GameFlow {

    // Turns each flow's result into the next flow's data. The only system that knows the order of the game.
    public class GameFlowSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            // Registered as its getter, setter and combined interface.
            builder.Register<UsedJoinCodeHolder>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<MainMenuToLobby>(Lifetime.Singleton);
            builder.Register<LobbyToMatch>(Lifetime.Singleton);
            builder.Register<MatchToLobby>(Lifetime.Singleton);
            builder.Register<LobbyToMainMenu>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            // GamePresenter is the entry point, so it's registered in GameLifetimeScope and needs these from outside.
            exports.Expose<MainMenuToLobby>();
            exports.Expose<LobbyToMatch>();
            exports.Expose<MatchToLobby>();
            exports.Expose<LobbyToMainMenu>();
        }
    }
}
