using UIForia.Style;

namespace UIForia.Editor {

    public static class StyleGeneratorConstants {

        public static readonly string k_ParseEntryTemplate = $"            {nameof(PropertyParsers.s_ParserTable)}[::INDEX::] = new ::PARSER_TYPE_NAME::();\n";
        
        public static readonly string k_PropertyParserClass = $@"::USINGS::
namespace UIForia.Style {{

    public static partial class {nameof(PropertyParsers)} {{

        static {nameof(PropertyParsers)}() {{

            {nameof(PropertyParsers.s_ParserTable)} = new {nameof(IStylePropertyParser)}[::PARSE_TABLE_COUNT::];
::PARSER_TABLE_CREATION::          

            {nameof(PropertyParsers.s_parseEntries)} = new {nameof(PropertyParsers.PropertyParseEntry)}[::PROPERTY_NAME_COUNT::];
::PROPERTY_ENTRIES::

            {nameof(PropertyParsers.s_PropertyNames)} = new string[] {{
::PROPERTY_NAMES::
            }};

        }}
    }}

    public partial struct {nameof(PropertyId)} {{
    
::PROPERTY_IDS::
    }} 

    public partial struct {nameof(StyleProperty2)} {{

::STYLE_PROPERTY_PACKERS::::STYLE_FROM_VALUE::::STYLE_AS_VALUE::
    }}
  
    public partial class DefaultStyleValue {{

::DEFAULT_STYLE_VALUES::    

    }}
  
}}

";

        public static readonly string k_PropertyEntryTemplate = $"            {nameof(PropertyParsers.s_parseEntries)}[::INDEX::] = new  {nameof(PropertyParsers.PropertyParseEntry)}(\"::PROPERTY_NAME::\",  {nameof(PropertyId)}.::PROPERTY_NAME::, {nameof(PropertyParsers.s_ParserTable)}[::PARSER_ID::]);\n";
        

    }

}