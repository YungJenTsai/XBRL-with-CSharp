using System;
using System.Collections.Generic;
using System.Text;
using TT.XBRLProcessor;
using System.Linq;
using System.Xml;

namespace TT.XBRLRender
{
    public class Cube
    {
        string _linkRoleUri;
        XBRLRender _xBRLRender;
        NetworkModel _parentNetworkModel;
        Segment _segment;
        List<RoleType> _roleTypes;
        string _definition;

        string _type;
        bool _isStatementOfEquity;
        bool _isStatementOfCashflow;
        HashSet<XmlQualifiedName> _filteredOutAxisSet;

        public Cube(string linkRoleUri, NetworkModel networkModel, XBRLRender xBRLRender)
        {
            LinkRoleUri = linkRoleUri;
            ParentNetworkModel = networkModel;
            XBRLRender = xBRLRender;
            RoleTypes = ParentNetworkModel.XBRLModel.RoleTypeDict[LinkRoleUri];

            RoleTypes[0].GenLabel(ParentNetworkModel.XBRLModel);

            Definition = RoleTypes[0].Label;
            if (Definition == null)
            {
                Definition = RoleTypes[0].Definition;

                if (Definition == null)
                    Definition = LinkRoleUri;
            }

            Definition.Trim();

            string cubeType = CubeType;

            string lower = Definition.ToLower();

            if (cubeType == "statement" &&
                !(lower.Contains("parenthetical") || 
                    lower.Contains("(table") ||
                    lower.Contains("(detail") ||
                    lower.Contains("(polic")
                    ) &&
              XBRLRender.IsStatementNamespace)
            {
                if(lower.Contains("148600") ||
                    lower.Contains("148610") ||
                    ((lower.Contains("stockholder") || lower.Contains("shareholder") || lower.Contains("change")) &&
                     (lower.Contains("equity") || lower.Contains("deficit"))) ||
                    ((lower.Contains("partners") || lower.Contains("accounts")) && lower.Contains("capital")) ||
                    (lower.Contains("statement") && lower.Contains("equity"))
                )
                {
                    IsStatementOfEquity = true;
                }
                else
                {
                    IsStatementOfEquity = false;
                }

                IsStatementOfCashflow = false;

                if (lower.Contains("cash") && !lower.Contains("supplement"))
                {
                    int cashIndex = lower.IndexOf("cash");
                    string cashSub = lower.Substring(cashIndex);

                    if (cashSub.Contains("flow"))
                        IsStatementOfCashflow = true;
                }

            }

        }



        public string LinkRoleUri { get => _linkRoleUri; set => _linkRoleUri = value; }
        public NetworkModel ParentNetworkModel { get => _parentNetworkModel; set => _parentNetworkModel = value; }
        public List<RoleType> RoleTypes { get => _roleTypes; set => _roleTypes = value; }
        public string CubeType
        {
            get
            {
                if (_type == null)
                {
                    string[] subStrs = Definition.Split('-');
                    if(subStrs.Length >= 2)
                    {
                        _type = subStrs[1].Trim().ToLower();
                    }
                }

                return _type;
            }

        }
        public string Definition { get => _definition; set => _definition = value; }
        public XBRLRender XBRLRender { get => _xBRLRender; set => _xBRLRender = value; }
        public bool IsStatementOfEquity { get => _isStatementOfEquity; set => _isStatementOfEquity = value; }
        public bool IsStatementOfCashflow { get => _isStatementOfCashflow; set => _isStatementOfCashflow = value; }

        public HashSet<XmlQualifiedName> FilteredOutAxisSet
        {
            get
            {
                if (_filteredOutAxisSet == null)
                    _filteredOutAxisSet = new HashSet<XmlQualifiedName>();

                return _filteredOutAxisSet;
            }
            set => _filteredOutAxisSet = value;
        }

        public Segment Segment { get => _segment; set => _segment = value; }

        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}
