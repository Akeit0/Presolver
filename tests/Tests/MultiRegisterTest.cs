using NUnit.Framework;
using static Presolver.Tests.MultiRegisterTest;

namespace Presolver.Tests
{
    [GenerateResolver]
    public sealed partial class MultiRegisterTestContainer : ContainerBase, ISingleton<IInterface1, IInterface2, A, A>
    {
        [Factory] Transient<IInterface3,IInterface4,B,B> GetB()=>new B();
    }

    public class MultiRegisterTest
    {
        public interface IInterface1
        {
        }

        public interface IInterface2
        {
        }

        public class A : IInterface1, IInterface2
        {
        }
        public interface IInterface3
        {
        }

        public interface IInterface4
        {
        }
        
        public class B : IInterface3, IInterface4
        {
        }

        [Test]
        public void Test()
        {
            var container = new MultiRegisterTestContainer();
            var a = container.Resolve<A>();
            var i1 = container.Resolve<IInterface1>();
            var i2 = container.Resolve<IInterface2>();
            Assert.True(a == i1 && a == i2 && i1 == i2);
            var b=container.Resolve<B>();
            var i3=container.Resolve<IInterface3>();
           var i4= container.Resolve<IInterface4>();
           Assert.True(b!=i3 && b!=i4 && i3!=i4);
           
            
        }
    }
}