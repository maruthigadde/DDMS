using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit2;

namespace SPOServiceUnitTests
{
    [ExcludeFromCodeCoverage]
    public class SystemUnderTestFactory<TContract, TSystemUnderTest> where TSystemUnderTest : class, TContract
    {
        private Func<TSystemUnderTest> _factory;
        private Action _preProcess = delegate { };
        private Action<TContract> _postProcess = delegate { };

        private readonly ConstructorInfo _ctor;
        private readonly IList<Tuple<Type, object>> _parameters = new List<Tuple<Type, object>>();

        private TContract _sut;
        public TContract Sut
        {
            get
            {
                CreateSut();
                return _sut;
            }
        }

        public SystemUnderTestFactory()
        {
            _ctor = GetGreediestCtor<TSystemUnderTest>();
            _factory = DefaultFactory;
            InjectDefaultDependencies();
        }

        private void InjectDefaultDependencies()
        {
            var fixture = new Fixture();
            foreach (var type in _ctor.GetParameters().Select(parameterInfo => parameterInfo.ParameterType))
            {
                if (type == typeof(string))
                    _parameters.Add(new Tuple<Type, object>(type, fixture.Create<string>()));
                else if (type.IsValueType)
                    _parameters.Add(new Tuple<Type, object>(type, Activator.CreateInstance(type)));
                else if (type.IsArray)
                    _parameters.Add(new Tuple<Type, object>(type, Substitute.For(new[] { type.GetElementType() }, null)));
                else if (type.IsClass)
                    _parameters.Add(new Tuple<Type, object>(type, new SpecimenContext(fixture).Resolve(type)));
                else
                    _parameters.Add(new Tuple<Type, object>(type, Substitute.For(new[] { type }, null)));
            }
        }

        private TSystemUnderTest DefaultFactory()
        {
            return (TSystemUnderTest)_ctor.Invoke(_parameters.Select(x => x.Item2).ToArray());
        }

        private static ConstructorInfo GetGreediestCtor<T>()
        {
            return typeof(T).GetConstructors()
                            .OrderByDescending(x => x.GetParameters().Length)
                            .First();
        }

        bool DependencyExistsFor<TDependency>()
        {
            return _parameters.Any(x => x.Item1 == typeof(TDependency));
        }

        public void CreateSut()
        {
            if (_sut != null) return;

            _preProcess();
            _sut = _factory();
            _postProcess(_sut);
        }

        public void CreateSutUsing(Func<TSystemUnderTest> factory)
        {
            _factory = factory;
        }

        public void BeforeSutCreated(Action sutPreProcessor)
        {
            if (_sut != null)
                throw new InvalidOperationException(@"Sut has already been created.");

            _preProcess = sutPreProcessor;
        }

        public void AfterSutCreated(Action<TContract> sutPostProcessor)
        {
            if (_sut != null)
                throw new InvalidOperationException(@"Sut has already been created.");

            _postProcess = sutPostProcessor;
        }

        public TDependency Dependency<TDependency>()
        {
            if (typeof(TDependency) == typeof(TSystemUnderTest))
                throw new InvalidOperationException(@"Access Sut through property.");

            if (!DependencyExistsFor<TDependency>())
                throw new InvalidOperationException(@"{typeof(TDependency).Name} is not a dependency of {typeof(TSystemUnderTest).Name}");

            return (TDependency)_parameters.First(x => x.Item1 == typeof(TDependency)).Item2;
        }

        public DoForDependency<TDependency> ForDependency<TDependency>()
        {
            if (!DependencyExistsFor<TDependency>())
                throw new InvalidOperationException(@"{typeof(TDependency).Name} is not a dependency of {typeof(TSystemUnderTest).Name}");

            return new DoForDependency<TDependency>(_parameters);
        }

        public class DoForDependency<TDependency>
        {
            readonly IList<Tuple<Type, object>> _parameters;

            public DoForDependency(IList<Tuple<Type, object>> parameters)
            {
                _parameters = parameters;
            }

            public void Use(TDependency dependency)
            {
                var tuple = _parameters.First(x => x.Item1 == typeof(TDependency));
                var index = _parameters.IndexOf(tuple);
                _parameters.RemoveAt(index);
                _parameters.Insert(index, new Tuple<Type, object>(tuple.Item1, dependency));
            }
        }
    }
}
