using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Plugin
{
    public enum ResultProvider
    {
        Unknown,
        Bing,
        Google,
        Octave
    }

    public class CalculationResult
    {
        public bool HasResult { get; private set; }
        public bool IsComplex { get; private set; }

        public string Result { get; private set; }
        public string CompleteEquation { get; private set; }

        public ResultProvider ResultProvider { get; private set; }


        public CalculationResult()
        {
            SetResultProvider();
            
            this.HasResult = false;
            this.IsComplex = false;
        }

        public CalculationResult(string resultonly, string complete, bool isComplex)
        {
            SetResultProvider();

            this.Result = resultonly.Trim();
            this.CompleteEquation = complete.Trim();
            
            this.HasResult = true;
            this.IsComplex = isComplex;
        }

        private void SetResultProvider()
        {
            StackTrace trace = new StackTrace();
            Type t = trace.GetFrame(2).GetMethod().DeclaringType;
            if (t == typeof(GoogleCalculator))
            {
                this.ResultProvider = ResultProvider.Google;
            }
            else if (t == typeof(BingCalculator))
            {
                this.ResultProvider = ResultProvider.Bing;
            }
            else if (t == typeof(OctaveCalculator))
            {
                this.ResultProvider = ResultProvider.Octave;
            }
            else
            {
                this.ResultProvider = ResultProvider.Unknown;
            }
        }

        internal static CalculationResult NoResult()
        {
            return new CalculationResult();
        }

        public override string ToString()
        {
            if (!this.HasResult)
            {
                return "no result";
            }
            else
            {
                return this.Result + " [" + this.ResultProvider+"]";
            }
        }
    }
}
