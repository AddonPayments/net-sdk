﻿using System;
using System.IO;
using GlobalPayments.Api.Terminals.Extensions;
using System.Text;
using System.Collections.Generic;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Terminals.Abstractions;

namespace GlobalPayments.Api.Terminals.PAX {
    public abstract class PaxBaseResponse : DeviceResponse {
        protected List<string> _messageIds;
        protected byte[] _buffer;

        internal HostResponse HostResponse { get; set; }
        internal AmountResponse AmountResponse { get; set; }
        internal AccountResponse AccountResponse { get; set; }
        internal TraceResponse TraceResponse { get; set; }
        internal AvsResponse AvsResponse { get; set; }
        internal CommercialResponse CommercialResponse { get; set; }
        internal EcomSubGroup EcomResponse { get; set; }
        internal ExtDataSubGroup ExtDataResponse { get; set; }
        internal CheckSubGroup CheckSubResponse { get; set; }
        internal CashierSubGroup CashierResponse { get; set; }

        internal PaxBaseResponse(byte[] buffer, params string[] messageIds) {
            _messageIds = new List<string>();
            _messageIds.AddRange(messageIds);

            _buffer = buffer;

            using (var br = new BinaryReader(new MemoryStream(buffer))) {
                ParseResponse(br);
            }
        }

        protected virtual void ParseResponse(BinaryReader br) {
            var code = (ControlCodes)br.ReadByte(); // STX
            Status = br.ReadToCode(ControlCodes.FS);
            Command = br.ReadToCode(ControlCodes.FS);
            Version = br.ReadToCode(ControlCodes.FS);
            DeviceResponseCode = br.ReadToCode(ControlCodes.FS);
            DeviceResponseText = br.ReadToCode(ControlCodes.FS);

            if (!_messageIds.Contains(Command)) {
                throw new MessageException(string.Format("Unexpected message type received. {0}.", Command));
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (byte b in _buffer) {
                if (Enum.IsDefined(typeof(ControlCodes), b)) {
                    var code = (ControlCodes)b;
                    sb.Append(string.Format("[{0}]", code.ToString()));
                }
                else sb.Append((char)b);
            }

            return sb.ToString();
        }
    }

    public class PaxTerminalResponse : PaxBaseResponse, ITerminalResponse {
        protected List<string> acceptedCodes = new List<string> { "000000", "100011" };

        /// <summary>
        /// response code returned by the gateway
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// response message returned by the gateway
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// the gateway transaction id
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// the device's transaction reference number
        /// </summary>
        public string TerminalRefNumber { get; set; }

        /// <summary>
        /// the multi-use payment token generated by the device in instances where tokenization is requested
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// value indicating the presence/non-presence of signature data 
        /// </summary>
        public string SignatureStatus { get; set; }

        /// <summary>
        /// byte array containing the bitmap data for the signature (you may have to call GetSignature depending on your device)
        /// </summary>
        public byte[] SignatureData { get; set; }

        // Transactional
        /// <summary>
        /// the type of transaction (Sale, Authorization, Verify etc...)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// the masked credit card number
        /// </summary>
        public string MaskedCardNumber { get; set; }

        /// <summary>
        /// value denoting whether a card was swiped, inserted, tapped or manually entered
        /// </summary>
        public string EntryMethod { get; set; }

        /// <summary>
        /// the authorization code returned by the issuer
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// the approval code issued by the device
        /// </summary>
        public string ApprovalCode { get; set; }

        /// <summary>
        /// the amount of the transaction
        /// </summary>
        public decimal? TransactionAmount { get; set; }

        /// <summary>
        /// the remaining balance in instances of partial approval or gift sales
        /// </summary>
        public decimal? AmountDue { get; set; }

        /// <summary>
        /// the balance of a prepaid or gift card when running a balance inquiry
        /// </summary>
        public decimal? BalanceAmount { get; set; }

        /// <summary>
        /// the card holder name as represented in the track data
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// the BIN range of the card used
        /// </summary>
        public string CardBIN { get; set; }

        /// <summary>
        /// flag indicating whether or not the card was present during the transaction
        /// </summary>
        public bool CardPresent { get; set; }

        /// <summary>
        /// card expiration date
        /// </summary>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// the tip amount applied to the transaction if any
        /// </summary>
        public decimal? TipAmount { get; set; }

        /// <summary>
        /// the cash back amount requested during a debit transaction
        /// </summary>
        public decimal? CashBackAmount { get; set; }

        /// <summary>
        /// Response code from the address verification system
        /// </summary>
        public string AvsResponseCode { get; set; }

        /// <summary>
        /// response text from the address verification system
        /// </summary>
        public string AvsResponseText { get; set; }

        /// <summary>
        /// response code from the CVN/CVV Check.
        /// </summary>
        public string CvvResponseCode { get; set; }

        /// <summary>
        /// response text from the CVN/CVV Check
        /// </summary>
        public string CvvResponseText { get; set; }

        /// <summary>
        /// For level II transactions, value indicating tax exemption status
        /// </summary>
        public bool TaxExempt { get; set; }

