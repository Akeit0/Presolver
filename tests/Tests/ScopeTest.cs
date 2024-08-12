using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using static Presolver.Tests.ScopeTest;
namespace Presolver.Tests
{
    [GenerateResolver]
    public sealed partial class ScopeTestContainer:ContainerBase,ISingleton<A>,ITransient<B>
    {
        [Factory] Transient<string> GetString()=> "Hello";
        
        [Factory] Scoped<A> GetA(ContainerBase c,string s)=>new A(c,s);
    }
    [GenerateResolver]
    public sealed partial class ScopeTestChildContainer:ChildContainer<ScopeTestContainer>//,ITransient<A>
    {
        public ScopeTestChildContainer(ScopeTestContainer parent) : base(parent)
        {
        }
        
        [Factory] Transient<string> GetString()=> "World";
        [Factory] Transient<A> GetA(string s)=>new A(this,s);
      
    }
    
    public class ScopeTest
    {
        public class A
        {
            static int counter = 0;
            public int Id { get; } = counter++;
            public string ContainerType { get; }
            public string Value { get; }
            public A(ContainerBase container,string value)
            {
                ContainerType = container.GetType().Name;
                Value = value;
            }
        }
        public class B
        {
            public ImmutableArray<A> SortedById { get; }
            public B(IReadOnlyList<A> readOnlyList)
            {
                SortedById = readOnlyList.OrderBy(x=>x.Id).ToImmutableArray();
            }
        }
        [Test]
        public void Test()
        {
            var container = new ScopeTestContainer();
            var childContainer = new ScopeTestChildContainer(container);
            
            var b1 =container.Resolve<B>();
            
            Assert.True(b1.SortedById.Select(x=>x.Id).SequenceEqual(new int[]{0,1}));
            
            var b2=childContainer.Resolve<B>();
            
          
            Assert.True(b2.SortedById[0] is {ContainerType:nameof(ScopeTestContainer),Value:"Hello"});
            Assert.True(b2.SortedById[1] is {ContainerType:nameof(ScopeTestChildContainer),Value:"World"});
           Assert.True(b2.SortedById[2] is {ContainerType:nameof(ScopeTestChildContainer),Value:"World"});
            
            Assert.True(b2.SortedById.Select(x=>x.Id).SequenceEqual(new[]{0,2,3}));
            
            var childOnly=childContainer.ResolveAll<A>(false);
            foreach (var a in childOnly)
            {
               Assert.True(a.ContainerType==nameof(ScopeTestChildContainer));
            }
            Assert.True(childOnly.Select(x=>x.Id).Order().SequenceEqual(new[]{3,4}));
        }
    }
}