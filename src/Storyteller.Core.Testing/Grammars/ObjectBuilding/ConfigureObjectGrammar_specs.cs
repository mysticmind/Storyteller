﻿using System.Linq;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Storyteller.Core.Grammars;
using Storyteller.Core.Grammars.ObjectBuilding;
using Storyteller.Core.Model;
using Storyteller.Core.Testing.Samples.Fixtures;

namespace Storyteller.Core.Testing.Grammars.ObjectBuilding
{
    [TestFixture]
    public class ConfigureObjectGrammar_specs : SpecRunningContext
    {
        [Test]
        public void execution()
        {
            execute(@"
=> ConfigureObject
* SetProps: data=Jasper-Missouri
* CityIs#1: city=Jasper
* StateIs#2: state=Missouri
");

            CountsShouldBe(2, 0, 0, 0);

            Step("1").Cell("city").Succeeded();
            Step("2").Cell("state").Succeeded();
        }

        [Test]
        public void building_up_the_cell()
        {
            var cell = ModelFor<Sentence>("ConfigureObject", "SetProps")
                .cells.Single();

            cell.Key.ShouldEqual("data");
            cell.Type.ShouldEqual(typeof (string));
            cell.DefaultValue.ShouldEqual("Austin-Texas");
        }
    }

    public class ConfigureObjectFixture : Fixture
    {
        public ConfigureObjectFixture()
        {
            this["SetProps"] = new ConfigureObjectGrammar<Address, string>("data", (a, data) =>
            {
                var values = data.Split('-');
                a.City = values.First();
                a.StateOrProvince = values.Last();
            }).Default("Austin-Texas");
        }

        [FormatAs("The city should be {city}")]
        public string CityIs()
        {
            return Context.State.CurrentObject.As<Address>().City;
        }

        [FormatAs("The city should be {state}")]
        public string StateIs()
        {
            return Context.State.CurrentObject.As<Address>().StateOrProvince;
        }



        public override void SetUp()
        {
            Context.State.CurrentObject = new Address();
        }
    }
}