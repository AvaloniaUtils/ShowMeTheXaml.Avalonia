﻿using System;
using System.Collections.Generic;
using System.Linq;
using XamlX.Compiler;
using XamlX.Emit;
using XamlX.Transform;
using XamlX.Transform.Transformers;
using XamlX.TypeSystem;

namespace ShowMeTheXaml.Avalonia.Infrastructure
{
    internal sealed class MiniCompiler : XamlCompiler<object, IXamlEmitResult>
    {
        public static MiniCompiler CreateDefault(RoslynTypeSystem typeSystem, params string[] additionalTypes)
        {
            var mappings = new XamlLanguageTypeMappings(typeSystem);
            foreach (var additionalType in additionalTypes)
                mappings.XmlnsAttributes.Add(typeSystem.GetType(additionalType));

            var configuration = new TransformerConfiguration(
                typeSystem,
                typeSystem.Assemblies.First(),
                mappings);
            return new MiniCompiler(configuration);
        }
        
        private MiniCompiler(TransformerConfiguration configuration)
            : base(configuration, new XamlLanguageEmitMappings<object, IXamlEmitResult>(), false)
        {
            Transformers.Add(new KnownDirectivesTransformer());
            Transformers.Add(new XamlIntrinsicsTransformer());
            Transformers.Add(new XArgumentsTransformer());
            Transformers.Add(new TypeReferenceResolver());
        }

        protected override XamlEmitContext<object, IXamlEmitResult> InitCodeGen(IFileSource file, IXamlTypeBuilder<object> declaringType, object codeGen,
            XamlRuntimeContext<object, IXamlEmitResult> context, bool needContextLocal)
            => throw new NotSupportedException();
    }
}