        /// <summary>
        /// For level II the business tax exemption ID
        /// </summary>
        public string TaxExemptId { get; set; }

        /// <summary>
        /// The ticket number associated with the transaction
        /// </summary>
        public string TicketNumber { get; set; }

        /// <summary>
        /// The type of payment method used (Credit, Debit, etc...)
        /// </summary>
        public string PaymentType { get; set; }

        // EMV
        /// <summary>
        /// The preferred name of the EMV application selected on the EMV card
        /// </summary>
        public string ApplicationPreferredName { get; set; }

        /// <summary>
        /// The aplication label from the EMV card
        /// </summary>
        public string ApplicationLabel { get; set; }

        /// <summary>
        /// the AID (Application ID) of the selected application on the EMV card
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// The cryptogram type used during the transaction
        /// </summary>
        public ApplicationCryptogramType ApplicationCryptogramType { get; set; }

        /// <summary>
        /// The actual cryptogram value generated for the transaction
        /// </summary>
        public string ApplicationCryptogram { get; set; }

        /// <summary>
        /// The CVM used in the transaction (PIN, Signature, etc...)
        /// </summary>
        public string CardHolderVerificationMethod { get; set; }

        /// <summary>
        /// The results of the terminals attempt to verify the cards authenticity.
        /// </summary>
        public string TerminalVerificationResults { get; set; }
        
        internal PaxTerminalResponse(byte[] buffer, params string[] messageIds) : base(buffer, messageIds) { }

        protected virtual void MapResponse() {
            // Host Data
            if (HostResponse != null) {
                ResponseCode = NormalizeResponse(HostResponse.HostResponseCode);
                ResponseText = HostResponse.HostResponseMessage;
                ApprovalCode = HostResponse.HostResponseCode;
                AuthorizationCode = HostResponse.AuthCode;
            }

            // Amount Response
            if (AmountResponse != null) {
                TransactionAmount = AmountResponse.ApprovedAmount;
                AmountDue = AmountResponse.AmountDue;
                TipAmount = AmountResponse.TipAmount;
                CashBackAmount = AmountResponse.CashBackAmount;
                BalanceAmount = AmountResponse.Balance1 ?? AmountResponse.Balance2;
            }

            // Account Response
            if (AccountResponse != null) {
                MaskedCardNumber = AccountResponse.AccountNumber.PadLeft(16, '*');
                EntryMethod = AccountResponse.EntryMode.ToString();
                ExpirationDate = AccountResponse.ExpireDate;
                PaymentType = AccountResponse.CardType.ToString().Replace("_", " ");
                CardHolderName = AccountResponse.CardHolder;
                CvvResponseCode = AccountResponse.CvdApprovalCode;
                CvvResponseText = AccountResponse.CvdMessage;
                CardPresent = AccountResponse.CardPresent;
            }

            // Trace Data
            if (TraceResponse != null) {
                TerminalRefNumber = TraceResponse.TransactionNumber;
                ReferenceNumber = TraceResponse.ReferenceNumber;
            }

            // AVS
            if (AvsResponse != null) {
                AvsResponseCode = AvsResponse.AvsResponseCode;
                AvsResponseText = AvsResponse.AvsResponseMessage;
            }

            // Commercial Info
            if (CommercialResponse != null) {
                TaxExempt = CommercialResponse.TaxExempt;
                TaxExemptId = CommercialResponse.TaxExemptId;
            }

            // Ext Data
            if (ExtDataResponse != null) {
                TransactionId = ExtDataResponse[EXT_DATA.HOST_REFERENCE_NUMBER];
                Token = ExtDataResponse[EXT_DATA.TOKEN];
                CardBIN = ExtDataResponse[EXT_DATA.CARD_BIN];
                SignatureStatus = ExtDataResponse[EXT_DATA.SIGNATURE_STATUS];

                // EMV Stuff
                ApplicationPreferredName = ExtDataResponse[EXT_DATA.APPLICATION_PREFERRED_NAME];
                ApplicationLabel = ExtDataResponse[EXT_DATA.APPLICATION_LABEL];
                ApplicationId = ExtDataResponse[EXT_DATA.APPLICATION_ID];
                ApplicationCryptogramType = ApplicationCryptogramType.TC;
                ApplicationCryptogram = ExtDataResponse[EXT_DATA.TRANSACTION_CERTIFICATE];
                CardHolderVerificationMethod = ExtDataResponse[EXT_DATA.CUSTOMER_VERIFICATION_METHOD];
                TerminalVerificationResults = ExtDataResponse[EXT_DATA.TERMINAL_VERIFICATION_RESULTS];
            }
        }

        private string NormalizeResponse(string input) {
            if (input == "0" || input == "85")
                return "00";
            return input;
        }
    }

    public class PaxTerminalReport : PaxBaseResponse, ITerminalReport {
        internal PaxTerminalReport(byte[] buffer, params string[] messageIds) : base(buffer, messageIds) {
        }

        protected virtual void MapResponse() { }
    }
}