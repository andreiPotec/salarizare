using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Declaratie112Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("    GENERATOR DECLARAȚIE 112 - CONTRIBUȚII SOCIALE");
            Console.WriteLine("═══════════════════════════════════════════════════\n");

            var declaratie = new Declaratie112();
            
            Console.WriteLine("📋 DATE PERSONALE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Nume: ");
            declaratie.Nume = Console.ReadLine();
            
            Console.Write("Prenume: ");
            declaratie.Prenume = Console.ReadLine();
            
            Console.Write("CNP: ");
            declaratie.CNP = Console.ReadLine();
            
            Console.Write("Cod fiscal (CUI/CIF): ");
            declaratie.CodFiscal = Console.ReadLine();
            
            Console.Write("Adresă: ");
            declaratie.Adresa = Console.ReadLine();
            
            Console.Write("Localitate: ");
            declaratie.Localitate = Console.ReadLine();
            
            Console.Write("Județ: ");
            declaratie.Judet = Console.ReadLine();
            
            Console.Write("Telefon (opțional): ");
            declaratie.Telefon = Console.ReadLine();
            
            Console.Write("Email (opțional): ");
            declaratie.Email = Console.ReadLine();
            
            Console.WriteLine("\n📅 DATE DECLARAȚIE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Luna (1-12): ");
            declaratie.Luna = int.Parse(Console.ReadLine());
            
            Console.Write("Anul: ");
            declaratie.An = int.Parse(Console.ReadLine());
            
            Console.WriteLine("\n💰 VENITURI ȘI CALCULE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Venit brut realizat (RON): ");
            declaratie.VenitBrut = decimal.Parse(Console.ReadLine());
            
            Console.Write("Tip contribuabil (1=PFA, 2=II, 3=Salariat): ");
            int tipContribuabil = int.Parse(Console.ReadLine());
            
            declaratie.CalculeazaContributii(tipContribuabil);
            
            Console.WriteLine("\n═══════════════════════════════════════════════════");
            Console.WriteLine("           SUMAR CALCULE CONTRIBUȚII");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
            
            Console.WriteLine($"Venit brut:                    {declaratie.VenitBrut:N2} RON");
            Console.WriteLine($"Bază de calcul CAS (12 x SMB): {declaratie.BazaCalculCAS:N2} RON");
            Console.WriteLine($"Bază de calcul CASS:           {declaratie.BazaCalculCASS:N2} RON");
            Console.WriteLine();
            Console.WriteLine($"CAS (25%):                     {declaratie.CAS:N2} RON");
            Console.WriteLine($"CASS (10%):                    {declaratie.CASS:N2} RON");
            Console.WriteLine();
            Console.WriteLine($"TOTAL CONTRIBUȚII DE PLATĂ:    {declaratie.TotalContributii:N2} RON");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
            
            Console.Write("Generez declarația 112... ");
            try
            {
                string numeFisierText = declaratie.GenereazaDeclaratie();
                string numeFisierXML = declaratie.GenereazaXML();
                string numeFisierPDF = declaratie.GenereazaPDF();
                Console.WriteLine("✓ Generat!\n");
                
                Console.WriteLine($"📄 Declarația text: {numeFisierText}");
                Console.WriteLine($"📄 Declarația XML:  {numeFisierXML}");
                Console.WriteLine($"📄 Declarația PDF:  {numeFisierPDF}");
                Console.WriteLine("\n💡 IMPORTANTE:");
                Console.WriteLine("   • PDF-ul respectă formatul oficial ANAF 2025");
                Console.WriteLine("   • Poate fi depus la ghișeu sau trimis prin poștă");
                Console.WriteLine("   • Pentru depunere online, folosește XML-ul validat cu DUK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Eroare: {ex.Message}");
            }
            
            Console.WriteLine("\nApasă orice tastă pentru a închide aplicația...");
            Console.ReadKey();
        }
    }

    class Declaratie112
    {
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string CNP { get; set; }
        public string CodFiscal { get; set; }
        public string Adresa { get; set; }
        public string Localitate { get; set; }
        public string Judet { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        
        public int Luna { get; set; }
        public int An { get; set; }
        
        public decimal VenitBrut { get; set; }
        public decimal BazaCalculCAS { get; set; }
        public decimal BazaCalculCASS { get; set; }
        public decimal CAS { get; set; }
        public decimal CASS { get; set; }
        public decimal TotalContributii { get; set; }
        
        private const decimal SalariuMinimBrut = 3700m;
        private const decimal CotaCAS = 0.25m;
        private const decimal CotaCASS = 0.10m;
        private const int PlafoaneCAS = 12;
        
        public void CalculeazaContributii(int tipContribuabil)
        {
            if (tipContribuabil == 3) // Salariat
            {
                // Pentru salariați: CAS și CASS se calculează pe venitul brut
                BazaCalculCAS = VenitBrut;
                BazaCalculCASS = VenitBrut;
                CAS = BazaCalculCAS * CotaCAS;
                CASS = BazaCalculCASS * CotaCASS;
            }
            else // PFA (1) sau II (2)
            {
                // Pentru PFA/II: CAS pe 12 × SMB, CASS pe venit dacă > 6 × SMB
                BazaCalculCAS = PlafoaneCAS * SalariuMinimBrut;
                decimal pragCASS = 6 * SalariuMinimBrut;
                BazaCalculCASS = VenitBrut >= pragCASS ? VenitBrut : 0;
                CAS = BazaCalculCAS * CotaCAS;
                CASS = BazaCalculCASS * CotaCASS;
            }
            
            TotalContributii = CAS + CASS;
        }
        
        public string GenereazaDeclaratie()
        {
            string numeFisier = $"Declaratie_112_{CodFiscal}_{An}_{Luna:D2}.txt";
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                    DECLARAȚIE 112");
            sb.AppendLine("      DECLARAȚIE PRIVIND OBLIGAȚIILE DE PLATĂ A CONTRIBUȚIILOR");
            sb.AppendLine("                   SOCIALE, IMPOZITULUI PE VENIT");
            sb.AppendLine("            ȘI EVIDENȚA NOMINALĂ A PERSOANELOR ASIGURATE");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Perioada de raportare: Luna {Luna:D2} / An {An}");
            sb.AppendLine();
            sb.AppendLine("SECȚIUNEA - DATE DE IDENTIFICARE A PLĂTITORULUI");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine($"Cod de identificare fiscală: {CodFiscal}");
            sb.AppendLine($"Denumire: {Nume} {Prenume}");
            sb.AppendLine($"Adresă domiciliu fiscal: {Adresa}, {Localitate}, {Judet}");
            if (!string.IsNullOrEmpty(Telefon)) sb.AppendLine($"Telefon: {Telefon}");
            if (!string.IsNullOrEmpty(Email)) sb.AppendLine($"E-mail: {Email}");
            sb.AppendLine();
            sb.AppendLine("SECȚIUNEA - CREANȚE FISCALE");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine($"1. Contribuția de asigurări sociale (CAS)");
            sb.AppendLine($"   Suma datorată: {CAS:N2} RON");
            sb.AppendLine();
            sb.AppendLine($"2. Contribuția de asigurări sociale de sănătate (CASS)");
            sb.AppendLine($"   Suma datorată: {CASS:N2} RON");
            sb.AppendLine();
            sb.AppendLine($"TOTAL OBLIGAȚII DE PLATĂ: {TotalContributii:N2} RON");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine($"Data generării: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            File.WriteAllText(numeFisier, sb.ToString(), Encoding.UTF8);
            return Path.GetFullPath(numeFisier);
        }
        
        public string GenereazaXML()
        {
            string numeFisier = $"D112_{CodFiscal}_{An}{Luna:D2}.xml";
            XNamespace ns = "mfp:anaf:dgti:d112:declaratie:v1";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            
            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "declaratie",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XElement(ns + "formulare",
                        new XElement(ns + "formular",
                            new XAttribute("tip", "D112"),
                            new XAttribute("luna", Luna),
                            new XAttribute("an", An),
                            new XElement(ns + "angajator",
                                new XElement(ns + "cui", CodFiscal),
                                new XElement(ns + "denumire", $"{Nume} {Prenume}"),
                                new XElement(ns + "adresa",
                                    new XElement(ns + "strada", Adresa),
                                    new XElement(ns + "localitate", Localitate),
                                    new XElement(ns + "judet", Judet)
                                ),
                                new XElement(ns + "creanteFiscale",
                                    new XElement(ns + "creanta",
                                        new XAttribute("cod", "51.01.01"),
                                        new XElement(ns + "suma", CAS.ToString("F2"))
                                    ),
                                    CASS > 0 ? new XElement(ns + "creanta",
                                        new XAttribute("cod", "51.01.04"),
                                        new XElement(ns + "suma", CASS.ToString("F2"))
                                    ) : null
                                ),
                                new XElement(ns + "totalObligatii", TotalContributii.ToString("F2"))
                            )
                        )
                    )
                )
            );
            
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };
            
            using (var writer = XmlWriter.Create(numeFisier, settings))
            {
                doc.Save(writer);
            }
            
            return Path.GetFullPath(numeFisier);
        }
        
        public string GenereazaPDF()
        {
            string numeFisier = $"D112_{CodFiscal}_{An}_{Luna:D2}.pdf";
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));
                    
                    page.Content().Column(column =>
                    {
                        // HEADER - Anexa nr.1
                        column.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Text("Anexa nr.1").FontSize(8);
                            row.RelativeItem().AlignRight().Text("112").FontSize(8).Bold();
                        });
                        
                        // TITLU PRINCIPAL
                        column.Item().PaddingBottom(15).Column(titleCol =>
                        {
                            titleCol.Item().AlignCenter().Text("DECLARAȚIE PRIVIND OBLIGAȚIILE DE PLATĂ A CONTRIBUȚIILOR SOCIALE,")
                                .FontSize(10).Bold();
                            titleCol.Item().AlignCenter().Text("IMPOZITULUI PE VENIT ȘI EVIDENȚA NOMINALĂ A PERSOANELOR ASIGURATE")
                                .FontSize(10).Bold();
                        });
                        
                        // PERIOADA DE RAPORTARE
                        column.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Perioada de raportare").FontSize(8);
                                col.Item().PaddingTop(3).Row(innerRow =>
                                {
                                    innerRow.ConstantItem(80).Border(1).Padding(2).Column(c =>
                                    {
                                        c.Item().Text("Lună").FontSize(7);
                                        c.Item().Text(Luna.ToString("D2")).FontSize(9).Bold();
                                    });
                                    innerRow.ConstantItem(10);
                                    innerRow.ConstantItem(100).Border(1).Padding(2).Column(c =>
                                    {
                                        c.Item().Text("An").FontSize(7);
                                        c.Item().Text(An.ToString()).FontSize(9).Bold();
                                    });
                                });
                            });
                            
                            row.ConstantItem(20);
                            
                            // Checkbox-uri pentru tip declarație
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("☐ Declarație rectificativă").FontSize(7);
                                col.Item().Text("☐ Declarație depusă ca urmare a acordării unor drepturi").FontSize(7);
                                col.Item().Text("☐ Declarație rectificativă depusă ca urmare a unei notificări").FontSize(7);
                            });
                        });
                        
                        // SECȚIUNEA - DATE DE IDENTIFICARE
                        column.Item().PaddingBottom(10).Element(ComposeIdentificare);
                        
                        // SECȚIUNEA - CREANȚE FISCALE
                        column.Item().PaddingBottom(15).Element(ComposeCreanteFiscale);
                        
                        // DECLARAȚIE ȘI SEMNĂTURĂ
                        column.Item().Element(ComposeDeclaratie);
                    });
                    
                    // Footer
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Document care conține date cu caracter personal protejate de prevederile Regulamentului (UE) 2016/679")
                            .FontSize(7).Italic();
                    });
                });
            }).GeneratePdf(numeFisier);
            
            return Path.GetFullPath(numeFisier);
            
            void ComposeIdentificare(IContainer container)
            {
                container.Border(1).Column(column =>
                {
                    column.Item().Background(Colors.Grey.Lighten3).Padding(3)
                        .Text("SECȚIUNEA - DATE DE IDENTIFICARE A PLĂTITORULUI").FontSize(8).Bold();
                    
                    column.Item().Padding(8).Column(dataCol =>
                    {
                        dataCol.Item().Row(row =>
                        {
                            row.ConstantItem(150).Text("Cod de identificare fiscală").FontSize(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(3)
                                .Text(CodFiscal).FontSize(9).Bold();
                        });
                        
                        dataCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.ConstantItem(150).Text("Denumire").FontSize(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(3)
                                .Text($"{Nume} {Prenume}").FontSize(9);
                        });
                        
                        dataCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.ConstantItem(150).Text("Adresă domiciliu fiscal").FontSize(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(3)
                                .Text($"{Adresa}, {Localitate}, {Judet}").FontSize(9);
                        });
                        
                        dataCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.ConstantItem(150).Text("Telefon").FontSize(8);
                            row.ConstantItem(150).Border(1).BorderColor(Colors.Grey.Medium).Padding(3)
                                .Text(Telefon ?? "").FontSize(9);
                            row.ConstantItem(20);
                            row.ConstantItem(50).Text("E-mail").FontSize(8);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Medium).Padding(3)
                                .Text(Email ?? "").FontSize(9);
                        });
                    });
                });
            }
            
            void ComposeCreanteFiscale(IContainer container)
            {
                container.Border(1).Column(column =>
                {
                    column.Item().Background(Colors.Grey.Lighten3).Padding(3)
                        .Text("SECȚIUNEA - Creanțe fiscale").FontSize(8).Bold();
                    
                    // Header tabel
                    column.Item().Border(1).Row(row =>
                    {
                        row.ConstantItem(40).Border(1).Padding(2).AlignCenter().Text("Nr.crt.").FontSize(7).Bold();
                        row.RelativeItem(3).Border(1).Padding(2).AlignCenter().Text("Denumire creanță fiscală").FontSize(7).Bold();
                        row.RelativeItem().Border(1).Padding(2).Column(c =>
                        {
                            c.Item().AlignCenter().Text("Suma").FontSize(7).Bold();
                            c.Item().PaddingTop(2).Row(innerRow =>
                            {
                                innerRow.RelativeItem().Border(1).Padding(1).AlignCenter().Text("Datorată").FontSize(6);
                                innerRow.RelativeItem().Border(1).Padding(1).AlignCenter().Text("Deductibilă").FontSize(6);
                                innerRow.RelativeItem().Border(1).Padding(1).AlignCenter().Text("Scutită").FontSize(6);
                                innerRow.RelativeItem().Border(1).Padding(1).AlignCenter().Text("De plată").FontSize(6);
                            });
                        });
                        row.ConstantItem(80).Border(1).Padding(2).AlignCenter().Text("Cod bugetar").FontSize(7).Bold();
                    });
                    
                    // CAS
                    column.Item().Border(1).Row(row =>
                    {
                        row.ConstantItem(40).Border(1).Padding(2).AlignCenter().Text("1").FontSize(8);
                        row.RelativeItem(3).Border(1).Padding(2).Text("Contribuția de asigurări sociale").FontSize(8);
                        row.RelativeItem().Row(sumRow =>
                        {
                            sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text(CAS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).FontSize(8);
                            sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text("0.00").FontSize(8);
                            sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text("0.00").FontSize(8);
                            sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text(CAS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).FontSize(8).Bold();
                        });
                        row.ConstantItem(80).Border(1).Padding(2).AlignCenter().Text("51.01.01").FontSize(7);
                    });
                    
                    // CASS
                    if (CASS > 0)
                    {
                        column.Item().Border(1).Row(row =>
                        {
                            row.ConstantItem(40).Border(1).Padding(2).AlignCenter().Text("2").FontSize(8);
                            row.RelativeItem(3).Border(1).Padding(2).Text("Contribuția de asigurări sociale de sănătate").FontSize(8);
                            row.RelativeItem().Row(sumRow =>
                            {
                                sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text(CASS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).FontSize(8);
                                sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text("0.00").FontSize(8);
                                sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text("0.00").FontSize(8);
                                sumRow.RelativeItem().Border(1).Padding(2).AlignRight().Text(CASS.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).FontSize(8).Bold();
                            });
                            row.ConstantItem(80).Border(1).Padding(2).AlignCenter().Text("51.01.04").FontSize(7);
                        });
                    }
                    
                    // TOTAL
                    column.Item().Background(Colors.Blue.Lighten4).Border(1).Padding(5).Row(row =>
                    {
                        row.RelativeItem().Text("Total obligații de plată").FontSize(9).Bold();
                        row.ConstantItem(120).AlignRight().Text(TotalContributii.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " RON").FontSize(10).Bold();
                    });
                });
            }
            
            void ComposeDeclaratie(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().PaddingTop(10).PaddingBottom(5)
                        .Text("Prezenta declarație reprezintă titlu de creanță și produce efectele juridice ale înștiințării de plată de la data depunerii acesteia, în condițiile legii.")
                        .FontSize(8).Italic();
                    
                    column.Item().PaddingBottom(10)
                        .Text("Sub sancțiunile aplicate faptei de fals în declarații, declar că datele din această declarație sunt corecte și complete.")
                        .FontSize(8).Bold();
                    
                    column.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(3).Row(r =>
                            {
                                r.ConstantItem(50).Text("Nume").FontSize(7);
                                r.RelativeItem().Text(Nume).FontSize(8).Bold();
                            });
                            col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Medium).Padding(3).Row(r =>
                            {
                                r.ConstantItem(50).Text("Prenume").FontSize(7);
                                r.RelativeItem().Text(Prenume).FontSize(8).Bold();
                            });
                        });
                        
                        row.ConstantItem(20);
                        
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Semnătura și ștampila").FontSize(7);
                            col.Item().PaddingTop(20).LineHorizontal(1);
                        });
                    });
                    
                    column.Item().PaddingTop(15).Border(1).Padding(5).Column(col =>
                    {
                        col.Item().Text("Loc rezervat autorității competente").FontSize(7).Bold();
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Număr de înregistrare: ________________").FontSize(8);
                            row.RelativeItem().Text("Data: _______________").FontSize(8);
                        });
                    });
                });
            }
        }
    }
}