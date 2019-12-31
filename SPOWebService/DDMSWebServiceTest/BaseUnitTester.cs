using AutoFixture;
using Ploeh.AutoFixture;
using System;

namespace SPOServiceUnitTests
{
    public abstract class BaseUnitTester
    {
        protected Fixture Fixture;

        protected const int Once = 1;
        protected const int Twice = 2;

        protected BaseUnitTester()
        {
            Fixture = new Fixture();
            var suffixGenerator = new StringGenerator(() => @"_" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5));
            Fixture.Customizations.Add(suffixGenerator);
        }
    }

    public abstract class BaseUnitTester<TContract, TSystemUnderTest> : BaseUnitTester where TSystemUnderTest : class, TContract
    {
        public SystemUnderTestFactory<TContract, TSystemUnderTest> SutFactory { get; set; }

        protected BaseUnitTester()
        {
            SutFactory = new SystemUnderTestFactory<TContract, TSystemUnderTest>();
        }
    }

    public abstract class BaseUnitTester<TSystemUnderTest> : BaseUnitTester<TSystemUnderTest, TSystemUnderTest> where TSystemUnderTest : class { }
}
