using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Epim.RestTest.Models
{
    [Serializable]
    [XmlRoot("Hire", Namespace = "http://www.logisticshub.no/elh")]
    public class ContainerHire
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlElement("CcuId")]
        public string ContainerId { get; set; }
        public long OrgNo { get; set; }
        public long Gln { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    //[Serializable]
    //[XmlRoot("Hire", Namespace = "http://www.logisticshub.no/elh")]
    //public class ExistingContainerHire
    //{
    //    [XmlAttribute("id")]
    //    public string Id { get; set; }
    //    [XmlElement("CcuId")]
    //    public string ContainerId { get; set; }
    //    public long OrgNo { get; set; }
    //    public long GlnLocation { get; set; }
    //    public string StartDate { get; set; }
    //    public string EndDate { get; set; }
    //}

    [Serializable]
    [XmlRoot(Namespace = "http://www.logisticshub.no/elh", ElementName = "Hires", DataType = "string", IsNullable = true)]
    public class ContainerHires
    {
        [XmlAttribute("pageSize")]
        public int PageSize { get; set; }
        [XmlAttribute("page")]
        public int Page { get; set; }
        [XmlAttribute("noOfPages")]
        public int NoOfPages { get; set; }
        [XmlAttribute("noOfItems")]
        public int NoOfItems { get; set; }
        [XmlElement("Hire")]
        public List<ContainerHire> Hires { get; set; }
    }
}
