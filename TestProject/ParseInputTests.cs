using CORE.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class ParseInputTests: TestDeliveryPlanBase
    {
       
        [Test]
        public async Task ExceedMaxWeight()
        {
            try
            {
                await this.TestFile("testdataExceedsWeight.csv");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Assert.Pass();
            }
        }

        [Test]
        public async Task InvalidWeight()
        {
            try
            {
                await TestFile("testdataInvalidWeight.csv");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Assert.Pass();
            }
        }


        [Test]
        public async Task ExceededNumberOfDrones()
        {
            try
            {
                await this.TestFile("testdataMoreThan100Drones.csv");
                Assert.Fail();
            }
            catch (MaximumNumberOfDronesExcededExceptio ex)
            {
                Trace.WriteLine(ex.Message);
                Assert.Pass();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                Assert.Fail();
            }
        }
    }
}
