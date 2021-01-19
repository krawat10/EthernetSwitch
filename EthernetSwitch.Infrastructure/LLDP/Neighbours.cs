using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;


namespace EthernetSwitch.Infrastructure.LLDP
{
	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(Lldp));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (Lldp)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "id")]
	public class Id
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlAttribute(AttributeName = "type")]
		public string Type { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "name")]
	public class Name
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "descr")]
	public class Descr
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "mgmt-ip")]
	public class Mgmtip
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "capability")]
	public class Capability
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlAttribute(AttributeName = "type")]
		public string Type { get; set; }

		[XmlAttribute(AttributeName = "enabled")]
		public string Enabled { get; set; }
	}

	[XmlRoot(ElementName = "chassis")]
	public class Chassis
	{

		[XmlElement(ElementName = "id")]
		public Id Id { get; set; }

		[XmlElement(ElementName = "name")]
		public Name Name { get; set; }

		[XmlElement(ElementName = "descr")]
		public Descr Descr { get; set; }

		[XmlElement(ElementName = "mgmtip")]
		public List<Mgmtip> Mgmtip { get; set; }

		[XmlElement(ElementName = "capability")]
		public List<Capability> Capability { get; set; }

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "ttl")]
	public class Ttl
	{

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public int Text { get; set; }
	}

	[XmlRoot(ElementName = "port")]
	public class Port
	{

		[XmlElement(ElementName = "id")]
		public Id Id { get; set; }

		[XmlElement(ElementName = "descr")]
		public Descr Descr { get; set; }

		[XmlElement(ElementName = "ttl")]
		public Ttl Ttl { get; set; }

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "interface")]
	public class Interface
	{

		[XmlElement(ElementName = "chassis")]
		public Chassis Chassis { get; set; }

		[XmlElement(ElementName = "port")]
		public Port Port { get; set; }

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "via")]
		public string Via { get; set; }

		[XmlAttribute(AttributeName = "rid")]
		public int Rid { get; set; }

		[XmlAttribute(AttributeName = "age")]
		public string Age { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "lldp")]
	public class Lldp
	{

		[XmlElement(ElementName = "interface")]
		public List<Interface> Interface { get; set; }

		[XmlAttribute(AttributeName = "label")]
		public string Label { get; set; }

		[XmlText]
		public string Text { get; set; }
	}


}
