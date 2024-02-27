using System;


namespace Euklidész {

    static class Program {

        /// <summary>
        /// <see cref="Math.DivRem"/> csak mindig pozitív maradékkal.
        /// </summary>
        static int PositiveDivRem(int a, int b, out int r) {
            // Ha a Math.DivRem nem használható:
            //int q = a / b;
            //r = a % b;

            int q = Math.DivRem(a, b, out r);
            if(r < 0) {
                r += b;
                q--;
            }

            return q;
        }

        /// <summary>
        /// Ineffektív módon de kiszámolja hogy a célszám a paraméterek milyen lineáris kombinációjából jön ki.
        /// </summary>
        static void BruteForceLinearCombination(int a, int b, int target, out ABCount combination) {
            int aFac = 0;
            int bFac = 0;

            // Bejárja a Z×Z-t egy spirál alakban, így biztosan megtalálja a lineáris kombinációt. (nem hatékony)
            int dirX = 1;
            int dirY = 0;
            int stepCount = 1;
            int step = 0;
            while(aFac * a + bFac * b != target) {
                aFac += dirX;
                bFac += dirY;

                step++;
                if(step == stepCount) {
                    (dirX, dirY) = (-dirY, dirX); // Balra (+ irányba) kanyarodik

                    if(dirY == 0) stepCount++; // Minden második kanyarodástól kezdve egyel többet kell menni
                    step = 0;
                }
            }

            combination = new ABCount(aFac, bFac);
        }


        /// <summary>
        /// Azt tárolja, hogy egy számban mennyi a és mennyi b van. Gyakorlatilag egy kételemű vektor, amely a lineáris kombináció együtthatóit jelképezi.
        /// <summary>
        struct ABCount {
            public int a;
            public int b;


            public ABCount(int a, int b) {
                this.a = a;
                this.b = b;
            }


            public override string ToString() => $"{a}*a + {b}*b";

            public override bool Equals(object? obj) => throw new NotImplementedException();
            public override int GetHashCode() => throw new NotImplementedException();


            public int Combine(int a, int b) => this.a *a + this.b * b;


            public static ABCount operator -(ABCount ab) => new ABCount(-ab.a, -ab.b);

            public static ABCount operator *(ABCount ab, int n) => new ABCount(ab.a * n, ab.b * n);
            public static ABCount operator *(int n, ABCount ab) => ab * n;

            public static ABCount operator +(ABCount l, ABCount r) => new ABCount(l.a + r.a, l.b + r.b);
            public static ABCount operator -(ABCount l, ABCount r) => l + (-r);


            public static bool operator ==(ABCount l, ABCount r) => l.a == r.a && l.b == r.b;
            public static bool operator !=(ABCount l, ABCount r) => !(l == r);
        }


        /// <summary>
        /// Euklidész algoritmusával kiszámolja <paramref name="a"/> és <paramref name="b"/> legnagyobb közös osztóját (<paramref name="gcd"/>), illetve hogy az a és b mely lineáris kombinációja eredményezi az LNKO-t (<paramref name="combination"/>.
        /// </summary>
        static void EuclidAlgorithmProMax(int a, int b, out int gcd, out ABCount combination) {
            // A lineáris kombináció kitalálsának módszere: nyomonköveti, hogy a remainderek mennyi a-ból és b-ből állnak.

            // Előző- és előző előtti a-b számlálók. A kezdőértékük maguk az a és a b. (1-1 van belőlük)
            ABCount lastLastCount = new ABCount(1, 0);
            ABCount lastCount = new ABCount(0, 1);

            ABCount count = new ABCount(); // Ebbe követjük nyomon hogy a jelenlegi remainder mennyi a-t és b-t tartalmaz.

            while(a != 0 && b != 0) {
                int q = PositiveDivRem(a, b, out int r);

                // a = q0 * b + r0  ->  r0 = a - q0*b
                // b = q1 * r0 + r1  ->  r1 = b - q1*r0
                // q1 = q2 * r1 + r2  ->  r2 = q1 - q2*r1
                // ...
                // q[n-1] = q[n] * r[n-1] + r[n]  ->  r[n] = q[n-1] - q[n]*r[n-1]

                // q: szorzó
                // r: ezek tartalmaznak a-kat és b-ket

                count = lastLastCount - q * lastCount;

                if(verbose) Console.WriteLine($"{a} = {q}*{b} + {r} ({count})");

                // Lecsúsztatás
                (lastLastCount, lastCount) = (lastCount, count);
                a = b;
                b = r;
            }

            combination = lastLastCount; // Visszalépünk egyet, mert az utolsó előtti maradék a és b tartalma érdekel (azért lastLastCount, mert az a ciklus végén már egyszer léptetve lett, ezért abban található az actually előző)
            gcd = a;
        }



        static bool verbose;


        static void Main(string[] args) {

            bool has_arg(string arg) {
                foreach(string a in args) if(a == arg) return true;
                return false;
            }

            verbose = has_arg("--verbose");

            if(has_arg("--grind")) {
                Grind();
            } else {
                Interactive();
            }

        }


        static void Interactive() {
            int read_int() {
                int read;
                while(!int.TryParse(Console.ReadLine() ?? "", out read)) /* üres */;
                return read;
            }

            while(true) {
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Írd be az át.");
                int a = read_int();
                Console.WriteLine("Írd be a bét.");
                int b = read_int();

                EuclidAlgorithmProMax(a, b, out int gcd, out ABCount combination);

                Console.WriteLine($"A legnagyobb közös osztó: {gcd}.");
                Console.WriteLine($"Az LNKO az a és b lineáris kombinációjaként: {gcd} = {combination.a}*{a} + {combination.b}*{b}.");
                if(combination.Combine(a, b) != gcd) Console.WriteLine("(hazugság, nem is jól számította ki a program a lineáris kombinációt valamiért)");
            }
        }

        static void Grind() {
            var rand = new Random();
            int gen_val() {
                return (int)rand.NextInt64(int.MinValue, (long)int.MaxValue + 1);
            }

            Console.WriteLine("Önteszt fut. Nyomj meg egy billentyűt a megállításhoz.");
            int runs = 0;
            while(!Console.KeyAvailable) {
                int a = gen_val();
                int b = gen_val();

                EuclidAlgorithmProMax(a, b, out int gcd, out ABCount combination);
                //BruteForceLinearCombination(a, b, gcd, out ABCount actualCombination);
                if(verbose) {
                    Console.WriteLine($"a: {a}, b: {b}");
                }

                if(combination.Combine(a, b) != gcd) {
                    Console.WriteLine($"HIBA!!!");
                    Console.WriteLine($"Euklidész szerint: {gcd} = {combination.a}*{a} + {combination.b}*{b} (a lineáris kombó értéke valójában: {combination.Combine(a, b)})");
                    break;
                }

                runs++;
            }

            Console.WriteLine($"{runs} alkalommal futott.");
        }

    }

}
