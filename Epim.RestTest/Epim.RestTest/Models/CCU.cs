using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Epim.RestTest.Models
{
    [Serializable]
    [XmlRoot("Ccu", Namespace = "http://www.logisticshub.no/elh")]
    public class Container
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("OrgNo")]
        public long OrgNo { get; set; }
        [XmlElement("Gln")]
        public long Gln { get; set; }

        // <ns2:Ccu id="0107epim1">
        //<ns2:OrgNo>992100400</ns2:OrgNo>
        //<ns2:Gln>7080001336059</ns2:Gln>
    }

    //[XmlRoot("user_list")]
    //public class UserList
    //{
    //    public UserList() { Items = new List<User>(); }
    //    [XmlElement("user")]
    //    public List<User> Items { get; set; }
    //}
    //public class User
    //{
    //    [XmlElement("id")]
    //    public Int32 Id { get; set; }

    //    [XmlElement("name")]
    //    public String Name { get; set; }
    //}

    [Serializable]
    [XmlRoot(Namespace = "http://www.logisticshub.no/elh", ElementName = "Ccus", DataType = "string", IsNullable = true)]
    public class ContainerCollection
    {
        [XmlAttribute("pageSize")]
        public string PageSize { get; set; }
        [XmlAttribute("page")]
        public string Page { get; set; }
        [XmlElement("Ccu")]
        public List<Container> Containers { get; set; }
    }
}
