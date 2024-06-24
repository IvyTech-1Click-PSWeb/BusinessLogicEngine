﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4963
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 2.0.50727.4963.
// 
#pragma warning disable 1591

namespace JGS.Web.DTVTriggerProviders.ChangePart {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4927")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ChangePartWrapperSoap", Namespace="http://tempuri.org/")]
    public partial class ChangePartWrapper : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback PerformChangePartOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public ChangePartWrapper() {
            this.Url = global::JGS.Web.DTVTriggerProviders.Properties.Settings.Default.JGS_Web_DTVTriggerProviders_ChangePart_ChangePartWrapper;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event PerformChangePartCompletedEventHandler PerformChangePartCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/PerformChangePart", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string PerformChangePart(ChangePartInfo ChangePartInfo, bool JustReturnXMLString) {
            object[] results = this.Invoke("PerformChangePart", new object[] {
                        ChangePartInfo,
                        JustReturnXMLString});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PerformChangePartAsync(ChangePartInfo ChangePartInfo, bool JustReturnXMLString) {
            this.PerformChangePartAsync(ChangePartInfo, JustReturnXMLString, null);
        }
        
        /// <remarks/>
        public void PerformChangePartAsync(ChangePartInfo ChangePartInfo, bool JustReturnXMLString, object userState) {
            if ((this.PerformChangePartOperationCompleted == null)) {
                this.PerformChangePartOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPerformChangePartOperationCompleted);
            }
            this.InvokeAsync("PerformChangePart", new object[] {
                        ChangePartInfo,
                        JustReturnXMLString}, this.PerformChangePartOperationCompleted, userState);
        }
        
        private void OnPerformChangePartOperationCompleted(object arg) {
            if ((this.PerformChangePartCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PerformChangePartCompleted(this, new PerformChangePartCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4927")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class ChangePartInfo {
        
        private string sesCustomerIDField;
        
        private string requestIdField;
        
        private string bCNField;
        
        private string newSerialNoField;
        
        private string newPartNoField;
        
        private string newRevisionLevelField;
        
        private string newFixedAssetTagField;
        
        private string notesField;
        
        private bool mustBeOnHoldField;
        
        private bool releaseIfHoldField;
        
        private bool mustBeTimedInField;
        
        private string timedInWorkCenterNameField;
        
        private FlexFields[] flexFieldListField;
        
        private string userNameField;
        
        private string passwordField;
        
        /// <remarks/>
        public string SesCustomerID {
            get {
                return this.sesCustomerIDField;
            }
            set {
                this.sesCustomerIDField = value;
            }
        }
        
        /// <remarks/>
        public string RequestId {
            get {
                return this.requestIdField;
            }
            set {
                this.requestIdField = value;
            }
        }
        
        /// <remarks/>
        public string BCN {
            get {
                return this.bCNField;
            }
            set {
                this.bCNField = value;
            }
        }
        
        /// <remarks/>
        public string NewSerialNo {
            get {
                return this.newSerialNoField;
            }
            set {
                this.newSerialNoField = value;
            }
        }
        
        /// <remarks/>
        public string NewPartNo {
            get {
                return this.newPartNoField;
            }
            set {
                this.newPartNoField = value;
            }
        }
        
        /// <remarks/>
        public string NewRevisionLevel {
            get {
                return this.newRevisionLevelField;
            }
            set {
                this.newRevisionLevelField = value;
            }
        }
        
        /// <remarks/>
        public string NewFixedAssetTag {
            get {
                return this.newFixedAssetTagField;
            }
            set {
                this.newFixedAssetTagField = value;
            }
        }
        
        /// <remarks/>
        public string Notes {
            get {
                return this.notesField;
            }
            set {
                this.notesField = value;
            }
        }
        
        /// <remarks/>
        public bool MustBeOnHold {
            get {
                return this.mustBeOnHoldField;
            }
            set {
                this.mustBeOnHoldField = value;
            }
        }
        
        /// <remarks/>
        public bool ReleaseIfHold {
            get {
                return this.releaseIfHoldField;
            }
            set {
                this.releaseIfHoldField = value;
            }
        }
        
        /// <remarks/>
        public bool MustBeTimedIn {
            get {
                return this.mustBeTimedInField;
            }
            set {
                this.mustBeTimedInField = value;
            }
        }
        
        /// <remarks/>
        public string TimedInWorkCenterName {
            get {
                return this.timedInWorkCenterNameField;
            }
            set {
                this.timedInWorkCenterNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable=false)]
        public FlexFields[] FlexFieldList {
            get {
                return this.flexFieldListField;
            }
            set {
                this.flexFieldListField = value;
            }
        }
        
        /// <remarks/>
        public string userName {
            get {
                return this.userNameField;
            }
            set {
                this.userNameField = value;
            }
        }
        
        /// <remarks/>
        public string Password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.4927")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class FlexFields {
        
        private string nameField;
        
        private string valueField;
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4927")]
    public delegate void PerformChangePartCompletedEventHandler(object sender, PerformChangePartCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.4927")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PerformChangePartCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PerformChangePartCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591