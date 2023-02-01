namespace TestProject
{
    public class DeliveryPlanTests : TestDeliveryPlanBase
    {
        [Test]
        public async Task Test_16_Locations()
        {
            var total = await TestFile("testdata16.csv");
            Assert.That(total, Is.EqualTo(6));
        }

        [Test]
        public async Task Test_16_LocationsDuplicated()
        {

            var total = await TestFile("testdata16-DuplicatedLocations.csv");
            Assert.That(total, Is.EqualTo(6));

        }
        [Test]
        public async Task Test_10k_Locations()
        {
            await TestFile("testdata10k.csv");
            Assert.Pass();
        }
        [Test]
        public async Task Test_10k_LocationsDuplicated()
        {
            await TestFile("testdata10k-DuplicatedLocations.csv");
            Assert.Pass();
        }

        [Test]
        public async Task Test_20k_Locations()
        {
            await TestFile("testdata20k.csv");
            Assert.Pass();
        }
        [Test]
        public async Task Test_20k_LocationsDuplicated()
        {
            await TestFile("testdata20k-DuplicatedLocations.csv");
            Assert.Pass();
        }

        [Test]
        public async Task Test_20k_Compare()
        {
            var dataGreater = await TestFile("testdata20k.csv");
            var dataLower= await TestFile("testdata20k-DuplicatedLocations.csv");
            Assert.Greater(dataGreater, dataLower );
        }
    }
}