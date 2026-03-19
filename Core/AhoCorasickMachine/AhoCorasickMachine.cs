namespace DingFrame
{
	using System.Collections.Generic;
	
	public class AhoCorasickMachine
	{
		private AhoNode root = new ();

		public void AddWord(string word)
		{
			var node = root;
			foreach (var ch in word)
			{
				if (!node.Children.ContainsKey(ch))
					node.Children[ch] = new AhoNode();

				node = node.Children[ch];
			}
			node.Outputs.Add(word);
		}

		public void BuildFailureLink()
		{
			var queue = new Queue<AhoNode>();
			foreach (var child in root.Children.Values)
			{
				child.Fail = root;
				queue.Enqueue(child);
			}

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				foreach (var kv in current.Children)
				{
					char ch = kv.Key;
					AhoNode child = kv.Value;

					AhoNode failState = current.Fail;
					while (failState != null && !failState.Children.ContainsKey(ch))
						failState = failState.Fail;

					child.Fail = failState != null ? failState.Children[ch] : root;

					child.Outputs.AddRange(child.Fail.Outputs);

					queue.Enqueue(child);
				}
			}
		}

		public List<(int Index, string Word)> FindMatches(string text)
		{
			List<(int, string)> results = new();
			AhoNode node = root;
			for (int i = 0; i < text.Length; i++)
			{
				var ch = text[i];
				while (node != root && !node.Children.ContainsKey(ch))
					node = node.Fail;
				
				if (node.Children.ContainsKey(ch))
					node = node.Children[ch];

				foreach (string world in node.Outputs)
					results.Add((i - world.Length + 1, world));

			}
			return results;
		}

		public bool ContainsAny(string text) => FindMatches(text).Count > 0;

		public string Replace(string text, char mask = '*')
		{
			if (string.IsNullOrEmpty(text)) return text;
			
			var chars = text.ToCharArray();
			foreach (var match in FindMatches(text))
			{
				for (int j = 0; j < match.Word.Length; j++)
				{
					var idx = match.Index + j;
					if (idx >= 0 && idx < chars.Length)
						chars[idx] = mask;
				}
			}
			return new string(chars);
		}
	}

}
