namespace testDeclaratie112;

public class CalculatorTaxe : ITaxCalculator
{
    
    public double CalculeazaTicheteMasa(Angajat angajat)
    {
        return angajat.TicheteMasa * 25.0;
    }
    
    public double CalculeazaCas(Angajat angajat)
    {
        return Math.Round(angajat.SalariuBrutCim * 0.25, 0);
    }

    public double CalculeazaCass(Angajat angajat)
    {
       return Math.Round((angajat.SalariuBrutCim + CalculeazaTicheteMasa(angajat)) * 0.1, 0);
    }
    
    public double CalculeazaBazaImpozitVenit(Angajat angajat)
    {
        return angajat.SalariuBrutCim - CalculeazaCas(angajat) - CalculeazaCass(angajat);
    }
    
    public double CalculeazaImpozitVenit(Angajat angajat)
    {
        return (CalculeazaBazaImpozitVenit(angajat) + CalculeazaTicheteMasa(angajat)) * 0.1;
    }
}