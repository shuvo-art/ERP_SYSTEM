# Product Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Product Microservice, including Catalog Management and Master Data.

**Base URL**: `http://localhost:8082/api/v1` (via Gateway) or `http://localhost:8083/api/v1` (Direct)

---

## 🏗️ Master Data Management (Admin Only)
These endpoints manage the entities used to categorize and label products.

### 🏢 Brands
Manage product brand names and logos.

**1. Get All Brands**
- **Endpoint**: `GET /brands`
- **Access**: Public
```bash
curl -X GET http://localhost:8082/api/v1/brands
```

**2. Create Brand**
- **Endpoint**: `POST /brands`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`
```bash
curl -X POST http://localhost:8082/api/v1/brands \
-H "Authorization: Bearer your_admin_token" \
-F "name=Denka" \
-F "logo=@/path/to/denka_logo.png"
```

**3. Update Brand**
- **Endpoint**: `PUT /brands/{id}`
- **Access**: Admin Only
```bash
curl -X PUT http://localhost:8082/api/v1/brands/1 \
-H "Authorization: Bearer your_admin_token" \
-F "name=Updated Denka"
```

**4. Delete Brand**
- **Endpoint**: `DELETE /brands/{id}`
- **Access**: Admin Only
```bash
curl -X DELETE http://localhost:8082/api/v1/brands/1 \
-H "Authorization: Bearer your_admin_token"
```

### 📂 Categories
Manage top-level product categories with optional background images.

**1. Get All Categories**
- **Endpoint**: `GET /categories`
- **Access**: Public
```bash
curl -X GET http://localhost:8082/api/v1/categories
```

**2. Create Category**
- **Endpoint**: `POST /categories`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`
```bash
curl -X POST http://localhost:8082/api/v1/categories \
-H "Authorization: Bearer your_admin_token" \
-F "name=Concrete Additives" \
-F "image=@/path/to/category_bg.jpg"
```

