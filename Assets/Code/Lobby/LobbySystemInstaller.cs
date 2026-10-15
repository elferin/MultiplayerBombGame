using Code.DependancyInjection;
using PurrLobby.Providers;
using UnityEngine;
using VContainer;

namespace Code.Lobby {

    public class LobbySystemInstaller : SystemInstaller {

        private const string LOBBY_PROVIDER_NAME = "UnityLobbyProvider";

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register(_ => CreateLobbyProvider(), Lifetime.Singleton);
            // One instance for ILobbyService and each of its parts.
            builder.Register<LobbyService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LobbyJoiner>(Lifetime.Singleton);
            builder.Register<LobbyCreator>(Lifetime.Singleton);
            builder.Register<MatchJoinCodeSharer>(Lifetime.Singleton);
            builder.Register<MatchJoinCodeReceiver>(Lifetime.Singleton);
            builder.Register<LobbyStartRules>(Lifetime.Singleton);
            builder.Register<MissingPlayerRemover>(Lifetime.Singleton);

            // Lobby state, each registered as its getter, setter and combined interface.
            builder.Register<HostLobbySettings>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LobbyStatusHolder>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CurrentLobbyHolder>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<IViewDataGatherer<LobbyViewData>, LobbyViewDataGatherer>(Lifetime.Singleton);
            builder.Register<LobbyAssetLoader>(Lifetime.Singleton);
            builder.Register<ILobbyViewControllerFactory, LobbyViewControllerFactory>(Lifetime.Singleton);
            builder.Register<ILobbyFlowFactory, LobbyFlowFactory>(Lifetime.Singleton);
            builder.Register<LobbyFlowInstantiator>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.Expose<LobbyFlowInstantiator>();
            // GamePresenter passes the host's join code between the match and the lobby.
            exports.Expose<MatchJoinCodeSharer>();
            exports.Expose<MatchJoinCodeReceiver>();

            // The main menu edits the host settings, so it needs the combined interface and the getter.
            exports.Expose<IHostLobbySettings>();
            exports.Expose<IHostLobbySettingsGetter>();
        }

        private UnityLobbyProvider CreateLobbyProvider() {
            // The provider is a MonoBehaviour, so it needs a GameObject that survives scene changes.
            var providerObject = new GameObject(LOBBY_PROVIDER_NAME);
            Object.DontDestroyOnLoad(providerObject);
            return providerObject.AddComponent<UnityLobbyProvider>();
        }

    }
}
