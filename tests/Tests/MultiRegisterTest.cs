using NUnit.Framework;
using static Presolver.Tests.MultiRegisterTest;

namespace Presolver.Tests
{
    [GenerateResolver]
    public sealed partial class MultiRegisterTestContainer : ContainerBase, ISingleton<IInterface1, IInterface2, A, A>
    {
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

        [Test]
        public void Test()
        {
            var container = new MultiRegisterTestContainer();
            var a = container.Resolve<A>();
            var i1 = container.Resolve<IInterface1>();
            var i2 = container.Resolve<IInterface2>();
        }
    }
}