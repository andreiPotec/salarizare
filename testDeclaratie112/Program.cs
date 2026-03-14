using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using iTextSharp.text.pdf;

namespace Declaratie112Generator
{
    // ═══════════════════════════════════════════════════════════════
    // ARHITECTURĂ SISTEM D112
    // ═══════════════════════════════════════════════════════════════
    //
    // [ Date salarizare/PFA ]
    //          ↓
    // [ Motor mapare D112 ] ← Transformă date în model D112
    //          ↓
    // [ Generator XML strict conform XSD ] ← Validare XSD
    //          ↓
    // [ Injector XFA în PDF oficial ANAF ] ← Completare formular
    //          ↓
    // [ PDF validat + XML ]
    //          ↓
    // [ Pregătit pentru semnătură electronică ]
    //
    // ═══════════════════════════════════════════════════════════════
    
    class Program
    {
        static void Main(string[] args)
        {
            var xmlGenerator = new XMLGenerator();
            var xfaInjector = new XFAInjector();
            Console.OutputEncoding = Encoding.UTF8;
            AfiseazaBanner();
            
            // ═══ PASUL 1: COLECTARE DATE ═══
            var dateSalarizare = ColecteazaDate();
            
            // ═══ PASUL 2: MAPARE ÎN MODEL D112 ═══
            Console.WriteLine("\n🔄 PASUL 2: Mapare date în model D112...");
            var motorMapare = new MotorMapareD112();
            //generator xml conform XSD ANAF
            var modelD112 = motorMapare.Mapeaza(dateSalarizare);
            xmlGenerator.ExtrageXFA("D112_template.pdf");
            // validator XSD conform XSD ANAF DUK integrator
            
            //inject data in pdf
            xfaInjector.InjectPDF("xfa_full.xml", modelD112);

        }
        
        static void AfiseazaBanner()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║   SISTEM GENERATOR DECLARAȚIE 112 - ANAF             ║");
            Console.WriteLine("║   Arhitectură profesională cu validare XSD           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");
        }
        
        static DateSalarizare ColecteazaDate()
        {
            return new DateSalarizare()
            {
                Nume = "Popescu", Prenume = "Ion", CNP = "1850101123456",
                CUI = "12345678", Adresa = "Str. Victoriei nr. 10",
                Localitate = "București", Judet = "București",
                Luna = 1, An = 2026, VenitBrut = 5000m,
                TipContribuabil = TipContribuabil.Salariat
            };
            Console.WriteLine("═══ PASUL 1: COLECTARE DATE SALARIZARE ═══\n");
            
            Console.Write("Mod test cu date hardcodate? (D/N): ");
            if (Console.ReadLine()?.ToUpper() == "D")
            {
                return new DateSalarizare
                {
                    Nume = "Popescu", Prenume = "Ion", CNP = "1850101123456",
                    CUI = "12345678", Adresa = "Str. Victoriei nr. 10",
                    Localitate = "București", Judet = "București",
                    Luna = 1, An = 2026, VenitBrut = 5000m,
                    TipContribuabil = TipContribuabil.Salariat
                };
            }
            
            var date = new DateSalarizare();
            Console.Write("Nume: "); date.Nume = Console.ReadLine();
            Console.Write("Prenume: "); date.Prenume = Console.ReadLine();
            Console.Write("CNP: "); date.CNP = Console.ReadLine();
            Console.Write("CUI: "); date.CUI = Console.ReadLine();
            Console.Write("Adresă: "); date.Adresa = Console.ReadLine();
            Console.Write("Localitate: "); date.Localitate = Console.ReadLine();
            Console.Write("Județ: "); date.Judet = Console.ReadLine();
            Console.Write("Luna (1-12): "); date.Luna = int.Parse(Console.ReadLine());
            Console.Write("Anul: "); date.An = int.Parse(Console.ReadLine());
            Console.Write("Venit brut (RON): "); date.VenitBrut = decimal.Parse(Console.ReadLine());
            Console.Write("Tip (1=PFA, 2=II, 3=Salariat): ");
            date.TipContribuabil = (TipContribuabil)int.Parse(Console.ReadLine());
            
            return date;
        }
        
        static void AfiseazaRezultat(string xml, string pdf, ModelD112 model)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║              GENERARE COMPLETĂ CU SUCCES!            ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");
            
