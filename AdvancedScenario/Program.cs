using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Extras.AggregateService;
using Autofac.Extras.AttributeMetadata;
using Autofac.Extras.DynamicProxy;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Autofac.Features.ResolveAnything;
using Castle.DynamicProxy;

namespace AdvancedScenario
{
    public class Program
    {
        public interface ICanSpeak
        {
            void Speak();
        }

        public class Person : ICanSpeak
        {
            public void Speak()
            {
                Console.WriteLine("HELLO!");
            }
        }

        public abstract class BaseHandler
        {
            public virtual string Handle(string message)
            {
                return $"Handled: {message}";
            }
        }

        public class HandlerA : BaseHandler
        {
            public override string Handle(string message)
            {
                return $"Handled by A: {message}";
            }
        }

        public class HandlerB : BaseHandler
        {
            public override string Handle(string message)
            {
                return $"Handled by B: {message}";
            }
        }

        public interface IHandlerFactory
        {
            T GetHandler<T>() where T : BaseHandler;
        }

        public class HandlerFactory : IHandlerFactory
        {
            public T GetHandler<T>() where T : BaseHandler
            {
                return Activator.CreateInstance<T>();
            }
        }

        public class ConsumerA
        {
            private HandlerA handlerA;

            public ConsumerA(HandlerA handlerA)
            {
                this.handlerA = handlerA;
            }

            public void DoWork()
            {
                Console.WriteLine(handlerA.Handle("ConsumerA"));
            }
        }

        public class ConsumerB
        {
            private HandlerB handlerB;

            public ConsumerB(HandlerB handlerB)
            {
                this.handlerB = handlerB;
            }

            public void DoWork()
            {
                Console.WriteLine(handlerB.Handle("ConsumerB"));
            }
        }

        public class HandlerRegistrationSource : IRegistrationSource
        {
            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
            {
                var swt = service as IServiceWithType;
                if (swt == null
                    || swt.ServiceType == null
                    || !swt.ServiceType.IsAssignableTo<BaseHandler>())
                {
                    yield break;
                }

                yield return new ComponentRegistration(
                    Guid.NewGuid(),
                    new DelegateActivator(
                        swt.ServiceType,
                        (c, p) =>
                        {
                            var provider = c.Resolve<IHandlerFactory>();
                            var method = provider
                                .GetType()
                                .GetMethod("GetHandler")
                                .MakeGenericMethod(swt.ServiceType);
                            return method.Invoke(provider, null);
                        }),
                    new CurrentScopeLifetime(),
                    InstanceSharing.None,
                    InstanceOwnership.OwnedByLifetimeScope,
                    new[] {service},
                    new ConcurrentDictionary<string, object>());
            }

            public bool IsAdapterForIndividualComponents => false;
        }

        public interface ICommand
        {
            void Execute();
        }

        public class SaveCommand : ICommand
        {
            public void Execute()
            {
                Console.WriteLine("Saving a file");
            }
        }

        public class OpenCommand : ICommand
        {
            public void Execute()
            {
                Console.WriteLine("Opening a file");
            }
        }

        public class Button
        {
            private ICommand command;
            private string name;

            public Button(ICommand command, string name)
            {
                this.command = command;
                this.name = name;
            }

            public void Click()
            {
                command.Execute();
                ;
            }

            public void PrintMe()
            {
                Console.WriteLine($"Name: {name}");
            }
        }

        public class Editor
        {
            public IEnumerable<Button> Buttons { get; }

            public Editor(IEnumerable<Button> buttons)
            {
                Buttons = buttons;
            }

            public void ClickAll()
            {
                foreach (var button in Buttons)
                {
                    button.Click();
                }
            }
        }

        public interface IReportingService
        {
            void Report();
        }

        public class ReportingService : IReportingService
        {
            public void Report()
            {
                Console.WriteLine("Here is your report");
            }
        }

        public class ReportingServiceWIthLogging : IReportingService
        {
            private IReportingService decorated;

