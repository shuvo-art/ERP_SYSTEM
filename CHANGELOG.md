# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Core documentation structure following industry standards.
- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `CHANGELOG.md`.
- `docs/` folder with architecture, api-guide, development, and deployment documentation.

### Changed
- Standardized JWT claim names to use `firstName` and `lastName` literals.
- Standardized API endpoints to use lowercase kebab-case across all microservices.

## [1.0.0] - 2026-01-29

### Added
- Initial project structure with multiple microservices:
  - Auth API
  - Product API
  - Job API
  - Target Market API
  - Partner API
  - Reference Project API
  - Contact API
  - About Us API
- Infrastructure setup with Terraform (AWS EKS).
- Configuration management with Ansible.
- Kubernetes manifests and Helm charts.
- Docker Compose setup for Local Development and Production.
- Nginx Gateway configuration.
- Shared Kernel for common utilities.
