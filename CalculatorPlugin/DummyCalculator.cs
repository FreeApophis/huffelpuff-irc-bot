using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin
{
	internal class DummyCalculator : WebCalculator
	{
	    public override CalculationResult Calculate(string equation)
	    {
	        throw new NotImplementedException();
	    }
	}
}
