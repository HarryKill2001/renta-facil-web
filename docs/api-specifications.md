# API Specifications - RentaFácil MVP

## Overview
This document provides comprehensive API specifications for all RentaFácil microservices, including Swagger/OpenAPI configurations, endpoint details, request/response schemas, and example usage.

## API Design Principles

### RESTful Standards
- Use HTTP verbs appropriately (GET, POST, PUT, DELETE)
- Use meaningful resource URIs
- Return appropriate HTTP status codes
- Use consistent response formats
- Implement proper pagination for collections

### Response Format Standard
All APIs follow a consistent response wrapper format:

```json
{
  "success": true,
  "data": { /* actual response data */ },
  "message": "Operation completed successfully",
  "errors": [] // Only present when success is false
}
```

## VehicleService API

### Base Configuration
- **Base URL**: `http://localhost:5001`
- **API Path**: `/api/vehicles`
- **Swagger URL**: `http://localhost:5001/swagger`

### Swagger Configuration
```csharp
// Program.cs - VehicleService
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RentaFácil Vehicle Service API",
        Version = "v1.0",
        Description = "API for vehicle management and availability checking",
        Contact = new OpenApiContact
        {
            Name = "RentaFácil Development Team",
            Email = "dev@rentafacil.com"
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    // Add security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
});
```

### Endpoints

#### 1. Get All Vehicles
```yaml
GET /api/vehicles
Summary: Retrieve all vehicles in the system
Tags: [Vehicles]
Responses:
  200:
    description: Vehicles retrieved successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/VehicleListResponse'
        example:
          success: true
          data: 
            - id: 1
              type: "SUV"
              model: "Toyota RAV4"
              year: 2023
              pricePerDay: 85.00
              available: true
              createdAt: "2024-01-15T10:30:00Z"
  500:
    description: Internal server error
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
```

#### 2. Get Vehicle by ID
```yaml
GET /api/vehicles/{id}
Summary: Retrieve a specific vehicle by ID
Tags: [Vehicles]
Parameters:
  - name: id
    in: path
    required: true
    schema:
      type: integer
      minimum: 1
    description: Vehicle unique identifier
Responses:
  200:
    description: Vehicle retrieved successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/VehicleResponse'
  404:
    description: Vehicle not found
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
        example:
          success: false
          message: "Vehicle not found"
          errors: ["Vehicle with ID 999 does not exist"]
  400:
    description: Invalid vehicle ID
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
```

#### 3. Create Vehicle
```yaml
POST /api/vehicles
Summary: Register a new vehicle
Tags: [Vehicles]
RequestBody:
  required: true
  content:
    application/json:
      schema:
        $ref: '#/components/schemas/CreateVehicleRequest'
      example:
        type: "SUV"
        model: "Honda CR-V"
        year: 2023
        pricePerDay: 90.00
Responses:
  201:
    description: Vehicle created successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/VehicleResponse'
  400:
    description: Invalid input data
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ValidationErrorResponse'
        example:
          success: false
          message: "Validation failed"
          errors: 
            type: ["Vehicle type is required"]
            year: ["Year must be between 2000 and 2024"]
```

#### 4. Check Vehicle Availability
```yaml
GET /api/vehicles/availability
Summary: Check vehicle availability for date range
Tags: [Vehicles]
Parameters:
  - name: startDate
    in: query
    required: true
    schema:
      type: string
      format: date
    description: Start date for availability check (YYYY-MM-DD)
    example: "2024-01-20"
  - name: endDate
    in: query
    required: true
    schema:
      type: string
      format: date
    description: End date for availability check (YYYY-MM-DD)
    example: "2024-01-25"
  - name: type
    in: query
    required: false
    schema:
      type: string
      enum: [SUV, Sedan, Compact]
    description: Filter by vehicle type
Responses:
  200:
    description: Available vehicles found
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/AvailabilityResponse'
        example:
          success: true
          data:
            - id: 1
              type: "SUV"
              model: "Toyota RAV4"
              year: 2023
              pricePerDay: 85.00
              totalPrice: 425.00
  400:
    description: Invalid date range
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
```

