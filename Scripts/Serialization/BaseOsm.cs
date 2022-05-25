using System;
using System.Globalization;
using System.Xml;

public class BaseOsm
{
    protected T GetAttribute<T>(string attrName, XmlAttributeCollection attributes)
    {
        string strValue = attributes[attrName].Value;
        return (T)Convert.ChangeType(strValue, typeof(T), CultureInfo.InvariantCulture);
    }
}
