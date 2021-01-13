using System.Collections.Generic;
using GlobalPayments.Api.PaymentMethods;

namespace GlobalPayments.Api.Entities {
    /// <summary>
    /// Data collection to supplement a hosted payment page.
    /// </summary>
    public class HostedPaymentData: AlternatePaymentMethod {
        /// <summary>
        /// A value indicating to the issuer that the shipping and billing addresses
        /// are expected to be the same. Used as a fraud prevention.
        /// </summary>
        public bool? AddressesMatch { get; set; }

        /// <summary>
        /// Value used to determine the challenge request preference for 3DS2
        /// </summary>
        public ChallengeRequestIndicator ChallengeRequest { get; set; }

        /// <summary>
        /// The customer's email address
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Indicates if the customer is known and has an account.
        /// </summary>
        public bool? CustomerExists { get; set; }

        /// <summary>
        /// The identifier for the customer.
        /// </summary>
        public string CustomerKey { get; set; }

        /// <summary>
        /// The customer's number.
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// The customer's mobile phone number
        /// </summary>
        public string CustomerPhoneMobile { get; set; }

        /// <summary>
        /// Indicates if the customer should be prompted to store their card.
        /// </summary>
        public bool? OfferToSaveCard { get; set; }

        /// <summary>
        /// The identifier for the customer's desired payment method.
        /// </summary>
        public string PaymentKey { get; set; }

        /// <summary>
        /// The product ID.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Supplementary data that can be supplied at the descretion
        /// of the merchant/application.
        /// </summary>
        public Dictionary<string, string> SupplementaryData { get; set; }

        /// <summary>
        /// The customer's FirstName.
        /// </summary>
        public string CustomerFirstName { get; set; }

        /// <summary>
        /// The customer's Lastname.
        /// </summary>
        public string CustomerLastName { get; set; }

        /// <summary>
        /// The Alternative Payment Type is an Array which store
        /// Different types of the PaymentMethods Available.
        /// </summary>
        public AlternativePaymentType[] PresetPaymentMethods { get; set; }

        #region Optional Values 3DSv2

        /// <summary>
        /// The work phone number provided by the Cardholder. Should be In format: of 'CountryCallingCode|Number' for example, '1|123456789'.
        /// European merchants: mandatory for SCA if captured by your application or website. Global Payments recommend you send at least one phone number (Mobile, Home or Work).
        /// </summary>
        public string CustomerWorkNumber { get; set; }

        /// <summary>
        /// The home phone number provided by the Cardholder. Should be In format: of 'CountryCallingCode|Number' for example, '1|123456789'.
        /// European merchants: mandatory for SCA if captured by your application or website. Global Payments recommend you send at least one phone number (Mobile, Home or Work).
        /// </summary>
        public string CustomerHomeNumber { get; set; }

        /// <summary>
        /// Date the customer opened their account with the merchant.
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountAgeDate { get; set; }

        /// <summary>
        /// Length of time the customer has had an account with the merchant. Allowed values:
        /// NO_ACCOUNT
        /// THIS_TRANSACTION
        /// LESS_THAN_THIRTY_DAYS
        /// THIRTY_TO_SIXTY_DAYS
        /// MORE_THEN_SIXTY_DAYS
        /// European merchants: optional for SCA.
        /// </sumary>
        public string AccountAgeIndicator { get; set; }

        /// <summary>
        /// Date the customer's account with the merchant was last changed. For example, if the billing or shipping details changed, new payment account or new users added.
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountChangeDate { get; set; }

        /// <summary>
        /// Length of time since the account has changed. Allowed values:
        /// THIS_TRANSACTION
        /// LESS_THAN_THIRTY_DAYS
        /// THIRTY_TO_SIXTY_DAYS
        /// MORE_THEN_SIXTY_DAY/// 
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountChangeIndicator { get; set; }

        /// <summary>
        /// Date the customer's account with the merchant had a password change or account reset.
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountPassChangeDate { get; set; }

        /// <summary>
        /// Length of time since the customer's account with the merchant has had a password change or account reset. Allowed values:
        /// NO_CHANGE
        /// THIS_TRANSACTION
        /// LESS_THAN_THIRTY_DAYS
        /// THIRTY_TO_SIXTY_DAYS
        /// MORE_THEN_SIXTY_DAYS
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountPassChangeIndicator { get; set; }

        /// <summary>
        /// European merchants: optional for SCA.
        /// </summary>
        public string AccountPurchaseCount { get; set; }