### Schemas for VehicleService
```yaml
components:
  schemas:
    Vehicle:
      type: object
      properties:
        id:
          type: integer
          example: 1
        type:
          type: string
          enum: [SUV, Sedan, Compact]
          example: "SUV"
        model:
          type: string
          example: "Toyota RAV4"
        year:
          type: integer
          minimum: 2000
          maximum: 2030
          example: 2023
        pricePerDay:
          type: number
          format: decimal
          minimum: 0.01
          example: 85.00
        available:
          type: boolean
          example: true
        createdAt:
          type: string
          format: date-time
          example: "2024-01-15T10:30:00Z"

    CreateVehicleRequest:
      type: object
      required: [type, model, year, pricePerDay]
      properties:
        type:
          type: string
          enum: [SUV, Sedan, Compact]
        model:
          type: string
          minLength: 1
          maxLength: 100
        year:
          type: integer
          minimum: 2000
          maximum: 2030
        pricePerDay:
          type: number
          format: decimal
          minimum: 0.01
          maximum: 10000

    VehicleResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          $ref: '#/components/schemas/Vehicle'
        message:
          type: string
          example: "Vehicle retrieved successfully"

    VehicleListResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          type: array
          items:
            $ref: '#/components/schemas/Vehicle'
        message:
          type: string
          example: "Vehicles retrieved successfully"

    AvailableVehicle:
      allOf:
        - $ref: '#/components/schemas/Vehicle'
        - type: object
          properties:
            totalPrice:
              type: number
              format: decimal
              description: Total price for the selected date range
              example: 425.00

    AvailabilityResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          type: array
          items:
            $ref: '#/components/schemas/AvailableVehicle'
        message:
          type: string
          example: "Available vehicles found"

    ErrorResponse:
      type: object
      properties:
        success:
          type: boolean
          example: false
        message:
          type: string
          example: "An error occurred"
        errors:
          type: array
          items:
            type: string
          example: ["Detailed error message"]

    ValidationErrorResponse:
      type: object
      properties:
        success:
          type: boolean
          example: false
        message:
          type: string
          example: "Validation failed"
        errors:
          type: object
          additionalProperties:
            type: array
            items:
              type: string
          example:
            type: ["Vehicle type is required"]
            year: ["Year must be between 2000 and 2024"]
```

## BookingService API

### Base Configuration
- **Base URL**: `http://localhost:5002`
- **API Path**: `/api`
- **Swagger URL**: `http://localhost:5002/swagger`

### Swagger Configuration
```csharp
// Program.cs - BookingService
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RentaFácil Booking Service API",
        Version = "v1.0",
        Description = "API for reservation and customer management",
        Contact = new OpenApiContact
        {
            Name = "RentaFácil Development Team",
            Email = "dev@rentafacil.com"
        }
    });

    // Custom schema filters
    options.SchemaFilter<EnumSchemaFilter>();
    options.OperationFilter<SwaggerDefaultValues>();
    
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
```

### Customer Endpoints

#### 1. Create Customer
```yaml
POST /api/customers
Summary: Register a new customer
Tags: [Customers]
RequestBody:
  required: true
  content:
    application/json:
      schema:
        $ref: '#/components/schemas/CreateCustomerRequest'
      example:
        name: "Juan Pérez"
        email: "juan.perez@email.com"
        phone: "+57 300 123 4567"
        documentNumber: "12345678"
Responses:
  201:
    description: Customer created successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/CustomerResponse'
  400:
    description: Validation error or duplicate customer
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ValidationErrorResponse'
  409:
    description: Customer already exists
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
        example:
          success: false
          message: "Customer already exists"
          errors: ["Customer with email juan.perez@email.com already exists"]
```

#### 2. Get Customer History
```yaml
GET /api/customers/{id}/history
Summary: Retrieve customer reservation history
Tags: [Customers]
Parameters:
  - name: id
    in: path
    required: true
    schema:
      type: integer
      minimum: 1
    description: Customer unique identifier
  - name: page
    in: query
    required: false
    schema:
      type: integer
      minimum: 1
      default: 1
    description: Page number for pagination
  - name: pageSize
    in: query
    required: false
    schema:
      type: integer
      minimum: 1
      maximum: 100
      default: 10
    description: Number of items per page
Responses:
  200:
    description: Customer history retrieved successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/CustomerHistoryResponse'
  404:
    description: Customer not found
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
```

