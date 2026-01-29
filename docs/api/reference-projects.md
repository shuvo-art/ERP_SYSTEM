# Reference Project Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Reference Project Microservice, showcasing completed engineering and ERP implementation projects.

**Base URL**: `http://localhost:8082/api/v1/reference-projects` (via Gateway)

---

## 🏗️ Project Showcase (Public)

### 1. List Projects
Retrieves a paginated list of projects with filtering options.

**Endpoint**: `GET /`

```bash
curl -X GET "http://localhost:8082/api/v1/reference-projects?page=1&limit=10&featured=true"
```

### 2. Get Project Details
Retrieves full details for a project using its ID or Slug.

**Endpoint**: `GET /{idOrSlug}`

```bash
curl -X GET http://localhost:8082/api/v1/reference-projects/smart-warehouse-implementation
```

---

## 🛠️ Project Management (Admin Only)

**Auth Required**: `Bearer <admin_token>`

### 3. Create Project
Adds a new reference project with hero images and gallery. This endpoint uses `multipart/form-data`.

**Endpoint**: `POST /`

```bash
curl -X POST http://localhost:8082/api/v1/reference-projects \
-H "Authorization: Bearer your_admin_token" \
-F "ProjectName=Integrated Logistics Hub" \
-F "ShortDescription=Full ERP integration for a multi-national logistics company." \
-F "Location=Singapore" \
-F "Status=Completed" \
-F "StartDate=2023-01-01" \
-F "CompletionDate=2024-01-01" \
-F "Featured=true" \
-F "HeroImage=@/path/to/hero.jpg" \
-F "GalleryImages=@/path/to/img1.jpg" \
-F "GalleryImages=@/path/to/img2.jpg" \
-F "ProjectOverviewJson={\"challenge\": \"Disorganize inventory\", \"solution\": \"AI-driven tracking\"}" \
-F "ProductsUsedJson=[{\"name\": \"IMS 3000\", \"category\": \"Inventory\"}]"
```

### 4. Update Project
Updates project details and assets. Uses `multipart/form-data`.

**Endpoint**: `PUT /{id}`

```bash
curl -X PUT http://localhost:8082/api/v1/reference-projects/123 \
-H "Authorization: Bearer your_admin_token" \
-F "Status=In Expansion"
```

### 5. Delete Project
Removes a project and its associated cloud assets.

**Endpoint**: `DELETE /{id}`

```bash
curl -X DELETE http://localhost:8082/api/v1/reference-projects/123 \
-H "Authorization: Bearer your_admin_token"
```

---

## 🗃️ Data Structures

### Project Overview JSON
```json
{
  "challenge": "What problem was the client facing?",
  "solution": "How our ERP system solved it.",
  "results": "Quantifiable metrics of success."
}
```

### Products Used JSON
```json
[
  {
    "name": "Product Name",
    "category": "Product Category"
  }
]
```