        /// <summary>
        /// The type of transaction being authenticated. Allowed values:
        /// GOODS_SERVICE_PURCHASE
        /// CHECK_ACCEPTANCE
        /// ACCOUNT_FUNDING
        /// QUASI_CASH_TRANSACTION
        /// PREPAID_ACTIVATION_AND_LOAD
        /// European merchants: optional for SCA.
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Optional field to pass additional information about the customer
        /// European merchants: optional for SCA.
        /// </summary>
        public string CardholderAccountIdentifier { get; set; }

        /// <summary>
        /// Indicates whether there has been suspicious activity on this customer's account, including fraudulent activity. (Note this is a boolean in the SDK). Allowed values:
        /// SUSPICIOUS_ACTIVITY.
        /// NO_SUSPICIOUS_ACTIVITY
        /// European merchants: optional for SCA.
        /// </summary>
        public string SuspiciousActivity { get; set; }

        /// <summary>
        /// Number of Add Card attempts in the last 24 hours.
        /// European merchants: optional for SCA.
        /// </summary>
        public string ProvisionAttemptsDay { get; set; }

        /// <summary>
        /// Date the payment account was associated with the customer's account.
        /// European merchants: optional for SCA.
        /// </summary>
        public string PaymentAccountAge { get; set; }

        /// <summary>
        /// Length of time the customer has had an account with the merchant. Allowed values:
        /// NO_ACCOUNT - The customer does not have an account, for example, guest checkout
        /// THIS_TRANSACTION
        /// LESS_THAN_THIRTY_DAYS
        /// THIRTY_TO_SIXTY_DAYS.
        /// MORE_THEN_SIXTY_DAYS
        /// European merchants: optional for SCA.
        /// </summary>
        public string PaymentAccountAgeIndicator { get; set; }

        /// <summary>
        /// The email address to which the merchandise was delivered. The field must be submitted in the form name@host.domain (for example, victor.cantera@example.com ).
        /// European merchants: optional for SCA.
        /// </summary>
        public string DeliveryEmail { get; set; }

        /// <summary>
        /// Indicates the delivery timeframe for the order. Allowed values:
        /// ELECTRONIC_DELIVERY
        /// SAME_DAY
        /// OVERNIGHT
        /// TWO_DAYS_OR_MORE
        /// European merchants: optional for SCA.
        /// </summary>
        public string DeliveryTimeframe { get; set; }

        /// <summary>
        /// Indicates shipping method chosen for the transaction. Must be speific to this transaction, not generally to the merchant. If one or more items are included in the sale, use the Shipping Indicator code for the physical goods. If all the goods are digital, use the Shipping Method code that describes the most expensive item. Allowed values:
        /// BILLING_ADDRESS
        /// ANOTHER_VERIFIED_ADDRESS
        /// UNVERIFIED_ADDRESS
        /// SHIP_TO_STORE
        /// DIGITAL_GOODS
        /// TRAVEL_AND_EVENT_TICKETS
        /// OTHER
        /// European merchants: optional for SCA.
        /// </summary>
        public string ShipIndicator { get; set; }

        /// <summary>
        /// Date the shipping address was first used with the merchant.
        /// European merchants: optional for SCA.
        /// </summary>
        public string ShippingAddressUsage { get; set; }

        /// <summary>
        /// When the shipping address was first used with the merchant.Allowed values:
        /// THIS_TRANSACTION
        /// LESS_THAN_THIRTY_DAYS
        /// THIRTY_TO_SIXTY_DAYS
        /// MORE_THEN_SIXTY_DAYS
        /// European merchants: optional for SCA.
        /// </summary>
        public string ShippingAddressUsageIndicator { get; set; }

        /// <summary>
        /// Indicates whether the account customer name matches the shipping address name.
        /// European merchants: optional for SCA.
        /// </summary>
        public string ShippingNameIndicator { get; set; }

        /// <summary>
        /// In the case of a preorder; the expected date when the merchandise will be available. 
        /// European merchants: optional for SCA.
        /// </summary>
        public string PreorderDate { get; set; }

        /// <summary>
        /// Indicates whether the customer is paying for merchandise that will be available at a future date.Allowed values:
        /// MERCHANDISE_AVAILABLE
        /// FUTURE_AVAILABILITY
        /// European merchants: optional for SCA.
        /// </summary>
        public string PreorderPurchaseIndicator { get; set; }

        /// <summary>
        /// Indicates whether the customer is reordering previous purchased items. Allowed values:
        /// FIRST_TIME_ORDER
        /// REORDER
        /// European merchants: optional for SCA.
        /// </summary>
        public string ReorderItemIndicator { get; set; }

