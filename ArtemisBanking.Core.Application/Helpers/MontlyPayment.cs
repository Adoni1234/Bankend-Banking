namespace ArtemisBanking.Core.Application.Helpers;

public static class MontlyPayment
{
    public static decimal Calculate(decimal capital, decimal annualRate, int numberOfPayments)
    {
        // 1. Convertir a double para cálculos matemáticos
        double capitalD = (double)capital;
        double anualRateLd = (double) annualRate / 12 / 100;
        int termMonthsD = numberOfPayments;

        // 2. Calcular cuota mensual
        double cuote = capitalD * 
                       (anualRateLd * Math.Pow(1 + anualRateLd, termMonthsD)) / 
                       (Math.Pow(1 + anualRateLd, termMonthsD) - 1);

        return (decimal)cuote;
    }
}