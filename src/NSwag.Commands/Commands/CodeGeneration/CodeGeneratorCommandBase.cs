//-----------------------------------------------------------------------
// <copyright file="CodeGeneratorCommandBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using NConsole;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration;

namespace NSwag.Commands.CodeGeneration
{
    public class CustomEnumNameGenerator : IEnumNameGenerator
    {
        private readonly DefaultEnumNameGenerator baseGenerator;

        public CustomEnumNameGenerator()
        {
            baseGenerator = new DefaultEnumNameGenerator();
        }

        public string Generate(int index, string name, object value, JsonSchema schema)
        {
            // throw new Exception("generating enum" + index.ToString() + name + value.ToString() + schema.ToString());
            var baseString = baseGenerator.Generate(index, name, value, schema);
            return ConversionUtilities.ConvertToUpperCamelCase(baseString
                    .Replace(":", "-").Replace('_', '-'), true)
                .Replace(".", "_");
        }
    }

    public class CustomCSharpTypeNameGenerator : ITypeNameGenerator
    {
        private readonly DefaultTypeNameGenerator baseGenerator;

        public CustomCSharpTypeNameGenerator()
        {
            baseGenerator = new DefaultTypeNameGenerator();
        }

        public string Generate(JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            var baseString = baseGenerator.Generate(schema, typeNameHint, reservedTypeNames);
            return ConversionUtilities.ConvertToUpperCamelCase(baseString
                .Replace('_', '-'), true);
        }
    }

    public class CustomCSharpPropertyNameGenerator : IPropertyNameGenerator
    {
        private readonly CSharpPropertyNameGenerator baseGenerator;

        public CustomCSharpPropertyNameGenerator()
        {
            baseGenerator = new CSharpPropertyNameGenerator();
        }

        public string Generate(JsonSchemaProperty property)
        {
            // throw new Exception("generating prop" + property.ToString());
            var baseString = baseGenerator.Generate(property);
            return ConversionUtilities.ConvertToUpperCamelCase(baseString.Replace('_', '-')
                .Replace("@", "")
                .Replace(".", "-"), true);
        }
    }

    public abstract class CodeGeneratorCommandBase<TSettings> : InputOutputCommandBase
        where TSettings : ClientGeneratorBaseSettings
    {
        protected CodeGeneratorCommandBase(TSettings settings)
        {
            Settings = settings;

            settings.CodeGeneratorSettings.EnumNameGenerator = new CustomEnumNameGenerator();
            settings.CodeGeneratorSettings.PropertyNameGenerator = new CustomCSharpPropertyNameGenerator();
            settings.CodeGeneratorSettings.TypeNameGenerator = new CustomCSharpTypeNameGenerator();
        }

        [JsonIgnore] public TSettings Settings { get; }

        [Argument(Name = "TemplateDirectory", IsRequired = false,
            Description = "The Liquid template directory (experimental).")]
        public string TemplateDirectory
        {
            get { return Settings.CodeGeneratorSettings.TemplateDirectory; }
            set { Settings.CodeGeneratorSettings.TemplateDirectory = value; }
        }

        [Argument(Name = "TypeNameGenerator", IsRequired = false,
            Description =
                "The custom ITypeNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string TypeNameGeneratorType { get; set; }

        [Argument(Name = "PropertyNameGeneratorType", IsRequired = false,
            Description =
                "The custom IPropertyNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string PropertyNameGeneratorType { get; set; }

        [Argument(Name = "EnumNameGeneratorType", IsRequired = false,
            Description =
                "The custom IEnumNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string EnumNameGeneratorType { get; set; }

        // TODO: Use InitializeCustomTypes method
        public void InitializeCustomTypes(AssemblyLoader.AssemblyLoader assemblyLoader)
        {
            if (!string.IsNullOrEmpty(TypeNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.TypeNameGenerator =
                    (ITypeNameGenerator)assemblyLoader.CreateInstance(TypeNameGeneratorType);
            }

            if (!string.IsNullOrEmpty(PropertyNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.PropertyNameGenerator =
                    (IPropertyNameGenerator)assemblyLoader.CreateInstance(PropertyNameGeneratorType);
            }

            if (!string.IsNullOrEmpty(EnumNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.EnumNameGenerator =
                    (IEnumNameGenerator)assemblyLoader.CreateInstance(EnumNameGeneratorType);
            }
        }
    }
}