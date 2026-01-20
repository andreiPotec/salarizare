using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Declaratie112Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  GENERATOR XML DECLARAȚIE 112 - Format Oficial ANAF");
            Console.WriteLine("═══════════════════════════════════════════════════\n");

            Console.Write("Rulează în mod TEST? (D/N): ");
            if (Console.ReadLine()?.ToUpper() == "D")
            {
                RuleazaTest();
                Console.WriteLine("\nApasă orice tastă...");
                Console.ReadKey();
                return;
            }

            var decl = new Declaratie112();
            
            Console.WriteLine("📋 DATE PERSONALE\n");
            Console.Write("Nume: "); decl.Nume = Console.ReadLine();
            Console.Write("Prenume: "); decl.Prenume = Console.ReadLine();
            Console.Write("CNP: "); decl.CNP = Console.ReadLine();
            Console.Write("CUI: "); decl.CodFiscal = Console.ReadLine();
            Console.Write("Adresă: "); decl.Adresa = Console.ReadLine();
            Console.Write("Localitate: "); decl.Localitate = Console.ReadLine();
            Console.Write("Județ: "); decl.Judet = Console.ReadLine();
            
            Console.WriteLine("\n📅 DATE DECLARAȚIE\n");
            Console.Write("Luna (1-12): "); decl.Luna = int.Parse(Console.ReadLine());
            Console.Write("Anul: "); decl.An = int.Parse(Console.ReadLine());
            
            Console.WriteLine("\n💰 VENIT\n");
            Console.Write("Venit brut (RON): "); decl.VenitBrut = decimal.Parse(Console.ReadLine());
            Console.Write("Tip (1=PFA, 2=II, 3=Salariat): ");
            decl.CalculeazaContributii(int.Parse(Console.ReadLine()));
            
            AfiseazaSumar(decl);
            GenereazaXML(decl);
            
            Console.WriteLine("\nApasă orice tastă...");
            Console.ReadKey();
        }
        
        static void RuleazaTest()
        {
            Console.WriteLine("\n🧪 MOD TEST\n");
            
            var test = new Declaratie112
            {
                Nume = "Popescu", Prenume = "Ion", CNP = "1850101123456",
                CodFiscal = "12345678", Adresa = "Str. Victoriei nr. 10",
                Localitate = "București", Judet = "București",
                Luna = 1, An = 2026, VenitBrut = 5000m
            };
            test.CalculeazaContributii(3);
            AfiseazaSumar(test);
            GenereazaXML(test);
        }
        
        static void AfiseazaSumar(Declaratie112 d)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════");
            Console.WriteLine($"Nume: {d.Nume} {d.Prenume} | CNP: {d.CNP}");
            Console.WriteLine($"Perioada: {d.Luna:D2}/{d.An} | Venit: {d.VenitBrut:N2} RON");
            Console.WriteLine($"CAS (25%): {d.CAS:N2} RON | CASS (10%): {d.CASS:N2} RON");
            Console.WriteLine($"TOTAL: {d.TotalContributii:N2} RON");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
        }
        
        static void GenereazaXML(Declaratie112 d)
        {
            string fisier = $"D112_{d.CodFiscal}_{d.An}{d.Luna:D2}.xml";
            d.GenereazaXML(fisier);
            Console.WriteLine($"✓ XML generat: {fisier}");
            Console.WriteLine("\n💡 Validează cu DUK Integrator apoi semnează electronic!");
        }
    }

    class Declaratie112
    {
        public string Nume, Prenume, CNP, CodFiscal, Adresa, Localitate, Judet;
        public int Luna, An;
        public decimal VenitBrut, BazaCalculCAS, BazaCalculCASS, CAS, CASS, TotalContributii;
        
        public void CalculeazaContributii(int tip)
        {
            if (tip == 3) // Salariat
            {
                BazaCalculCAS = BazaCalculCASS = VenitBrut;
                CAS = VenitBrut * 0.25m;
                CASS = VenitBrut * 0.10m;
            }
            else // PFA/II
            {
                BazaCalculCAS = 12 * 3700m;
                BazaCalculCASS = VenitBrut >= 22200m ? VenitBrut : 0;
                CAS = BazaCalculCAS * 0.25m;
                CASS = BazaCalculCASS * 0.10m;
            }
            TotalContributii = CAS + CASS;
        }
        
        public void GenereazaXML(string fisier)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = new UTF8Encoding(false)
            };
            
            using (var w = XmlWriter.Create(fisier, settings))
            {
                w.WriteStartDocument();
                w.WriteComment($" Declaratie 112 generată automat - {DateTime.Now:yyyy-MM-dd HH:mm} ");
                
                // Elementul rădăcină
                w.WriteStartElement("declaratie112");
                
                // Antet
                w.WriteStartElement("antet");
                w.WriteElementString("luna", Luna.ToString());
                w.WriteElementString("an", An.ToString());
                w.WriteElementString("dataDepunere", DateTime.Now.ToString("yyyy-MM-dd"));
                w.WriteEndElement();
                
                // Declarant (Anexa 1 - Date identificare)
                w.WriteStartElement("declarant");
                w.WriteElementString("cui", CodFiscal);
                w.WriteElementString("denumire", $"{Nume} {Prenume}");
                w.WriteElementString("adresa", $"{Adresa}, {Localitate}, {Judet}");
                w.WriteEndElement();
                
                // Creanțe fiscale (Anexa 1 - Secțiunea Creanțe)
                w.WriteStartElement("creanteFiscale");
                
                // CAS
                w.WriteStartElement("creanta");
                w.WriteElementString("cod", "51.01.01");
                w.WriteElementString("denumire", "Contributia de asigurari sociale");
                w.WriteElementString("sumaDatorata", CAS.ToString("0.00"));
                w.WriteElementString("sumaDeductibila", "0.00");
                w.WriteElementString("sumaScutita", "0.00");
                w.WriteElementString("sumaDePlata", CAS.ToString("0.00"));
                w.WriteEndElement();
                
                // CASS
                if (CASS > 0)
                {
                    w.WriteStartElement("creanta");
                    w.WriteElementString("cod", "51.01.04");
                    w.WriteElementString("denumire", "Contributia de asigurari sociale de sanatate");
                    w.WriteElementString("sumaDatorata", CASS.ToString("0.00"));
                    w.WriteElementString("sumaDeductibila", "0.00");
                    w.WriteElementString("sumaScutita", "0.00");
                    w.WriteElementString("sumaDePlata", CASS.ToString("0.00"));
                    w.WriteEndElement();
                }
                
                w.WriteElementString("totalObligatii", TotalContributii.ToString("0.00"));
                w.WriteEndElement(); // creanteFiscale
                
                // Asigurați (Anexa 1.2 - Evidența nominală)
                w.WriteStartElement("asigurati");
                w.WriteStartElement("asigurat");
                w.WriteElementString("cnp", CNP);
                w.WriteElementString("nume", Nume);
                w.WriteElementString("prenume", Prenume);
                w.WriteElementString("bazaCalculCAS", BazaCalculCAS.ToString("0.00"));
                w.WriteElementString("bazaCalculCASS", BazaCalculCASS.ToString("0.00"));
                w.WriteElementString("contributiiCAS", CAS.ToString("0.00"));
                w.WriteElementString("contributiiCASS", CASS.ToString("0.00"));
                w.WriteEndElement(); // asigurat
                w.WriteEndElement(); // asigurati
                
                w.WriteEndElement(); // declaratie112
                w.WriteEndDocument();
            }
        }
    }
}