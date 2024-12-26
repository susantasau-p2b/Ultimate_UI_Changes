using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace P2BUltimate.Models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", IsNullable = false)]
    public partial class Response
    {

        private string issuerField;

        private ResponseStatus statusField;

        private Assertion assertionField;

        private string destinationField;

        private string idField;

        private string inResponseToField;

        private System.DateTime issueInstantField;

        private decimal versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public string Issuer
        {
            get
            {
                return this.issuerField;
            }
            set
            {
                this.issuerField = value;
            }
        }

        /// <remarks/>
        public ResponseStatus Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
        public Assertion Assertion
        {
            get
            {
                return this.assertionField;
            }
            set
            {
                this.assertionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Destination
        {
            get
            {
                return this.destinationField;
            }
            set
            {
                this.destinationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InResponseTo
        {
            get
            {
                return this.inResponseToField;
            }
            set
            {
                this.inResponseToField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime IssueInstant
        {
            get
            {
                return this.issueInstantField;
            }
            set
            {
                this.issueInstantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    public partial class ResponseStatus
    {

        private ResponseStatusStatusCode statusCodeField;

        private string statusMessageField;

        /// <remarks/>
        public ResponseStatusStatusCode StatusCode
        {
            get
            {
                return this.statusCodeField;
            }
            set
            {
                this.statusCodeField = value;
            }
        }

        /// <remarks/>
        public string StatusMessage
        {
            get
            {
                return this.statusMessageField;
            }
            set
            {
                this.statusMessageField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
    public partial class ResponseStatusStatusCode
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:assertion", IsNullable = false)]
    public partial class Assertion
    {

        private string issuerField;

        private AssertionSubject subjectField;

        private AssertionConditions conditionsField;

        private AssertionAuthnStatement authnStatementField;

        private AssertionAttribute[] attributeStatementField;

        private string idField;

        private System.DateTime issueInstantField;

        private decimal versionField;

        /// <remarks/>
        public string Issuer
        {
            get
            {
                return this.issuerField;
            }
            set
            {
                this.issuerField = value;
            }
        }

        /// <remarks/>
        public AssertionSubject Subject
        {
            get
            {
                return this.subjectField;
            }
            set
            {
                this.subjectField = value;
            }
        }

        /// <remarks/>
        public AssertionConditions Conditions
        {
            get
            {
                return this.conditionsField;
            }
            set
            {
                this.conditionsField = value;
            }
        }

        /// <remarks/>
        public AssertionAuthnStatement AuthnStatement
        {
            get
            {
                return this.authnStatementField;
            }
            set
            {
                this.authnStatementField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Attribute", IsNullable = false)]
        public AssertionAttribute[] AttributeStatement
        {
            get
            {
                return this.attributeStatementField;
            }
            set
            {
                this.attributeStatementField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime IssueInstant
        {
            get
            {
                return this.issueInstantField;
            }
            set
            {
                this.issueInstantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionSubject
    {

        private AssertionSubjectNameID nameIDField;

        private AssertionSubjectSubjectConfirmation subjectConfirmationField;

        /// <remarks/>
        public AssertionSubjectNameID NameID
        {
            get
            {
                return this.nameIDField;
            }
            set
            {
                this.nameIDField = value;
            }
        }

        /// <remarks/>
        public AssertionSubjectSubjectConfirmation SubjectConfirmation
        {
            get
            {
                return this.subjectConfirmationField;
            }
            set
            {
                this.subjectConfirmationField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionSubjectNameID
    {

        private string formatField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionSubjectSubjectConfirmation
    {

        private AssertionSubjectSubjectConfirmationSubjectConfirmationData subjectConfirmationDataField;

        private string methodField;

        /// <remarks/>
        public AssertionSubjectSubjectConfirmationSubjectConfirmationData SubjectConfirmationData
        {
            get
            {
                return this.subjectConfirmationDataField;
            }
            set
            {
                this.subjectConfirmationDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Method
        {
            get
            {
                return this.methodField;
            }
            set
            {
                this.methodField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionSubjectSubjectConfirmationSubjectConfirmationData
    {

        private string inResponseToField;

        private System.DateTime notOnOrAfterField;

        private string recipientField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InResponseTo
        {
            get
            {
                return this.inResponseToField;
            }
            set
            {
                this.inResponseToField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime NotOnOrAfter
        {
            get
            {
                return this.notOnOrAfterField;
            }
            set
            {
                this.notOnOrAfterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Recipient
        {
            get
            {
                return this.recipientField;
            }
            set
            {
                this.recipientField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionConditions
    {

        private AssertionConditionsAudienceRestriction audienceRestrictionField;

        private System.DateTime notOnOrAfterField;

        /// <remarks/>
        public AssertionConditionsAudienceRestriction AudienceRestriction
        {
            get
            {
                return this.audienceRestrictionField;
            }
            set
            {
                this.audienceRestrictionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime NotOnOrAfter
        {
            get
            {
                return this.notOnOrAfterField;
            }
            set
            {
                this.notOnOrAfterField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionConditionsAudienceRestriction
    {

        private string audienceField;

        /// <remarks/>
        public string Audience
        {
            get
            {
                return this.audienceField;
            }
            set
            {
                this.audienceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionAuthnStatement
    {

        private AssertionAuthnStatementAuthnContext authnContextField;

        private System.DateTime authnInstantField;

        private string sessionIndexField;

        private System.DateTime sessionNotOnOrAfterField;

        /// <remarks/>
        public AssertionAuthnStatementAuthnContext AuthnContext
        {
            get
            {
                return this.authnContextField;
            }
            set
            {
                this.authnContextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime AuthnInstant
        {
            get
            {
                return this.authnInstantField;
            }
            set
            {
                this.authnInstantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SessionIndex
        {
            get
            {
                return this.sessionIndexField;
            }
            set
            {
                this.sessionIndexField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime SessionNotOnOrAfter
        {
            get
            {
                return this.sessionNotOnOrAfterField;
            }
            set
            {
                this.sessionNotOnOrAfterField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionAuthnStatementAuthnContext
    {

        private object authnContextClassRefField;

        /// <remarks/>
        public object AuthnContextClassRef
        {
            get
            {
                return this.authnContextClassRefField;
            }
            set
            {
                this.authnContextClassRefField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:oasis:names:tc:SAML:2.0:assertion")]
    public partial class AssertionAttribute
    {

        private string attributeValueField;

        private string nameField;

        private string nameFormatField;

        /// <remarks/>
        public string AttributeValue
        {
            get
            {
                return this.attributeValueField;
            }
            set
            {
                this.attributeValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NameFormat
        {
            get
            {
                return this.nameFormatField;
            }
            set
            {
                this.nameFormatField = value;
            }
        }
    }
}