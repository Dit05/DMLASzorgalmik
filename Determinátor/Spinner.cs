using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;


namespace Determinátor {

    class Spinner {

        string chars;
        public int sleep = 100;
        CancellationTokenSource? tokenSource = null;
        Task? spinTask = null;

        [MemberNotNullWhen(true, nameof(tokenSource))]
        [MemberNotNullWhen(true, nameof(spinTask))]
        public bool Spinning => tokenSource != null && spinTask != null;


        public Spinner(string chars = "|/-\\") {
            this.chars = chars;
        }


        public void Start() {
            if(Spinning) return;

            tokenSource = new CancellationTokenSource();
            spinTask = Task.Run( () => Spin(tokenSource.Token) );
        }

        public void Stop() {
            if(!Spinning) return;

            tokenSource.Cancel();
            spinTask.Wait();

            tokenSource = null;
            spinTask = null;
        }


        async void Spin(CancellationToken cancelToken) {
            int i = 0;

            while(!cancelToken.IsCancellationRequested) {
                Console.CursorLeft = 0;
                Console.Write(chars[i]);
                i = (i + 1) % chars.Length;
                await Task.Delay(sleep, cancelToken).ContinueWith( task => task.Exception == default /* Amikor a delay cancelelődik, exceptiont dob, ezt meg kell etetni egy másik taskkal. */ );
            }

            Console.CursorLeft = 0;
            Console.Write(' ');
            Console.CursorLeft = 0;
        }

    }

}
