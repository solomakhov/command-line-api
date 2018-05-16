// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace System.CommandLine
{
    public class CommandDefinition : SymbolDefinition
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        public CommandDefinition(
            IReadOnlyCollection<SymbolDefinition> symbols) :
            this(executableName.Value, "", symbols)
        {
        }

        public CommandDefinition(
            string name,
            string description,
            ArgumentDefinition argumentDefinition,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, argumentDefinition)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
        }

        public CommandDefinition(
            string name,
            string description,
            IReadOnlyCollection<SymbolDefinition> symbolDefinitions,
            ArgumentDefinition argumentDefinition = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            var validSymbolAliases = symbolDefinitions
                                     .SelectMany(o => o.RawAliases)
                                     .ToArray();

            ArgumentDefinitionBuilder builder;
            if (argumentDefinition == null)
            {
                builder = new ArgumentDefinitionBuilder();
            }
            else
            {
                builder = ArgumentDefinitionBuilder.From(argumentDefinition);
            }

            builder.ValidTokens.UnionWith(validSymbolAliases);

            if (argumentDefinition == null)
            {
                ArgumentDefinition = builder.ZeroOrMore();
            }
            else
            {
                ArgumentDefinition = argumentDefinition;
            }

            foreach (SymbolDefinition symbol in symbolDefinitions)
            {
                symbol.Parent = this;
                SymbolDefinitions.Add(symbol);
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; }

        public override string ToString() => Name;
    }
}
