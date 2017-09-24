using Microsoft.VisualStudio.Text;

namespace VSMerlin32.Coloring.Data
{
	internal class SnapshotHelper
	{
		public SnapshotSpan Snapshot { get; private set; }
		public Merlin32TokenTypes TokenType { get; private set; }

		public SnapshotHelper(SnapshotSpan span, Merlin32TokenTypes type)
		{
			Snapshot = span;
			TokenType = type;
		}
	}
}