        /// <summary>
        /// Number of transactions (successful and abandoned) for this customer account with the merchant in the previous 24 hours.
        /// European merchants: optional for SCA.
        /// </summary>
        public string TransactionActivityDay { get; set; }

        /// <summary>
        /// Number of transactions (successful and abandoned) for this customer account with the merchant in the previous year.
        /// European merchants: optional for SCA.
        /// </summary>
        public string TransactionActivityYear { get; set; }

        /// <summary>
        /// The total amount of prepaid or gift cards purchased. Format: major units, for example, USD 123.45 = 123.
        /// European merchants: optional for SCA.
        /// </summary>
        public string GiftCardAmount { get; set; }

        /// <summary>
        /// The total number of prepaid or gift cards purchased.
        /// European merchants: optional for SCA.
        /// </summary>
        public string GiftCardCount { get; set; }

        /// <summary>
        /// The currency code of prepaid or gift cards purchased. For example, Euro should be submitted as EUR.
        /// ISO 4217
        /// European merchants: optional for SCA.
        /// </summary>
        public string GiftCardCurrency { get; set; }

        /// <summary>
        /// Indicates the maximum number of authorizations permitted for instalment payments.
        /// European merchants: required for instalment transactions for SCA.
        /// </summary>
        public string RecurringMaxInstallments { get; set; }

        /// <summary>
        /// Date after which no further recurring authorizations shall be performed.
        /// European merchants: required for recurring transactions for SCA.
        /// </summary>
        public string RecurringExpiry { get; set; }

        /// <summary>
        /// The minimum number of days between recurring authorizations.
        /// European merchants: required for recurring transactions for SCA.
        /// </summary>
        public string RecurringFrequency { get; set; }

        /// <summary>
        /// Method used by the customer previously to authenticate. Allowed values:
        /// FRICTIONLESS_AUTHENTICATION
        /// CHALLENGE_OCCURRED
        /// AVS_VERIFIED
        /// OTHER_ISSUER_METHOD
        /// European merchants: optional for SCA.
        /// </summary>
        public string PriorTransAuthMethod { get; set; }

        /// <summary>
        /// ACS Transaction ID for a prior 3DS authenticated transaction.
        /// European merchants: optional for SCA.
        /// </summary>
        public string PriorTransAuthIdentifier { get; set; }

        /// <summary>
        /// Date and time in UTC of the prior customer authentication. Minimum of 3 microseconds precision, can be up to 6. Must also include timezone.
        /// Format: yyyy-MM-ddTHH:mm:ss.SSS(Z|±hh:mm)
        /// European merchants: optional for SCA.
        /// </summary>
        public string PriorTransAuthTimestamp { get; set; }

        /// <summary>
        /// Data that documents and supports a specific authentication process.
        /// European merchants: optional for SCA.
        /// </summary>
        public string PriorTransAuthData { get; set; }

        /// <summary>
        /// Method used by the customer previously to authenticate with the merchant. Allowed values:
        /// NOT_AUTHENTICATED
        /// MERCHANT_SYSTEM_AUTHENTICATION
        /// FEDERATED_ID_AUTHENTICATION
        /// ISSUER_CREDENTIAL_AUTHENTICATION
        /// THIRD_PARTY_AUTHENTICATION
        /// FIDO_AUTHENTICATION
        /// European merchants: optional for SCA.
        /// </summary>
        public string CardLoginAuthType { get; set; }

        /// <summary>
        /// The timestamp of the authentication with the merchant. Minimum of 3 microseconds precision, can be up to 6. Must also include timezone.
        /// Format: yyyy-MM-ddTHH:mm:ss.SSS(Z|±hh:mm)
        /// European merchants: optional for SCA.
        /// </summary>
        public string CardLoginAuthTimestamp { get; set; }

        /// <summary>
        /// Not currently in use.
        /// European merchants: optional for SCA.
        /// <summary>
        public string CardLoginAuthData { get; set; }

        /// <summary>
        /// Allows you to communicate the status of trusted beneficiary/whitelist between the Issuer ACSt. Values accepted:
        /// TRUE = Merchant is whitelisted by cardholder
        /// FALSE = Merchant is not whitelisted by cardholder
        /// European merchants: optional for SCA.
        /// </summary>
        public string WhiteListStatus { get; set; }

    #endregion

    public HostedPaymentData() {
            SupplementaryData = new Dictionary<string, string>();
        }
    }
}
