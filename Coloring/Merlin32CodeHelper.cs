using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using VSMerlin32.Coloring.Data;

namespace VSMerlin32.Coloring
{
    internal class Merlin32CodeHelper
    {
        private static readonly string CommentRegex = @"((\u003B)|(\u002A))(.*)"; // ;
        private static readonly string TextRegex = @"(""|')[^']*(""|')";
        // OPCODE_REG and below are initialized dynamically below.
        private static readonly string RegexBoilerplate = @"(\b|\s)(?<{0}>{1})(\b|\s)";
        private static readonly string Opcode = "OPCODE";
        private static readonly string Data = "DATA";
        private static readonly string Directive = "DIRECTIVE";
        private static readonly string Elup = "ELUP";
        private static string _opcodeRegex = "";
        private static string _directiveRegex = "";
        private static string _dataRegex = "";

        public static IEnumerable<SnapshotHelper> GetTokens(SnapshotSpan span)
        {
            string TempRegex; // temp var string
            ITextSnapshotLine containingLine = span.Start.GetContainingLine();
            int curLoc = containingLine.Start.Position;
            string formattedLine = containingLine.GetText();

            int commentMatch = int.MaxValue;
            Regex reg = new Regex(CommentRegex);
            foreach (Match match in reg.Matches(formattedLine))
            {
                commentMatch = match.Index < commentMatch ? match.Index : commentMatch;
                yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Comment);
            }

            reg = new Regex(TextRegex);
            foreach (Match match in reg.Matches(formattedLine))
            {
                if (match.Index < commentMatch)
                    yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Text);
            }

            // OG NEW
            // OPCODES
            TempRegex = "";
            foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
            {
                TempRegex += token.ToString() + ("|");
            }
            // we remove the last "|" added
            TempRegex = TempRegex.Remove(TempRegex.LastIndexOf("|", StringComparison.Ordinal));
            _opcodeRegex = string.Format(RegexBoilerplate, Opcode, TempRegex);

            reg = new Regex(_opcodeRegex,RegexOptions.IgnoreCase);
            Match opcodeMatch = reg.Match(formattedLine);
            if (opcodeMatch.Success)
            {
                foreach (Capture opcode in opcodeMatch.Groups[Opcode].Captures)
                {
                    // An opcode after within a comment doesn't get a SnapShotSpan...
                    if (opcode.Index < commentMatch)
                        yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, opcode.Index + curLoc), opcode.Length), Merlin32TokenTypes.Merlin32Opcode);
                }
            }

            // OG NEW
            // DIRECTIVES
            TempRegex = "";
            string elupDirective = Resources.directives.ELUP;
            foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
            {
                if (token.ToString() != elupDirective)
                    TempRegex += token.ToString() + ("|");
            }
            // we remove the last "|" added
            TempRegex = TempRegex.Remove(TempRegex.LastIndexOf("|", StringComparison.Ordinal));
            _directiveRegex = string.Format(RegexBoilerplate, Directive, TempRegex);

            reg = new Regex(_directiveRegex, RegexOptions.IgnoreCase);
            Match directiveMatch = reg.Match(formattedLine);
            if (directiveMatch.Success)
            {
                foreach (Capture directive in directiveMatch.Groups[Directive].Captures)
                {
                    if (directive.Index < commentMatch)
                        yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, directive.Index + curLoc), directive.Length), Merlin32TokenTypes.Merlin32Directive);
                }
            }

            // We also need to check for special ELUP directive...
            reg = new Regex(Resources.directives.ELUPRegex);
            Match elupMatch = reg.Match(formattedLine);
            if (elupMatch.Success)
            {
                foreach (Capture elup in elupMatch.Groups[Elup].Captures)
                {
                    if (elup.Index < commentMatch)
                        yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, elup.Index + curLoc), elup.Length), Merlin32TokenTypes.Merlin32Directive);
                }
            }

            // OG NEW
            // DATADEFINES
            TempRegex = "";
            foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
            {
                TempRegex += token.ToString() + ("|");
            }
            // we remove the last "|" added
            TempRegex = TempRegex.Remove(TempRegex.LastIndexOf("|", StringComparison.Ordinal));
            _dataRegex = string.Format(RegexBoilerplate, Data, TempRegex);

            reg = new Regex(_dataRegex, RegexOptions.IgnoreCase);
            Match dataMatch = reg.Match(formattedLine);
            if (dataMatch.Success)
            {
                foreach (Capture data in dataMatch.Groups[Data].Captures)
                {
                    if (data.Index < commentMatch)
                        yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, data.Index + curLoc), data.Length), Merlin32TokenTypes.Merlin32DataDefine);
                }
            }
        }
    }
}
