namespace DingFrame
{
	using System;
	using System.Collections.Generic;

	public static class RPNCalculator
	{
		private static Dictionary<string, int> operatorsPrecedence = new Dictionary<string, int>
		{
			{ "-", 1 },
			{ "+", 1 },
			{ "*", 2 },
			{ "/", 2 },
			{ "u-", 3 }
		};

		public static double Evaluate(string expression)
		{
			List<string> tokens = Tokenize(expression);
			Queue<string> postfix = ToPostfix(tokens);
			return CalculatePostfix(postfix);
		}

		private static List<string> Tokenize(string expr)
		{
			var tokens = new List<string>();
			int len = expr.Length;
			for (int i = 0; i < len; )
			{
				char c = expr[i];

				if (char.IsWhiteSpace(c))
				{
					i++;
					continue;
				}
				
				if (char.IsDigit(c) || c == '.')
				{
					int start = i;
					while (i < len && (char.IsDigit(expr[i]) || expr[i] == '.'))
					{
						i++;
					}
					tokens.Add(expr.Substring(start, i - start));
					continue;
				}
				
				if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')')
				{
					string op = c.ToString();
					if (c == '-')
					{
						if (tokens.Count == 0 ||
							tokens[^1] == "(" ||
							operatorsPrecedence.ContainsKey(tokens[tokens.Count - 1]))
						{
							op = "u-";
						}
					}
					tokens.Add(op);
					i++;
					continue;
				}

				throw new ArgumentException($"不合法字符：{c} at pos {i}");
			}
			return tokens;
		}

		private static Queue<string> ToPostfix(List<string> tokens)
		{
			Queue<string> output = new ();
			Stack<string> operators = new ();

			foreach (var token in tokens)
			{
				if (double.TryParse(token, out double num))
				{
					output.Enqueue(token);
				}
				else if (operatorsPrecedence.ContainsKey(token))
				{
					while (operators.Count > 0
						&& operatorsPrecedence.ContainsKey(operators.Peek())
						&& operatorsPrecedence[operators.Peek()] >= operatorsPrecedence[token])
					{
						output.Enqueue(operators.Pop());
					}
					operators.Push(token);
				}
				else if (token == "(")
				{
					operators.Push(token);
				}
				else if (token == ")")
				{
					while (operators.Count > 0 && operators.Peek() != "(")
					{
						output.Enqueue(operators.Pop());
					}
					if (operators.Count == 0)
						throw new ArgumentException("括号不匹配");
					operators.Pop();
				}
				else
				{
					throw new ArgumentException($"未知 token：{token}");
				}
			}

			while (operators.Count > 0)
			{
				var op = operators.Pop();
				if (op == "(" || op == ")")
					throw new ArgumentException("括号不匹配");
				output.Enqueue(op);
			}

			return output;
		}

		private static double CalculatePostfix(Queue<string> tokens)
		{
			var stack = new Stack<double>();
			while (tokens.Count > 0)
			{
				string token = tokens.Dequeue();
				if (double.TryParse(token, out double num))
				{
					stack.Push(num);
				}
				else
				{
					if (token == "u-")
					{
						if (stack.Count < 1) throw new InvalidOperationException("运算符数量不匹配");
						double a = stack.Pop();
						stack.Push(-a);
					}
					else
					{
						if (stack.Count < 2) throw new InvalidOperationException("运算符数量不匹配");
						double right = stack.Pop();
						double left = stack.Pop();
						switch (token)
						{
							case "+": stack.Push(left + right); break;
							case "-": stack.Push(left - right); break;
							case "*": stack.Push(left * right); break;
							case "/": stack.Push(left / right); break;
							default: throw new ArgumentException($"未知二元运算符：{token}");
						}
					}
				}
			}

			if (stack.Count != 1) throw new InvalidOperationException("表达式不合法");

			return stack.Pop();
		}
	}
}
