[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ProtonAnalytics.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(ProtonAnalytics.App_Start.NinjectWebCommon), "Stop")]

namespace ProtonAnalytics.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Repositories;
    using System.Configuration;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ConnectionStringSettings>().ToConstant(ConfigurationManager.ConnectionStrings["DefaultConnection"]);
            kernel.Bind<IGenericRepository>().To<GenericRepository>();
            kernel.Bind<FeatureTogglesRepository>().ToSelf();
            kernel.Bind<NLog.ILogger>().ToMethod((context) => NLog.LogManager.GetCurrentClassLogger());

            // Make sure we creat ea feature toggle repo. Otherwise, hitting the homoe page causes a crash.
            new FeatureTogglesRepository(kernel.Get<ConnectionStringSettings>());
        }
    }
}
