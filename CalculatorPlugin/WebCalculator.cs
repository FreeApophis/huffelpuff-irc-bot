using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin
{
    abstract class WebCalculator
    {
        public abstract CalculationResult Calculate(string equation);
    }
}
