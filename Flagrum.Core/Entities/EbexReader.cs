using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Flagrum.Core.Entities;

public class EbexReader : IDisposable
{
    private readonly Stream _stream;

    public EbexReader(Stream stream)
    {
        _stream = stream;
    }

    public void Dispose()
    {
        _stream.Close();
        _stream.Dispose();
    }

    public void Read()
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(_stream);

        var document = xmlDocument.DocumentElement;
        var objectsContainer = document!["objects"];
        var objects = new List<object>();

        foreach (var child in objectsContainer!.ChildNodes)
        {
            if (child is XmlElement {Name: "object"} element)
            {
                objects.Add(ReadObject(element));
            }
        }
    }

    private object ReadObject(XmlElement element)
    {
        var index = Convert.ToInt32(element["objectIndex"].Value);
        var name = element["name"].Value;
        var type = element["csType"].Value ?? element["type"].Value;
        var path = element["path"].Value;
        var ownerIndex = Convert.ToInt32(element["ownerIndex"].Value);
        var ownerPath = element["ownerPath"].Value;
        return index;
    }
}