﻿using AutoDI.AssemblyGenerator;
using AutoDI.Fody.Tests.ResolveTestsNamespace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoDI.Fody.Tests
{
    [TestClass]
    public class ResolverTests
    {
        private static Assembly _testAssembly;

        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            var gen = new Generator();

            _testAssembly = (await gen.Execute()).SingleAssembly();

            DI.Init(_testAssembly);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            DI.Dispose();
        }

        private object Resolve<T>()
        {
            string genericTypeName = TypeMixins.GetTypeName(typeof(T), GetType());
            Type resolveType = _testAssembly.GetType(genericTypeName);
            return DI.Global.GetService(resolveType);
        }

        [TestMethod]
        public void CanResolveSingleInterfaceImplementationsByInterface()
        {
            Assert.IsTrue(Resolve<IService>().Is<Service>(GetType()));
        }

        [TestMethod]
        public void CanResolveSingleInterfaceImplementationsByClass()
        {
            Assert.IsNotNull(Resolve<Service>());
        }

        [TestMethod]
        public void CanResolveSingletonInstance()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(ReferenceEquals(Resolve<IService>(), Resolve<IService>()));
        }

        [TestMethod]
        public void CanResolveSingletonByClassOrInterface()
        {
            var @interface = Resolve<IService>();
            var @class = Resolve<Service>();
            Assert.IsNotNull(@interface);
            Assert.IsNotNull(@class);
            Assert.IsTrue(ReferenceEquals(@interface, @class));
        }

        [TestMethod]
        public void CanResolveSingletonByAnyInterface()
        {
            var interface1 = Resolve<IService>();
            var interface2 = Resolve<IService2>();
            Assert.IsNotNull(interface1);
            Assert.IsNotNull(interface2);
            Assert.IsTrue(ReferenceEquals(interface1, interface2));
        }

        [TestMethod]
        public void CanResolveClassByBaseType()
        {
            //This works because it is the only derived class from Base2
            Assert.IsTrue(Resolve<Base2>().Is<Derived2>(GetType()));
        }

        [TestMethod]
        public void CannotResolveClassByBaseTypeIfThereAreMultipleDerivedClasses()
        {
            Assert.IsNull(Resolve<Base>());
        }
    }

    //<assembly>
    //<ref: AutoDI />
    //<weaver: AutoDI />
    namespace ResolveTestsNamespace
    {
        public interface IService
        { }

        public interface IService2
        { }

        public class Service : IService, IService2
        { }

        public abstract class Base
        { }

        public abstract class Base2 : Base
        { }

        public class Derived1 : Base
        { }

        public class Derived2 : Base2
        { }
    }
    //</assembly>
}
