# ERP System Documentation Index

Welcome to the official documentation for the ERP System. This project provides a robust microservices-based platform for enterprise resource planning.

## 📁 Document List

1.  **[Architecture Overview](architecture.md)**
    *   System Design
    *   Microservices list
    *   Shared Kernel details
    *   Clean Architecture layers

2.  **[Development Guide](development.md)**
    *   Local setup instructions
    *   Prerequisites
    *   Docker Compose usage
    *   Coding standards

3.  **[API Guide](api-guide.md)**
    *   Authentication protocol (JWT/OTP)
    *   Standard Response format
    *   HTTP status codes
    *   Core endpoint summary

4.  **Microservice Specific Docs**
    *   🔐 **[Auth API Details](api/auth.md)**: Detailed specifications and `curl` commands.
    *   📦 **[Product API Details](api/products.md)**: Catalog and asset management.
    *   ℹ️ **[About Us API Details](api/about-us.md)**: Company profile and section management.
    *   📧 **[Contact API Details](api/contact.md)**: Distributors and enquiries.
    *   💼 **[Job API Details](api/jobs.md)**: Recruitment and applications.
    *   🤝 **[Partner API Details](api/partners.md)**: Partner management.

5.  **[Deployment Guide](deployment.md)**
    *   Terraform (Infrastructure as Code)
    *   Ansible (Configuration Management)
    *   Kubernetes (Helm Charts)
    *   CI/CD Pipelines (Jenkins)

## 🏗️ Technical Stack Summary

*   **Backend**: .NET 8, C#, Dapper
*   **Database**: SQL Server 2022
*   **Infrastructure**: AWS (EKS, ECR), Terraform
*   **DevOps**: Jenkins, Ansible, Helm
*   **Gateway**: Nginx
*   **Authentication**: JWT, BCrypt, OTP via SMTP

## 🤝 Community

*   [Contributing Guidelines](../CONTRIBUTING.md)
*   [Code of Conduct](../CODE_OF_CONDUCT.md)
*   [Changelog](../CHANGELOG.md)
