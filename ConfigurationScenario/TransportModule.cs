using System;
using Autofac;

namespace ConfigurationScenario
{
    interface IVehicle
    {
        void Go();
    }

    class Truck : IVehicle
    {
        private IDriver driver;

        public Truck(IDriver driver)
        {
            this.driver = driver;
        }

        public void Go()
        {
            driver.Drive();
        }
    }

    interface IDriver
    {
        void Drive();
    }

    class SaneDriver : IDriver
    {
        public void Drive()
        {
            Console.WriteLine("Driving safely to destination");
        }
    }

    class CrazyDriver : IDriver
    {
        public void Drive()
        {
            Console.WriteLine("Going too fast and crashing into a tree");
        }
    }

    class TransportModule : Module
    {
        public bool ObeySpeedLimit { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (ObeySpeedLimit)
            {
                builder.RegisterType<SaneDriver>().As<IDriver>();
            }
            else
            {
                builder.RegisterType<CrazyDriver>().As<IDriver>();
            }

            builder.RegisterType<Truck>().As<IVehicle>();
        }
    }
}