using System;

namespace UIForia.Style {

    public interface IStyleCodeGenerator {

        void AddNamespace(string namespaceName);

        void SetTypeHandler<TType>(StyleTypeHandler styleTypeHandler);

        void AddStyleProperty(string propertyName, Type propertyType, string defaultValue, PropertyFlags flags = 0);

        void AddStyleShorthand<TParserType>(string name) where TParserType : IStyleShorthandParser;

    }

}