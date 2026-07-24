# Artemis Banking
Artemis Banking es una plataforma de banca en línea diseñada para la gestión integral de productos financieros, permitiendo a los usuarios administrar préstamos, tarjetas de crédito y cuentas de ahorro en un entorno seguro y eficiente.

El sistema opera bajo un esquema de permisos basado en roles (Administrador, Cajero y Cliente), asegurando que cada usuario acceda únicamente a las funcionalidades correspondientes a su perfil.

## Tecnologías utilizadas
- **Lenguaje:** C#
- **FrontEnd:** ASP.NET Core MVC, Razor Pages, Bootstrap y HTML.
- **Backend:** Entity Framework Core (Enfoque Code First), Web API, JWT (para la API) e Identity (para gestión de seguridad y usuarios).
- **Arquitectura:** Onion (Cebolla), garantizando una separación clara entre la lógica de negocio y la presentación.
- **Automatización:** Hangfire (para procesos en segundo plano como control de cuotas).

## Roles del sistema
### Administrador
El administrador es el responsable de la gestión operativa y supervisión del sistema.
- **Dashboard:** Visualización de indicadores clave como total de transacciones, clientes activos e inactivos y monto promedio de deuda.
- **Gestión de Usuarios:** Crear, editar y activar/desactivar cuentas de administradores, cajeros y clientes.
- **Gestión de Productos:** Asignación y administración de préstamos, tarjetas de crédito y cuentas de ahorro.

### Cajero
El cajero se encarga de las operaciones presenciales y transaccionales directas.
- Realizar depósitos y retiros de efectivo en cuentas de ahorro.
- Procesar pagos de tarjetas de crédito y cuotas de préstamos.
- Ejecutar transacciones a cuentas de terceros.

### Cliente
El cliente es el usuario final que gestiona sus propios productos financieros.
- **Home:** Visualización de balances de cuentas, estado de préstamos y límites de tarjetas de crédito.
- **Transacciones:** Realizar transferencias express, pagos a beneficiarios y transferencias entre cuentas propias.
- **Pagos y Avances:** Pago de productos y solicitud de avances de efectivo desde tarjetas de crédito.

## API Endpoints

La API de **Artemis Banking v1.0** es la responsable de la distribución general de los datos del sistema. A continuación se detallan los controladores con sus respectivas responsabilidades:

### Account
*Gestión de la seguridad, autenticación y recuperación de acceso de los usuarios.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/Account/login` | Autentica al usuario y genera un token JWT. | `LoginDto` |
| `POST` | `/api/v1/Account/confirm` | Confirma la cuenta de un usuario recién registrado. | `ConfirmDto` |
| `POST` | `/api/v1/Account/get-reset-token` | Genera un token para el proceso de olvido de contraseña. | `ResetTokenDto` |
| `POST` | `/api/v1/Account/reset-password` | Establece una nueva contraseña utilizando un token de validación. | `ResetPasswordApiRequestDto` |

### Pay
*Controlador encargado de procesar los pagos de productos bancarios (Préstamos y Tarjetas).*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/Pay/loan` | Procesa el pago de una cuota de préstamo existente. | `PayLoanDto` |
| `POST` | `/api/v1/Pay/credit-card` | Procesa el pago del balance de una tarjeta de crédito. | `PayCreditCardDto` |

### User
*Gestión administrativa de los perfiles de usuario y mantenimiento de cuentas.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/User` | Obtiene un listado paginado y filtrado de usuarios. | `page`, `pageSize`, `role` |
| `GET` | `/api/v1/User/{id}` | Obtiene la información detallada de un usuario por su ID único. | `{id}` (Path) |
| `PUT` | `/api/v1/User/{id}` | Permite modificar la información de un usuario existente. | `{id}`, `UpdateUserDto` |
| `PATCH` | `/api/v1/User/{id}/status` | Cambia el estado de activación del usuario en el sistema. | `{id}`, `status` (Query) |

### CreditCard
*Mantenimiento y consulta de productos de tipo Tarjeta de Crédito.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/CreditCard` | Consulta tarjetas filtrando por número de cédula o estado. | `identityCardNumber`, `isActive` |
| `POST` | `/api/v1/CreditCard` | Crea y vincula una nueva tarjeta de crédito a un cliente. | `CreateCreditCardDto` |

### SavingsAccount
*Gestión de las cuentas de ahorro y balances de los clientes.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/SavingsAccount` | Lista las cuentas de ahorro asociadas a un cliente. | `clientId`, `page` |
| `POST` | `/api/v1/SavingsAccount` | Apertura una nueva cuenta de ahorro para un cliente específico. | `CreateSavingsAccountDto` |

### Loan
*Gestión de préstamos y aplicación del sistema de amortización francés.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/Loan` | Recupera los préstamos vigentes de un cliente. | `clientId` |
| `POST` | `/api/v1/Loan` | Registra un nuevo préstamo calculando cuotas fijas. | `CreateLoanDto` |

### CardTransaction
*Controlador especializado en el procesamiento de transacciones desde comercios externos.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/CardTransaction` | Punto de entrada para que comercios procesen pagos. | `ProcessCardTransactionDto` |
| `GET` | `/api/CardTransaction` | Historial de transacciones procesadas por la pasarela. | `page`, `pageSize` |

### Commerce
*Gestión y mantenimiento de los comercios afiliados que utilizan la API.*

| Método | Endpoint | Descripción | Parámetros / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Commerce` | Lista todos los comercios afiliados al sistema. | `page`, `pageSize` |
| `POST` | `/api/Commerce` | Registra un nuevo comercio para permitirle recibir pagos. | `CreateCommerceDto` |
| `GET` | `/api/Commerce/{id}` | Obtiene los datos generales de un comercio por su ID. | `{id}` |
| `PUT` | `/api/Commerce/{id}` | Actualiza la información técnica o de contacto del comercio. | `{id}`, `UpdateCommerceDto` |
| `PATCH` | `/api/Commerce/{id}` | Habilita o inhabilita el acceso de un comercio al sistema. | `{id}`, `ChangeCommerceStatusDto` |

---
> **Información de Seguridad:** La API utiliza el esquema **Bearer Token**. Todos los endpoints (excepto Login y Recovery) requieren la cabecera `Authorization: Bearer <token_jwt>`.

## Como ejecutar este Software?
1. Clone este repositorio en su máquina local.
2. Configure los archivos `appsettings.json` en los proyectos de presentación (`WebApp` y `API`) completando las secciones de `ConnectionStrings`, `MailSettings` y `JWTSettings`.
3. Ejecute las migraciones para crear la base de datos y cargar los datos iniciales (Seeding) mediante el comando:
   `dotnet ef database update`
4. Inicie la aplicación utilizando el comando `dotnet run` para el proyecto deseado o ejecute la solución completa desde Visual Studio.



## Artemis Banking.
Participantes:
- Adoni Daniel Martinez Brito 2024-1106
- Ashley Michel Cabrera Mena 2024-1276

#reposiotrio
https://github.com/Adoni1234/Artemis-banking
