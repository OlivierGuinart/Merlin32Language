using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VSMerlin32.Coloring.Classification
{
    internal static class OrdinaryClassificationDefinition
    {
        #region Type definition

        /// <summary>
        /// Defines the "opcode" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Opcode)]
        internal static ClassificationTypeDefinition Opcode = null;

        /// <summary>
        /// Defines the "directive" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Directive)]
        internal static ClassificationTypeDefinition Directive = null;

        /// <summary>
        /// Defines the "datadefine" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32DataDefine)]
        internal static ClassificationTypeDefinition Datadefine = null;

        /// <summary>
        /// Defines the "text" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Text)]
        internal static ClassificationTypeDefinition Text = null;

        /// <summary>
        /// Defines the "comment" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Comment)]
        internal static ClassificationTypeDefinition Comment = null;

        #endregion
    }
}
