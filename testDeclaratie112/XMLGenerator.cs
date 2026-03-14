using System.Xml;
using iTextSharp.text.pdf;

namespace Declaratie112Generator;

public class XMLGenerator
{
    
    public void ExtrageXFA(string pdf)
    {
        PdfReader reader = new PdfReader(pdf);

        XfaForm xfa = new XfaForm(reader);

        XmlDocument dom = xfa.DomDocument;

        dom.Save("xfa_full.xml");

        reader.Close();
    }
    static void ListeazaCampuri(string xfaFile)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(xfaFile);

        XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("xfa", "http://www.xfa.org/schema/xfa-template/2.8/");

        var nodes = doc.SelectNodes("//xfa:field", ns);

        foreach (XmlNode node in nodes)
        {
            var name = node.Attributes["name"]?.Value;
            if (!string.IsNullOrEmpty(name))
                Console.WriteLine(name);
        }
    }
}