            public ReportingServiceWIthLogging(IReportingService decorated)
            {
                this.decorated = decorated;
            }

            public void Report()
            {
                Console.WriteLine("Starting log");
                decorated.Report();
                Console.WriteLine("Ending log");
            }
        }

        public class ParentWithProperty
        {
            public ChildWithProperty Child { get; set; }

            public override string ToString()
            {
                return "Parent";
            }
        }

        public class ChildWithProperty
        {
            public ParentWithProperty Parent { get; set; }

            public override string ToString()
            {
                return "Child";
            }
        }

        public class ParentWithConstructor1
        {
            public ChildWithProperty1 Child;

            public ParentWithConstructor1(ChildWithProperty1 child)
            {
                Child = child;
            }

            public override string ToString()
            {
                return "Parent with a ChildWithProperty";
            }
        }

        public class ChildWithProperty1
        {
            public ParentWithConstructor1 Parent { get; set; }

            public override string ToString()
            {
                return "Child";
            }
        }

        [MetadataAttribute]
        public class AgeMetadataAttribute : Attribute
        {
            public int Age { get; set; }

            public AgeMetadataAttribute(int age)
            {
                Age = age;
            }
        }

        public interface IArtwork
        {
            void Display();
        }

        [AgeMetadata(100)]
        public class CenturyArtwork : IArtwork
        {
            public void Display()
            {
                Console.WriteLine("Displaying a century old piece");
            }
        }

        [AgeMetadata(1000)]
        public class MillenniumArtwork : IArtwork
        {
            public void Display()
            {
                Console.WriteLine("Displaying a really old piece of art");
            }
        }

        public class ArtDisplay
        {
            private IArtwork artwork;

            public ArtDisplay([MetadataFilter("Age", 100)] IArtwork artwork)
            {
                this.artwork = artwork;
            }

            public void Display() => artwork.Display();
        }

        #region Aggregate services
        
        public interface IService1
        {
        }

        public interface IService2
        {
        }

        public interface IService3
        {
        }

        public interface IService4
        {
        }

        public class Service1 : IService1
        {
        }

        public class Service2 : IService2
        {
        }

        public class Service3 : IService3
        {
        }

        public class Service4 : IService4
        {
            private string name;

            public Service4(string name)
            {
                this.name = name;
            }
        }
        
        public interface IMyAggregateService
        {
            IService1 Service1 { get; }
            IService2 Service2 { get; }
            IService3 Service3 { get; }
            // IService4 Service4 { get; 
            IService4 GetFourthService(string name);
        }

        public class Consumer
        {
            public IMyAggregateService AllServices;

            public Consumer(IMyAggregateService allServices)
            {
                AllServices = allServices;
            }
        }
        
        #endregion

        #region Type interceptors
        
        public class CallLogger : IInterceptor
        {
            private TextWriter output;

            public CallLogger(TextWriter output)
            {
                this.output = output;
            }
            
            public void Intercept(IInvocation invocation)
            {
                var methodName = invocation.Method.Name;
                output.WriteLine("Calling method {0} with args {1}", 
                    methodName, 
                    string.Join(",", invocation.Arguments.Select(a => (a?? "").ToString())));
                
                invocation.Proceed();
                output.WriteLine("Done calling {0}, result was {1}",
                    methodName, invocation.ReturnValue);
            }
        }
        
        public interface IAudit
        {
            int Start(DateTime reportDate);
        }

        [Intercept(typeof(CallLogger))]
        public class Audit : IAudit
        {
            public virtual int Start(DateTime reportDate)
            {
                Console.WriteLine($"starting report on {reportDate}");
                return 42;
            }
        }
        
        #endregion
        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new CallLogger(Console.Out))
                .As<IInterceptor>()
                .AsSelf();
            builder.RegisterType<Audit>()
                .EnableClassInterceptors();
            using var container = builder.Build();
            var audit = container.Resolve<Audit>();
            audit.Start(DateTime.Now);

        }
    }
}