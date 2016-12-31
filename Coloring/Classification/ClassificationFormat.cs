using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
//using Microsoft.VisualStudio.Language.StandardClassification;

namespace VSMerlin32.Coloring.Classification
{
    #region Format definition

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Comment)]
    [Name("Merlin32CommentFormat")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class CommentFormat : ClassificationFormatDefinition
    {
        public CommentFormat()
        {
            this.DisplayName = "This is a comment"; //human readable version of the name
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Opcode)]
    [Name("Merlin32OpcodeFormat")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class OpcodeFormat : ClassificationFormatDefinition
    {
        public OpcodeFormat()
        {
            this.DisplayName = "This is an opcode"; //human readable version of the name
            this.ForegroundColor = Colors.Blue;
            // this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Directive)]
    [Name("Merlin32DirectiveFormat")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class DirectiveFormat : ClassificationFormatDefinition
    {
        public DirectiveFormat()
        {
            this.DisplayName = "This is an directive"; //human readable version of the name
            this.ForegroundColor = Colors.DarkCyan;
            // this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32DataDefine)]
    [Name("Merlin32DataDefineFormat")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class DataDefineFormat : ClassificationFormatDefinition
    {
        public DataDefineFormat()
        {
            this.DisplayName = "This is data definition"; //human readable version of the name
            this.ForegroundColor = Colors.DarkOrchid;
            // this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Text)]
    [Name("Merlin32TextFormat")]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class TextFormat : ClassificationFormatDefinition
    {
        public TextFormat()
        {
            this.DisplayName = "This is a text"; //human readable version of the name
            this.ForegroundColor = Colors.DarkRed;
        }
    }
    #endregion //Format definition
}
