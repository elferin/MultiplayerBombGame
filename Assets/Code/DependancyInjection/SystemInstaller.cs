using VContainer;

namespace Code.DependancyInjection {

    public abstract class SystemInstaller : ISystemInstaller {

        public void Install(IContainerBuilder builder) {
            // Kept per Install call so one installer instance can't share a scope between containers.
            IScopedObjectResolver internalScope = null;

            IObjectResolver GetSystemScope(IObjectResolver resolver) {
                return internalScope ??= resolver.CreateScope(InstallInternal);
            }

            // VContainer doesn't dispose child scopes with their parent.
            builder.RegisterDisposeCallback(_ => internalScope?.Dispose());

            InstallPublic(new SystemExports(builder, GetSystemScope));
        }

        // Registrations visible only inside this system.
        protected abstract void InstallInternal(IContainerBuilder builder);

        // Types from the internal scope that other systems may resolve.
        protected abstract void InstallPublic(SystemExports exports);
    }
}
