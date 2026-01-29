# About Us Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the About Us Microservice.

**Base URL**: `http://localhost:8082/api/v1/about-us` (via Gateway) or `http://localhost:8089/api/v1/about-us` (Direct)

---

## ℹ️ Public Content (Read-Only)

### 1. Get All About Us Data
Retrieves the complete company profile including all sections (About, Mission, Vision, Core Values, Team, etc.).

**Endpoint**: `GET /`

```bash
curl -X GET http://localhost:8082/api/v1/about-us
```

### 2. Get Specific Section
Retrieves data for a single section by its name.

**Endpoint**: `GET /sections/{sectionName}`

**Common Section Names**: `about_us`, `mission`, `vision`, `core_values`, `customer_solutions`, `business_principles`, `video`, `journey_milestones`, `team`, `quick_reference`.

```bash
curl -X GET http://localhost:8082/api/v1/about-us/sections/mission
```

---

## 🛠️ Content Management (Admin Only)

**Auth Required**: `Bearer <admin_token>`

### 3. Update Section Metadata
Updates the title, description, or assets (banners, videos, PDFs) for a specific section. This endpoint uses `multipart/form-data`.

**Endpoint**: `PATCH /sections/{sectionName}`

```bash
curl -X PATCH http://localhost:8082/api/v1/about-us/sections/about_us \
-H "Authorization: Bearer your_admin_token" \
-F "Title=Modernizing Infrastructure" \
-F "Description=We provide cutting-edge ERP solutions." \
-F "BannerImage=@/path/to/banner.jpg"
```

### 4. Add Item to Section
Adds a new entry (e.g., a new team member, a new milestone, or a new core value) to a specific section. Uses `multipart/form-data`.

**Endpoint**: `POST /sections/{sectionName}`

```bash
# Example: Adding a Team Member
curl -X POST http://localhost:8082/api/v1/about-us/sections/team \
-H "Authorization: Bearer your_admin_token" \
-F "Title=Jane Smith" \
-F "Designation=Chief Technology Officer" \
-F "ShortDescription=Expert in cloud architecture." \
-F "Photo=@/path/to/jane_photo.jpg" \
-F "OrderIndex=1" \
-F "SocialLinksJson={\"linkedin\": \"https://linkedin.com/in/janesmith\"}"
```

### 5. Delete Item from Section
Removes a specific item from a section.

**Endpoint**: `DELETE /sections/{sectionName}/items/{itemId}`

```bash
curl -X DELETE http://localhost:8082/api/v1/about-us/sections/team/items/550e8400-e29b-41d4-a716-446655440000 \
-H "Authorization: Bearer your_admin_token"
```

---

## 📊 Available Metadata Fields per Section

When using **Update Section Metadata**, different sections support different file fields:

| Section Name | Field Name | Description |
| :--- | :--- | :--- |
| `about_us` | `BannerImage` | Large header image. |
| `video` | `Thumbnail` | Video preview image. |
| `video` | `VideoUrl` | URL to the video (YouTube/Vimeo). |
| `quick_reference` | `CompanyProfilePdf` | Downloadable PDF. |
| `quick_reference` | `ProductBrochurePdf` | Downloadable PDF. |

---

## 🧑‍💻 Request DTO Reference (Forms)

Most write operations use `multipart/form-data`. Key fields for `AboutUsItemRequest`:
- `Title`: Name/Title of the item.
- `ShortDescription`: Content/Bio.
- `Date`: (Optional) For milestones.
- `Designation`: (Optional) For team members.
- `OrderIndex`: Numeric sort order.
- `Icon`: (Optional) File upload for values/milestones.
- `Image` / `Photo`: (Optional) File upload for team/milestones.
