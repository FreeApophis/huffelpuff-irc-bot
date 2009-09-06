using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Plugin
{
    class MagicCalculator
    {
        private List<WebCalculator> calculators = new List<WebCalculator>();
        private List<CalculationResult> results = new List<CalculationResult>();

        public MagicCalculator()
        {
            calculators.Add(new GoogleCalculator());
            calculators.Add(new BingCalculator());
            calculators.Add(new OctaveCalculator());
        }

        public CalculationResult Calculate(string equation)
        {
            CalculateAll(equation);
            foreach (CalculationResult result in results)
            {
                if (result.HasResult)
                {
                    return result;
                }
            }
            return CalculationResult.NoResult();
        }

        public List<CalculationResult> CalculateAll(string equation)
        {
            results.Clear();
            foreach (WebCalculator calc in calculators)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(calulatorThreadStarter), new { Calc = calc, Eq = equation });
            }
            while (results.Count < calculators.Count)
            {
                Thread.Sleep(50);
            }
            return results;

        }

        private void calulatorThreadStarter(object state)
        {
            var tState = Cast(state, new { Calc = (WebCalculator)new DummyCalculator(), Eq = "" });
            results.Add(tState.Calc.Calculate(tState.Eq));
        }

        private T Cast<T>(object obj, T type)
        {
            return (T)obj;
        }

    }
}
