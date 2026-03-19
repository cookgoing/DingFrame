namespace DingFrame
{
	using System;
	using System.Linq;
	using System.Collections.Generic;

	// bug, 无法解析负数
	public class DingCalculator
	{
		private Dictionary<char, int> operatorsPrecedence = new Dictionary<char, int>
		{
			{ '-', 1 },
			{ '+', 2 },
			{ '/', 3 },
			{ '*', 4 },
		};

		public double? Excute(string formulaStr, out string errorString)
		{
			errorString = null;
			try
			{
				char[] operationArr = operatorsPrecedence.Keys.ToArray();
				char[] splitArr = new char[operationArr.Length + 2];
				Array.Copy(operationArr, splitArr, operationArr.Length);
				splitArr[splitArr.Length - 2] = '(';
				splitArr[splitArr.Length - 1] = ')';
				return Calculate(ParseFormulaElement(formulaStr, splitArr), operationArr, out errorString);
			}
			catch (Exception ex)
			{
				errorString = ex.Message;
				DLogger.Error($"message: {errorString}\nformulaStr: {formulaStr}\nstack: {ex.StackTrace}", "DingCalculator");
				return null;
			}
		}

		private List<string> ParseFormulaElement(string formulaStr, char[] splitArr)
		{
			formulaStr.Trim();
			formulaStr = string.Concat(formulaStr.Where(c => !char.IsWhiteSpace(c)));

			List<string> elementList = new();
			int startIdx = 0;
			int operationIdx = -1;
			while ((operationIdx = formulaStr.IndexOfAny(splitArr, startIdx)) != -1)
			{
				if (operationIdx > startIdx) elementList.Add(formulaStr.Substring(startIdx, operationIdx - startIdx));
				elementList.Add(formulaStr.Substring(operationIdx, 1));
				startIdx = operationIdx + 1;
			}
			elementList.Add(formulaStr.Substring(startIdx));

			return elementList;
		}

		private double? Calculate(List<string> elementList, char[] operationArr, out string errorString)
		{
			errorString = null;
			int idx = 0;
			while (true)
			{
				if (elementList.Count == 1)
				{
					if (double.TryParse(elementList[0], out double value)) return value;
					else
					{
						errorString = $"illegal value: {elementList[0]}";
						DLogger.Error(errorString, "DingCalculator");
						return null;
					}
				}

				if (elementList.Count < 3)
				{
					errorString = $"illegal formula: {string.Join(string.Empty, elementList)}";
					DLogger.Error(errorString, "DingCalculator");
					return null;
				}

				bool isOperation = elementList[idx].Length == 1 && Array.IndexOf(operationArr, elementList[idx][0]) != -1;
				if (isOperation && idx == 0)
				{
					if (idx == 0)
					{
						errorString = "illegal formula, first elemnt is not number";
						DLogger.Error(errorString, "DingCalculator");
						return null;
					}
					else --idx;
				}

				if (elementList[idx] == "(")
				{
					int bracketIdx = elementList.LastIndexOf(")");
					double? bracketValue = Calculate(elementList.GetRange(idx + 1, bracketIdx - idx - 1), operationArr, out errorString);
					if (bracketValue == null) return null;

					elementList[idx] = bracketValue.Value.ToString();
					elementList.RemoveRange(idx + 1, bracketIdx - idx);
				}

				if (idx == 0) idx += 2;

				bool touchEnd = idx == elementList.Count - 1;
				char leftOp = elementList[idx - 1][0];
				char rightOp = !touchEnd ? elementList[idx + 1][0] : ' ';
				int leftPrecedence = 0, rightPrecedence = -1;
				if (!operatorsPrecedence.TryGetValue(leftOp, out leftPrecedence)
					|| (!touchEnd && !operatorsPrecedence.TryGetValue(rightOp, out rightPrecedence)))
				{
					errorString = $"illegal operation. leftOp: {leftOp}; rightOp: {rightOp}; number: {elementList[idx]}.";
					DLogger.Error(errorString, "DingCalculator");
					return null;
				}

				if (leftPrecedence < rightPrecedence)
				{
					idx += 2;
					continue;
				}

				if (!double.TryParse(elementList[idx - 2], out double leftValue)
					|| !double.TryParse(elementList[idx], out double rightValue))
				{
					errorString = $"illegal number. leftValue: {elementList[idx - 2]}; rightValue: {elementList[idx]}";
					DLogger.Error(errorString, "DingCalculator");
					return null;
				}

				double? newValue = Calculate(leftValue, rightValue, leftOp, out errorString);
				if (newValue == null) return null;

				idx -= 2;
				elementList.RemoveRange(idx + 1, 2);
				elementList[idx] = newValue.Value.ToString();
			}
		}

		private double? Calculate(double left, double right, char op, out string errorString)
		{
			errorString = null;
			switch (op)
			{
				case '+': return left + right;
				case '-': return left - right;
				case '*': return left * right;
				case '/': return left / right;
				default:
					errorString = $"illegal operation: {op}";
					DLogger.Error(errorString, "DingCalculator");
					return null;
			}
		}
	}
}