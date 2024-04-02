using System;
using System.Collections.Generic;


namespace Intervallumos {

    static class Program {

        static void Main(string[] args) {

            Console.WriteLine("Írd be az 1ütthatókat a legnagyobb fokúéval kezdve! Ha kész vagy, mondd, hogy \"kész\".");
            var coeffs = new List<double>();

            while(true) {
                string inp = Console.ReadLine()!;
                if(inp.Trim().ToLower() == "kész") break;

                if(double.TryParse(inp, out double x) && double.IsRealNumber(x)) {
                    if(coeffs.Count > 0 || !x.Kb(0)) {
                        coeffs.Add(x);
                        Console.WriteLine($"Jó.");
                    } else {
                        Console.WriteLine("Az első együttható ne legyen 0.");
                    }
                } else {
                    Console.WriteLine("Ez nem néz ki valós számnak.");
                }
            }

            coeffs.Reverse();
            Polynomial poly = new Polynomial(coeffs.ToArray());
            Polynomial deriv = poly.GetDerivative();

            Console.WriteLine($"A kifejezés: {poly.ToString()}");
            Console.WriteLine($"(deriváltja: {deriv.ToString()})");

            // TODO pontosság

            // Kitalál 2 olyan számot, hogy a behelyettesítési értékük más előjelű (kicsit béna megoldás)
            double x0 = 0;
            double x1 = double.NaN;
            int sign0 = Math.Sign(poly.Evaluate(x0));
            double x1p = x0; // + és - irányba is vizsgáljuk
            double x1m = x0;

            int safety = 2048;
            do {
                double dp = deriv.Evaluate(x1p);
                double dm = deriv.Evaluate(x1m);

                x1p += 1 / (Math.Abs(dp) + 0.0001);
                x1m -= 1 / (Math.Abs(dm) + 0.0001);

                double fxp = poly.Evaluate(x1p);
                double fxm = poly.Evaluate(x1m);

                int sign1p = Math.Sign(fxp);
                if(sign0 != 0 && sign0 != sign1p) {
                    x1 = x1p;
                    break;
                }

                int sign1m = Math.Sign(fxm);
                if(sign0 != 0 && sign0 != sign1m) {
                    x1 = x1m;
                    break;
                }
            } while(safety-- > 0);

            if(safety <= 0) {
                Console.WriteLine("Nem sikerült eltérő előjelű helyeket találni, szerintem ennek a polinómnak nincs is valós gyöke! (elképzelhető, hogy éppen csak érinti az abcisszatengelyt, ezért nincs negatív része)");
                return;
            }

            // Sorrend +teremtése
            if(x0 > x1) (x0, x1) = (x1, x0);

            Console.WriteLine($"x0 = {x0} (f(x0) = {poly.Evaluate(x0)})");
            Console.WriteLine($"x1 = {x1} (f(x1) = {poly.Evaluate(x1)})");

            Console.WriteLine("Mekkora legyen a pontosság? (add meg k értékét, és a tolerancia 10^-k lesz)");
            int exponent;
            while(!int.TryParse(Console.ReadLine()!, out exponent));

            double targetTolerance = Math.Pow(0.1, exponent);

            double a = x0;
            double b = x1;
            int n = 0;
            while(true) {
                double c = (a+b)/2;

                if(poly.Evaluate(c) < 0 != poly.Evaluate(a) < 0) {
                    b = c;
                } else {
                    a = c;
                }

                n++;
                // Ezt jobban is meg lehet csinálni, ha zártan kiszámoljuk előre, hogy hány iterációt kell csinálnunk.
                double tolerance = Math.Abs(x0 + x1) / Math.Pow(2, n);
                if(tolerance <= targetTolerance) break;
            }

            double approxRoot = (a+b)/2;
            Console.WriteLine($"Gyök helye: {approxRoot} (helyettesítési érték ezen helyen: {poly.Evaluate(approxRoot)})");
        }

    }


    static class NumberExtensions {
        static readonly IReadOnlyList<string> SUP = new string[] { "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹" };

        public static bool Kb(this double x, double y) => Math.Abs(x - y) < double.Epsilon;

        public static string ToSuperscriptString(this long num) {
            string str = "";

            while(num > 0) {
                str = SUP[(int)(num % 10)] + str;
                num /= 10;
            }

            return str;
        }

    }

    readonly ref struct Polynomial {

        /// <summary>
        /// Az együtthatók a legkisebb fokúval kezdve.
        /// Tök jó hogy nem <see cref="ReadOnlySpan{T}"/>, ezért amúgy "mutable" a struct.
        /// </summary>
        public readonly Span<double> coeffs;

        public int Degree => coeffs.Length - 1;


        public Polynomial(Span<double> co) {
            if(co.Length == 0) throw new ArgumentException("Legalább 1 együttható kell.", nameof(co));
            if(co.Length > 1 && co[^1].Kb(0)) throw new Exception("Az első együttható nem lehet 0 ha nem nulladfokú a polinóm.");

            coeffs = co;
        }


        public string ToString(string variable = "x") {
            if(coeffs.Length == 1) return coeffs[0].ToString();

            var sb = new System.Text.StringBuilder();

            bool first = true;
            for(int i = coeffs.Length - 1; i >= 0; i--) {
                if(coeffs[i].Kb(0)) continue;

                if(!first) sb.Append(coeffs[i] > 0 ? " + " : " - ");
                first = false;

                sb.Append(first ? coeffs[i] : Math.Abs(coeffs[i]));
                switch(i) {
                    case 0: break;
                    case 1:
                        sb.Append(variable);
                        break;
                    default:
                        sb.Append(variable);
                        sb.Append(((long)i).ToSuperscriptString());
                        break;
                }

            }

            return sb.ToString();
        }


        public double Evaluate(double x) {
            double val = coeffs[^1];

            for(int i = coeffs.Length - 2; i >= 0; i--) {
                val *= x;
                val += coeffs[i];
            }

            return val;
        }

        public Polynomial GetDerivative() {
            Span<double> mem = new double[Math.Max(1, coeffs.Length - 1)];

            if(coeffs.Length <= 1) {
                mem.Fill(0);
                return new Polynomial(mem);
            }

            for(int i = coeffs.Length - 1; i >= 1; i--) {
                mem[i - 1] = coeffs[i] * i;
            }

            /*while(mem[^1].Kb(0) && mem.Length > 1) {
                mem = mem.Slice(start: 0, length: mem.Length - 1);
            }*/

            return new Polynomial(mem);
        }

    }

}
