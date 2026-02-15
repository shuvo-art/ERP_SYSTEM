# Reference Project Microservice API Documentation

This document provides detailed API specifications, `curl` examples, and data structures for the Reference Project Microservice. This service manages architectural categories, project portfolios, and relationships between projects and products.

**Base URL**: `http://localhost:8086/api/v1` (Direct) or `http://localhost/api/v1` (via Gateway)

---

## 📂 Category Management

Categories must be created before projects, as every project requires an existing category.

### 1. List Categories
Retrieves all available project categories.

**Endpoint**: `GET /categories`

```bash
curl -X GET "http://localhost:8086/api/v1/categories"
```

### 2. Create Category (Admin Only)
Adds a new category with an optional image upload.

**Endpoint**: `POST /categories`
**Request Format**: `multipart/form-data`

```bash
curl -X POST http://localhost:8086/api/v1/categories \
-H "Authorization: Bearer your_admin_token" \
-F "name=Tunnel and Underground" \
-F "slug=tunnel-underground" \
-F "image=@/path/to/category_icon.png"
```

---

## 🏗️ Project Portfolio Management

### 3. List Projects
Retrieves a paginated list of projects with multi-criteria filtering.

**Endpoint**: `GET /reference-projects`

**Query Parameters**:
- `page`: Page number (default: 1)
- `limit`: Items per page (default: 10)
- `status`: Filter by status (`ongoing`, `completed`, `upcoming`)
- `featured`: Filter by featured flag (`true`, `false`)
- `categoryId`: Filter by specific category ID
- `search`: Search in project name, location, or description

```bash
curl -X GET "http://localhost:8086/api/v1/reference-projects?page=1&limit=10&categoryId=1&status=completed"
```

### 4. Get Project Details
Retrieves full details for a project using its ID.

**Endpoint**: `GET /reference-projects/{id}`

```bash
curl -X GET http://localhost:8086/api/v1/reference-projects/5
```

### 5. Create Project (Admin Only)
Adds a new reference project with multiple image uploads and product links.

**Endpoint**: `POST /reference-projects`
**Request Format**: `multipart/form-data`

| Field | Type | Description |
| :--- | :--- | :--- |
| `ProjectName` | String | **Required**. Name of the project. |
| `CategoryId` | Integer | **Required**. ID of an existing category. |
| `Location` | String | Project geographic location. |
| `OwnerName` | String | Name of the project owner. |
| `Contractor` | String | Main contractor name. |
| `EngineerName` | String | Lead engineer or firm. |
| `ClientName` | String | Client organization. |
| `ShortDescription` | String | Brief summary. |
| `DetailsDescription`| String (HTML) | Rich text/HTML content for details page. |
| `Status` | String | `ongoing`, `completed`, or `upcoming`. |
| `StartDate` | Date | Project start date (ISO 8601). |
| `CompletionDate` | Date | Project completion date (ISO 8601). |
| `Featured` | Boolean | Highlight on the landing page. |
| `HeroImage` | File | Single main display image. |
| `GalleryImages` | File[] | Multiple images for the project gallery. |
| `DetailImages` | File[] | Multiple images specifically for the details section. |
| `ProductIdsJson` | String (JSON) | Array of existing product IDs, e.g., `[1, 4, 7]`. |
| `ProjectOverviewJson`| String (JSON) | Structured metadata, e.g., `{"Scope": "Full Build"}`. |

```bash
curl -X POST http://localhost:8086/api/v1/reference-projects \
-H "Authorization: Bearer your_admin_token" \
-F "ProjectName=Karnaphuli River Tunnel" \
-F "CategoryId=3" \
-F "Location=Patenga, Chittagong" \
-F "OwnerName=Bangladesh Bridge Authority" \
-F "Contractor=CCCC" \
-F "Status=ongoing" \
-F "HeroImage=@hero.jpg" \
-F "GalleryImages=@g1.jpg" \
-F "GalleryImages=@g2.jpg" \
-F "DetailImages=@d1.jpg" \
-F "ProductIdsJson=[1, 4]" \
-F "ProjectOverviewJson={\"Length\": \"3.32km\", \"Type\": \"Shield Tunnel\"}"
```

### 6. Delete Project (Admin Only)
Removes a project and its associated images from the cloud storage.

**Endpoint**: `DELETE /reference-projects/{id}`

```bash
curl -X DELETE http://localhost:8086/api/v1/reference-projects/5 \
-H "Authorization: Bearer your_admin_token"
```

---

## 🗃️ Response Data Structures

### Project Response Object
```json
{
  "id": 5,
  "projectName": "Karnaphuli River Tunnel",
  "slug": "karnaphuli-river-tunnel-63859238491",
  "categoryId": 3,
  "categoryName": "Tunnel",
  "location": "Patenga, Chittagong",
  "heroImageUrl": "https://res.cloudinary.com/...",
  "galleryImages": [
    "https://res.cloudinary.com/g1.jpg",
    "https://res.cloudinary.com/g2.jpg"
  ],
  "detailImages": [
    "https://res.cloudinary.com/d1.jpg"
  ],
  "productsUsed": [
    { "id": 1, "name": "Denka Power CSA T-R" },
    { "id": 4, "name": "Denka Power CSA T-S" }
  ],
  "projectOverview": {
    "Length": "3.32km",
    "Type": "Shield Tunnel"
  },
  "status": "ongoing",
  "createdAt": "2026-02-15T07:00:00Z"
}
```

---

## 🛡️ Validation Rules
1. **Category Existence**: `CategoryId` must match an existing record in the `ProjectCategories` table.
2. **Product Verification**: All IDs in `ProductIdsJson` must exist in the `Products` table.
3. **Gallery Requirement**: At least one `GalleryImages` file is required for project creation.
4. **Image Types**: Only `.jpg`, `.jpeg`, `.png`, and `.webp` formats are permitted.
