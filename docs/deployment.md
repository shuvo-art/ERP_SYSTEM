# Deployment Guide

The ERP System supports multiple deployment strategies depending on the environment.

## 🏗️ Infrastructure as Code (Terraform)

Infrastructure provisioning for AWS is managed via Terraform in the `/terraform` directory.

### Structure
- `modules/`: Reusable resources (VPC, EKS, RDS, S3).
- `live/`: Environment-specific configurations (`dev`, `staging`, `prod`).

### Deployment Steps
1. Navigate to the environment: `cd terraform/live/prod`
2. Initialize: `terraform init`
3. Plan: `terraform plan`
4. Apply: `terraform apply`

## ⚙️ Configuration Management (Ansible)

Ansible is used for OS-level hardening and installing dependencies on base servers.

### Key Playbooks
- `provision.yml`: Installs Docker, Nginx, and system utilities.
- `maintenance.yml`: Handles system updates.

### Execution
```bash
ansible-playbook -i inventory/prod/hosts.ini playbooks/provision.yml
```

## ☸️ Kubernetes (Orchestration)

Production services are deployed to AWS EKS using Helm charts located in `/k8s`.

### Deployment Steps
1. Ensure `kubectl` is configured for the target cluster.
2. Deploy using Helm:
```bash
helm upgrade --install erp-system ./k8s/erp-system -f ./k8s/values-prod.yaml
```

## 🔄 CI/CD Pipeline (Jenkins)

The CI/CD pipeline is defined in the `Jenkinsfile` at the root of the project.

### Pipeline Stages
1. **Checkout**: Pull latest code.
2. **Build**: `dotnet build` and `docker build`.
3. **Push**: Push images to AWS ECR.
4. **Provision**: Run Terraform if infra changes are detected.
5. **Deploy**: Update K8s deployment using Helm.

## 🐳 Docker Compose (Alternative)

For lightweight production or staging environments, `docker-compose.prod.yml` can be used directly on a VM with Docker installed.

```bash
docker compose -f docker-compose.prod.yml up -d
```
