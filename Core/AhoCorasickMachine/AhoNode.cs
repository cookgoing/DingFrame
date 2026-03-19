namespace DingFrame
{
	using System.Collections.Generic;

	public class AhoNode
	{
		public Dictionary<char, AhoNode> Children = new();
		public AhoNode Fail;
		public List<string> Outputs = new();

		public bool IsTerminal => Outputs.Count > 0;
	}
}
