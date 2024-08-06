using System;
using System.Collections.Generic;
using NUnit.Framework;
using static Presolver.Tests.DisposableTests;
namespace Presolver.Tests
{
    
    [GenerateResolver]
    public partial class DisposableTestsContainer : ContainerBase, IScoped<IDisposable, A>, ISingleton<HashSet<IDisposable>>
    {
        [Instance(InstanceOptions.AddToContainer)]
        B B { get; } = new B();
        
        [Instance]
        C C { get; } = new C();
    }

    public  class DisposableTests
    {
        public class A : IDisposable
        {
            readonly HashSet<IDisposable> disposables;

            public A(HashSet<IDisposable> disposables)
            {
                this.disposables = disposables;
                disposables.Add(this);
            }

            public void Dispose()
            {
                disposables.Remove(this);
            }
        }

        public class B : IDisposable
        {
            public bool IsDisposed { get; private set; } = false;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
        public class C : IDisposable
        {
            public bool IsDisposed { get; private set; } = false;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
        
        
        [Test]
        public void Test1()
        {
            DisposableTestsContainer disposableTestsContainer = new();
            var b = disposableTestsContainer.Resolve<B>();
            var c = disposableTestsContainer.Resolve<C>();
            var set = disposableTestsContainer.Resolve<HashSet<IDisposable>>();
            Assert.That(set.Count, Is.EqualTo(0));
            disposableTestsContainer.Resolve<IDisposable>();
            var list = disposableTestsContainer.ResolveAll<IDisposable>();
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(set.Count, Is.EqualTo(1));
            var scope = disposableTestsContainer.CreateScope();
            scope.Resolve<IDisposable>();
            Assert.That(set.Count, Is.EqualTo(2));
            scope.Dispose();
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.False(b.IsDisposed);
            disposableTestsContainer.Dispose();
            Assert.That(set.Count, Is.EqualTo(0));
            Assert.True(b.IsDisposed);
            Assert.False(c.IsDisposed);
        }
    }
}