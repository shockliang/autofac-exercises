using System;
using Autofac;

namespace ConfigurationScenario
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new TransportModule {ObeySpeedLimit = true});
            using (var c = builder.Build())
            {
                c.Resolve<IVehicle>().Go();
            }
        }
    }
}