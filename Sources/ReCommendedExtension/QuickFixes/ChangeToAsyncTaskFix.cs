using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using ReCommendedExtension.Highlightings;

namespace ReCommendedExtension.QuickFixes
{
    [QuickFix]
    public sealed class ChangeToAsyncTaskFix : QuickFixBase
    {
        [NotNull]
        readonly AvoidAsyncVoidHighlighting highlighting;

        [NotNull]
        readonly IDeclaredType taskType;

        public ChangeToAsyncTaskFix([NotNull] AvoidAsyncVoidHighlighting highlighting)
        {
            this.highlighting = highlighting;

            var psiModule = highlighting.MethodDeclaration.GetPsiModule();

            var predefinedType = psiModule.GetPredefinedType();

            taskType = predefinedType.Task;
        }

        public override bool IsAvailable(JetBrains.Util.IUserDataHolder cache) => true;

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                highlighting.MethodDeclaration.SetType(taskType.ToIType());
            }

            return _ => { };
        }

        public override string Text => $"Change return type to '{taskType.GetClrName().ShortName}'";
    }
}