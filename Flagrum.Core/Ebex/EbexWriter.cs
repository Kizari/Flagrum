using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Flagrum.Core.Ebex.Entities;

namespace Flagrum.Core.Ebex;

public class EbexWriter : IDisposable
{
    private readonly List<EbexElement> _elements;
    private readonly List<EbexElement> _objects;
    private readonly MemoryStream _stream;
    private readonly StreamWriter _streamWriter;
    private readonly XmlWriter _writer;

    public EbexWriter(List<EbexElement> objects)
    {
        _objects = objects;
        _elements = new List<EbexElement>();
        _stream = new MemoryStream();
        _streamWriter = new StreamWriter(_stream, Encoding.UTF8);
        _writer = XmlWriter.Create(_streamWriter, new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            IndentChars = "\t"
        });
    }

    public void Dispose()
    {
        _writer.Close();
        _writer.Dispose();
        _streamWriter.Close();
        _streamWriter.Dispose();
        _stream.Close();
        _stream.Dispose();
    }

    public Stream Write()
    {
        _writer.WriteStartElement("package");
        _writer.WriteAttributeString("name", "nh02_initialize");

        foreach (var @object in _objects)
        {
            GetElementsRecursively(@object);
        }

        WriteElements();

        _writer.WriteEndElement();
        _writer.Flush();
        return _stream;
    }

    private void WriteElements()
    {
        var index = 0;
        _writer.WriteStartElement("objects");

        foreach (var element in _elements)
        {
            _writer.WriteStartElement("object");
            _writer.WriteAttributeString("objectIndex", index++.ToString());
            _writer.WriteAttributeString("name", element.Name);
            _writer.WriteAttributeString("type", element.Type?.Name);
            _writer.WriteAttributeString("path", "ERROR");
            _writer.WriteAttributeString("checked", element.IsChecked.ToString());
            _writer.WriteEndElement();
        }

        _writer.WriteEndElement();
    }

    private void GetElementsRecursively(EbexElement element)
    {
        if (_elements.Contains(element))
        {
            return;
        }

        _elements.Add(element);

        foreach (var child in element.Children)
        {
            GetElementsRecursively(child);
        }
    }
}