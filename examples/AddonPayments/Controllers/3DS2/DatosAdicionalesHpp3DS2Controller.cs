using GlobalPayments.Api;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddonPayments.Controllers._3DS2
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatosAdicionalesHpp3DS2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult DatosAdicionalesHpp3DS2()
        {

            // configure client & request settings
            var service = new HostedService(new GatewayConfig
            {
                MerchantId = "addonnettest",
                AccountId = "3ds2",
                SharedSecret = "secret",
                ServiceUrl = "https://hpp.sandbox.addonpayments.com/pay",
                HostedPaymentConfig = new HostedPaymentConfig
                {
                    Version = "2"
                }
            });

            // Add 3D Secure 2 Mandatory and Recommended Fields
            var hostedPaymentData = new HostedPaymentData
            {
                CustomerEmail = "soporte@addonpayments.com",
                CustomerPhoneMobile = "34|914353028",
                AddressesMatch = false,
                // Campos recomendados de 3D Secure 2
                ChallengeRequest = ChallengeRequestIndicator.NO_PREFERENCE,
                CustomerWorkNumber = "44|07123456789",
                AccountAgeDate = "20190110",
                AccountAgeIndicator = "NO_ACCOUNT",
                AccountChangeDate = "20190128",
                CustomerHomeNumber = "44|07123456789",
                AccountChangeIndicator = "THIS_TRANSACTION",
                AccountPassChangeDate = "20190115",
                AccountPassChangeIndicator = "THIS_TRANSACTION",
                AccountPurchaseCount = "3",
                TransactionType = "GOODS_SERVICE_PURCHASE",
                CardholderAccountIdentifier = "1f0aae6b-0bac-479f-9ee5-29b9b6cf3aa0",
                SuspiciousActivity = "SUSPICIOUS_ACTIVITY",
                ProvisionAttemptsDay = "1",
                PaymentAccountAge = "201901101",
                PaymentAccountAgeIndicator = "NO_ACCOUNT",
                DeliveryEmail = "deliveryemail@email.com",
                DeliveryTimeframe = "TWO_DAYS_OR_MORE",
                ShipIndicator = "UNVERIFIED_ADDRESS",
                ShippingAddressUsage = "20190128",
                ShippingAddressUsageIndicator = "THIS_TRANSACTION",
                ShippingNameIndicator = "TRUE",
                PreorderDate = "20190212",
                ReorderItemIndicator = "FIRST_TIME_ORDER",
                TransactionActivityDay = "1",
                TransactionActivityYear = "3",
                // GiftCardAmount = "250",
                // GiftCardCount = "1",
                // GiftCardCurrency = "EUR",
                RecurringMaxInstallments = "5",
                RecurringExpiry = "20190205",
                RecurringFrequency = "25",
                PriorTransAuthMethod = "FRICTIONLESS_AUTHENTICATION",
                PriorTransAuthIdentifier = "26c3f619-39a4-4040-bf1f-6fd433e6d615",
                PriorTransAuthTimestamp = "20190110125733",
                PriorTransAuthData = "string",
                CardLoginAuthType = "MERCHANT_SYSTEM_AUTHENTICATION",
                CardLoginAuthTimestamp = "20180613110212",
                CardLoginAuthData = "string",
                WhiteListStatus = "false"
            };

            var billingAddress = new Address
            {
                StreetAddress1 = "Dirección de facturación 1",
                StreetAddress2 = "Dirección de facturación 2",
                StreetAddress3 = "Dirección de facturación 3",
                City = "Elche",
                PostalCode = "03201",
                Country = "826"
            };

            var shippingAddress = new Address
            {
                StreetAddress1 = "Dirección de envío 1",
                StreetAddress2 = "Dirección de envío 2",
                StreetAddress3 = "Dirección de envío 3",
                City = "Elche",
                State = "ES",
                PostalCode = "03201",
                Country = "724",
            };

            try
            {
                // Lanzamos la operación al servidor de Addon Payments
                var hppJson = service.Charge(19.99m)
                      .WithCurrency("EUR")
                      .WithHostedPaymentData(hostedPaymentData)
                      .WithAddress(billingAddress, AddressType.Billing)
                      .WithAddress(shippingAddress, AddressType.Shipping)
                      .Serialize();


                return Content(hppJson);
            }

            catch (ApiException exce)
            {
                RespuestaError respuesta = new RespuestaError { resultado = "Error en el envío de datos <br><br>" + exce };
                return Ok(respuesta);
            }
        }
    }
}