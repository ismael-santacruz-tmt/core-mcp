# Preambulo
Vas a crear un **MCP Server en C#** que actúe como intermediario entre un ERP (sistema de gestión empresarial) y un **cliente web ASP.NET Core MVC** que permita a los usuarios invocar las herramientas (tools) expuestas por el MCP Server.
El objetivo es que el MCP Server traduzca las llamadas del cliente web a las API REST del ERP, manejando autenticación, errores y modelado de datos. El cliente web ofrecerá una interfaz sencilla para listar e invocar las herramientas del MCP Server, mostrando los resultados en formato JSON.


# ✅ Checklist común (Server + Cliente MCP en C#)

## 0) Preparación (común)
- [x] Instalar **.NET 8+** y **Git**.
- [x] Crear repo con dos carpetas:  
  - /server (MCP Server C# que proxy-a tu ERP)  
  - /client (ASP.NET Core MVC: UI para invocar tools)
- [x] Definir 2–3 endpoints ERP **MVP** (ruta, método, body/params, ejemplos JSON).
- [x] Establecer **variables de entorno** (comunes):  
  - `BASE_URL` = `https://isv.telematel.dev`  
  - `API_KEY` = `{IsmaApikey}` (se configurará en `appsettings.json`).
- [ ] Crear `.gitignore` y **no** commitear secretos. (Este paso se realizará por separado)


## 1) Proyecto **SERVER** (C# • MCP que envuelve tu ERP)

### 1.1 Bootstrap
- [x] `dotnet new console -n ErpMcpServer`
- [x] Paquetes NuGet:
  - [x] `ModelContextProtocol --prerelease`
  - [x] `System.Text.Json`
  - [x] `System.Net.Http.Json`
  - [x] (Opcional) `Microsoft.Extensions.Configuration.Json`
  - [x] (Opcional) `Microsoft.Extensions.Logging.Console`

### 1.2 Configuración
- [ ] Leer `BASE_URL`, `API_KEY` desde entorno o `appsettings`.
- [ ] Configurar `HttpClient` con:
- `BaseAddress = BASE_URL`
- Header `Authorization: X-Api-Key {API_KEY}`
- `Accept: application/json`
- Timeouts razonables (p.ej. 30–60s).

### 1.3 Modelado DTOs (alineados al ERP)
- [ ] Inputs (records) p.ej. `GetInvoiceArgs`, `CreatePaymentArgs`.
- [ ] Outputs (records) p.ej. `Invoice`, `PaymentResult`.
- [ ] Reglas: campos obligatorios/opcionales, decimales, fechas ISO 8601.

### 1.4 Registro de **Tools MCP**
- [ ] Instanciar `var server = new McpServer(new McpServerOptions { Name, Version });`
- [ ] Tool 1: `get_invoice_by_id` → `GET /invoices/{id}`
- [ ] Tool 2: `create_payment` → `POST /payments`
- [ ] (Opc) Tool 3: `search_customers` → `GET /customers?query=..&page=..`
- [ ] Validar inputs (antes de llamar al ERP) y mapear errores del ERP.

### 1.5 Arranque y Transporte
- [ ] `await server.RunStdioAsync();` (desarrollo local).
- [ ] (Opc) Exponer HTTP/SSE si el SDK lo soporta para despliegues remotos.

### 1.6 Observabilidad y Seguridad
- [ ] Logging mínimo por tool (inicio/fin/duración), **sin** secretos.
- [ ] Redactar args sensibles; incluir `correlationId` del ERP si existe.
- [ ] Manejo de errores claro (mensaje ERP + status, sin stack interno).

### 1.7 Pruebas Server
- [ ] Prueba `get_invoice_by_id` con id válido e inválido.
- [ ] Prueba `create_payment` con monto > 0 y error de validación.
- [ ] (Opc) Tests unitarios (mock `HttpClient`).

### 1.8 Entregables Server
- [ ] `Program.cs` con registro de tools y `RunStdioAsync`.
- [ ] DTOs, helpers HTTP, README con **cómo ejecutar**.
- [ ] Script `run-server` (PowerShell o bash) con ejemplo de env vars.

### 1.9 DoD (Definition of Done) Server
- [ ] Tools listados por un cliente MCP externo.
- [ ] Respuestas correctas para casos “happy path”.
- [ ] Errores mapeados de forma legible (sin secretos, con contexto).
- [ ] Logs básicos y tiempos de respuesta.

---

## 2) Proyecto **CLIENTE** (ASP.NET Core MVC • UI para invocar MCP)

### 2.1 Bootstrap
- [ ] `dotnet new mvc -n McpWebDashboard`
- [ ] Paquetes NuGet:
- [ ] `ModelContextProtocol --prerelease`
- [ ] `System.Net.Http.Json`
- [ ] (Opc) `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Logging.Console`

### 2.2 Configuración
- [ ] `appsettings.json` → sección `Mcp.Server`:
- `Command` (ej. `dotnet`)
- `Arguments` (ej. `["run","--project","../server/ErpMcpServer"]`)
- `Name` (identificador)
- [ ] Clase `McpSettings` y binding en `Program.cs`.

### 2.3 Servicio MCP (capa de acceso)
- [ ] `McpClientService` (Singleton) que:
- Inicializa `StdioClientTransport` con la config.
- `ListToolsAsync()` → devuelve listado para la UI.
- `CallToolAsync(string name, Dictionary<string,object?> args)` → devuelve JSON.
- Implementa `IAsyncDisposable` para cerrar transporte al apagar.

### 2.4 Modelos UI
- [ ] `ToolDescriptor` (nombre, descripción, schema opcional).
- [ ] `CallToolRequest` (tool + args).
- [ ] `CallToolResult` (payload JSON / error).

### 2.5 Controladores
- [ ] `HomeController.Index` → lista tools.
- [ ] `HomeController.Call(string tool)` GET → muestra formulario de args (simple: key/value; opcional: JSON editor).
- [ ] `HomeController.Call(...)` POST → invoca tool y muestra resultado.

### 2.6 Vistas
- [ ] `Index.cshtml` → tabla de tools + botón “Invocar”.
- [ ] `Call.cshtml` → formulario (inputs básicos) + panel de **resultado JSON** embellecido.
- [ ] UX:
- Validar inputs.
- Mostrar spinner durante la llamada.
- Mostrar errores del MCP de forma clara.

### 2.7 Observabilidad y Seguridad (cliente)
- [ ] Logging de invocaciones (tool, duración).
- [ ] No registrar args sensibles (p.ej., referencias bancarias).
- [ ] (Opc) Autenticación en la web (si habrá usuarios reales).

### 2.8 Pruebas Cliente
- [ ] Carga de página `Index` → lista tools.
- [ ] `get_invoice_by_id` → muestra factura real en la UI.
- [ ] `create_payment` → muestra `paymentId` y `status`.
- [ ] Manejo de errores legible (id inexistente, validación).

### 2.9 Entregables Cliente
- [ ] Proyecto MVC funcional.
- [ ] README con pasos: arranque del server + ejecución de la web.
- [ ] Script `run-web` (PowerShell o bash).

### 2.10 DoD Cliente
- [ ] Descubre tools del server.
- [ ] Invoca tools con parámetros desde el navegador.
- [ ] Muestra resultados y errores de forma entendible.
- [ ] Cierra transporte al apagar la app.

---

## 3) Integración, CI/CD y Deploy (opcional)
- [ ] **Scripts**: `build-all`, `run-all` (levanta server + web).
- [ ] **Docker**:
- `server/Dockerfile` (ENV `BASE_URL`,`API_KEY`).
- `client/Dockerfile` (build + runtime).
- `docker-compose.yml` para ambos (volúmenes/env).
- [ ] **CI** (GitHub Actions/Azure DevOps):
- Build y tests.
- Publicación de artefactos.
- [ ] **Entornos**:
- Dev: `RunStdioAsync` y servidor local.
- Prod: server publicado (stdio/HTTP), web detrás de reverso/proxy.

---

## 4) Secuencia sugerida
1. **Server**: bootstrap → config → DTOs → tools → arranque → pruebas locales.
2. **Cliente**: bootstrap → config → `McpClientService` → UI básica → invocar tools.
3. **QA**: casos “happy path” + errores controlados.
4. (Opc) Docker/CI.
5. Extensiones (más tools, auth, LLM, editor JSON, métricas).

---

### Obtener Facturas de Compra
- **Ruta:** `/pur-invoice`
- **Método:** `GET`
- **Ejemplo de solicitud (CURL):**
  ```c
  CURL *hnd = curl_easy_init();

  curl_easy_setopt(hnd, CURLOPT_CUSTOMREQUEST, "GET");
  curl_easy_setopt(hnd, CURLOPT_URL, "https://isv.api.telematel.dev/pur-invoice");

  CURLcode ret = curl_easy_perform(hnd);
  ```
- **Ejemplo de respuesta:**
  ```json
  {
    "items": [
      {
        "supplier": {
          "occasional": true,
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "supplier_ou": {
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "country": {
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "province": {
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "currency": {
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "pay_method": {
          "pay_type": "cash",
          "edit_doc": true,
          "edit_card": true,
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "cur_amount": 1,
        "cur_exchange": 1,
        "code_draft": "string",
        "code_prefix": "string",
        "reference_doc": "string",
        "business_name": "string",
        "fiscal_id": "string",
        "address": "string",
        "postal_code": "string",
        "city": "string",
        "date_inv": "2025-09-04",
        "date_rec": "2025-09-04",
        "date_accounting": "2025-09-04",
        "blocked": true,
        "payments": 1,
        "offset_days": 1,
        "cadence": 1,
        "counter_value": 1,
        "disc_pp": 1,
        "financial_sur": 1,
        "freight": 1,
        "packaging": 1,
        "amount_gross": 1,
        "amount_total": 1,
        "income_tax": 1,
        "status_pur_invoice": "draft",
        "accounting_diff": "first_product",
        "company": {
          "code": "string",
          "name": "string",
          "row_guid": "123e4567-e89b-12d3-a456-426614174000"
        },
        "code": "string",
        "name": "string",
        "description": "string",
        "row_version": 1,
        "row_created": "2025-09-04T13:23:47.715Z",
        "row_modified": "2025-09-04T13:23:47.715Z",
        "row_guid": "123e4567-e89b-12d3-a456-426614174000"
      }
    ],
    "total_count": 1
  }
  ```

### Crear Pago
- **Ruta:** `/acc-payable/pay`
- **Método:** `POST`
- **Ejemplo de solicitud (C#):**
  ```csharp
  using System.Net.Http.Headers;
  var client = new HttpClient();
  var request = new HttpRequestMessage
  {
      Method = HttpMethod.Post,
      RequestUri = new Uri("https://api.core.telematel.com/acc-payable/pay"),
      Content = new StringContent("{\"pay_date\":\"\",\"detailed_acc_entry\":true,\"cash_register\":{\"code\":\"\",\"name\":\"\",\"row_guid\":\"\"},\"account\":{\"code\":\"\",\"name\":\"\",\"row_guid\":\"\"},\"company\":{\"code\":\"\",\"name\":\"\",\"row_guid\":\"\"},\"items\":[{\"row_guid\":\"\"}]}")
      {
          Headers =
          {
              ContentType = new MediaTypeHeaderValue("application/json")
          }
      }
  };
  using (var response = await client.SendAsync(request))
  {
      response.EnsureSuccessStatusCode();
      var body = await response.Content.ReadAsStringAsync();
      Console.WriteLine(body);
  }
  ```
- **Cuerpo de la solicitud:**
  ```json
  {
    "pay_date": "",
    "detailed_acc_entry": true,
    "cash_register": {
      "code": "",
      "name": "",
      "row_guid": ""
    },
    "account": {
      "code": "",
      "name": "",
      "row_guid": ""
    },
    "company": {
      "code": "",
      "name": "",
      "row_guid": ""
    },
    "items": [
      {
        "row_guid": ""
      }
    ]
  }
  ```
- **Ejemplo de respuesta:**
  ```json
  {
    "code": "string",
    "code_prefix": "string",
    "counter_value": 1,
    "reference": "string",
    "acc_pay_date": "2025-09-04",
    "due_date": "2025-09-04",
    "pay_date": "2025-09-04",
    "amount": 1,
    "cur_exchange": 1,
    "cur_amount": 1,
    "cancelled": true,
    "blocked": true,
    "business_name": "string",
    "fiscal_id": "string",
    "address": "string",
    "postal_code": "string",
    "city": "string",
    "acc_pay_type": "standard",
    "acc_pay_source": "manual",
    "credit_debit": "credit",
    "status_acc_pay": "pending",
    "company": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "supplier": {
      "occasional": true,
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "supplier_ou": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "currency": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "account": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "payment_document": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "country": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "province": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "pur_invoice": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "bank_account": {
      "code": "string",
      "name": "string",
      "row_guid": "123e4567-e89b-12d3-a456-426614174000"
    },
    "name": "string",
    "description": "string",
    "row_version": 1,
    "row_created": "2025-09-04T13:41:45.525Z",
    "row_modified": "2025-09-04T13:41:45.525Z",
    "row_guid": "123e4567-e89b-12d3-a456-426614174000"
  }
  ```
