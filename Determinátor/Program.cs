using System;
using System.Collections.Generic;


namespace Determinátor {

    readonly struct Nothing {
        public static readonly Nothing Value = new Nothing();
    }

    readonly ref struct Permutation {

        private static readonly List<int[]> cache = new List<int[]>() { new int[0], new int[1] { 0 }, new int[4] { 0, 1, 1, 0 } };

        private static IEnumerable<Nothing> GenerateFromCache(int[] digits) {
            int n = digits.Length;
            int[] line = cache[n];

            for(int i = 0; i < line.Length; i += n) {
                line.AsSpan(i, n).CopyTo(digits);
                yield return Nothing.Value;
            }
        }

        private static int[] GenerateNextCacheLine() {
            int n = cache.Count;

            int fact = 1;
            for(int i = 1; i <= n; i++) fact *= i;

            int[] last = new int[n - 1];

            int ptr = 0;
            int[] line = new int[fact * n]; // Egy darab arraybe vannak tömörítve a kombinációk.

            // n darab elemet választhatunk az első helyre
            for(int i = 0; i < n; i++) {
                // A maradék n-1 elemet kiválasztjuk P(n-1) módon, ez már megvan a cachéban.
                foreach(var _ in GenerateFromCache(last)) {
                    line[ptr++] = i; // Az első az első helyre vett elem.

                    for(int j = 0; j < last.Length; j++) {
                        int melyikSzámot = last[j];

                        if(melyikSzámot >= i) melyikSzámot++; // Az i-edik elemet már kiválasztottuk, a többi elem pedig "balra csúszik".
                        line[ptr++] = melyikSzámot;
                    }
                }
            }

            return line;
        }

        public static long CalculateTotalCacheSize() {
            long total = 0;
            for(int i = 0; i < cache.Count; i++) {
                total += cache[i].Length;
            }
            return total;
        }


        readonly ReadOnlySpan<int> span;


        public int this[int i] {
            get => span[i];
        }


        public Permutation(ReadOnlySpan<int> span) {
            this.span = span;
        }


        public override string ToString() {
            var sb = new System.Text.StringBuilder();

            sb.Append('[');
            for(int i = 0; i < span.Length - 1; i++) {
                sb.Append(span[i]);
                sb.Append(", ");
            }
            sb.Append(span[span.Length - 1]);
            sb.Append(']');

            return sb.ToString();
        }


        public int CountInversions() {
            int count = 0;

            for(int k = 0; k < span.Length; k++) {
                for(int l = k + 1; l < span.Length; l++) {
                    if(k.CompareTo(l) != this[k].CompareTo(this[l])) count++;
                }
            }

            return count;
        }


        public static IEnumerable<Nothing> GenerateAll(int[] digits) {
            while(cache.Count <= digits.Length) {
                cache.Add(GenerateNextCacheLine());
            }

            return GenerateFromCache(digits);
        }

        public static IEnumerable<Nothing> GenerateAllPoorly(int[] digits) {
            if(digits.Length <= 0) yield break;

            int ptr = 0;

            while(ptr >= 0) {

                bool good = true;
                for(int i = 0; i < ptr; i++) {
                    if(digits[i] == digits[ptr]) {
                        good = false;
                        break;
                    }
                }

                if(good) {
                    ptr++;
                    if(ptr == digits.Length) {
                        yield return Nothing.Value; // Készen áll a permutáció
                        ptr--;
                        digits[ptr]++;
                    }
                } else {
                    digits[ptr]++;
                }


                while(digits[ptr] >= digits.Length) {
                    digits[ptr] = 0;
                    ptr--;

                    if(ptr < 0) yield break;
                    digits[ptr]++;
                }

            }
        }

    }



    static class Program {

        delegate bool TryParse<T>(string str, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? parsed);
        static T GetChecked<T>(TryParse<T> parser) {
            T? parsed;
            while(!parser(Console.ReadLine() ?? "null", out parsed));
            return parsed;
        }


        static void Main(string[] args) {

            bool has_flag(string flag) {
                foreach(string a in args) if(a == flag) return true;
                return false;
            }


            if(has_flag("--show-permutations")) {
                Console.WriteLine("Hány elem permutációját?");
                int order = GetChecked<int>(int.TryParse);

                Show(order);
                return;
            }

            if(has_flag("--benchmark-permutations")) {
                CompositeBenchmark();
                return;
            }


            // Sima determináns kiszámolás
            int n;
            if(args.Length == 1 && int.TryParse(args[0], out n)) {
                // Akkor ez az n.
            } else {
                Console.WriteLine("Mekkora legyen a mátrix?");
                n = GetChecked<int>(int.TryParse);
            }

            double[,] mátrix = new double[n, n];

            var rand = new Random();
            for(int s = 0; s < n; s++) {
                Console.Write("[");
                for(int o = 0; o < n; o++) {
                    mátrix[s, o] = rand.Next(-10, 10+1);
                    Console.Write(mátrix[s, o].ToString().PadLeft(4));
                }
                Console.WriteLine("  ]");
            }

            // >> ITT A LÉNYEG! <<
            int[] permutációAlap = new int[n];
            double det = 0;

            var stopper = new System.Diagnostics.Stopwatch();
            stopper.Start();
            long step = 0;
            long stepCount = Factorial(n); // Permutációxáma: n!
            int lastPercent = -1;
            foreach(var _ in Permutation.GenerateAll(permutációAlap)) { // Szumma
                step++;
                double progress = step / (double)stepCount;
                int percent = (int)(progress * 100);
                if(percent != lastPercent) {
                    lastPercent = percent;
                    Console.CursorLeft = 0;
                    Console.Write($"{percent}%".PadRight(4));
                }

                // Összeszorzás
                var f = new Permutation(permutációAlap);

                double szorz = Math.Pow(-1, f.CountInversions()); // Definíció szerint a -1-et emeljük kitevőre.
                for(int i = 0; i < n; i++) {
                    szorz *= mátrix[i, f[i]];
                }

                det += szorz;
            }
            stopper.Stop();

            Console.WriteLine($"\n\nA determináns: {det}");
            Console.WriteLine($"Idő: {stopper.Elapsed}");
        }


        static void Show(int n) {
            var spinner = new Spinner();
            spinner.Start();

            int[] working = new int[n];
            foreach(var _ in Permutation.GenerateAll(working)) {
                spinner.Stop();
                var perm = new Permutation(working);
                Console.WriteLine($"{perm.ToString()}, inverziószám: {perm.CountInversions()}");
                spinner.Start();
            }
            spinner.Stop();
        }


        static void CompositeBenchmark() {
            Console.WriteLine("\nDinamikus programozás, 1. futás:");
            Benchmark(Permutation.GenerateAll, 11);
            Console.WriteLine("\nDinamikus programozás, 2. futás:");
            Benchmark(Permutation.GenerateAll, 11);
            Console.WriteLine("\nDinamikátlan programozás:");
            Benchmark(Permutation.GenerateAllPoorly, 11);
        }


        static long Factorial(int n) => n > 1 ? n * Factorial(n - 1) : 1;


        static void Benchmark(Func<int[], IEnumerable<Nothing>> generator, int limit = int.MaxValue) {
            var stopper = new System.Diagnostics.Stopwatch();

            for(int n = 0; n <= limit; n++) {
                var perm = new int[n];

                long fact = Factorial(n);

                stopper.Start();
                int count = 0;
                foreach(var _ in generator(perm)) {
                    count++;
                }
                stopper.Stop();

                Console.WriteLine($"P({n}): {count}, n!: {fact} (time: {stopper.Elapsed})");
            }
        }
    }

}