### Reservation Endpoints

#### 1. Create Reservation
```yaml
POST /api/reservations
Summary: Create a new vehicle reservation
Tags: [Reservations]
RequestBody:
  required: true
  content:
    application/json:
      schema:
        $ref: '#/components/schemas/CreateReservationRequest'
      example:
        customerId: 1
        vehicleId: 1
        startDate: "2024-01-20"
        endDate: "2024-01-25"
        customerInfo:
          name: "Juan Pérez"
          email: "juan.perez@email.com"
          phone: "+57 300 123 4567"
          documentNumber: "12345678"
Responses:
  201:
    description: Reservation created successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ReservationResponse'
        example:
          success: true
          data:
            id: 1
            customerId: 1
            vehicleId: 1
            startDate: "2024-01-20T00:00:00Z"
            endDate: "2024-01-25T00:00:00Z"
            totalPrice: 425.00
            status: "Confirmed"
            confirmationNumber: "RF-20240115-001"
            createdAt: "2024-01-15T10:30:00Z"
          message: "Reservation created successfully"
  400:
    description: Invalid reservation data or vehicle unavailable
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ValidationErrorResponse'
  409:
    description: Vehicle not available for selected dates
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
        example:
          success: false
          message: "Vehicle not available"
          errors: ["Vehicle is already reserved for the selected dates"]
```

#### 2. Get Reservations
```yaml
GET /api/reservations
Summary: Retrieve reservations with optional filtering
Tags: [Reservations]
Parameters:
  - name: page
    in: query
    schema:
      type: integer
      minimum: 1
      default: 1
  - name: pageSize
    in: query
    schema:
      type: integer
      minimum: 1
      maximum: 100
      default: 10
  - name: status
    in: query
    schema:
      type: string
      enum: [Confirmed, Cancelled, Completed]
    description: Filter by reservation status
  - name: customerId
    in: query
    schema:
      type: integer
      minimum: 1
    description: Filter by customer ID
  - name: vehicleId
    in: query
    schema:
      type: integer
      minimum: 1
    description: Filter by vehicle ID
  - name: startDate
    in: query
    schema:
      type: string
      format: date
    description: Filter reservations starting from this date
  - name: endDate
    in: query
    schema:
      type: string
      format: date
    description: Filter reservations ending before this date
Responses:
  200:
    description: Reservations retrieved successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/PaginatedReservationResponse'
```

#### 3. Get Reservation by ID
```yaml
GET /api/reservations/{id}
Summary: Retrieve detailed reservation information
Tags: [Reservations]
Parameters:
  - name: id
    in: path
    required: true
    schema:
      type: integer
      minimum: 1
    description: Reservation unique identifier
Responses:
  200:
    description: Reservation retrieved successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ReservationDetailResponse'
  404:
    description: Reservation not found
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
```

#### 4. Update Reservation Status
```yaml
PATCH /api/reservations/{id}/status
Summary: Update reservation status
Tags: [Reservations]
Parameters:
  - name: id
    in: path
    required: true
    schema:
      type: integer
      minimum: 1
RequestBody:
  required: true
  content:
    application/json:
      schema:
        $ref: '#/components/schemas/UpdateReservationStatusRequest'
      example:
        status: "Cancelled"
        reason: "Customer request"
Responses:
  200:
    description: Reservation status updated successfully
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ReservationResponse'
  400:
    description: Invalid status transition
    content:
      application/json:
        schema:
          $ref: '#/components/schemas/ErrorResponse'
  404:
    description: Reservation not found
```

