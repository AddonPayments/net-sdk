﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GlobalPayments.Api.Entities {
    public class ThreeDSecure {
        public string AcsTransactionId { get; set; }

        public string AcsEndVersion { get; set; }

        public string AcsStartVersion { get; set; }

        /// <summary>
        /// The algorithm used.
        /// </summary>
        public int Algorithm { get; set; }

        private decimal? _amount;
        internal decimal? Amount {
            get { return _amount; }
            set {
                _amount = value;
                MerchantData.Add("_amount", _amount.ToString(), false);
            }
        }

        public string AuthenticationSource { get; set; }

        public string AuthenticationType { get; set; }

        public string AuthenticationValue { get; set; }

        public string CardHolderResponseInfo { get; set; }

        /// <summary>
        /// Consumer authentication (3DSecure) verification value.
        /// </summary>
        public string Cavv { get; set; }

        public bool ChallengeMandated { get; set; }

        public string CriticalityIndicator { get; set; }

        private string _currency;
        internal string Currency {
            get { return _currency; }
            set {
                _currency = value;
                MerchantData.Add("_currency", _currency, false);
            }
        }

        public string DirectoryServerTransactionId { get; set; }

        public string DirectoryServerEndVersion { get; set; }

        public string DirectoryServerStartVersion { get; set; }

        /// <summary>
        /// Consumer authentication (3DSecure) electronic commerce indicator.
        /// </summary>
        public int? Eci { get; set; }

        /// <summary>
        /// The enrolment status:
        /// </summary>
        public string Enrolled { get; set; }

        /// <summary>
        /// The URL of the Issuing Bank's ACS.
        /// </summary>
        public string IssuerAcsUrl { get; set; }

        private MerchantDataCollection _merchantData;
        /// <summary>
        /// A KVP collection of merchant supplied data
        /// </summary>
        public MerchantDataCollection MerchantData {
            get {
                if (_merchantData == null)
                    _merchantData = new MerchantDataCollection();
                return _merchantData;
            }
            set {
                if (_merchantData != null)
                    value.MergeHidden(_merchantData);

                _merchantData = value;
                if (_merchantData.HasKey("_amount")) {
                    _amount = _merchantData.GetValue<decimal>("_amount");
                }
                if (_merchantData.HasKey("_currency")) {
                    _currency = _merchantData.GetValue<string>("_currency");
                }
                if (_merchantData.HasKey("_orderid")) {
                    _orderId = _merchantData.GetValue<string>("_orderid");
                }
                if (_merchantData.HasKey("_version")) {
                    Secure3dVersion version;
                    if (Enum.TryParse(_merchantData.GetValue<string>("_version"), out version)) {
                        _version = version;
                    }
                }
            }
        }

        public string MessageCategory { get; set; }

        public string MessageExtensionId { get; set; }

        public string MessageExtensionName { get; set; }

        public string MessageVersion { get; set; }

        private string _orderId;

        /// <summary>
        /// The order ID used for the initial transaction
        /// </summary>
        public string OrderId {
            get { return _orderId; }
            set {
                _orderId = value;
                MerchantData.Add("_orderid", _orderId, false);
            }
        }

        /// <summary>
        /// The Payer Authentication Request returned by the Enrolment Server. Must be sent to the Issuing Bank's ACS (Access Control Server) URL.
        /// </summary>
        public string PayerAuthenticationRequest { get; set; }

        /// <summary>
        /// Consumer authentication (3DSecure) source.
        /// </summary>
        public string PaymentDataSource { get; set; }

        /// <summary>
        /// Consumer authentication (3DSecure) type.
        /// </summary>
        /// <remarks>
        /// Default value is `"3DSecure"`.
        /// </remarks>
        public string PaymentDataType { get; set; }

        public string SdkInterface { get; set; }

        public IEnumerable<string> SdkUiType { get; set; }

        public string ServerTransactionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        public string StatusReason { get; set; }

        public Secure3dVersion? _version;
        public Secure3dVersion? Version {
            get { return _version; }
            set {
                _version = value;
                MerchantData.Add("_version", value.ToString(), false);
            }
        }

        /// <summary>
        /// Consumer authentication (3DSecure) transaction ID.
        /// </summary>
        public string Xid { get; set; }

        public ThreeDSecure() {
            PaymentDataType = "3DSecure";
        }

        public void Merge(ThreeDSecure secureEcom) {
            if (secureEcom != null) {
                AcsTransactionId = MergeValue(AcsTransactionId, secureEcom.AcsTransactionId);
                AcsEndVersion = MergeValue(AcsEndVersion, secureEcom.AcsEndVersion);
                AcsStartVersion = MergeValue(AcsStartVersion, secureEcom.AcsStartVersion);
                Algorithm = MergeValue(Algorithm, secureEcom.Algorithm);
                Amount = MergeValue(Amount, secureEcom.Amount);
                AuthenticationSource = MergeValue(AuthenticationSource, secureEcom.AuthenticationSource);
                AuthenticationType = MergeValue(AuthenticationType, secureEcom.AuthenticationType);
                AuthenticationValue = MergeValue(AuthenticationValue, secureEcom.AuthenticationValue);
                CardHolderResponseInfo = MergeValue(CardHolderResponseInfo, secureEcom.CardHolderResponseInfo);
                Cavv = MergeValue(Cavv, secureEcom.Cavv);
                ChallengeMandated = MergeValue(ChallengeMandated, secureEcom.ChallengeMandated);
                CriticalityIndicator = MergeValue(CriticalityIndicator, secureEcom.CriticalityIndicator);
                Currency = MergeValue(Currency, secureEcom.Currency);
                DirectoryServerTransactionId = MergeValue(DirectoryServerTransactionId, secureEcom.DirectoryServerTransactionId);
                DirectoryServerEndVersion = MergeValue(DirectoryServerEndVersion, secureEcom.DirectoryServerEndVersion);
                DirectoryServerStartVersion = MergeValue(DirectoryServerStartVersion, secureEcom.DirectoryServerStartVersion);
                Eci = MergeValue(Eci, secureEcom.Eci);
                Enrolled = MergeValue(Enrolled, secureEcom.Enrolled);
                IssuerAcsUrl = MergeValue(IssuerAcsUrl, secureEcom.IssuerAcsUrl);
                MessageCategory = MergeValue(MessageCategory, secureEcom.MessageCategory);
                MessageExtensionId = MergeValue(MessageExtensionId, secureEcom.MessageExtensionId);
                MessageExtensionName = MergeValue(MessageExtensionName, secureEcom.MessageExtensionName);
                MessageVersion = MergeValue(MessageVersion, secureEcom.MessageVersion);
                OrderId = MergeValue(OrderId, secureEcom.OrderId);
                PayerAuthenticationRequest = MergeValue(PayerAuthenticationRequest, secureEcom.PayerAuthenticationRequest);
                PaymentDataSource = MergeValue(PaymentDataSource, secureEcom.PaymentDataSource);
                PaymentDataType = MergeValue(PaymentDataType, secureEcom.PaymentDataType);
                SdkInterface = MergeValue(SdkInterface, secureEcom.SdkInterface);
                SdkUiType = MergeValue(SdkUiType, secureEcom.SdkUiType);
                ServerTransactionId = MergeValue(ServerTransactionId, secureEcom.ServerTransactionId);
                Status = MergeValue(Status, secureEcom.Status);
                StatusReason = MergeValue(StatusReason, secureEcom.StatusReason);
                Version = MergeValue(Version, secureEcom.Version);
                Xid = MergeValue(Xid, secureEcom.Xid);

                //this.merchantData = mergeValue(merchantData, secureEcom.getMerchantData());
            }
        }

        private T MergeValue<T>(T currentValue, T mergeValue) {
            if (mergeValue == null) {
                return currentValue;
            }
            return mergeValue;
        }
    }

    public class MerchantDataCollection : IEnumerable<KeyValuePair<string, string>> {
        private List<MerchantKVP> _collection;

        public string this[string key] {
            get {
                var kvp = _collection.FirstOrDefault(p => p.Key == key);
                if (kvp != null && kvp.Visible)
                    return kvp.Value;
                return null;
            }
            internal set {
                var kvp = _collection.FirstOrDefault(p => p.Key == key);
                if (kvp != null)
                    kvp.Value = value;
            }
        }

        public int Count {
            get {
                return _collection.Where(p => p.Visible).Count();
            }
        }

        internal IEnumerable<MerchantKVP> HiddenValues {
            get { return _collection.Where(p => p.Visible == false); }
        }

        public MerchantDataCollection() {
            _collection = new List<MerchantKVP>();
        }

        internal void Add(string key, string value, bool visible) {
            if (HasKey(key)) {
                if (visible)
                    throw new ApiException(string.Format("Key {0} already exists in the collection.", key));
                else this[key] = value;
            }
                

            _collection.Add(new MerchantKVP {
                Key = key,
                Value = value,
                Visible = visible
            });
        }
        public void Add(string key, string value) {
            Add(key, value, true);
        }

        internal T GetValue<T>(string key, Func<string, T> converter = null) {
            var kvp = _collection.FirstOrDefault(p => p.Key == key);
            if (kvp != null) {
                if (converter != null)
                    return converter(kvp.Value);
                else return (T)Convert.ChangeType(kvp.Value, typeof(T));
            }
            return default(T);
        }

        internal bool HasKey(string key) {
            return GetValue<string>(key) != null;
        }

        internal void MergeHidden(MerchantDataCollection collection) {
            foreach (var item in collection.HiddenValues) {
                if (!this.HasKey(item.Key))
                    _collection.Add(item);
            }
        }

        public static MerchantDataCollection Parse(string kvpString, Func<string, string> decoder = null) {
            var collection = new MerchantDataCollection();

            // decrypt the string
            var decryptedKvp = Encoding.UTF8.GetString(Convert.FromBase64String(kvpString));
            if (decoder != null) {
                decryptedKvp = decoder(decryptedKvp);
            }

            // build out the object
            var merchantData = decryptedKvp.Split('|');
            foreach (var kvp in merchantData) {
                var data = kvp.Split(':');
                collection.Add(data[0], data[1], bool.Parse(data[2]));
            }

            return collection;
        }

        public string ToString(Func<string, string> encryption = null) {
            var sb = new StringBuilder();

            _collection.ForEach((kvp) => {
                sb.Append(string.Format("{0}:{1}:{2}|", kvp.Key, kvp.Value, kvp.Visible));
            });

            var formatted = sb.ToString().TrimEnd('|');
            if (encryption != null)
                formatted = encryption(formatted);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(formatted));
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            var enumerator = new List<KeyValuePair<string, string>>(_collection.Select((kvp) => {
                return new KeyValuePair<string, string>(kvp.Key, kvp.Value);
            }));

            return enumerator.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

    internal class MerchantKVP {
        public string Key { get; set; }
        public string Value { get; set; }
        internal bool Visible { get; set; }
    }
}
