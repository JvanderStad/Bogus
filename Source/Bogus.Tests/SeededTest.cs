﻿using NUnit.Framework;

namespace Bogus.Tests
{
    public class SeededTest
    { 
        [SetUp]
        public void BeforeEachTest()
        {
            //set the random gen manually to a seeded value
            Randomizer.Seed = new System.Random(3116);
        }
    }
}