### Schemas for BookingService
```yaml
components:
  schemas:
    Customer:
      type: object
      properties:
        id:
          type: integer
          example: 1
        name:
          type: string
          example: "Juan Pérez"
        email:
          type: string
          format: email
          example: "juan.perez@email.com"
        phone:
          type: string
          example: "+57 300 123 4567"
        documentNumber:
          type: string
          example: "12345678"
        createdAt:
          type: string
          format: date-time
          example: "2024-01-15T10:30:00Z"

    CreateCustomerRequest:
      type: object
      required: [name, email, phone, documentNumber]
      properties:
        name:
          type: string
          minLength: 2
          maxLength: 100
          example: "Juan Pérez"
        email:
          type: string
          format: email
          maxLength: 255
          example: "juan.perez@email.com"
        phone:
          type: string
          pattern: '^\+?[\d\s\-()]+$'
          maxLength: 20
          example: "+57 300 123 4567"
        documentNumber:
          type: string
          minLength: 1
          maxLength: 50
          example: "12345678"

    Reservation:
      type: object
      properties:
        id:
          type: integer
          example: 1
        customerId:
          type: integer
          example: 1
        vehicleId:
          type: integer
          example: 1
        startDate:
          type: string
          format: date-time
          example: "2024-01-20T00:00:00Z"
        endDate:
          type: string
          format: date-time
          example: "2024-01-25T00:00:00Z"
        totalPrice:
          type: number
          format: decimal
          example: 425.00
        status:
          type: string
          enum: [Confirmed, Cancelled, Completed]
          example: "Confirmed"
        confirmationNumber:
          type: string
          example: "RF-20240115-001"
        createdAt:
          type: string
          format: date-time
          example: "2024-01-15T10:30:00Z"

    CreateReservationRequest:
      type: object
      required: [vehicleId, startDate, endDate]
      properties:
        customerId:
          type: integer
          description: Existing customer ID (optional if customerInfo provided)
          example: 1
        vehicleId:
          type: integer
          minimum: 1
          example: 1
        startDate:
          type: string
          format: date
          example: "2024-01-20"
        endDate:
          type: string
          format: date
          example: "2024-01-25"
        customerInfo:
          $ref: '#/components/schemas/CreateCustomerRequest'
          description: Customer information (required if customerId not provided)

    ReservationDetail:
      type: object
      properties:
        id:
          type: integer
          example: 1
        customer:
          $ref: '#/components/schemas/Customer'
        vehicle:
          type: object
          properties:
            id:
              type: integer
              example: 1
            model:
              type: string
              example: "Toyota RAV4"
            type:
              type: string
              example: "SUV"
            year:
              type: integer
              example: 2023
        startDate:
          type: string
          format: date-time
          example: "2024-01-20T00:00:00Z"
        endDate:
          type: string
          format: date-time
          example: "2024-01-25T00:00:00Z"
        totalPrice:
          type: number
          format: decimal
          example: 425.00
        status:
          type: string
          enum: [Confirmed, Cancelled, Completed]
          example: "Confirmed"
        confirmationNumber:
          type: string
          example: "RF-20240115-001"
        createdAt:
          type: string
          format: date-time
          example: "2024-01-15T10:30:00Z"

    UpdateReservationStatusRequest:
      type: object
      required: [status]
      properties:
        status:
          type: string
          enum: [Confirmed, Cancelled, Completed]
          example: "Cancelled"
        reason:
          type: string
          maxLength: 500
          example: "Customer request"

    CustomerHistory:
      type: object
      properties:
        customer:
          $ref: '#/components/schemas/Customer'
        reservations:
          type: array
          items:
            type: object
            properties:
              id:
                type: integer
                example: 1
              vehicleModel:
                type: string
                example: "Toyota RAV4"
              startDate:
                type: string
                format: date
                example: "2024-01-20"
              endDate:
                type: string
                format: date
                example: "2024-01-25"
              totalPrice:
                type: number
                format: decimal
                example: 425.00
              status:
                type: string
                example: "Confirmed"
              confirmationNumber:
                type: string
                example: "RF-20240115-001"

    PaginationInfo:
      type: object
      properties:
        currentPage:
          type: integer
          example: 1
        pageSize:
          type: integer
          example: 10
        totalCount:
          type: integer
          example: 25
        totalPages:
          type: integer
          example: 3

    PaginatedReservationResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          type: object
          properties:
            reservations:
              type: array
              items:
                $ref: '#/components/schemas/Reservation'
            pagination:
              $ref: '#/components/schemas/PaginationInfo'
        message:
          type: string
          example: "Reservations retrieved successfully"

    CustomerResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          $ref: '#/components/schemas/Customer'
        message:
          type: string
          example: "Customer created successfully"

    ReservationResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          $ref: '#/components/schemas/Reservation'
        message:
          type: string
          example: "Reservation created successfully"

    ReservationDetailResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          $ref: '#/components/schemas/ReservationDetail'
        message:
          type: string
          example: "Reservation retrieved successfully"

    CustomerHistoryResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        data:
          $ref: '#/components/schemas/CustomerHistory'
        message:
          type: string
          example: "Customer history retrieved successfully"
```

