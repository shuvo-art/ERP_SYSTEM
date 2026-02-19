# Product Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the **Product Microservice**, covering Product Catalog CRUD operations and Master Data management.

**Base URL (via Gateway)**: `http://localhost:8082/api/v1`
**Base URL (Direct)**: `http://localhost:8083/api/v1`

> **Note**: Replace `your_admin_token` with a valid JWT Bearer token obtained from the Auth API login endpoint. All `Admin Only` endpoints require the `Admin` role.

---

## Table of Contents

- [📦 Product Catalog](#-product-catalog)
  - [1. Get All Products](#1-get-all-products-with-filters--pagination)
  - [2. Get Product by ID](#2-get-product-by-id)
  - [3. Get Product by Slug](#3-get-product-by-slug)
  - [4. Create Product](#4-create-product)
  - [5. Update Product (Full Replace)](#5-update-product-full-replace)
  - [6. Patch Product (Partial Update)](#6-patch-product-partial-update)
  - [7. Delete Product](#7-delete-product)
- [🏗️ Master Data Management](#%EF%B8%8F-master-data-management-admin-only)
  - [🏢 Brands](#-brands)
  - [📂 Categories](#-categories)
  - [📂 Sub-Categories](#-sub-categories)
  - [📏 Units](#-units)
  - [🌍 Countries](#-countries)
- [📄 Data Structures](#-data-structures)

---

## 📦 Product Catalog

### 1. Get All Products (with Filters & Pagination)

Retrieves a paginated list of products with optional filtering.

- **Endpoint**: `GET /products`
- **Access**: Public (No Auth Required)
- **Query Parameters**:

| Parameter    | Type   | Default | Description                              |
|-------------|--------|---------|------------------------------------------|
| `categoryId` | int    | –       | Filter by category ID                    |
| `brandId`    | int    | –       | Filter by brand ID                       |
| `search`     | string | –       | Search by product name or description    |
| `page`       | int    | 1       | Page number                              |
| `pageSize`   | int    | 10      | Number of records per page               |

**Basic — Get first page:**
```bash
curl -X GET "http://localhost:8083/api/v1/products"
```

**With Filters & Pagination:**
```bash
curl -X GET "http://localhost:8083/api/v1/products?categoryId=1&brandId=2&search=coating&page=1&pageSize=10"
```

**Filter by Category Only:**
```bash
curl -X GET "http://localhost:8083/api/v1/products?categoryId=1"
```

**Search Products:**
```bash
curl -X GET "http://localhost:8083/api/v1/products?search=CharCoat&page=1&pageSize=20"
```

**Response** (`200 OK`):
```json
{
  "data": [
    {
      "id": 1,
      "name": "CharCoat CTI 300",
      "slug": "charcoat-cti-300",
      "shortDescription": "High solids thin-film insulating coating.",
      "mainImage": "https://res.cloudinary.com/.../products/main/img.jpg",
      "categoryId": 1,
      "subCategoryId": 1,
      "brandId": 1,
      "unitId": 1,
      "countryId": 1,
      "categoryName": "Coatings",
      "subCategoryName": "Insulating",
      "brandName": "Denka",
      "unitName": "KG",
      "countryName": "Japan",
      "overviewHtml": "<h3>Overview</h3><p>Detailed info...</p>",
      "advantageHtml": "<ul><li>Bonds to porous surfaces</li></ul>",
      "applicationRangeHtml": "<h3>Application</h3><p>...</p>",
      "precautionHtml": "<h3>Precaution</h3><p>...</p>",
      "specifications": {
        "packSizes": [],
        "packagingDetails": [],
        "colors": [],
        "thicknesses": [],
        "densities": [],
        "appearances": [],
        "dosageCoverages": [],
        "shelfLife": []
      },
      "relatedImages": [],
      "technicalDataSheets": [],
      "safetyDataSheets": [],
      "certificates": [],
      "createdAt": "2026-02-19T06:00:00Z",
      "updatedAt": null
    }
  ],
  "total": 45,
  "page": 1,
  "pageSize": 10
}
```

---

### 2. Get Product by ID

Retrieves a single product with all its details including specifications, documents, and images.

- **Endpoint**: `GET /products/{id}`
- **Access**: Public

```bash
curl -X GET http://localhost:8083/api/v1/products/1
```

**Response** (`200 OK`):
```json
{
  "id": 1,
  "name": "CharCoat CTI 300",
  "slug": "charcoat-cti-300",
  "shortDescription": "High solids thin-film insulating coating.",
  "mainImage": "https://res.cloudinary.com/.../products/main/img.jpg",
  "categoryId": 1,
  "subCategoryId": 1,
  "brandId": 1,
  "unitId": 1,
  "countryId": 1,
  "categoryName": "Coatings",
  "subCategoryName": "Insulating",
  "brandName": "Denka",
  "unitName": "KG",
  "countryName": "Japan",
  "overviewHtml": "<h3>Overview</h3><p>Detailed info...</p>",
  "advantageHtml": "<ul><li>Bonds to porous surfaces</li></ul>",
  "applicationRangeHtml": "<h3>Application</h3><p>...</p>",
  "precautionHtml": "<h3>Precaution</h3><p>...</p>",
  "specifications": {
    "packSizes": ["20 Liters Pail", "12 Liters Pail"],
    "packagingDetails": [],
    "colors": ["Gray", "Black"],
    "thicknesses": ["1 m2/L at 1 mm thickness"],
    "densities": [],
    "appearances": [],
    "dosageCoverages": [],
    "shelfLife": []
  },
  "relatedImages": [
    "https://res.cloudinary.com/.../products/gallery/img1.jpg",
    "https://res.cloudinary.com/.../products/gallery/img2.jpg"
  ],
  "technicalDataSheets": [
    { "name": "Technical Specification V12", "url": "https://res.cloudinary.com/.../tds/doc.pdf" }
  ],
  "safetyDataSheets": [
    { "name": "SafetyDataSheet V12", "url": "https://res.cloudinary.com/.../sds/doc.pdf" }
  ],
  "certificates": [
    { "name": "CertificateName V12", "url": "https://res.cloudinary.com/.../certificates/doc.pdf" }
  ],
  "createdAt": "2026-02-19T06:00:00Z",
  "updatedAt": null
}
```

**Error Response** (`404 Not Found`):
```json
(empty body)
```

---

### 3. Get Product by Slug

Retrieves a product by its SEO-friendly slug URL.

- **Endpoint**: `GET /products/slug/{slug}`
- **Access**: Public

```bash
curl -X GET http://localhost:8083/api/v1/products/slug/charcoat-cti-300
```

**Response**: Same structure as [Get Product by ID](#2-get-product-by-id).

---

### 4. Create Product

Creates a new product with all associated data: images, documents, specifications, and rich-text HTML content.

- **Endpoint**: `POST /products`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

#### Form Fields Reference

| Field                          | Type            | Required | Description                                           |
|-------------------------------|-----------------|----------|-------------------------------------------------------|
| `Name`                         | string          | ✅ Yes   | Product name                                          |
| `ShortDescription`             | string          | No       | Brief product description                             |
| `CategoryId`                   | int             | No       | ID from CategoryMaster                                |
| `SubCategoryId`                | int             | No       | ID from SubCategoryMaster                             |
| `BrandId`                      | int             | No       | ID from BrandMaster                                   |
| `UnitId`                       | int             | No       | ID from UnitMaster                                    |
| `CountryId`                    | int             | No       | ID from CountryMaster                                 |
| `OverviewHtml`                 | string (HTML)   | No       | Rich-text overview content                            |
| `AdvantageHtml`                | string (HTML)   | No       | Rich-text advantages content                          |
| `ApplicationRangeHtml`         | string (HTML)   | No       | Rich-text application range content                   |
| `PrecautionHtml`               | string (HTML)   | No       | Rich-text precaution content                          |
| `SpecificationsJson`           | string (JSON)   | No       | Structured specifications (see [Data Structures](#-data-structures)) |
| `MainImageFile`                | file (image)    | No       | Main product image (JPG/PNG)                          |
| `RelatedImageFiles`            | file[] (images) | No       | Additional gallery images (multiple allowed)          |
| `TechnicalDataSheetFiles`      | file[] (PDF)    | No       | Technical Data Sheet documents                        |
| `TechnicalDataSheetNamesJson`  | string          | No       | Custom name(s) for TDS — plain string or JSON array   |
| `SafetyDataSheetFiles`         | file[] (PDF)    | No       | Safety Data Sheet documents                           |
| `SafetyDataSheetNamesJson`     | string          | No       | Custom name(s) for SDS — plain string or JSON array   |
| `CertificateFiles`             | file[] (PDF)    | No       | Product certificate documents                         |
| `CertificateNamesJson`         | string          | No       | Custom name(s) for certificates — plain string or JSON array |

#### Full Example (All Fields)

```bash
curl -X POST http://localhost:8083/api/v1/products \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "Name=CharCoat CTI 300" \
  -F "ShortDescription=High solids thin-film insulating coating." \
  -F "CategoryId=1" \
  -F "SubCategoryId=1" \
  -F "BrandId=1" \
  -F "UnitId=1" \
  -F "CountryId=1" \
  -F "OverviewHtml=<h3>Product Overview</h3><p>Detailed info...</p>" \
  -F "AdvantageHtml=<h3>Advantages</h3><ul><li>Bonds to porous surfaces</li></ul>" \
  -F "ApplicationRangeHtml=<h3>Application Range</h3><p>Suitable for concrete, steel...</p>" \
  -F "PrecautionHtml=<h3>Precautions</h3><p>Use in well-ventilated areas.</p>" \
  -F 'SpecificationsJson={"PackSizes":["20 Liters Pail","12 Liters Pail"],"Colors":["Gray","Black"],"Thicknesses":["1 m2/L at 1 mm thickness"]}' \
  -F "MainImageFile=@/path/to/main-image.jpg;type=image/jpeg" \
  -F "RelatedImageFiles=@/path/to/gallery1.jpg;type=image/jpeg" \
  -F "RelatedImageFiles=@/path/to/gallery2.jpg;type=image/jpeg" \
  -F "TechnicalDataSheetFiles=@/path/to/tds-document.pdf;type=application/pdf" \
  -F "TechnicalDataSheetNamesJson=Technical Specification V12" \
  -F "SafetyDataSheetFiles=@/path/to/sds-document.pdf;type=application/pdf" \
  -F "SafetyDataSheetNamesJson=SafetyDataSheet V12" \
  -F "CertificateFiles=@/path/to/certificate.pdf;type=application/pdf" \
  -F "CertificateNamesJson=ISO 9001 Certificate"
```

#### Minimal Example (Required Fields Only)

```bash
curl -X POST http://localhost:8083/api/v1/products \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Basic Product"
```

#### Multiple Documents with Multiple Names

When uploading multiple files for the same document type, provide names as a JSON array:

```bash
curl -X POST http://localhost:8083/api/v1/products \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Multi-Doc Product" \
  -F "TechnicalDataSheetFiles=@/path/to/tds1.pdf;type=application/pdf" \
  -F "TechnicalDataSheetFiles=@/path/to/tds2.pdf;type=application/pdf" \
  -F 'TechnicalDataSheetNamesJson=["TDS Part 1","TDS Part 2"]'
```

> **Document Name Resolution Logic:**
> 1. If a specific name exists for the file's index → use that name
> 2. If only ONE name is provided but MULTIPLE files → auto-appends index (e.g., "Name (1)", "Name (2)")
> 3. If no custom name → falls back to the original file name

**Response** (`201 Created`):
```json
{
  "id": 5,
  "name": "CharCoat CTI 300",
  "slug": "charcoat-cti-300",
  "shortDescription": "High solids thin-film insulating coating.",
  "mainImage": "https://res.cloudinary.com/.../products/main/img.jpg",
  "categoryId": 1,
  "specifications": {
    "packSizes": ["20 Liters Pail", "12 Liters Pail"],
    "colors": ["Gray", "Black"],
    "thicknesses": ["1 m2/L at 1 mm thickness"]
  },
  "technicalDataSheets": [
    { "name": "Technical Specification V12", "url": "https://..." }
  ],
  "..."
}
```

---

### 5. Update Product (Full Replace)

Replaces all fields of an existing product. Files are **replaced** if new ones are provided (old files are deleted from Cloudinary).

- **Endpoint**: `PUT /products/{id}`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

> **Behavior**: All text fields are overwritten. For files:
> - `MainImageFile` → Replaces existing main image (old one deleted)
> - `RelatedImageFiles` → **Replaces all** existing gallery images if provided
> - `TechnicalDataSheetFiles` → **Replaces all** existing TDS if provided
> - `SafetyDataSheetFiles` → **Replaces all** existing SDS if provided
> - `CertificateFiles` → **Replaces all** existing certificates if provided

```bash
curl -X PUT http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "Name=CharCoat CTI 300 Plus (Updated)" \
  -F "ShortDescription=Updated high-performance ceramic beaded coating." \
  -F "CategoryId=1" \
  -F "SubCategoryId=1" \
  -F "BrandId=1" \
  -F "UnitId=1" \
  -F "CountryId=1" \
  -F "OverviewHtml=<h3>Updated Overview</h3><p>New detailed info...</p>" \
  -F "AdvantageHtml=<h3>Updated Advantages</h3><ul><li>Better bonding</li></ul>" \
  -F "ApplicationRangeHtml=<h3>Updated Application</h3><p>All surfaces...</p>" \
  -F "PrecautionHtml=<h3>Updated Precautions</h3><p>Safety first.</p>" \
  -F 'SpecificationsJson={"PackSizes":["25 Liters Pail"],"Colors":["White","Gray"],"Thicknesses":["2 m2/L at 1 mm thickness"]}' \
  -F "MainImageFile=@/path/to/new-main-image.jpg;type=image/jpeg" \
  -F "RelatedImageFiles=@/path/to/new-gallery1.jpg;type=image/jpeg" \
  -F "TechnicalDataSheetFiles=@/path/to/new-tds.pdf;type=application/pdf" \
  -F "TechnicalDataSheetNamesJson=Updated TDS V2" \
  -F "SafetyDataSheetFiles=@/path/to/new-sds.pdf;type=application/pdf" \
  -F "SafetyDataSheetNamesJson=Updated SDS V2" \
  -F "CertificateFiles=@/path/to/new-cert.pdf;type=application/pdf" \
  -F "CertificateNamesJson=Updated Certificate"
```

#### Update Text Only (No File Changes)

```bash
curl -X PUT http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=CharCoat CTI 300 Plus" \
  -F "ShortDescription=Updated description only" \
  -F "CategoryId=1" \
  -F "SubCategoryId=1" \
  -F "BrandId=1" \
  -F "UnitId=1" \
  -F "CountryId=1" \
  -F "OverviewHtml=<h3>Overview</h3><p>Updated content</p>" \
  -F "AdvantageHtml=<h3>Advantages</h3><p>Updated</p>" \
  -F "ApplicationRangeHtml=<h3>Application</h3><p>Updated</p>" \
  -F "PrecautionHtml=<h3>Precaution</h3><p>Updated</p>"
```

**Response** (`200 OK`): Returns the full updated product object.

---

### 6. Patch Product (Partial Update)

Partially updates a product. Only the provided fields are modified; all others remain unchanged.

- **Endpoint**: `PATCH /products/{id}`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

> **Behavior Differences from PUT**:
> - **Text fields**: Only provided fields are changed
> - **Specifications**: Uses **merge** behavior — new values are **unioned** with existing values (no duplicates)
> - **MainImageFile**: Replaces existing (old one deleted)
> - **RelatedImageFiles**: **Adds** to existing gallery (additive, not replace)
> - **Document Files**: **Adds** to existing documents (additive, not replace)

#### Patch Name & Description Only

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "Name=CharCoat CTI 300 Premium" \
  -F "ShortDescription=Updated short description only"
```

#### Patch Category & Brand

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "CategoryId=2" \
  -F "BrandId=3"
```

#### Patch Specifications (Merge Behavior)

Existing specs are merged with new ones. For example, if the product already has `Colors: ["Gray"]` and you patch with `Colors: ["White"]`, the result will be `Colors: ["Gray", "White"]`.

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F 'SpecificationsJson={"Colors":["White","Red"],"PackSizes":["50 Liters Drum"]}'
```

#### Patch — Add More Gallery Images (Additive)

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "RelatedImageFiles=@/path/to/extra-image.jpg;type=image/jpeg"
```

#### Patch — Add More Documents (Additive)

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "TechnicalDataSheetFiles=@/path/to/new-tds.pdf;type=application/pdf" \
  -F "TechnicalDataSheetNamesJson=TDS Addendum V3" \
  -F "CertificateFiles=@/path/to/new-cert.pdf;type=application/pdf" \
  -F "CertificateNamesJson=ISO 14001 Certificate"
```

#### Patch — Replace Main Image Only

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "MainImageFile=@/path/to/new-main-image.jpg;type=image/jpeg"
```

#### Patch — Full Example (Multiple Fields)

```bash
curl -X PATCH http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: multipart/form-data" \
  -F "Name=CharCoat CTI 300 Pro" \
  -F "ShortDescription=Patched description" \
  -F "CategoryId=2" \
  -F "OverviewHtml=<h3>Patched Overview</h3><p>New info...</p>" \
  -F 'SpecificationsJson={"Colors":["Blue"],"Densities":["1.8 g/cm3"]}' \
  -F "MainImageFile=@/path/to/patched-main.jpg;type=image/jpeg" \
  -F "RelatedImageFiles=@/path/to/patched-extra.jpg;type=image/jpeg" \
  -F "SafetyDataSheetFiles=@/path/to/patched-sds.pdf;type=application/pdf" \
  -F "SafetyDataSheetNamesJson=Patched SDS"
```

**Response** (`200 OK`): Returns the full updated product object.

---

### 7. Delete Product

Deletes a product and all its associated Cloudinary assets (main image, gallery images, TDS, SDS, certificates).

- **Endpoint**: `DELETE /products/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/products/1 \
  -H "Authorization: Bearer your_admin_token"
```

**Response** (`204 No Content`): Empty body on success.

**Error Response** (`404 Not Found`): If product doesn't exist.

---

## 🏗️ Master Data Management (Admin Only)

These endpoints manage the lookup entities used to categorize and label products.

---

### 🏢 Brands

Manage product brand names and logos.

**Route**: `/api/v1/brands`

#### 1. Get All Brands

- **Endpoint**: `GET /brands`
- **Access**: Public
- **Query Parameters**: `search` (string), `id` (int), `slug` (string)

```bash
# Get all brands
curl -X GET http://localhost:8083/api/v1/brands

# Search brands
curl -X GET "http://localhost:8083/api/v1/brands?search=Denka"

# Get brand by ID
curl -X GET "http://localhost:8083/api/v1/brands?id=1"

# Get brand by slug
curl -X GET "http://localhost:8083/api/v1/brands?slug=denka"
```

#### 2. Create Brand

- **Endpoint**: `POST /brands`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

```bash
curl -X POST http://localhost:8083/api/v1/brands \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Denka" \
  -F "Logo=@/path/to/denka_logo.png;type=image/png"
```

#### 3. Update Brand

- **Endpoint**: `PUT /brands/{id}`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

```bash
# Update name and logo
curl -X PUT http://localhost:8083/api/v1/brands/1 \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Denka International" \
  -F "Logo=@/path/to/new_logo.png;type=image/png"

# Update name only (keep existing logo)
curl -X PUT http://localhost:8083/api/v1/brands/1 \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Denka Updated"
```

#### 4. Delete Brand

- **Endpoint**: `DELETE /brands/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/brands/1 \
  -H "Authorization: Bearer your_admin_token"
```

---

### 📂 Categories

Manage top-level product categories with optional background images.

**Route**: `/api/v1/categories`

#### 1. Get All Categories

- **Endpoint**: `GET /categories`
- **Access**: Public
- **Query Parameters**: `search` (string), `id` (int), `slug` (string)

```bash
# Get all categories
curl -X GET http://localhost:8083/api/v1/categories

# Search categories
curl -X GET "http://localhost:8083/api/v1/categories?search=Concrete"

# Get category by ID
curl -X GET "http://localhost:8083/api/v1/categories?id=1"

# Get category by slug
curl -X GET "http://localhost:8083/api/v1/categories?slug=concrete-additives"
```

#### 2. Create Category

- **Endpoint**: `POST /categories`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

```bash
curl -X POST http://localhost:8083/api/v1/categories \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Concrete Additives" \
  -F "Image=@/path/to/category_bg.jpg;type=image/jpeg"
```

#### 3. Update Category

- **Endpoint**: `PUT /categories/{id}`
- **Access**: Admin Only
- **Content-Type**: `multipart/form-data`

```bash
# Update name and image
curl -X PUT http://localhost:8083/api/v1/categories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Waterproofing Solutions" \
  -F "Image=@/path/to/new_bg.jpg;type=image/jpeg"

# Update name only (keep existing image)
curl -X PUT http://localhost:8083/api/v1/categories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -F "Name=Waterproofing Solutions Updated"
```

#### 4. Delete Category

- **Endpoint**: `DELETE /categories/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/categories/1 \
  -H "Authorization: Bearer your_admin_token"
```

---

### 📂 Sub-Categories

Manage sub-categories with many-to-many relationship to categories.

**Route**: `/api/v1/subcategories`

#### 1. Get All Sub-Categories

- **Endpoint**: `GET /subcategories`
- **Access**: Public
- **Query Parameters**: `search` (string), `id` (int), `slug` (string)

```bash
# Get all sub-categories
curl -X GET http://localhost:8083/api/v1/subcategories

# Search sub-categories
curl -X GET "http://localhost:8083/api/v1/subcategories?search=Quick"

# Get by ID
curl -X GET "http://localhost:8083/api/v1/subcategories?id=1"

# Get by slug
curl -X GET "http://localhost:8083/api/v1/subcategories?slug=quick-hardening"
```

#### 2. Create Sub-Category

- **Endpoint**: `POST /subcategories`
- **Access**: Admin Only
- **Content-Type**: `application/json`

> **Note**: A sub-category can belong to **multiple categories** (many-to-many). Provide an array of `categoryIds`.

```bash
# Link to one category
curl -X POST http://localhost:8083/api/v1/subcategories \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryIds": [1],
    "name": "Quick Hardening"
  }'

# Link to multiple categories
curl -X POST http://localhost:8083/api/v1/subcategories \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryIds": [1, 2, 3],
    "name": "Multi-Purpose Additive"
  }'
```

#### 3. Update Sub-Category (Full Replace)

- **Endpoint**: `PUT /subcategories/{id}`
- **Access**: Admin Only
- **Content-Type**: `application/json`

```bash
curl -X PUT http://localhost:8083/api/v1/subcategories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryIds": [1, 2],
    "name": "Extra Quick Hardening"
  }'
```

#### 4. Patch Sub-Category (Partial Update)

- **Endpoint**: `PATCH /subcategories/{id}`
- **Access**: Admin Only
- **Content-Type**: `application/json`

> **Behavior**: `categoryIds` uses **additive merge** — new IDs are unioned with existing IDs (no duplicates removed).

```bash
# Patch name only
curl -X PATCH http://localhost:8083/api/v1/subcategories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"name": "Rapid Hardening"}'

# Add more category links (additive)
curl -X PATCH http://localhost:8083/api/v1/subcategories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"categoryIds": [4, 5]}'

# Patch both
curl -X PATCH http://localhost:8083/api/v1/subcategories/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ultra Rapid Hardening",
    "categoryIds": [3]
  }'
```

#### 5. Delete Sub-Category

- **Endpoint**: `DELETE /subcategories/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/subcategories/1 \
  -H "Authorization: Bearer your_admin_token"
```

---

### 📏 Units

Manage measurement units (e.g., KG, Liter, Pail).

**Route**: `/api/v1/units`

#### 1. Get All Units

- **Endpoint**: `GET /units`
- **Access**: Public
- **Query Parameters**: `search` (string), `id` (int)

```bash
# Get all units
curl -X GET http://localhost:8083/api/v1/units

# Search units
curl -X GET "http://localhost:8083/api/v1/units?search=KG"

# Get unit by ID
curl -X GET "http://localhost:8083/api/v1/units?id=1"
```

#### 2. Create Unit

- **Endpoint**: `POST /units`
- **Access**: Admin Only
- **Content-Type**: `application/json`

```bash
curl -X POST http://localhost:8083/api/v1/units \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"name": "KG"}'
```

#### 3. Update Unit

- **Endpoint**: `PUT /units/{id}`
- **Access**: Admin Only
- **Content-Type**: `application/json`

```bash
curl -X PUT http://localhost:8083/api/v1/units/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"name": "Liter"}'
```

#### 4. Delete Unit

- **Endpoint**: `DELETE /units/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/units/1 \
  -H "Authorization: Bearer your_admin_token"
```

---

### 🌍 Countries

Manage list of countries available for product origin.

**Route**: `/api/v1/countries`

#### 1. Get All Countries

- **Endpoint**: `GET /countries`
- **Access**: Public
- **Query Parameters**: `search` (string), `id` (int)

```bash
# Get all countries
curl -X GET http://localhost:8083/api/v1/countries

# Search countries
curl -X GET "http://localhost:8083/api/v1/countries?search=Japan"

# Get country by ID
curl -X GET "http://localhost:8083/api/v1/countries?id=1"
```

#### 2. Create Country

- **Endpoint**: `POST /countries`
- **Access**: Admin Only
- **Content-Type**: `application/json`

```bash
curl -X POST http://localhost:8083/api/v1/countries \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"name": "Japan"}'
```

#### 3. Update Country

- **Endpoint**: `PUT /countries/{id}`
- **Access**: Admin Only
- **Content-Type**: `application/json`

```bash
curl -X PUT http://localhost:8083/api/v1/countries/1 \
  -H "Authorization: Bearer your_admin_token" \
  -H "Content-Type: application/json" \
  -d '{"name": "Malaysia"}'
```

#### 4. Delete Country

- **Endpoint**: `DELETE /countries/{id}`
- **Access**: Admin Only

```bash
curl -X DELETE http://localhost:8083/api/v1/countries/1 \
  -H "Authorization: Bearer your_admin_token"
```

---

## 📄 Data Structures

### Product Entity

```json
{
  "id": 1,
  "name": "CharCoat CTI 300",
  "slug": "charcoat-cti-300",
  "shortDescription": "High solids thin-film insulating coating.",
  "mainImage": "https://res.cloudinary.com/.../img.jpg",
  "categoryId": 1,
  "subCategoryId": 1,
  "brandId": 1,
  "unitId": 1,
  "countryId": 1,
  "categoryName": "Coatings",
  "subCategoryName": "Insulating",
  "brandName": "Denka",
  "unitName": "KG",
  "countryName": "Japan",
  "overviewHtml": "<h3>Overview</h3><p>...</p>",
  "advantageHtml": "<ul><li>...</li></ul>",
  "applicationRangeHtml": "<h3>Application</h3><p>...</p>",
  "precautionHtml": "<h3>Precaution</h3><p>...</p>",
  "specifications": { "...see below..." },
  "relatedImages": ["https://...jpg", "https://...jpg"],
  "technicalDataSheets": [{ "name": "TDS V1", "url": "https://...pdf" }],
  "safetyDataSheets": [{ "name": "SDS V1", "url": "https://...pdf" }],
  "certificates": [{ "name": "ISO Cert", "url": "https://...pdf" }],
  "createdAt": "2026-02-19T06:00:00Z",
  "updatedAt": null
}
```

### Specifications JSON Object

All fields are arrays of strings. Provide only the fields you need:

```json
{
  "PackSizes": ["20 Liters Pail", "12 Liters Pail"],
  "PackagingDetails": ["Sealed container", "Palletized"],
  "Colors": ["Gray", "Black", "White"],
  "Thicknesses": ["1 m2/L at 1 mm thickness"],
  "Densities": ["1.5 g/cm³"],
  "Appearances": ["Liquid", "Paste"],
  "DosageCoverages": ["1 kg/m²", "2 kg/m²"],
  "ShelfLife": ["12 Months", "24 Months"]
}
```

### Product Document Object

```json
{
  "name": "Technical Specification V12",
  "url": "https://res.cloudinary.com/.../products/documents/tds/doc.pdf"
}
```

### Document Name Formats

Custom names for documents can be provided in two formats:

**Single name (plain string):**
```
-F "TechnicalDataSheetNamesJson=My Custom TDS Name"
```

**Multiple names (JSON array):**
```
-F 'TechnicalDataSheetNamesJson=["TDS Part 1","TDS Part 2"]'
```

---

## ⚠️ Error Responses

All endpoints return consistent error responses:

| Status Code | Description                    | Example Body                                           |
|------------|--------------------------------|--------------------------------------------------------|
| `400`       | Bad Request / Validation Error | `{ "message": "Invalid JSON format in SpecificationsJson" }` |
| `401`       | Unauthorized                   | Challenge response (no valid token)                    |
| `403`       | Forbidden                      | User lacks `Admin` role                                |
| `404`       | Not Found                      | Empty body                                             |
| `500`       | Internal Server Error          | `{ "message": "Error creating product", "details": "..." }` |

---

## 🔑 Authentication

All `Admin Only` endpoints require a valid JWT token in the `Authorization` header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

To obtain a token, use the Auth API login endpoint:

```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@erpsystem.com", "password": "your_password"}'
```

The response will include an `accessToken` to use as the Bearer token.

---

## 🔗 Quick Reference

| Action                  | Method   | Endpoint                    | Auth   | Content-Type         |
|------------------------|----------|-----------------------------|--------|----------------------|
| Get All Products        | `GET`    | `/products`                 | No     | –                    |
| Get Product by ID       | `GET`    | `/products/{id}`            | No     | –                    |
| Get Product by Slug     | `GET`    | `/products/slug/{slug}`     | No     | –                    |
| Create Product          | `POST`   | `/products`                 | Admin  | `multipart/form-data`|
| Update Product (Full)   | `PUT`    | `/products/{id}`            | Admin  | `multipart/form-data`|
| Patch Product (Partial) | `PATCH`  | `/products/{id}`            | Admin  | `multipart/form-data`|
| Delete Product          | `DELETE` | `/products/{id}`            | Admin  | –                    |
| Get Brands              | `GET`    | `/brands`                   | No     | –                    |
| Create Brand            | `POST`   | `/brands`                   | Admin  | `multipart/form-data`|
| Update Brand            | `PUT`    | `/brands/{id}`              | Admin  | `multipart/form-data`|
| Delete Brand            | `DELETE` | `/brands/{id}`              | Admin  | –                    |
| Get Categories          | `GET`    | `/categories`               | No     | –                    |
| Create Category         | `POST`   | `/categories`               | Admin  | `multipart/form-data`|
| Update Category         | `PUT`    | `/categories/{id}`          | Admin  | `multipart/form-data`|
| Delete Category         | `DELETE` | `/categories/{id}`          | Admin  | –                    |
| Get Sub-Categories      | `GET`    | `/subcategories`            | No     | –                    |
| Create Sub-Category     | `POST`   | `/subcategories`            | Admin  | `application/json`   |
| Update Sub-Category     | `PUT`    | `/subcategories/{id}`       | Admin  | `application/json`   |
| Patch Sub-Category      | `PATCH`  | `/subcategories/{id}`       | Admin  | `application/json`   |
| Delete Sub-Category     | `DELETE` | `/subcategories/{id}`       | Admin  | –                    |
| Get Units               | `GET`    | `/units`                    | No     | –                    |
| Create Unit             | `POST`   | `/units`                    | Admin  | `application/json`   |
| Update Unit             | `PUT`    | `/units/{id}`               | Admin  | `application/json`   |
| Delete Unit             | `DELETE` | `/units/{id}`               | Admin  | –                    |
| Get Countries           | `GET`    | `/countries`                | No     | –                    |
| Create Country          | `POST`   | `/countries`                | Admin  | `application/json`   |
| Update Country          | `PUT`    | `/countries/{id}`           | Admin  | `application/json`   |
| Delete Country          | `DELETE` | `/countries/{id}`           | Admin  | –                    |