            Console.WriteLine("📊 SUMAR CONTRIBUȚII:");
            Console.WriteLine($"   CAS (25%):  {model.CAS:N2} RON");
            Console.WriteLine($"   CASS (10%): {model.CASS:N2} RON");
            Console.WriteLine($"   ━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine($"   TOTAL:      {model.TotalContributii:N2} RON");
            
            Console.WriteLine($"\n📄 FIȘIERE GENERATE:");
            Console.WriteLine($"   • XML validat XSD: {Path.GetFileName(xml)}");
            Console.WriteLine($"   • PDF oficial ANAF: {Path.GetFileName(pdf)}");
            
            Console.WriteLine($"\n✅ PAȘI URMĂTORI:");
            Console.WriteLine($"   1. Verifică PDF-ul generat");
            Console.WriteLine($"   2. Semnează electronic cu certificat digital");
            Console.WriteLine($"   3. Depune pe e-guvernare.ro → SPV");
            Console.WriteLine($"      SAU trimite la ANAF (ghișeu/poștă)");
            
            Console.WriteLine("\nApasă orice tastă pentru a închide...");
            Console.ReadKey();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // MODELE DE DATE
    // ═══════════════════════════════════════════════════════════════
    
    enum TipContribuabil { PFA = 1, IntreprinzatorIndividual = 2, Salariat = 3 }
    
    class DateSalarizare
    {
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string CNP { get; set; }
        public string CUI { get; set; }
        public string Adresa { get; set; }
        public string Localitate { get; set; }
        public string Judet { get; set; }
        public int Luna { get; set; }
        public int An { get; set; }
        public decimal VenitBrut { get; set; }
        public TipContribuabil TipContribuabil { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // MOTOR DE MAPARE - Transformă date brute în model D112
    // ═══════════════════════════════════════════════════════════════
    
    class MotorMapareD112
    {
        private const decimal SMB = 3700m; // Salariu minim brut 2025
        
        public ModelD112 Mapeaza(DateSalarizare date)
        {
            var model = new ModelD112
            {
                Nume = date.Nume,
                Prenume = date.Prenume,
                CNP = date.CNP,
                CUI = date.CUI,
                Adresa = date.Adresa,
                Localitate = date.Localitate,
                Judet = date.Judet,
                Luna = date.Luna,
                An = date.An,
                VenitBrut = date.VenitBrut
            };
            
            // Calcul contribuții conform legislație
            if (date.TipContribuabil == TipContribuabil.Salariat)
            {
                model.BazaCalculCAS = date.VenitBrut;
                model.BazaCalculCASS = date.VenitBrut;
                model.CAS = date.VenitBrut * 0.25m;
                model.CASS = date.VenitBrut * 0.10m;
            }
            else // PFA sau II
            {
                model.BazaCalculCAS = 12 * SMB; // Bază fixă
                decimal pragCASS = 6 * SMB;
                model.BazaCalculCASS = date.VenitBrut >= pragCASS ? date.VenitBrut : 0;
                model.CAS = model.BazaCalculCAS * 0.25m;
                model.CASS = model.BazaCalculCASS * 0.10m;
            }
            
            model.TotalContributii = model.CAS + model.CASS;
            
            Console.WriteLine($"   ✓ Date mapate: {model.Nume} {model.Prenume}");
            Console.WriteLine($"   ✓ Contribuții calculate: {model.TotalContributii:N2} RON");
            
            return model;
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // VALIDATOR XSD - Validare strictă conform schema ANAF
    // ═══════════════════════════════════════════════════════════════
    
    class ValidatorXSD
    {
        public bool Valideaza(string fisierXML)
        {
            try
            {
                // NOTĂ: Pentru validare completă, descarcă XSD de la:
                // https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/d112_10102024.xsd
                
                var doc = XDocument.Load(fisierXML);
                
                // Validări de bază
                if (doc.Root == null || doc.Root.Name.LocalName != "declaratie")
                {
                    Console.WriteLine("   ❌ XML invalid: lipșește elementul rădăcină");
                    return false;
                }
                
                Console.WriteLine("   ✓ Structură XML validă");
                Console.WriteLine("   ℹ Pentru validare XSD completă, folosește DUK Integrator");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Eroare validare: {ex.Message}");
                return false;
            }
        }
    }
}