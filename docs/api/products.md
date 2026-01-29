# Product Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Product Microservice.

**Base URL**: `http://localhost:8082/api/v1/products` (via Gateway) or `http://localhost:8083/api/v1/products` (Direct)

---

## 📦 Public Catalog (Read-Only)

### 1. Get All Products
Retrieves a list of all products in the catalog.

**Endpoint**: `GET /`

```bash
curl -X GET http://localhost:8082/api/v1/products
```

### 2. Get Product by ID
Retrieves full details for a specific product, including specifications, advantages, and precautions.

**Endpoint**: `GET /{id}`

```bash
curl -X GET http://localhost:8082/api/v1/products/1
```

---

## 🛠️ Product Management (Admin Only)

**Auth Required**: `Bearer <admin_token>`

### 3. Create Product
Creates a new product with images and documentation. This endpoint uses `multipart/form-data`.

**Endpoint**: `POST /`

```bash
curl -X POST http://localhost:8082/api/v1/products \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Industrial Sealant X1" \
-F "Description=High-performance industrial grade sealant." \
-F "Category=Sealants" \
-F "SubCategory=Industrial" \
-F "Brand=ErpBrand" \
-F "ImageFile=@/path/to/main_image.jpg" \
-F "RelatedImageFiles=@/path/to/extra1.jpg" \
-F "RelatedImageFiles=@/path/to/extra2.jpg" \
-F "TechnicalDataSheetFiles=@/path/to/tds.pdf" \
-F "OverviewDetails=Perfect for high-temperature environments."
```

### 4. Update Product
Updates an existing product's details and assets. Uses `multipart/form-data`.

**Endpoint**: `PUT /{id}`

```bash
curl -X PUT http://localhost:8082/api/v1/products/1 \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Updated Sealant X1 Plus" \
-F "Description=Updated description for version 2."
```

### 5. Delete Product
Removes a product and all its associated assets from the cloud storage.

**Endpoint**: `DELETE /{id}`

```bash
curl -X DELETE http://localhost:8082/api/v1/products/1 \
-H "Authorization: Bearer your_admin_token"
```

---

## 🛠️ Data Formats

### 1. Specifications (JSON)
When sending `SpecificationsJson`, use the following structure:
```json
[
  { "key": "Temperature Range", "value": "-50°C to +200°C" },
  { "key": "Drying Time", "value": "24 Hours" }
]
```

### 2. Document Types
Supported document keys for internal mapping:
- `technical_data_sheet`
- `safety_data_sheet`
- `sales_brochure`
- `company_profile`
