using Microsoft.AspNetCore.Mvc;
using GlobalPayments.Api.Utils;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using System;
using System.Net;

namespace AddonPayments.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class PayByLinkController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PayByLink(Datos detalles) {

            // Shared Secret del terminal
            string SharedSecret = "secret";

            // Timestamp
            string time = GenerationUtils.GenerateTimestamp();

            // Order ID
            string orderID = GenerationUtils.GenerateOrderId();

            // Calculamos la firma concatenando los valores obligatorios
            var firma = GenerationUtils.GenerateHash(SharedSecret, time, "addonnettest", orderID, detalles.Op.importe, "EUR");

            // Petición Pay By Link
            using (HttpClient client = new HttpClient()) {

                PayByLink datos = new PayByLink {
                    TIMESTAMP = time,
                    AMOUNT = detalles.Op.importe,
                    ORDER_ID = orderID,
                    SHA1HASH = firma,
                    MERCHANT_RESPONSE_URL = "https://midominio.es/ResponseHpp",
                    MERCHANT_ID = "addonnettest",
                    ACCOUNT = "api",
                    AUTO_SETTLE_FLAG = "1",
                    CURRENCY = "EUR",
                    HPP_VERSION = "2",
                    COMMENT1 = detalles.Cliente.comments,
                    HPP_LANG = detalles.Cliente.lang,
                    HPP_CUSTOMER_EMAIL = detalles.Cliente.email,
                    HPP_CUSTOMER_PHONENUMBER_MOBILE = detalles.Cliente.workphone,
                    HPP_BILLING_STREET1 = detalles.Billing.billing1,
                    HPP_BILLING_STREET2 = detalles.Billing.billing2,
                    HPP_BILLING_STREET3 = detalles.Billing.billing3,
                    HPP_BILLING_CITY = detalles.Billing.billingcity,
                    HPP_BILLING_POSTALCODE = detalles.Billing.billingcode,
                    HPP_SHIPPING_STREET1 = detalles.Shipping.street1,
                    HPP_SHIPPING_STREET2 = detalles.Shipping.street2,
                    HPP_SHIPPING_STREET3 = detalles.Shipping.street3,
                    HPP_SHIPPING_CITY = detalles.Shipping.enviocity,
                    HPP_SHIPPING_STATE = detalles.Shipping.enviostate,
                    HPP_SHIPPING_POSTALCODE = detalles.Shipping.enviocode,
                    CUST_NUM = detalles.Fraud.custnum,
                    VAR_REF = detalles.Fraud.varref,
                    PROD_ID = detalles.Fraud.prodid,
                    SUPPLEMENTARY_DATA = detalles.Cliente.suplementary
                };

                try {
                    // Llamada al servidor de Addon Payments
                    try {
                        var jsonObject = JsonConvert.SerializeObject(datos);
                        StringContent content = new StringContent(jsonObject, Encoding.UTF8, MediaTypeNames.Application.Json);
                        HttpResponseMessage response = await client.PostAsync("https://hpp.sandbox.addonpayments.com/pay", content);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == HttpStatusCode.BadRequest) {
                            throw new Exception(responseBody);
                        }
                        
                        Respuesta jsonResponse = JsonConvert.DeserializeObject<Respuesta>(responseBody);


                        return Ok(responseBody);

                    } catch (Exception ex){
                        return BadRequest(ex.Message);
                    }
                } catch (HttpRequestException e) {
                    return BadRequest(e);
                }
            }
        }
    }

    public class PayByLink
    {
        public string TIMESTAMP { set; get; }
        public string AMOUNT { set; get; }
        public string ORDER_ID { set; get; }
        public string SHA1HASH { set; get; }
        public string MERCHANT_RESPONSE_URL { set; get; }
        public string MERCHANT_ID { set; get; }
        public string ACCOUNT { set; get; }
        public string AUTO_SETTLE_FLAG { set; get; }
        public string CURRENCY { set; get; }
        public string HPP_VERSION { set; get; }
        public string COMMENT1 { set; get; }
        public string HPP_LANG { set; get; }
        public string HPP_CUSTOMER_EMAIL { set; get; }
        public string HPP_CUSTOMER_PHONENUMBER_MOBILE { set; get; }
        public string HPP_BILLING_STREET1 { set; get; }
        public string HPP_BILLING_STREET2 { set; get; }
        public string HPP_BILLING_STREET3 { set; get; }
        public string HPP_BILLING_CITY { set; get; }
        public string HPP_BILLING_POSTALCODE { set; get; }
        public string HPP_SHIPPING_STREET1 { set; get; }
        public string HPP_SHIPPING_STREET2 { set; get; }
        public string HPP_SHIPPING_STREET3 { set; get; }
        public string HPP_SHIPPING_CITY { set; get; }
        public string HPP_SHIPPING_STATE { set; get; }
        public string HPP_SHIPPING_POSTALCODE { set; get; }
        public string CUST_NUM { set; get; }
        public string VAR_REF { set; get; }
        public string PROD_ID { set; get; }
        public string SUPPLEMENTARY_DATA { set; get; }
    }
}