## API Usage Examples

### Vehicle Search Flow
```javascript
// 1. Search for available vehicles
const searchParams = {
  startDate: '2024-01-20',
  endDate: '2024-01-25',
  type: 'SUV'
};

const response = await fetch(`/api/vehicles/availability?${new URLSearchParams(searchParams)}`);
const result = await response.json();

// 2. Select a vehicle and create reservation
const reservationData = {
  vehicleId: result.data[0].id,
  startDate: '2024-01-20',
  endDate: '2024-01-25',
  customerInfo: {
    name: 'Juan Pérez',
    email: 'juan.perez@email.com',
    phone: '+57 300 123 4567',
    documentNumber: '12345678'
  }
};

const reservationResponse = await fetch('/api/reservations', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(reservationData)
});

const reservation = await reservationResponse.json();
console.log('Confirmation Number:', reservation.data.confirmationNumber);
```

### Error Handling Examples
```javascript
// Handle API errors consistently
async function handleApiCall(apiCall) {
  try {
    const response = await apiCall();
    const result = await response.json();
    
    if (!result.success) {
      throw new Error(result.message);
    }
    
    return result.data;
  } catch (error) {
    if (error.response) {
      // API error with response
      const errorData = await error.response.json();
      console.error('API Error:', errorData.message);
      
      if (errorData.errors) {
        // Validation errors
        Object.entries(errorData.errors).forEach(([field, messages]) => {
          console.error(`${field}: ${messages.join(', ')}`);
        });
      }
    } else {
      // Network or other error
      console.error('Network Error:', error.message);
    }
    throw error;
  }
}
```

## Testing APIs

### Postman Collection Structure
```json
{
  "info": {
    "name": "RentaFácil API Collection",
    "description": "Complete API collection for testing RentaFácil services"
  },
  "variable": [
    {
      "key": "vehicleServiceUrl",
      "value": "http://localhost:5001"
    },
    {
      "key": "bookingServiceUrl", 
      "value": "http://localhost:5002"
    }
  ],
  "item": [
    {
      "name": "Vehicle Service",
      "item": [
        {
          "name": "Get All Vehicles",
          "request": {
            "method": "GET",
            "url": "{{vehicleServiceUrl}}/api/vehicles"
          }
        },
        {
          "name": "Check Availability",
          "request": {
            "method": "GET",
            "url": {
              "raw": "{{vehicleServiceUrl}}/api/vehicles/availability",
              "query": [
                {"key": "startDate", "value": "2024-01-20"},
                {"key": "endDate", "value": "2024-01-25"},
                {"key": "type", "value": "SUV"}
              ]
            }
          }
        }
      ]
    }
  ]
}
```

### cURL Examples
```bash
# Get available vehicles
curl -X GET "http://localhost:5001/api/vehicles/availability?startDate=2024-01-20&endDate=2024-01-25&type=SUV" \
  -H "Accept: application/json"

# Create reservation
curl -X POST "http://localhost:5002/api/reservations" \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": 1,
    "startDate": "2024-01-20",
    "endDate": "2024-01-25",
    "customerInfo": {
      "name": "Juan Pérez",
      "email": "juan.perez@email.com",
      "phone": "+57 300 123 4567",
      "documentNumber": "12345678"
    }
  }'

# Get customer history
curl -X GET "http://localhost:5002/api/customers/1/history?page=1&pageSize=10" \
  -H "Accept: application/json"
```

## Rate Limiting and Throttling

### Implementation Example
```csharp
// Startup configuration for rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter("Global", _ =>
            new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
            
    options.AddPolicy("ApiPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter("Api", _ =>
            new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 60,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Apply to controllers
[EnableRateLimiting("ApiPolicy")]
[ApiController]
public class VehiclesController : ControllerBase
{
    // Controller implementation
}
```

This comprehensive API specification provides a solid foundation for implementing and consuming the RentaFácil microservices APIs with proper documentation, examples, and testing guidance.