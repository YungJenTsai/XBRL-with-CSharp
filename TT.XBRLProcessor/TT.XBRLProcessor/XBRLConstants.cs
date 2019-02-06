using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TT.XBRLProcessor
{
    public static class XBRLConstants
    {
        // namespace W3
        public const string XmlSchemaInstanceNamespaceUri = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XmlSchemaNamespaceUri = "http://www.w3.org/2001/XMLSchema";
        public const string XLinkNamespaceUri = "http://www.w3.org/1999/xlink";

        // namespace URIs

        public const string XbrlNamespaceUri = "http://www.xbrl.org/2003/instance";
        public const string IXbrlNamespaceUri = "http://www.xbrl.org/2008/inlineXBRL";
        public const string IXbrl11NamespaceUri = "http://www.xbrl.org/2013/inlineXBRL";
        public const string XbrlLinkbaseNamespaceUri = "http://www.xbrl.org/2003/linkbase";
        public const string XbrlDimensionsNamespaceUri = "http://xbrl.org/2005/xbrldt";
        public const string XbrlEssenceAliasArcroleNamespaceUri = "http://www.xbrl.org/2003/arcrole/essence-alias";
        public const string XbrlGeneralSpecialArcroleNamespaceUri = "http://www.xbrl.org/2003/arcrole/general-special";
        public const string XbrlSimilarTuplesArcroleNamespaceUri = "http://www.xbrl.org/2003/arcrole/similar-tuples";
        public const string XbrlRequiresElementArcroleNamespaceUri = "http://www.xbrl.org/2003/arcrole/requires-element";
        public const string XbrlFactFootnoteArcroleNamespaceUri = "http://www.xbrl.org/2003/arcrole/fact-footnote";
        public const string XbrlIso4217NamespaceUri = "http://www.xbrl.org/2003/iso4217";
        public const string XbrlRefUri = "http://www.xbrl.org/2006/ref";
        public const string XmlNamespaceUri = "http://www.w3.org/XML/1998/namespace";

        // role URIs

        public const string XbrlLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/label";
        public const string XbrlTerseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/terseLabel";
        public const string XbrlVerboseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/verboseLabel";
        public const string XbrlPositiveLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/positiveLabel";
        public const string XbrlPositiveTerseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/positiveTerseLabel";
        public const string XbrlPositiveVerboseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/positiveVerboseLabel";
        public const string XbrlNegativeLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/negativeLabel";
        public const string XbrlNegativeTerseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/negativeTerseLabel";
        public const string XbrlNegativeVerboseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/negativeVerboseLabel";
        public const string XbrlZeroLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/zeroLabel";
        public const string XbrlZeroTerseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/zeroTerseLabel";
        public const string XbrlZeroVerboseLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/zeroVerboseLabel";
        public const string XbrlTotalLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/totalLabel";
        public const string XbrlPeriodStartLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/periodStartLabel";
        public const string XbrlPeriodEndLabelRoleNamespaceUri = "http://www.xbrl.org/2003/role/periodEndLabel";
        public const string XbrlDocumentationRoleNamespaceUri = "http://www.xbrl.org/2003/role/documentation";
        public const string XbrlDocumentationGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/definitionGuidance";
        public const string XbrlDisclosureGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/disclosureGuidance";
        public const string XbrlPresentationGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/presentationGuidance";
        public const string XbrlMeasurementGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/measurementGuidance";
        public const string XbrlCommentaryGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/commentaryGuidance";
        public const string XbrlExampleGuidanceRoleNamespaceUri = "http://www.xbrl.org/2003/role/exampleGuidance";
        public const string XbrlCalculationLinkbaseReferenceRoleNamespaceUri = "http://www.xbrl.org/2003/role/calculationLinkbaseRef";
        public const string XbrlDefinitionLinkbaseReferenceRoleNamespaceUri = "http://www.xbrl.org/2003/role/definitionLinkbaseRef";
        public const string XbrlLabelLinkbaseReferenceRoleNamespaceUri = "http://www.xbrl.org/2003/role/labelLinkbaseRef";
        public const string XbrlPresentationLinkbaseReferenceRoleNamespaceUri = "http://www.xbrl.org/2003/role/presentationLinkbaseRef";
        public const string XbrlReferenceLinkbaseReferenceRoleNamespaceUri = "http://www.xbrl.org/2003/role/referenceLinkbaseRef";

        public const string XbrlDefautLinkRoleNamespaceUri = "http://www.xbrl.org/2003/role/link";

        // Namespace for elements and attributes
        public const string XbrlLinkbaseSchemaRefNamespaceUri = "{http://www.xbrl.org/2003/linkbase}schemaRef";
        public const string XbrlLinkbaseRoleTypeNamespaceUri = "{http://www.xbrl.org/2003/linkbase}roleType";
        public const string XbrlLinkbaseArcroleTypeNamespaceUri = "{http://www.xbrl.org/2003/linkbase}arcroleType";
        public const string XmlSchemaInstanceSchemaLocationNamespaceUri = "{http://www.w3.org/2001/XMLSchema-instance}schemaLocation";
        public const string XLinkHRefNamespaceUri = "{http://www.w3.org/1999/xlink}href";
        public const string XbrlContextNamespaceUri = "{http://www.xbrl.org/2003/instance}context";
        public const string XbrlLinkbaseRoleRefNamespaceUri = "{http://www.xbrl.org/2003/linkbase}roleRef";
        public const string XbrlLinkbaseArcroleRefNamespaceUri = "{http://www.xbrl.org/2003/linkbase}arcroleRef";
        public const string XLinkTypeNamespaceUri = "{http://www.w3.org/1999/xlink}type";
        public const string XLinkRoleNamespaceUri = "{http://www.w3.org/1999/xlink}role";
        public const string XLinkArcRoleNamespaceUri = "{http://www.w3.org/1999/xlink}arcrole";
        public const string XLinkLabelNamespaceUri = "{http://www.w3.org/1999/xlink}label";
        public const string XLinkFromLabelNamespaceUri = "{http://www.w3.org/1999/xlink}from";
        public const string XLinkToLabelNamespaceUri = "{http://www.w3.org/1999/xlink}to";


        public const int BufferSize = 0x2000;

        public const int LargeBufferSize = BufferSize * 1024;

        public static readonly char[] DelimiterChars = new char[] { '\\', '/' };

        public static readonly string[] NumericXsdType = new string[] {"integer", "positiveInteger", "negativeInteger", "nonNegativeInteger", "nonPositiveInteger",
                       "long", "unsignedLong", "int", "unsignedInt", "short", "unsignedShort",
                       "byte", "unsignedByte", "decimal", "float", "double"};

        public const string Arcrole_ConceptLabel = "http://www.xbrl.org/2003/arcrole/concept-label";
        public const string Arcrole_ConceptReference = "http://www.xbrl.org/2003/arcrole/concept-reference";
        public const string Arcrole_Footnote = "http://www.xbrl.org/2003/role/footnote";
        public const string Arcrole_FactFootnote = "http://www.xbrl.org/2003/arcrole/fact-footnote";
        public const string Arcrole_FactExplanatoryFact = "http://www.xbrl.org/2009/arcrole/fact-explanatoryFact";
        public const string Arcrole_ParentChild = "http://www.xbrl.org/2003/arcrole/parent-child";
        public const string Arcrole_SummationItem = "http://www.xbrl.org/2003/arcrole/summation-item";
        public const string Arcrole_EssenceAlias = "http://www.xbrl.org/2003/arcrole/essence-alias";
        public const string Arcrole_SimilarTuples = "http://www.xbrl.org/2003/arcrole/similar-tuples";
        public const string Arcrole_RequiresElement = "http://www.xbrl.org/2003/arcrole/requires-element";
        public const string Arcrole_GeneralSpecial = "http://www.xbrl.org/2003/arcrole/general-special";
        public const string Arcrole_DimStartsWith = "http://xbrl.org/int/dim";
        public const string Arcrole_All = "http://xbrl.org/int/dim/arcrole/all";
        public const string Arcrole_NotAll = "http://xbrl.org/int/dim/arcrole/notAll";
        public const string Arcrole_HypercubeDimension = "http://xbrl.org/int/dim/arcrole/hypercube-dimension";
        public const string Arcrole_DimensionDomain = "http://xbrl.org/int/dim/arcrole/dimension-domain";
        public const string Arcrole_DomainMember = "http://xbrl.org/int/dim/arcrole/domain-member";
        public const string Arcrole_DimensionDefault = "http://xbrl.org/int/dim/arcrole/dimension-default";
        public const string Arcrole_DtrTypesStartsWith = "http://www.xbrl.org/dtr/type/";
        public const string Arcrole_DtrNumeric = "http://www.xbrl.org/dtr/type/numeric";
        public const string Arcrole_DefaultLinkRole = "http://www.xbrl.org/2003/role/link";
        public const string Arcrole_StandardLabel = "http://www.xbrl.org/2003/role/label";
        public const string Arcrole_GenStandardLabel = "http://www.xbrl.org/2008/role/label";
        public const string Arcrole_DocumentationLabel = "http://www.xbrl.org/2003/role/documentation";
        public const string Arcrole_GenDocumentationLabel = "http://www.xbrl.org/2008/role/documentation";
        public const string Arcrole_StandardReference = "http://www.xbrl.org/2003/role/reference";
        public const string Arcrole_GenStandardReference = "http://www.xbrl.org/2010/role/reference";
        public const string Arcrole_PeriodStartLabel = "http://www.xbrl.org/2003/role/periodStartLabel";
        public const string Arcrole_PeriodEndLabel = "http://www.xbrl.org/2003/role/periodEndLabel";
        public const string Arcrole_VerboseLabel = "http://www.xbrl.org/2003/role/verboseLabel";
        public const string Arcrole_TerseLabel = "http://www.xbrl.org/2003/role/terseLabel";
        public const string Arcrole_ConceptNameLabelRole = "XBRL-concept-name"; //# fake label role to show concept QName instead of label
        public const string Arcrole_LinkLinkbase = "http://www.w3.org/1999/xlink/properties/linkbase";

        public const string Arcrole_ElementLabel = "http://xbrl.org/arcrole/2008/element-label";

        public static XmlQualifiedName QNameXbrldtHypercubeItem = new XmlQualifiedName("{http://xbrl.org/2005/xbrldt}xbrldt:hypercubeItem");
        public static XmlQualifiedName QNameXbrldtDimensionItem = new XmlQualifiedName("dimensionItem", "http://xbrl.org/2005/xbrldt");
        public static XmlQualifiedName QNameXbrldtContextElement = new XmlQualifiedName("{http://xbrl.org/2005/xbrldt}xbrldt:contextElement");

        public static XmlQualifiedName QNameXbrliItem = new XmlQualifiedName("item","http://www.xbrl.org/2003/instance");
        public static XmlQualifiedName QNameXbrliTupe = new XmlQualifiedName("tuple", "http://www.xbrl.org/2003/instance");


        public static bool IsNumericXsdType(string s)
        {
            for(int i = 0; i <NumericXsdType.Length; i++)
            {
                if(s == NumericXsdType[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static readonly Dictionary<string, string> StandardNamespaceSchemaLocations = new Dictionary<string, string>  {
            { "xbrli", "http://www.xbrl.org/2003/xbrl-instance-2003-12-31.xsd" },
            { "link", "http://www.xbrl.org/2003/xbrl-linkbase-2003-12-31.xsd" },
            { "xl", "http://www.xbrl.org/2003/xl-2003-12-31.xsd" },
            { "xlink", "http://www.w3.org/1999/xlink" },
            { "xbrldt", "http://www.xbrl.org/2005/xbrldt-2005.xsd" },
            { "xbrldi", "http://www.xbrl.org/2006/xbrldi-2006.xsd" },
            { "gen", "http://www.xbrl.org/2008/generic-link.xsd"},
            { "genLabel", "http://www.xbrl.org/2008/generic-label.xsd"},
            { "genReference", "http://www.xbrl.org/2008/generic-reference.xsd"}
        };


        public static readonly string[] standardLabelRoles = new string[]  {
                    "http://www.xbrl.org/2003/role/label",
                    "http://www.xbrl.org/2003/role/terseLabel",
                    "http://www.xbrl.org/2003/role/verboseLabel",
                    "http://www.xbrl.org/2003/role/positiveLabel",
                    "http://www.xbrl.org/2003/role/positiveTerseLabel",
                    "http://www.xbrl.org/2003/role/positiveVerboseLabel",
                    "http://www.xbrl.org/2003/role/negativeLabel",
                    "http://www.xbrl.org/2003/role/negativeTerseLabel",
                    "http://www.xbrl.org/2003/role/negativeVerboseLabel",
                    "http://www.xbrl.org/2003/role/zeroLabel",
                    "http://www.xbrl.org/2003/role/zeroTerseLabel",
                    "http://www.xbrl.org/2003/role/zeroVerboseLabel",
                    "http://www.xbrl.org/2003/role/totalLabel",
                    "http://www.xbrl.org/2003/role/periodStartLabel",
                    "http://www.xbrl.org/2003/role/periodEndLabel",
                    "http://www.xbrl.org/2003/role/documentation",
                    "http://www.xbrl.org/2003/role/definitionGuidance",
                    "http://www.xbrl.org/2003/role/disclosureGuidance",
                    "http://www.xbrl.org/2003/role/presentationGuidance",
                    "http://www.xbrl.org/2003/role/measurementGuidance",
                    "http://www.xbrl.org/2003/role/commentaryGuidance",
                    "http://www.xbrl.org/2003/role/exampleGuidance"};

        public static readonly string[] standardReferenceRoles = new string[]  {
                    "http://www.xbrl.org/2003/role/reference",
                    "http://www.xbrl.org/2003/role/definitionRef",
                    "http://www.xbrl.org/2003/role/disclosureRef",
                    "http://www.xbrl.org/2003/role/mandatoryDisclosureRef",
                    "http://www.xbrl.org/2003/role/recommendedDisclosureRef",
                    "http://www.xbrl.org/2003/role/unspecifiedDisclosureRef",
                    "http://www.xbrl.org/2003/role/presentationRef",
                    "http://www.xbrl.org/2003/role/measurementRef",
                    "http://www.xbrl.org/2003/role/commentaryRef",
                    "http://www.xbrl.org/2003/role/exampleRef"};

        public static readonly string[] StandardLinkbaseRefRoles = new string[]  {
                    "http://www.xbrl.org/2003/role/calculationLinkbaseRef",
                    "http://www.xbrl.org/2003/role/definitionLinkbaseRef",
                    "http://www.xbrl.org/2003/role/labelLinkbaseRef",
                    "http://www.xbrl.org/2003/role/presentationLinkbaseRef",
                    "http://www.xbrl.org/2003/role/referenceLinkbaseRef"};

        public static readonly string[] standardMiscRoles = new string[] {
                    "http://www.xbrl.org/2003/role/link",
                    "http://www.xbrl.org/2003/role/footnote"};

        public static readonly string[] standardRoles = StandardMiscRoles.Union(StandardLinkbaseRefRoles).ToArray()
                                                                .Union(StandardReferenceRoles).ToArray()
                                                                .Union(StandardLabelRoles).ToArray();

        public static string[] StandardRoles => standardRoles;

        public static string[] StandardMiscRoles => standardMiscRoles;

        public static string[] StandardReferenceRoles => standardReferenceRoles;

        public static string[] StandardLabelRoles => standardLabelRoles;



        /*
        public static IEnumerable<XElement> ElementsCaseInsensitive(this XContainer source,
            XName name)
        {
            return source.Elements()
                .Where(e => e.Name.Namespace == name.Namespace
                    && e.Name.LocalName.Equals(name.LocalName, StringComparison.OrdinalIgnoreCase));
        }
        */

    }
}
