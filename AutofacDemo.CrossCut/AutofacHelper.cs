using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AutofacDemo.CrossCut
{
    public static class AutofacHelper
    {
        private static AutofacServiceProvider? _autofacServiceProvider;
        private static ContainerBuilder? _containerBuilder;
        private static IContainer? _container;
        private static readonly object Lock = new object();

        static AutofacHelper()
        {
            AppDomain.CurrentDomain.ProcessExit +=
                AutofacHelper_Dtor;
        }

        private static void AutofacHelper_Dtor(object? sender, EventArgs e)
        {
            if (_autofacServiceProvider != null)
            {
                _autofacServiceProvider.Dispose();
                _autofacServiceProvider = null;
            }
        }

        public static IServiceProvider GetServiceProvider(string namespaceFilter,
            IServiceCollection? serviceCollection = null, bool includeServiceCollection = true)
        {
            lock (Lock)
            {
                if (_autofacServiceProvider == null)
                {
                    // https://autofaccn.readthedocs.io/en/latest/integration/netcore.html

                    var container = GetAutofacContainer(namespaceFilter, serviceCollection, includeServiceCollection);
                    _autofacServiceProvider = new AutofacServiceProvider(container);
                }
            }

            return _autofacServiceProvider;
        }


        public static IContainer GetAutofacContainer(string namespaceFilter,
            IServiceCollection? serviceCollection = null, bool includeServiceCollection = true)
        {
            lock (Lock)
            {
                if (_container == null)
                {
                    _containerBuilder =
                        GetAutofacContainerBuilder(namespaceFilter, serviceCollection, includeServiceCollection);
                    _container = _containerBuilder.Build();
                }
            }

            return _container;
        }

        public static ContainerBuilder GetAutofacContainerBuilder(string namespaceFilter,
            IServiceCollection? serviceCollection = null, bool includeServiceCollection = true
        )
        {
            lock (Lock)
            {
                if (_containerBuilder == null)
                {
                    _containerBuilder = new ContainerBuilder();

                    if (includeServiceCollection)
                    {
                        if (serviceCollection == null) serviceCollection = new ServiceCollection();
                        _containerBuilder.Populate(serviceCollection);
                    }

                    ConfigureAutofacContainer(_containerBuilder, Assembly.GetExecutingAssembly(), namespaceFilter);
                }
            }

            return _containerBuilder;
        }


        public static void ConfigureAutofacContainer(ContainerBuilder builder, Assembly locationAssembly,
            string namespaceFilter, bool filterNamespaceAndDllNames = true)
        {
            var assembliesToScan = AssembliesToScan(locationAssembly, namespaceFilter, filterNamespaceAndDllNames);

            builder.RegisterAssemblyModules(assembliesToScan);

            // https://github.com/cleancodelabs/AutoMapper.Contrib.Autofac.DependencyInjection
            builder.RegisterAutoMapper(assembliesToScan);

            // Register the Command's Validators (Validators based on FluentValidation library)
            builder.RegisterAssemblyTypes(assembliesToScan)
                .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                .AsImplementedInterfaces();
        }

        public static Assembly[] AssembliesToScan(Assembly locationAssembly,
            string namespaceFilter,
            bool filterDllNames = true)
        {
            if (locationAssembly == null) throw new ArgumentNullException(nameof(locationAssembly));

            var dir = new FileInfo(locationAssembly.Location).Directory;

            var loadedAssembliesFiles =
                AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => new FileInfo(a.Location))
                    .ToList();

            var files = dir?.EnumerateFiles("*.dll")
                .Where(a => filterDllNames ||
                            a.Name.Contains(namespaceFilter, StringComparison.InvariantCulture)).ToList();

            var filesToLoad = files?.Select(a => a.FullName).Except(loadedAssembliesFiles.Select(a => a.FullName))
                .ToList();

            filesToLoad.AsParallel()
                .ForAll(a => Assembly.LoadFrom(a));

            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains(namespaceFilter, StringComparison.InvariantCulture) == true)
                .ToArray();

            return assembliesToScan;
        }
    }
}