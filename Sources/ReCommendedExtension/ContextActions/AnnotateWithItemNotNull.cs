﻿using System.Diagnostics;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;

namespace ReCommendedExtension.ContextActions
{
    [ContextAction(Group = "C#", Name = "Annotate with [ItemNotNull] attribute" + ZoneMarker.Suffix,
        Description = "Annotates with the [ItemNotNull] attribute.")]
    public sealed class AnnotateWithItemNotNull : AnnotateWithCodeAnnotation
    {
        static bool IsAvailableForType([NotNull] IType type, [NotNull] ITreeNode context)
        {
            if (type.IsGenericEnumerableOrDescendant() || type.IsGenericArray(context))
            {
                var elementType = CollectionTypeUtil.ElementTypeByCollectionType(type, context);
                if (elementType != null && elementType.Classify == TypeClassification.REFERENCE_TYPE)
                {
                    return true;
                }
            }

            if (type.IsGenericTask())
            {
                var resultType = type.GetTaskUnderlyingType();
                if (resultType != null && resultType.Classify == TypeClassification.REFERENCE_TYPE)
                {
                    return true;
                }
            }

            if (type.IsLazy())
            {
                var typeElement = new DeclaredTypeFromCLRName(PredefinedType.LAZY_FQN, context.GetPsiModule()).GetTypeElement();
                var valueType = type.GetGenericUnderlyingType(typeElement);
                if (valueType != null && valueType.Classify == TypeClassification.REFERENCE_TYPE)
                {
                    return true;
                }
            }

            return false;
        }

        public AnnotateWithItemNotNull([NotNull] ICSharpContextActionDataProvider provider) : base(provider) { }

        protected override string AnnotationAttributeTypeName
        {
            get
            {
                Debug.Assert(ContainerElementNullnessProvider.ItemNotNullAttributeShortName != null);

                return ContainerElementNullnessProvider.ItemNotNullAttributeShortName;
            }
        }

        protected override bool CanBeAnnotated(IDeclaredElement declaredElement, ITreeNode context, IPsiModule module)
        {
            var method = declaredElement as IMethod;
            if (method != null && IsAvailableForType(method.ReturnType, context))
            {
                return true;
            }

            var parameter = declaredElement as IParameter;
            if (parameter != null && IsAvailableForType(parameter.Type, context))
            {
                return true;
            }

            var property = declaredElement as IProperty;
            if (property != null && IsAvailableForType(property.Type, context))
            {
                return true;
            }

            var delegateType = declaredElement as IDelegate;
            if (delegateType != null && IsAvailableForType(delegateType.InvokeMethod.ReturnType, context))
            {
                return true;
            }

            var field = declaredElement as IField;
            if (field != null && IsAvailableForType(field.Type, context))
            {
                return true;
            }

            return false;
        }
    }
}