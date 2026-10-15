using Code.DependancyInjection;
using Code.FlowSystem;
using VContainer;

namespace Code.MainMenu {

    public class MainMenuSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<MainMenuAssetLoader>(Lifetime.Singleton);

            // Menu state, registered as its getter, setter and combined interface.
            // Player name and host settings come from the lobby system.
            builder.Register<MainMenuScreenLocationNavigator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<JoinLobbyState>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<IMainMenuLobbyRules, MainMenuLobbyRules>(Lifetime.Singleton);

            builder.Register<IViewDataGatherer<MainMenuViewData>, MainMenuViewDataGatherer>(Lifetime.Singleton);
            builder.Register<IMainMenuViewControllerFactory, MainMenuViewControllerFactory>(Lifetime.Singleton);
            builder.Register<IMainMenuFlowFactory, MainMenuFlowFactory>(Lifetime.Singleton);
            builder.Register<MainMenuFlowInstantiator>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.RegisterFlow<MainMenuFlowResult, MainMenuFlowData>();
        }

    }
}
