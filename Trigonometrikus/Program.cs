using System;
using System.Diagnostics.CodeAnalysis;


namespace Trigonometrikus {

    static class Program {

        delegate bool TryParse<T>(string text, [NotNullWhen(true)] out T? parsed);

        static T CheckedInput<T>(TryParse<T> tryParse) {
            T? parsed;
            while(!tryParse(Console.ReadLine()!, out parsed)); // Átkozott üres while ciklus
            return parsed;
        }


        static (double scaleSquared, double alphaRad) ConvertToTrigonometric(double a, double b) {
            if(Math.Abs(a) < double.Epsilon && Math.Abs(b) < double.Epsilon) return (0, double.NaN);

            double scaleSquared = a*a + b*b;

            double alphaRad = 0;
            // Addig forgatjuk negatív irányba, amíg a lentről zárt, balról nyitott első síknegyedbe nem kerül
            while(a <= 0 || b < 0) {
                (a, b) = (b, -a);
                alphaRad += Math.PI / 2; // Közbe számoljuk a forgatásokat, így kerülünk vissza a rendes szöghöz
            }

            // Oordinátatengelyen van? (0 a deréxögű 3szög alsó oldala?)
            if(Math.Abs(a) < double.Epsilon) {
                alphaRad += 0; // Meghagyjuk a 90° többszörösét
            } else {
                alphaRad += Math.Atan(b / a);
            }

            return (scaleSquared, alphaRad);
        }


        static void Main() {
            do {
                // Bekérés
                Console.WriteLine();
                Console.WriteLine("Kérem a komplex szám algebrai alakjába az a-t és b-t!");
                Console.WriteLine("a?");
                double a = CheckedInput<double>(double.TryParse);
                Console.WriteLine("b?");
                double b = CheckedInput<double>(double.TryParse);

                // Átalakítás
                (double scaleSquared, double alphaRad) = ConvertToTrigonometric(a, b);

                if(double.IsNaN(alphaRad)) {
                    Console.WriteLine("Az origót (nullát) nem értelmezzük trigonometrikus alakban.");
                } else {
                    // Átváltás szögbe
                    double alphaDeg = alphaRad / Math.PI * 180.0;

                    // Nagyon szép kiírási formázás
                    double scaleSqrt = Math.Sqrt(scaleSquared);
                    string scaleString;
                    if(Math.Abs(Math.Round(scaleSqrt) - scaleSqrt) < 0.00001) scaleString = ((long)Math.Round(scaleSqrt)).ToString(); // Eléggé négyzetszám
                    else scaleString = $"√{scaleSquared.ToString("G8")}"; // Nem négyzetszám

                    string alphaDegString = alphaDeg.ToString("G8");

                    Console.WriteLine($"A ({a}+{b}i) komplex szám trigonometrikus alakja: {scaleString}*(cos {alphaDegString}° + i*sin {alphaDegString}°) (csúnya, radiános kiírás: {scaleSqrt} * (cos {alphaRad} + i*sin {alphaRad}) )");
                }

                Console.WriteLine();
                Console.WriteLine("Még egyszer? (y: igen)");
            } while(char.ToLower(Console.ReadKey().KeyChar) == 'y');
        }

    }

}
