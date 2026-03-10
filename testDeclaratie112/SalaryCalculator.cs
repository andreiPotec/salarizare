namespace testDeclaratie112;

public class SalaryCalculator : ISalaryCalculator
{
    private readonly double _normaLunară = 18 * 8.0;
    
    public double CalculateGrossSalary(Angajat angajat)
    {
        // Calcul salariu brut
        return angajat.SalariuBrutCim / _normaLunară * angajat.OreLucrate;
        
    }
}