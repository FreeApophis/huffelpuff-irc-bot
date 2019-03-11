using ArithmeticParser;
using ArithmeticParser.Visitors;

namespace Plugin
{
    internal class ArithmeticParserCalculator : ICalculator
    {
        public CalculationResult Calculate(string equation)
        {
            var parser = new Parser(equation);
            var parseTree = parser.Parse();

            var calculator = new CalculateVisitor();
            parseTree.Accept(calculator);

            return new CalculationResult($"{calculator.Result}", $"{equation} = {calculator.Result}", false);
        }
    }
}