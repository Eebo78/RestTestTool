using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Epim.RestTest.Models;
using Epim.RestTest.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epim.RestTest.Integration.Tests
{
    [TestClass]
    public class Certificates
    {
       

        [TestMethod]
        public void DeserializeXml()
        {
            ContainerCollection containers = null;
            string path = "Ccus.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(ContainerCollection));

            StreamReader reader = new StreamReader(path);
            containers = (ContainerCollection)serializer.Deserialize(reader);
            reader.Close();

            Assert.IsNotNull(containers.Containers);
        
        }

        [TestMethod]
        public void SerializeContainerTest()
        {
            var container = new Container
            {
                Id = "PerfTest_Epim_1",
                OrgNo = 992100400,
                Gln = 7080001336059
            };

            var vm = new TestDataViewModel();
            var body = vm.Serialize(container);

            Assert.IsTrue(true);
        }
    }
}
