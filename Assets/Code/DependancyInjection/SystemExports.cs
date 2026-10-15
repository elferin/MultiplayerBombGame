using System;
using VContainer;

namespace Code.DependancyInjection {

    public class SystemExports {

        private readonly IContainerBuilder _builder;
        private readonly Func<IObjectResolver, IObjectResolver> _getSystemScope;

        public SystemExports(IContainerBuilder builder, Func<IObjectResolver, IObjectResolver> getSystemScope) {
            _builder = builder;
            _getSystemScope = getSystemScope;
        }

        // Makes T, as registered in the system's internal scope, resolvable from the outer container.
        public void Expose<T>() {
            _builder.Register(resolver => _getSystemScope(resolver).Resolve<T>(), Lifetime.Singleton);
        }

        // Runs once the container is built, with this system's internal scope and the outer container.
        public void OnContainerBuilt(Action<IObjectResolver, IObjectResolver> callback) {
            _builder.RegisterBuildCallback(container => callback(_getSystemScope(container), container));
        }
    }
}