**3. Update Category**
- **Endpoint**: `PUT /categories/{id}`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`
```bash
curl -X PUT http://localhost:8082/api/v1/categories/1 \
-H "Authorization: Bearer your_admin_token" \
-F "name=Waterproofing Solutions" \
-F "image=@/path/to/new_bg.jpg"
```

**4. Delete Category**
- **Endpoint**: `DELETE /categories/{id}`
- **Access**: Admin Only
```bash
curl -X DELETE http://localhost:8082/api/v1/categories/1 \
-H "Authorization: Bearer your_admin_token"
```

### 📂 Sub-Categories
Manage sub-categories linked to a parent category.

**1. Get All Sub-Categories**
- **Endpoint**: `GET /subcategories`
- **Access**: Public
```bash
curl -X GET http://localhost:8082/api/v1/subcategories
```

**2. Create Sub-Category**
- **Endpoint**: `POST /subcategories`
- **Access**: Admin Only
- **Body (JSON)**:
```json
{
  "categoryId": 1,
  "name": "Quick Hardening"
}
```
```bash
curl -X POST http://localhost:8082/api/v1/subcategories \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"categoryId": 1, "name": "Quick Hardening"}'
```

**3. Update Sub-Category**
- **Endpoint**: `PUT /subcategories/{id}`
- **Access**: Admin Only
```bash
curl -X PUT http://localhost:8082/api/v1/subcategories/1 \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"categoryId": 1, "name": "Extra Quick Hardening"}'
```

**4. Delete Sub-Category**
- **Endpoint**: `DELETE /subcategories/{id}`
- **Access**: Admin Only
```bash
curl -X DELETE http://localhost:8082/api/v1/subcategories/1 \
-H "Authorization: Bearer your_admin_token"
```

### 🌍 Countries
Manage list of countries available for product origin.

**1. Get All Countries**
- **Endpoint**: `GET /countries`
- **Access**: Public
```bash
curl -X GET http://localhost:8082/api/v1/countries
```

**2. Create Country**
- **Endpoint**: `POST /countries`
- **Access**: Admin Only
```bash
curl -X POST http://localhost:8082/api/v1/countries \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"name": "Japan"}'
```

**3. Update Country**
- **Endpoint**: `PUT /countries/{id}`
- **Access**: Admin Only
```bash
curl -X PUT http://localhost:8082/api/v1/countries/1 \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"name": "Malaysia"}'
```

**4. Delete Country**
- **Endpoint**: `DELETE /countries/{id}`
- **Access**: Admin Only
```bash
curl -X DELETE http://localhost:8082/api/v1/countries/1 \
-H "Authorization: Bearer your_admin_token"
```

### 🌍 Units
Manage measurement units (e.g., KG, Liter, Pail).

**1. Get All Units**
- **Endpoint**: `GET /units`
- **Access**: Public
```bash
curl -X GET http://localhost:8082/api/v1/units
```

**2. Create Unit**
- **Endpoint**: `POST /units`
- **Access**: Admin Only
```bash
curl -X POST http://localhost:8082/api/v1/units \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"name": "KG"}'
```

**3. Update Unit**
- **Endpoint**: `PUT /units/{id}`
- **Access**: Admin Only
```bash
curl -X PUT http://localhost:8082/api/v1/units/1 \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{"name": "Liter"}'
```

**4. Delete Unit**
- **Endpoint**: `DELETE /units/{id}`
- **Access**: Admin Only
```bash
curl -X DELETE http://localhost:8082/api/v1/units/1 \
-H "Authorization: Bearer your_admin_token"
```

---

## 📦 Product Catalog

### 1. Get All Products (with Filters & Pagination)
Retrieves a list of products matching the provided criteria.

**Endpoint**: `GET /products`
**Parameters**:
- `categoryId` (int, optional): Filter by category.
- `brandId` (int, optional): Filter by brand.
- `search` (string, optional): Search by name or description.
- `page` (int, default 1): Page number.
- `pageSize` (int, default 10): Records per page.

```bash
curl -X GET "http://localhost:8082/api/v1/products?categoryId=1&brandId=2&search=sealant&page=1&pageSize=10"
```

**Response**:
```json
{
  "total": 45,
  "page": 1,
  "pageSize": 10,
  "data": [ ... ]
}
```

### 2. Get Product by ID
**Endpoint**: `GET /products/{id}`
```bash
curl -X GET http://localhost:8082/api/v1/products/1
```

---

## 🛠️ Product Admin (Admin Only)

### 3. Create Product
Uses `multipart/form-data` to handle rich text, structured JSON, and multiple file uploads.

**Endpoint**: `POST /products`
**Auth Required**: Admin Only

```bash
curl -X POST http://localhost:8082/api/v1/products \
-H "Authorization: Bearer your_admin_token" \
-F "Name=CharCoat CTI 300" \
-F "ShortDescription=High-performance ceramic beaded coating." \
-F "CategoryId=1" \
-F "SubCategoryId=2" \
-F "BrandId=1" \
-F "UnitId=1" \
-F "CountryId=1" \
-F "OverviewHtml=<h3>Overview</h3><p>Detailed info...</p>" \
-F "AdvantageHtml=<ul><li>Bonds to porous surfaces</li></ul>" \
-F "SpecificationsJson={\"PackSizes\":[\"20kg bag\"],\"Colors\":[\"White\"]}" \
-F "MainImageFile=@/path/to/img.jpg" \
-F "RelatedImageFiles=@/path/to/extra1.jpg" \
-F "RelatedImageFiles=@/path/to/extra2.jpg" \
-F "TechnicalDataSheetFiles=@/path/to/tds.pdf" \
-F "SafetyDataSheetFiles=@/path/to/sds.pdf" \
-F "CertificateFiles=@/path/to/cert.pdf"
```

### 4. Update Product
Updates an existing product's details and assets. Uses `multipart/form-data`. New files provided will be appended or replace existing main image.

**Endpoint**: `PUT /products/{id}`
**Auth Required**: Admin Only

```bash
curl -X PUT http://localhost:8082/api/v1/products/1 \
-H "Authorization: Bearer your_admin_token" \
-F "Name=Updated CharCoat CTI 300 Plus" \
-F "ShortDescription=Updated description"
```

### 5. Delete Product
Removes a product and all its associated assets (images and PDFs) from Cloudinary.

**Endpoint**: `DELETE /products/{id}`
**Auth Required**: Admin Only

```bash
curl -X DELETE http://localhost:8082/api/v1/products/1 \
-H "Authorization: Bearer your_admin_token"
```

---

## 📄 Data Structures

### Specifications JSON Object
```json
{
  "PackSizes": ["20kg", "50kg"],
  "Colors": ["Gray", "White"],
  "Thicknesses": ["1mm", "2mm"],
  "Densities": ["1.5 g/cm3"],
  "Appearances": ["Powder"],
  "DosageCoverages": ["1kg/m2"],
  "ShelfLife": ["12 Months"]
}
```
