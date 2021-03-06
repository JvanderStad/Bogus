﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Bogus.Extensions.Brazil;
using Bogus.Extensions.Canada;
using Bogus.Extensions.Denmark;
using Bogus.Extensions.Finland;
using Bogus.Extensions.UnitedStates;
using FluentAssertions;
using NUnit.Framework;

namespace Bogus.Tests
{
    public class PersonTest: SeededTest
    {
        public class User
        {
            public string FirstName { get; set; }
            public string Email { get; set; }
            public string LastName { get; set; }
        }

        [Test]
        public void new_person_on_every_generate()
        {
            var faker = new Faker<User>()
                .RuleFor(b => b.Email, f => f.Person.Email)
                .RuleFor(b => b.FirstName, f => f.Person.FirstName)
                .RuleFor(b => b.LastName, f => f.Person.LastName);

            var fakes = faker.Generate(3).ToList();

            fakes.Select(f => f.Email).Distinct().Count().Should().Be(3);
            fakes.Select(f => f.FirstName).Distinct().Count().Should().Be(3);
            fakes.Select(f => f.LastName).Distinct().Count().Should().Be(3);
        }


        [Test]
        public void check_ssn_on_person()
        {
            var p = new Person();
            p.Ssn().Should().Be("778-69-2879");
        }

        [Test]
        public void can_generate_valid_sin()
        {
            var obtained = Get(10, p => p.Sin());

            var truth = new[]
                {
                    "788 391 886",
                    "465 826 378",
                    "059 694 794",
                    "388 842 841",
                    "254 884 844",
                    "699 577 375",
                    "001 827 872",
                    "248 908 691",
                    "069 387 884",
                    "108 829 094",
                };
            obtained.Should().Equal(truth);
        }

        [Test]
        public void can_generate_cpf_for_brazil()
        {

            var obtained = Get(10, p => p.Cpf());


            var expect = new[]
                {
                    "778.692.879-03",
                    "357.233.595-76",
                    "019.398.798-84",
                    "273.787.448-32",
                    "214.788.888-57",
                    "699.175.356-40",
                    "001.725.853-76",
                    "337.805.979-69",
                    "361.678.676-23",
                    "094.805.520-00"
                };

            obtained.Should().Equal(expect);

        }

        [Test]
        public void can_generate_cpr_nummer_for_denmark()
        {
            var p = new Person();
            var obtained = p.Cpr();

            obtained.Dump();

            var a = obtained.Split('-')[0];
            var b = obtained.Split('-')[1];

            a.Length.Should().Be(6);
            b.Length.Should().Be(4);
        }

        [Test]
        public void can_generate_henkilötunnus_for_finland()
        {
            var p = new Person();
            var obtained = p.Henkilötunnus();

            var a = obtained.Split('-')[0];
            var b = obtained.Split('-')[1];

            a.Length.Should().Be(6);
            b.Length.Should().Be(4);
        }

        IEnumerable<string> Get(int times, Func<Person, string> a)
        {
            return Enumerable.Range(0, times)
                .Select(i =>
                    {
                        var p = new Person();
                        return a(p);
                    }).ToArray();
        }
    }

}