# Job Microservice API Documentation

This document provides detailed API specifications and `curl` examples for the Job Microservice, covering Recruitment and Job Applications.

**Base URL**: `http://localhost:8082` (via Gateway)

---

## 💼 Job Postings

**Base URL**: `/api/v1/jobs`

### 1. List Jobs
Retrieves all job openings with filtering.

**Endpoint**: `GET /`

```bash
curl -X GET "http://localhost:8082/api/v1/jobs?status=active&location=Remote"
```

### 2. Create Job (Admin)
Creates a new job posting.

**Endpoint**: `POST /`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X POST http://localhost:8082/api/v1/jobs \
-H "Authorization: Bearer your_admin_token" \
-F "Title=Senior .NET Developer" \
-F "Department=Engineering" \
-F "Location=Dhaka, Bangladesh" \
-F "JobType=Full-time" \
-F "ContractType=Permanent" \
-F "Description=We are looking for an expert in Microservices..." \
-F "ResponsibilitiesJson=[\"Develop scalable APIs\", \"Review code\", \"Mentor juniors\"]" \
-F "QualificationsJson=[\"5+ years experience\", \"B.Sc in CS\"]" \
-F "Status=active" \
-F "BannerImage=@/path/to/job_banner.jpg"
```

---

## 📝 Job Applications

### 3. Apply for Job
Submit an application for a specific job.

**Endpoint**: `POST /api/v1/jobs/{jobId}/apply`

```bash
curl -X POST http://localhost:8082/api/v1/jobs/550e8400-e29b-41d4-a716-446655440000/apply \
-F "FirstName=Jane" \
-F "LastName=Doe" \
-F "Email=jane.doe@example.com" \
-F "Phone=\"+8801700000000\"" \
-F "CoverMessage=I am very excited to apply for this role." \
-F "Resume=@/path/to/resume.pdf" \
-F "ExperienceJson=[{\"company\": \"Tech Soft\", \"years\": 3}]"
```

### 4. List Applications (Admin)
Retrieves submitted applications.

**Endpoint**: `GET /api/v1/applications`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X GET http://localhost:8082/api/v1/applications \
-H "Authorization: Bearer your_admin_token"
```

### 5. Update Application Status (Admin)
Moves an application through the recruitment funnel.

**Endpoint**: `PATCH /api/v1/applications/{id}`
**Auth Required**: `Bearer <admin_token>`

```bash
curl -X PATCH http://localhost:8082/api/v1/applications/acb123... \
-H "Authorization: Bearer your_admin_token" \
-H "Content-Type: application/json" \
-d '{
  "status": "interview",
  "notes": "Qualified candidate, scheduled for technical interview."
}'
```
