﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using GlobalPayments.Api.Builders;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using GlobalPayments.Api.Utils;

namespace GlobalPayments.Api.Gateways {
    internal class RealexConnector : XmlGateway, IPaymentGateway, IRecurringService, ISecure3dProvider {
        public string MerchantId { get; set; }
        public string AccountId { get; set; }
        public string SharedSecret { get; set; }
        public string Channel { get; set; }
        public string RebatePassword { get; set; }
        public string RefundPassword { get; set; }
        public bool SupportsHostedPayments { get { return true; } }
        public bool SupportsRetrieval { get { return false; } }
        public bool SupportsUpdatePaymentDetails { get { return true; } }
        public string PaymentValues { get; set; }
        public HostedPaymentConfig HostedPaymentConfig { get; set; }
        public Secure3dVersion Version { get { return Secure3dVersion.One; } }

        #region transaction handling
        public Transaction ProcessAuthorization(AuthorizationBuilder builder) {
            var et = new ElementTree();
            string timestamp = builder.Timestamp ?? GenerationUtils.GenerateTimestamp();
            string orderId = builder.OrderId ?? GenerationUtils.GenerateOrderId();

            // amount and currency are required for googlePay
            if (builder.PaymentMethod is CreditCardData) {
                var card = builder.PaymentMethod as CreditCardData;
                if (builder.TransactionModifier == TransactionModifier.EncryptedMobile) {
                    if (card.Token == null || card.MobileType == null) {
                        throw new BuilderException("Token and  MobileType can not be null");
                    }
                    if (card.MobileType == MobilePaymentMethodType.GOOGLEPAY.ToString() && (builder.Amount == null || builder.Currency == null))
                        throw new BuilderException("Amount and Currency can not be null for capture.");
                }
            }

            // apm validations
            if (builder.PaymentMethod is AlternatePaymentMethod) {
                var apm = builder.PaymentMethod as AlternatePaymentMethod;
                if (apm.ReturnUrl == null || apm.StatusUpdateUrl == null || apm.AccountHolderName == null || apm.Country == null || apm.Descriptor == null) {
                    throw new BuilderException("PaymentMethod, ReturnUrl, StatusUpdateUrl, AccountHolderName, Country, Descriptor can not be null ");
                }
            }

            // Build Request
            var request = et.Element("request")
                .Set("timestamp", timestamp)
                .Set("type", MapAuthRequestType(builder));
            et.SubElement(request, "merchantid").Text(MerchantId);
            et.SubElement(request, "account", AccountId);
            et.SubElement(request, "channel", Channel);
            et.SubElement(request, "orderid", orderId);
            if (builder.Amount.HasValue) {
                et.SubElement(request, "amount").Text(builder.Amount.ToNumericCurrencyString()).Set("currency", builder.Currency);
            }

            // Hydrate the payment data fields
            if (builder.PaymentMethod is CreditCardData) {
                var card = builder.PaymentMethod as CreditCardData;

                if (builder.TransactionModifier == TransactionModifier.EncryptedMobile) {
                    et.SubElement(request, "mobile", card.MobileType);
                    et.SubElement(request, "token", card.Token);
                }
                else {
                    var cardElement = et.SubElement(request, "card");
                    et.SubElement(cardElement, "number", card.Number);
                    et.SubElement(cardElement, "expdate", card.ShortExpiry);
                    et.SubElement(cardElement, "chname").Text(card.CardHolderName);
                    et.SubElement(cardElement, "type", card.CardType.ToUpper());

                    if (card.Cvn != null) {
                        var cvnElement = et.SubElement(cardElement, "cvn");
                        et.SubElement(cvnElement, "number", card.Cvn);
                        et.SubElement(cvnElement, "presind", (int)card.CvnPresenceIndicator);
                    }
                }

                string hash = string.Empty;
                if (builder.TransactionType == TransactionType.Verify)
                    hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, card.Number);
                else {
                    if (builder.TransactionModifier == TransactionModifier.EncryptedMobile && card.MobileType == MobilePaymentMethodType.APPLEPAY)
                        hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, "", "", card.Token);
                    else if (builder.TransactionModifier == TransactionModifier.EncryptedMobile && card.MobileType == MobilePaymentMethodType.GOOGLEPAY)
                        hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, builder.Amount.ToNumericCurrencyString(), builder.Currency, card.Token);
                    else
                        hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, builder.Amount.ToNumericCurrencyString(), builder.Currency, card.Number);
                }
                et.SubElement(request, "sha1hash").Text(hash);
            }
            else if (builder.PaymentMethod is AlternatePaymentMethod) {
                var apm = builder.PaymentMethod as AlternatePaymentMethod;

                et.SubElement(request, "paymentmethod", apm.AlternativePaymentMethodType);

                var paymentmethoddetails = et.SubElement(request, "paymentmethoddetails");
                et.SubElement(paymentmethoddetails, "returnurl", apm.ReturnUrl);
                et.SubElement(paymentmethoddetails, "statusupdateurl", apm.StatusUpdateUrl);
                et.SubElement(paymentmethoddetails, "descriptor", apm.Descriptor);
                et.SubElement(paymentmethoddetails, "country", apm.Country);
                et.SubElement(paymentmethoddetails, "accountholdername", apm.AccountHolderName);

                // issueno
                string hash = string.Empty;
                hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, builder.Amount.ToNumericCurrencyString(), builder.Currency, apm.AlternativePaymentMethodType.ToString());
                et.SubElement(request, "sha1hash").Text(hash);
            }
            if (builder.PaymentMethod is RecurringPaymentMethod) {
                var recurring = builder.PaymentMethod as RecurringPaymentMethod;
                et.SubElement(request, "payerref").Text(recurring.CustomerKey);
                et.SubElement(request, "paymentmethod").Text(recurring.Key ?? recurring.Id);

                // CVN
                if (!string.IsNullOrEmpty(builder.Cvn)) {
                    var paymentData = et.SubElement(request, "paymentdata");
                    var cvn = et.SubElement(paymentData, "cvn");
                    et.SubElement(cvn, "number").Text(builder.Cvn);
                }

                string hash = string.Empty;
                if (builder.TransactionType == TransactionType.Verify)
                    hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, recurring.CustomerKey);
                else hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, builder.Amount.ToNumericCurrencyString(), builder.Currency, recurring.CustomerKey);
                et.SubElement(request, "sha1hash").Text(hash);
            }
            else {
                // TODO: Token Processing
            }

            #region AUTO/MULTI SETTLE
            if (builder.TransactionType == TransactionType.Sale || builder.TransactionType == TransactionType.Auth) {
                var autoSettle = builder.TransactionType == TransactionType.Sale ? "1" : builder.MultiCapture == true ? "MULTI" : "0";
                et.SubElement(request, "autosettle").Set("flag", autoSettle);
            }
            #endregion

            #region CUSTOM DATA
            if (builder.CustomData != null) {
                var custom = et.SubElement(request, "custom");
                foreach (string[] values in builder.CustomData) {
                    for (int i = 1; i <= values.Length; i++) {
                        et.SubElement(custom, "field" + i.ToString().PadLeft(2, '0'), values[i - 1]);
                    }
                }
            }
            #endregion

            #region CUSTOMER DATA
            if (builder.CustomerData != null) {
                Customer customerValue = builder.CustomerData;

                var customer = et.SubElement(request, "customer");
                et.SubElement(customer, "customerid", customerValue.Id);
                et.SubElement(customer, "firstname", customerValue.FirstName);
                et.SubElement(customer, "lastname", customerValue.LastName);
                et.SubElement(customer, "dateofbirth", customerValue.DateOfBirth);
                et.SubElement(customer, "customerpassword", customerValue.CustomerPassword);
                et.SubElement(customer, "email", customerValue.Email);
                et.SubElement(customer, "domainname", customerValue.DomainName);
                et.SubElement(customer, "devicefingerprint", customerValue.DeviceFingerPrint);
                et.SubElement(customer, "phonenumber", customerValue.HomePhone);
            }
            #endregion

            #region DCC
            if (builder.DccRateData != null) {
                DccRateData dccRateData = builder.DccRateData;

                var dccInfo = et.SubElement(request, "dccinfo");
                et.SubElement(dccInfo, "ccp", dccRateData.DccProcessor.ToString().ToLower());
                et.SubElement(dccInfo, "type", "1");
                et.SubElement(dccInfo, "ratetype", dccRateData.DccRateType.ToString().Substring(0, 1));

                // authorization elements
                et.SubElement(dccInfo, "rate", dccRateData.CardHolderRate);
                if (dccRateData.CardHolderAmount != null) {
                    et.SubElement(dccInfo, "amount", dccRateData.CardHolderAmount)
                            .Set("currency", dccRateData.CardHolderCurrency);
                }
            }
            #endregion

            #region DESCRIPTION
            if (builder.Description != null) {
                var comments = et.SubElement(request, "comments");
                et.SubElement(comments, "comment", builder.Description).Set("id", "1");
            }
            #endregion

            #region FRAUD
            // fraud filter mode
            if (builder.FraudFilterMode == FraudFilterMode.PASSIVE) {
                et.SubElement(request, "fraudfilter").Set("mode", builder.FraudFilterMode.ToString());
            }

            // recurring fraud filter
            if (builder.RecurringType != null || builder.RecurringSequence != null) {
                et.SubElement(request, "recurring")
                    .Set("type", builder.RecurringType.ToString().ToLower())
                    .Set("sequence", builder.RecurringSequence.ToString().ToLower());
            }

            // fraud decision manager
            if (builder.DecisionManager != null) {
                DecisionManager dmValues = builder.DecisionManager;

                var fraud = et.SubElement(request, "fraud");

                var dm = et.SubElement(fraud, "dm");
                et.SubElement(dm, "billtohostname", dmValues.BillToHostName);
                et.SubElement(dm, "billtohttpbrowsercookiesaccepted", !dmValues.BillToHttpBrowserCookiesAccepted ? "false" : "true");
                et.SubElement(dm, "billtohttpbrowseremail", dmValues.BillToHttpBrowserEmail);
                et.SubElement(dm, "billtohttpbrowsertype", dmValues.BillToHttpBrowserType);
                et.SubElement(dm, "billtoipnetworkaddress", dmValues.BillToIpNetworkAddress);
                et.SubElement(dm, "businessrulesscorethreshold", dmValues.BusinessRulesCoreThreshold);
                et.SubElement(dm, "billtopersonalid", dmValues.BillToPersonalId);
                et.SubElement(dm, "invoiceheadertendertype", dmValues.InvoiceHeaderTenderType);
                et.SubElement(dm, "invoiceheaderisgift", !dmValues.InvoiceHeaderIsGift ? "false" : "true");
                et.SubElement(dm, "decisionmanagerprofile", dmValues.DecisionManagerProfile);
                et.SubElement(dm, "invoiceheaderreturnsaccepted", !dmValues.InvoiceHeaderReturnsAccepted ? "false" : "true");
                et.SubElement(dm, "itemhosthedge", dmValues.ItemHostHedge);
                et.SubElement(dm, "itemnonsensicalhedge", dmValues.ItemNonsensicalHedge);
                et.SubElement(dm, "itemobscenitieshedge", dmValues.ItemObscenitiesHedge);
                et.SubElement(dm, "itemphonehedge", dmValues.ItemPhoneHedge);
                et.SubElement(dm, "itemtimehedge", dmValues.ItemTimeHedge);
                et.SubElement(dm, "itemvelocityhedge", dmValues.ItemVelocityHedge);
            }
            #endregion

            #region 3DS
            if (builder.PaymentMethod is ISecure3d) {
                ThreeDSecure secureEcom = ((ISecure3d)builder.PaymentMethod).ThreeDSecure;

                if (secureEcom != null) {
                    var mpi = et.SubElement(request, "mpi");
                    et.SubElement(mpi, "eci", secureEcom.Eci);
                    et.SubElement(mpi, "cavv", secureEcom.Cavv);
                    et.SubElement(mpi, "xid", secureEcom.Xid);
                    et.SubElement(mpi, "ds_trans_id", secureEcom.DirectoryServerTransactionId);
                    et.SubElement(mpi, "authentication_value", secureEcom.AuthenticationValue);
                    et.SubElement(mpi, "message_version", secureEcom.MessageVersion);
                }
            }
            #endregion

            #region PRODUCT DATA
            if (builder.MiscProductData != null) {
                List<string[]> productValues = builder.MiscProductData;

                Element products = et.SubElement(request, "products");
                string[] str = { "productid", "productname", "quantity", "unitprice", "gift", "type", "risk" };
                foreach (string[] values in productValues) {
                    Element product = et.SubElement(products, "product");
                    for (int x = 0; x < values.Length; x++) {
                        et.SubElement(product, str[x], values[x]);
                    }
                }
            }
            #endregion

            #region REFUND HASH
            if (builder.TransactionType == TransactionType.Refund) {
                et.SubElement(request, "refundhash", GenerationUtils.GenerateHash(RefundPassword) ?? string.Empty);
            }
            #endregion

            #region STORED CREDENTIAL
            if (builder.StoredCredential != null) {
                var storedCredentialElement = et.SubElement(request, "storedcredential");
                et.SubElement(storedCredentialElement, "type", builder.StoredCredential.Type.ToString().ToLower());
                et.SubElement(storedCredentialElement, "initiator", builder.StoredCredential.Initiator.ToString().ToLower());
                et.SubElement(storedCredentialElement, "sequence", builder.StoredCredential.Sequence.ToString().ToLower());
                et.SubElement(storedCredentialElement, "srd", builder.StoredCredential.SchemeId);
            }
            #endregion

            #region SUPPLEMENTARY DATA
            if (builder.SupplementaryData != null) {
                var supplementaryData = et.SubElement(request, "supplementarydata");
                Dictionary<string, List<string[]>> suppData = builder.SupplementaryData;

                foreach (string key in suppData.Keys) {
                    List<string[]> dataSets = suppData[key];

                    foreach (string[] data in dataSets) {
                        Element item = et.SubElement(supplementaryData, "item").Set("type", key);
                        for (int i = 1; i <= data.Length; i++) {
                            et.SubElement(item, "field" + i.ToString().PadLeft(2, '0'), data[i - 1]);
                        }
                    }
                }
            }
            #endregion

            #region TSS INFO
            if (builder.CustomerId != null || builder.ProductId != null || builder.CustomerIpAddress != null || builder.ClientTransactionId != null || builder.BillingAddress != null || builder.ShippingAddress != null) {
                var tssInfo = et.SubElement(request, "tssinfo");
                et.SubElement(tssInfo, "custnum", builder.CustomerId);
                et.SubElement(tssInfo, "prodid", builder.ProductId);
                et.SubElement(tssInfo, "varref", builder.ClientTransactionId);
                et.SubElement(tssInfo, "custipaddress", builder.CustomerIpAddress);
                if (builder.BillingAddress != null)
                    tssInfo.Append(BuildAddress(et, builder.BillingAddress));
                if (builder.ShippingAddress != null)
                    tssInfo.Append(BuildAddress(et, builder.ShippingAddress));
            }
            #endregion

            var response = DoTransaction(et.ToString(request));
            return MapResponse(response, builder);
        }

        public string SerializeRequest(AuthorizationBuilder builder) {
            // check for hpp config
            if (HostedPaymentConfig == null)
                throw new ApiException("Hosted configuration missing, Please check you configuration.");

            var encoder = (HostedPaymentConfig.Version == HppVersion.VERSION_2) ? null : JsonEncoders.Base64Encoder;
            var request = new JsonDoc(encoder);

            var orderId = builder.OrderId ?? GenerationUtils.GenerateOrderId();
            var timestamp = builder.Timestamp ?? GenerationUtils.GenerateTimestamp();

            // check for right transaction types
            if (builder.TransactionType != TransactionType.Sale && builder.TransactionType != TransactionType.Auth && builder.TransactionType != TransactionType.Verify)
                throw new UnsupportedTransactionException("Only Charge and Authorize are supported through hpp.");

            request.Set("MERCHANT_ID", MerchantId);
            request.Set("ACCOUNT", AccountId);
            request.Set("HPP_CHANNEL", Channel);
            request.Set("ORDER_ID", orderId);
            if (builder.Amount != null)
                request.Set("AMOUNT", builder.Amount.ToNumericCurrencyString());
            request.Set("CURRENCY", builder.Currency);
            request.Set("TIMESTAMP", timestamp);
            request.Set("AUTO_SETTLE_FLAG", (builder.TransactionType == TransactionType.Sale) ? "1" : builder.MultiCapture == true ? "MULTI" : "0");
            request.Set("COMMENT1", builder.Description);
            // request.Set("COMMENT2", );
            if (HostedPaymentConfig.RequestTransactionStabilityScore.HasValue)
                request.Set("RETURN_TSS", HostedPaymentConfig.RequestTransactionStabilityScore.Value ? "1" : "0");
            if (HostedPaymentConfig.DynamicCurrencyConversionEnabled.HasValue)
                request.Set("DCC_ENABLE", HostedPaymentConfig.DynamicCurrencyConversionEnabled.Value ? "1" : "0");
            if (builder.HostedPaymentData != null) {
                AlternativePaymentType[] PaymentTypes = builder.HostedPaymentData.PresetPaymentMethods;
                if (PaymentTypes != null) {
                    PaymentValues = string.Join("|", PaymentTypes);
                }
                request.Set("CUST_NUM", builder.HostedPaymentData.CustomerNumber);
                if (HostedPaymentConfig.DisplaySavedCards.HasValue && builder.HostedPaymentData.CustomerKey != null) {
                    request.Set("HPP_SELECT_STORED_CARD", builder.HostedPaymentData.CustomerKey);
                }
                if (builder.HostedPaymentData.OfferToSaveCard.HasValue) {
                    request.Set("OFFER_SAVE_CARD", builder.HostedPaymentData.OfferToSaveCard.Value ? "1" : "0");
                }
                if (builder.HostedPaymentData.CustomerExists.HasValue) {
                    request.Set("PAYER_EXIST", builder.HostedPaymentData.CustomerExists.Value ? "1" : "0");
                }
                if (!HostedPaymentConfig.DisplaySavedCards.HasValue) {
                    request.Set("PAYER_REF", builder.HostedPaymentData.CustomerKey);
                }
                request.Set("PMT_REF", builder.HostedPaymentData.PaymentKey);
                request.Set("PROD_ID", builder.HostedPaymentData.ProductId);
                request.Set("HPP_CUSTOMER_COUNTRY", builder.HostedPaymentData.Country);
                request.Set("HPP_CUSTOMER_FIRSTNAME", builder.HostedPaymentData.CustomerFirstName);
                request.Set("HPP_CUSTOMER_LASTNAME", builder.HostedPaymentData.CustomerLastName);
                request.Set("MERCHANT_RESPONSE_URL", builder.HostedPaymentData.ReturnUrl);
                request.Set("HPP_TX_STATUS_URL", builder.HostedPaymentData.StatusUpdateUrl);
                request.Set("PM_METHODS", PaymentValues);

                // 3DSv2
                request.Set("HPP_CUSTOMER_EMAIL", builder.HostedPaymentData.CustomerEmail);
                request.Set("HPP_CUSTOMER_PHONENUMBER_MOBILE", builder.HostedPaymentData.CustomerPhoneMobile);
                request.Set("HPP_CHALLENGE_REQUEST_INDICATOR", builder.HostedPaymentData.ChallengeRequest);
                if (builder.HostedPaymentData.AddressesMatch != null) {
                    request.Set("HPP_ADDRESS_MATCH_INDICATOR", builder.HostedPaymentData.AddressesMatch.Value ? "TRUE" : "FALSE");
                }

                // SUPPLIMENTARY DATA
                if (builder.HostedPaymentData.SupplementaryData != null) {
                    foreach (string key in builder.HostedPaymentData.SupplementaryData.Keys) {
                        request.Set(key, builder.HostedPaymentData.SupplementaryData[key]);
                    }
                }
            }
            if (builder.ShippingAddress != null) {
                // FRAUD VALUES
                request.Set("SHIPPING_CODE", builder.ShippingAddress.PostalCode);
                request.Set("SHIPPING_CO", builder.ShippingAddress.Country);

                // 3DS2 VALUES
                request.Set("HPP_SHIPPING_STREET1", builder.ShippingAddress.StreetAddress1);
                request.Set("HPP_SHIPPING_STREET2", builder.ShippingAddress.StreetAddress2);
                request.Set("HPP_SHIPPING_STREET3", builder.ShippingAddress.StreetAddress3);
                request.Set("HPP_SHIPPING_CITY", builder.ShippingAddress.City);
                request.Set("HPP_SHIPPING_STATE", builder.ShippingAddress.State);
                request.Set("HPP_SHIPPING_POSTALCODE", builder.ShippingAddress.PostalCode);
                request.Set("HPP_SHIPPING_COUNTRY", builder.ShippingAddress.Country);
            }
            if (builder.BillingAddress != null) {
                // FRAUD VALUES
                request.Set("BILLING_CODE", builder.BillingAddress.PostalCode);
                request.Set("BILLING_CO", builder.BillingAddress.Country);

                // 3DS2 VALUES
                request.Set("HPP_BILLING_STREET1", builder.BillingAddress.StreetAddress1);
                request.Set("HPP_BILLING_STREET2", builder.BillingAddress.StreetAddress2);
                request.Set("HPP_BILLING_STREET3", builder.BillingAddress.StreetAddress3);
                request.Set("HPP_BILLING_CITY", builder.BillingAddress.City);
                request.Set("HPP_BILLING_STATE", builder.BillingAddress.State);
                request.Set("HPP_BILLING_POSTALCODE", builder.BillingAddress.PostalCode);
                request.Set("HPP_BILLING_COUNTRY", builder.BillingAddress.Country);
            }
            request.Set("CUST_NUM", builder.CustomerId);
            request.Set("VAR_REF", builder.ClientTransactionId);
            request.Set("HPP_LANG", HostedPaymentConfig.Language);
            request.Set("MERCHANT_RESPONSE_URL", HostedPaymentConfig.ResponseUrl);
            request.Set("CARD_PAYMENT_BUTTON", HostedPaymentConfig.PaymentButtonText);
            if (HostedPaymentConfig.CardStorageEnabled.HasValue) {
                request.Set("CARD_STORAGE_ENABLE", HostedPaymentConfig.CardStorageEnabled.Value ? "1" : "0");
            }
            if (builder.TransactionType == TransactionType.Verify) {
                request.Set("VALIDATE_CARD_ONLY", builder.TransactionType == TransactionType.Verify ? "1" : "0");
            }
            if (HostedPaymentConfig.FraudFilterMode != FraudFilterMode.NONE) {
                request.Set("HPP_FRAUDFILTER_MODE", HostedPaymentConfig.FraudFilterMode.ToString());
            }
            if (builder.RecurringType != null || builder.RecurringSequence != null) {
                request.Set("RECURRING_TYPE", builder.RecurringType.ToString().ToLower());
                request.Set("RECURRING_SEQUENCE", builder.RecurringSequence.ToString().ToLower());
            }
            request.Set("HPP_VERSION", HostedPaymentConfig.Version);
            request.Set("HPP_POST_DIMENSIONS", HostedPaymentConfig.PostDimensions);
            request.Set("HPP_POST_RESPONSE", HostedPaymentConfig.PostResponse);

            var toHash = new List<string> {
                timestamp,
                MerchantId,
                orderId,
                (builder.Amount != null) ? builder.Amount.ToNumericCurrencyString() : null,
                builder.Currency
            };

            if (HostedPaymentConfig.CardStorageEnabled.HasValue || (builder.HostedPaymentData != null && builder.HostedPaymentData.OfferToSaveCard.HasValue) || HostedPaymentConfig.DisplaySavedCards.HasValue) {
                toHash.Add(builder.HostedPaymentData.CustomerKey ?? null);
                toHash.Add(builder.HostedPaymentData.PaymentKey ?? null);
            }

            if (HostedPaymentConfig.FraudFilterMode != FraudFilterMode.NONE) {
                toHash.Add(HostedPaymentConfig.FraudFilterMode.ToString());
            }

            request.Set("SHA1HASH", GenerationUtils.GenerateHash(SharedSecret, toHash.ToArray()));
            return request.ToString();
        }

        public Transaction ManageTransaction(ManagementBuilder builder) {
            var et = new ElementTree();
            string timestamp = GenerationUtils.GenerateTimestamp();
            string orderId = builder.OrderId ?? GenerationUtils.GenerateOrderId();

            // Build Request
            var request = et.Element("request")
                .Set("timestamp", timestamp)
                .Set("type", MapManageRequestType(builder));
            et.SubElement(request, "merchantid").Text(MerchantId);
            et.SubElement(request, "account", AccountId);
            et.SubElement(request, "channel", Channel);
            et.SubElement(request, "orderid", orderId);
            et.SubElement(request, "pasref", builder.TransactionId);
            if (builder.Amount.HasValue) {
                var amtElement = et.SubElement(request, "amount", builder.Amount.ToNumericCurrencyString());
                if (!builder.MultiCapture) {
                    amtElement.Set("currency", builder.Currency);
                }
            }
            else if (builder.TransactionType == TransactionType.Capture) {
                throw new BuilderException("Amount cannot be null for capture.");
            }

            // DCC
            if (builder.DccRateData != null) {
                DccRateData dccRateData = builder.DccRateData;

                Element dccInfo = et.SubElement(request, "dccinfo");
                et.SubElement(dccInfo, "ccp", dccRateData.DccProcessor);
                et.SubElement(dccInfo, "type", "1");
                et.SubElement(dccInfo, "ratetype", dccRateData.DccRateType);

                // settlement elements
                et.SubElement(dccInfo, "rate", dccRateData.CardHolderRate);
                if (dccRateData.CardHolderAmount != null) {
                    et.SubElement(dccInfo, "amount", dccRateData.CardHolderAmount)
                            .Set("currency", dccRateData.CardHolderCurrency);
                }
            }

            // Capture Authcode
            if (builder.TransactionType == TransactionType.Capture && builder.MultiCapture == true) {
                et.SubElement(request, "authcode").Text(builder.AuthorizationCode);
            }

            // Check is APM for Refund
            if (builder.AlternativePaymentType != null) {
                et.SubElement(request, "paymentmethod", builder.AlternativePaymentType);
            }

            // payer authentication response
            if (builder.TransactionType == TransactionType.VerifySignature) {
                et.SubElement(request, "pares", builder.PayerAuthenticationResponse);
            }

            // reason code
            if (builder.ReasonCode != null) {
                et.SubElement(request, "reasoncode").Text(builder.ReasonCode.ToString());
            }

            // description
            if (builder.Description != null) {
                var comments = et.SubElement(request, "comments");
                et.SubElement(comments, "comment", builder.Description).Set("id", "1");
            }

            // tssinfo
            if (builder.CustomerId != null || builder.ClientTransactionId != null) {
                var tssInfo = et.SubElement(request, "tssinfo");
                et.SubElement(tssInfo, "custnum", builder.CustomerId);
                et.SubElement(tssInfo, "varref", builder.ClientTransactionId);
            }

            // data supplimentary
            if (builder.SupplementaryData != null) {
                var supplementaryData = et.SubElement(request, "supplementarydata");
                Dictionary<string, List<string[]>> suppData = builder.SupplementaryData;

                foreach (string key in suppData.Keys) {
                    List<string[]> dataSets = suppData[key];

                    foreach (string[] data in dataSets) {
                        Element item = et.SubElement(supplementaryData, "item").Set("type", key);
                        for (int i = 1; i <= data.Length; i++) {
                            et.SubElement(item, "field" + i.ToString().PadLeft(2, '0'), data[i - 1]);
                        }
                    }
                }
            }

            et.SubElement(request, "sha1hash", GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, builder.Amount.ToNumericCurrencyString(), builder.Currency, builder.AlternativePaymentType != null ? builder.AlternativePaymentType.ToString() : null));

            // rebate hash
            if (builder.TransactionType == TransactionType.Refund) {
                if (builder.AuthorizationCode != null) {
                    et.SubElement(request, "authcode").Text(builder.AuthorizationCode);
                }
                et.SubElement(request, "refundhash", GenerationUtils.GenerateHash(builder.AlternativePaymentType != null ? RefundPassword : RebatePassword));
            }
            var response = DoTransaction(et.ToString(request));
            return MapResponse(response, builder);
        }

        public T ProcessReport<T>(ReportBuilder<T> builder) where T : class {
            throw new UnsupportedTransactionException("Reporting functionality is not supported through this gateway.");
        }

        public TResult ProcessRecurring<TResult>(RecurringBuilder<TResult> builder) where TResult : class {
            var et = new ElementTree();
            string timestamp = GenerationUtils.GenerateTimestamp();
            string orderId = builder.OrderId ?? GenerationUtils.GenerateOrderId();

            // Build Request
            var request = et.Element("request")
                .Set("type", MapRecurringRequestType(builder))
                .Set("timestamp", timestamp);
            et.SubElement(request, "merchantid").Text(MerchantId);
            et.SubElement(request, "account", AccountId);
            et.SubElement(request, "orderid", orderId);

            if (builder.TransactionType == TransactionType.Create || builder.TransactionType == TransactionType.Edit) {
                if (builder.Entity is Customer) {
                    var customer = builder.Entity as Customer;
                    request.Append(BuildCustomer(et, customer));
                    et.SubElement(request, "sha1hash").Text(GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, null, null, customer.Key));
                }
                else if (builder.Entity is RecurringPaymentMethod) {
                    var payment = builder.Entity as RecurringPaymentMethod;
                    var cardElement = et.SubElement(request, "card");
                    et.SubElement(cardElement, "ref").Text(payment.Key ?? payment.Id);
                    et.SubElement(cardElement, "payerref").Text(payment.CustomerKey);

                    if (payment.PaymentMethod != null) {
                        var card = payment.PaymentMethod as CreditCardData;
                        string expiry = card.ShortExpiry;
                        et.SubElement(cardElement, "number").Text(card.Number);
                        et.SubElement(cardElement, "expdate").Text(expiry);
                        et.SubElement(cardElement, "chname").Text(card.CardHolderName);
                        et.SubElement(cardElement, "type").Text(card.CardType);

                        string sha1hash = string.Empty;
                        if (builder.TransactionType == TransactionType.Create)
                            sha1hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, orderId, null, null, payment.CustomerKey, card.CardHolderName, card.Number);
                        else sha1hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, payment.CustomerKey, payment.Key ?? payment.Id, expiry, card.Number);
                        et.SubElement(request, "sha1hash").Text(sha1hash);
                    }
                }
            }
            else if (builder.TransactionType == TransactionType.Delete) {
                if (builder.Entity is RecurringPaymentMethod) {
                    var payment = builder.Entity as RecurringPaymentMethod;
                    var cardElement = et.SubElement(request, "card");
                    et.SubElement(cardElement, "ref").Text(payment.Key ?? payment.Id);
                    et.SubElement(cardElement, "payerref").Text(payment.CustomerKey);

                    string sha1hash = GenerationUtils.GenerateHash(SharedSecret, timestamp, MerchantId, payment.CustomerKey, payment.Key ?? payment.Id);
                    et.SubElement(request, "sha1hash").Text(sha1hash);
                }
            }

            var response = DoTransaction(et.ToString(request));
            return MapRecurringResponse<TResult>(response, builder);
        }

        public Transaction ProcessSecure3d(Secure3dBuilder builder) {
            TransactionType transType = builder.TransactionType;
            if (transType.Equals(TransactionType.VerifyEnrolled)) {
                AuthorizationBuilder authBuilder = new AuthorizationBuilder(transType, builder.PaymentMethod)
                        .WithAmount(builder.Amount)
                        .WithCurrency(builder.Currency)
                        .WithOrderId(builder.OrderId);

                return ProcessAuthorization(authBuilder);
            }
            else if (transType.Equals(TransactionType.VerifySignature)) {
                // get our three d secure object
                ThreeDSecure secureEcom = builder.ThreeDSecure;

                // create our transaction reference
                TransactionReference reference = new TransactionReference {
                    OrderId = secureEcom.OrderId
                };

                ManagementBuilder managementBuilder = new ManagementBuilder(transType)
                        .WithAmount(secureEcom.Amount)
                        .WithCurrency(secureEcom.Currency)
                        .WithPayerAuthenticationResponse(builder.PayerAuthenticationResponse)
                        .WithPaymentMethod(reference);
                return ManageTransaction(managementBuilder);
            }
            throw new UnsupportedTransactionException(string.Format("Unknown transaction type {0}", transType));
        }

        #endregion

        #region response mapping
        private Transaction MapResponse(string rawResponse, TransactionBuilder<Transaction> builder) {
            var root = new ElementTree(rawResponse).Get("response");

            var acceptedCodes = new List<string>();
            if (builder is AuthorizationBuilder) {
                acceptedCodes = MapAcceptedCodes(MapAuthRequestType(builder as AuthorizationBuilder));
            }
            else if (builder is ManagementBuilder) {
                acceptedCodes = MapAcceptedCodes(MapManageRequestType(builder as ManagementBuilder));
            }

            CheckResponse(root, acceptedCodes);
            var result = new Transaction {
                ResponseCode = root.GetValue<string>("result"),
                ResponseMessage = root.GetValue<string>("message"),
                CvnResponseCode = root.GetValue<string>("cvnresult"),
                AvsResponseCode = root.GetValue<string>("avspostcoderesponse"),
                Timestamp = root.GetAttribute<string>("timestamp"),
                TransactionReference = new TransactionReference {
                    AuthCode = root.GetValue<string>("authcode"),
                    OrderId = root.GetValue<string>("orderid"),
                    PaymentMethodType = PaymentMethodType.Credit,
                    TransactionId = root.GetValue<string>("pasref"),
                    AlternativePaymentType = root.GetValue<string>("paymentmethod"),
                    BatchNumber = root.GetValue<string>("batchid")
                },
                MultiCapture = builder.MultiCapture
            };

            // dccinfo
            if (root.Has("dccinfo")) {
                DccRateData dccRateData = new DccRateData();
                if (builder is AuthorizationBuilder && ((AuthorizationBuilder)builder).DccRateData != null) {
                    dccRateData = ((AuthorizationBuilder)builder).DccRateData;
                }

                dccRateData.CardHolderCurrency = root.GetValue<string>("cardholdercurrency");
                dccRateData.CardHolderAmount = root.GetValue<decimal>("cardholderamount");
                dccRateData.CardHolderRate = root.GetValue<string>("cardholderrate");
                dccRateData.MerchantCurrency = root.GetValue<string>("merchantcurrency");
                dccRateData.MerchantAmount = root.GetValue<decimal>("merchantamount");
                dccRateData.MarginRatePercentage = root.GetValue<string>("marginratepercentage");
                dccRateData.ExchangeRateSourceName = root.GetValue<string>("exchangeratesourcename");
                dccRateData.CommissionPercentage = root.GetValue<string>("commissionpercentage");

                string exchangeTimestamp = root.GetValue<string>("exchangeratesourcetimestamp");
                if (!string.IsNullOrEmpty(exchangeTimestamp)) {
                    dccRateData.ExchangeRateSourceTimestamp = DateTime.ParseExact(exchangeTimestamp, "yyyyMMdd hh:mm", CultureInfo.InvariantCulture);
                }

                result.DccRateData = dccRateData;
            }

            // 3d secure enrolled
            if (root.Has("enrolled")) {
                result.ThreeDSecure = new ThreeDSecure();
                result.ThreeDSecure.Enrolled = root.GetValue<string>("enrolled");
                result.ThreeDSecure.PayerAuthenticationRequest = root.GetValue<string>("pareq");
                result.ThreeDSecure.Xid = root.GetValue<string>("xid");
                result.ThreeDSecure.IssuerAcsUrl = root.GetValue<string>("url");
            }

            // threedsecure
            if (root.Has("threedsecure")) {
                result.ThreeDSecure = new ThreeDSecure();
                result.ThreeDSecure.Status = root.GetValue<string>("status");
                result.ThreeDSecure.Xid = root.GetValue<string>("xid");
                result.ThreeDSecure.Cavv = root.GetValue<string>("cavv");

                var eci = root.GetValue<string>("eci");
                if (!string.IsNullOrEmpty(eci))
                    result.ThreeDSecure.Eci = int.Parse(eci);

                var algorithm = root.GetValue<string>("algorithm");
                if (!string.IsNullOrEmpty(algorithm))
                    result.ThreeDSecure.Algorithm = int.Parse(algorithm);
            }

            // stored credential
            result.SchemeId = root.GetValue<string>("srd");

            return result;
        }

        private TResult MapRecurringResponse<TResult>(string rawResponse, RecurringBuilder<TResult> builder) where TResult : class {
            var root = new ElementTree(rawResponse).Get("response");

            // check response
            CheckResponse(root);
            return builder.Entity as TResult;
        }

        private void CheckResponse(Element root, List<string> acceptedCodes = null) {
            if (acceptedCodes == null)
                acceptedCodes = new List<string> { "00" };

            var responseCode = root.GetValue<string>("result");
            var responseMessage = root.GetValue<string>("message");
            if (!acceptedCodes.Contains(responseCode)) {
                throw new GatewayException(
                    string.Format("Unexpected Gateway Response: {0} - {1}", responseCode, responseMessage),
                    responseCode,
                    responseMessage
                );
            }
        }
        #endregion

        #region transaction type mapping
        private string MapAuthRequestType(AuthorizationBuilder builder) {
            var trans = builder.TransactionType;
            var payment = builder.PaymentMethod;

            switch (trans) {
                case TransactionType.Sale:
                case TransactionType.Auth: {
                        if (payment is Credit) {
                            if (builder.TransactionModifier == TransactionModifier.Offline) {
                                if (builder.PaymentMethod != null)
                                    return "manual";
                                return "offline";
                            }
                            else if (builder.TransactionModifier == TransactionModifier.EncryptedMobile) {
                                return "auth-mobile";
                            }
                            return "auth";
                        }
                        else if (payment is AlternatePaymentMethod) {
                            return "payment-set";
                        }
                        else return "receipt-in";
                    }
                case TransactionType.Capture: {
                        return "settle";
                    }
                case TransactionType.Verify: {
                        if (payment is Credit) {
                            return "otb";
                        }
                        else {
                            if (builder.TransactionModifier == TransactionModifier.Secure3D)
                                return "realvault-3ds-verifyenrolled";
                            else return "receipt-in-otb";
                        }
                    }
                case TransactionType.Refund: {
                        if (payment is Credit) {
                            return "credit";
                        }
                        else if (payment is AlternatePaymentMethod) {
                            return "payment-credit";
                        }
                        else { return "payment-out"; }
                    }
                case TransactionType.VerifyEnrolled: {
                        return "3ds-verifyenrolled";
                    }
                case TransactionType.DccRateLookup: {
                        if (payment is Credit) {
                            return "dccrate";
                        }
                        return "realvault-dccrate";
                    }
                default: {
                        throw new UnsupportedTransactionException();
                    }
            }
        }

        private string MapManageRequestType(ManagementBuilder builder) {
            TransactionType trans = builder.TransactionType;
            switch (trans) {
                case TransactionType.Capture:
                    if (builder.MultiCapture == true)
                        return "multisettle";
                    else
                        return "settle";
                case TransactionType.Hold:
                    return "hold";
                case TransactionType.Refund:
                    if (builder.AlternativePaymentType != null)
                        return "payment-credit";
                    return "rebate";
                case TransactionType.Release:
                    return "release";
                case TransactionType.Void:
                case TransactionType.Reversal:
                    return "void";
                case TransactionType.VerifySignature:
                    return "3ds-verifysig";
                default:
                    return "unknown";
            }
        }

        private string MapRecurringRequestType<T>(RecurringBuilder<T> builder) where T : class {
            var entity = builder.Entity;
            switch (builder.TransactionType) {
                case TransactionType.Create:
                    if (entity is Customer)
                        return "payer-new";
                    else if (entity is IPaymentMethod)
                        return "card-new";
                    throw new UnsupportedTransactionException();
                case TransactionType.Edit:
                    if (entity is Customer)
                        return "payer-edit";
                    else if (entity is IPaymentMethod)
                        return "card-update-card";
                    throw new UnsupportedTransactionException();
                case TransactionType.Delete:
                    if (entity is RecurringPaymentMethod)
                        return "card-cancel-card";
                    throw new UnsupportedTransactionException();
                default:
                    throw new UnsupportedTransactionException();
            }
        }

        private List<string> MapAcceptedCodes(string transactionType) {
            switch (transactionType) {
                case "3ds-verifysig":
                case "3ds-verifyenrolled":
                    return new List<string> { "00", "110" };
                case "payment-set":
                    return new List<string> { "01" };
                default:
                    return new List<string> { "00" };
            }
        }
        #endregion

        #region hydration
        private Element BuildCustomer(ElementTree et, Customer customer) {
            var payer = et.Element("payer")
                        .Set("ref", customer.Key ?? GenerationUtils.GenerateRecurringKey())
                        .Set("type", "Retail");
            et.SubElement(payer, "title", customer.Title);
            et.SubElement(payer, "firstname", customer.FirstName);
            et.SubElement(payer, "surname", customer.LastName);
            et.SubElement(payer, "company", customer.Company);

            if (customer.Address != null) {
                var address = et.SubElement(payer, "address");
                et.SubElement(address, "line1", customer.Address.StreetAddress1);
                et.SubElement(address, "line2", customer.Address.StreetAddress2);
                et.SubElement(address, "line3", customer.Address.StreetAddress3);
                et.SubElement(address, "city", customer.Address.City);
                et.SubElement(address, "county", customer.Address.Province);
                et.SubElement(address, "postcode", customer.Address.PostalCode);
                var country = et.SubElement(address, "country", customer.Address.Country);
                if (country != null)
                    country.Set("code", customer.Address.CountryCode);
            }

            var phone = et.SubElement(payer, "phonenumbers");
            et.SubElement(phone, "home", customer.HomePhone);
            et.SubElement(phone, "work", customer.WorkPhone);
            et.SubElement(phone, "fax", customer.Fax);
            et.SubElement(phone, "mobile", customer.MobilePhone);

            et.SubElement(payer, "email", customer.Email);

            // comments
            return payer;
        }

        private Element BuildAddress(ElementTree et, Address address) {
            if (address == null)
                return null;

            var code = address.PostalCode;
            if (!string.IsNullOrEmpty(code) && !code.Contains("|")) {
                code = string.Format("{0}|{1}", address.PostalCode, address.StreetAddress1);
                if (address.Country == "GB") {
                    var encStreetAddress = string.IsNullOrEmpty(address.StreetAddress1) ? "" : Regex.Replace(address.StreetAddress1, "[^0-9]", "");
                    code = string.Format("{0}|{1}", Regex.Replace(address.PostalCode, "[^0-9]", ""), encStreetAddress);
                }
            }

            var addressNode = et.Element("address").Set("type", address.Type == AddressType.Billing ? "billing" : "shipping");
            et.SubElement(addressNode, "code").Text(code);
            et.SubElement(addressNode, "country").Text(address.Country);

            return addressNode;
        }
        #endregion
    }
}
