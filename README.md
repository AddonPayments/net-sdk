<a href="https://desarrolladores.addonpayments.com/" target="_blank">
    <img src="https://desarrolladores.addonpayments.com/assets/images/branding/comercia/logo.svg?v=?v=1.14.1" alt="Addon Payments logo" title="Addon Payments" align="right" width="225" />
</a>

# SDK de .NET Comercia Global Payments

Este SDK ha sido adaptado por Comercia Global Payments para facilitar la integración de su terminal Addon Payments en su servidor .NET.

## Soluciones

### General

* Procesamiento de pagos API
* Apple Pay y Google Pay (en desarrollo)
* Almacenamiento seguro de tarjetas y gestión de clientes
* Pagos recurrentes
* Crédito y Débito
* Minimizar los requisitos de cumplimiento de PCI con las soluciones de HPP
* 140+ Monedas de autorización y 16 Monedas de liquidación
* Normas incorporadas para la prevención del fraude
* Comprobaciones 3D Secure, AVS y CVV
* Cifrado seguro de extremo a extremo
* Compatible con la versión 2 de 3D Secure

## Requisitos

- .NET Standard 1.3 proyecto compatible, por ejemplo:
    - .NET Core 1.0+
    - .NET Framework 4.6+
    - Mono 4.6+

## Instalación

La instalación del SDK en su solución se realiza normalmente utilizando NuGet, o añadiendo el proyecto a su solución y haciendo referencia a él directamente.

El proyecto está compilado con entornos compatibles con las herramientas `dotnet` 2.0+ (por ejemplo, Visual Studio 2017+) y requiere la versión 1.3 o posterior de.NET Standard.

Para instalar vía [Nuget Manager Console](https://docs.nuget.org/consume/package-manager-console), debe introducir el siguiente comando en su consola:

```
PM> Install-Package AddonPayments.Api -Version 2.1.0
```

Para instalar a través de una descarga directa:

Puede descargar y descomprimir la librería, o usando Git puede [clonar el repositorio](https://github.com/addonpayments/net-sdk) desde GitHub.

```
git clone https://github.com/addonpayments/net-sdk
```
*Ver más sobre [cómo clonar repositorios](https://help.github.com/articles/cloning-a-repository/).*

## Documentación y ejemplos

Puede encontrar una documentación adaptada a cada operativa de pago, ejecutando el archivo "index.html" desde su servidor.

Este archivo se encuentra dentro de la carpeta "examples" del SDK. Si lo prefiere, también puede ver nuestra documentación oficial en la página web de desarrolladores de [Addon Payments](https://desarrolladores.addonpayments.com) donde encontrará además tarjetas con las que realizar pruebas de compra y el resto de librerías disponibles.

*Consejo rápido*: ¡[El paquete de pruebas incluido](https://github.com/addonpayments/net-sdk/tree/master/examples/AddonPayments/wwwroot) puede ser una gran fuente de ejemplos de código para usar el SDK!

#### Procesar un pago

```csharp
var card = new CreditCardData
{
    Number = "4263970000005262",
    ExpMonth = 12,
    ExpYear = 2025,
    Cvn = "131",
    CardHolderName = "James Mason"
};

try
{
    var response = card.Charge(129.99m)
        .WithCurrency("EUR")
        .Execute();

    var result = response.ResponseCode; // 00 == Success
    var message = response.ResponseMessage; // [ test system ] AUTHORISED
}
catch (ApiException e)
{
    // Manejo de errores
}
```

#### Datos de tarjeta de prueba

Nombre      | Número           | Exp Mes   | Exp Año  | CVN
----------- | ---------------- | --------- | -------- | ----
Visa        | 4263970000005262 | 12        | 2025     | 123
MasterCard  | 2223000010005780 | 12        | 2019     | 900
MasterCard  | 5425230000004415 | 12        | 2025     | 123
Discover    | 6011000000000087 | 12        | 2025     | 123
Amex        | 374101000000608  | 12        | 2025     | 1234
JCB         | 3566000000000000 | 12        | 2025     | 123
Diners Club | 36256000000725   | 12        | 2025     | 123

#### Excepciones

Durante su integración usted podrá probar las respuestas específicas del emisor tales como "Tarjeta Rechazada".

Debido a que nuestros entornos de pruebas no llegan a los bancos emisores para obtener autorizaciones, existen números de tarjeta que activarán las respuestas del banco emisor.

En la documentación de la carpeta "examples\AddonPayments\wwwroot" podrá encontrar un buscador de errores dinámico donde se muestra una descripción detallada de cada error y su posible solución.

Póngase en contacto con el equipo de soporte de [Addon Payments](mailto:soporte@addonpayments.com) para obtener una lista completa de los valores utilizados y simular los resultados de la transacción AVS/CVV.

Ejemplo de código de manejo de errores:

```csharp
try
{
    var response = card.Charge(19.95m)
        .WithCurrency("EUR")
        .WithAddress(address)
        .Execute();
}
catch (BuilderException e)
{
    // Manejar los errores del constructor
}
catch (ConfigurationException e)
{
    // Manejar los errores relacionados con la configuración de sus servicios
}
catch (GatewayException e)
{
    // Manejar los errores/excepciones de la puerta de enlace
}
catch (UnsupportedTransactionException e)
{
    // Manejar errores cuando la puerta de enlace configurada no soporta
    // operación deseada
}
catch (ApiException e)
{
    // Manejar todos los demás errores
}
```

## Soporte

En caso de que quiera hablar con un especialista de Addon Payments, deberá llamar al teléfono [914 353 028](tel:914353028) o enviar un email a [soporte@addonpayments.com](mailto:soporte@addonpayments.com).

## Contribuyendo

¡Todo nuestro código es de código abierto y animamos a otros desarrolladores a contribuir y ayudar a mejorarlo!

1. Fork it
2. Cree su rama de características (`git checkout -b mi-nueva-feature`)
3. Asegúrese de que las pruebas de SDK son correctas
4. Confirme sus cambios (`git commit -am 'Añadir un commit'`)
5. Empujar a la rama (`git push origin mi-nueva-feature`)
6. Crear una nueva solicitud de extracción

## Licencia

Este proyecto está licenciado bajo la GNU General Public License v2.0. Consulte el archivo ["LICENSE.md"](LICENSE.md) ubicado en la raíz del proyecto para obtener más detalles.
