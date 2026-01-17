# ERP System - Helm Charts

This repository contains the Helm charts for deploying the microservices-based ERP System to Kubernetes. It uses a "Umbrella Chart" pattern where `erp-system` is the parent chart that manages all microservices as dependencies.

## 📂 Chart Structure

```text
k8s/
└── erp-system/              # The Umbrella (Parent) Chart
    ├── Chart.yaml           # Metadata and Dependencies
    ├── values.yaml          # Default Configuration (Base)
    ├── values-dev.yaml      # Development Overrides
    ├── values-prod.yaml     # Production Overrides
    └── charts/              # Subcharts (Microservices)
        ├── auth-api/        # Authentication Service
        ├── product-api/     # Product Management Service
        └── frontend/        # Web Interface
```

## 📋 Prerequisites

- [Helm 3](https://helm.sh/docs/intro/install/)
- A running Kubernetes cluster (EKS, Minikube, or Docker Desktop)
- `kubectl` configured to talk to your cluster.

## 🚀 Deployment

### 1. Development (Locally or Dev Cluster)
Use `values-dev.yaml` for development specific settings (e.g., NodePort, simple replicas).

```bash
helm upgrade --install erp-system ./erp-system \
  -f ./erp-system/values-dev.yaml \
  --namespace erp-dev \
  --create-namespace
```

### 2. Production
Use `values-prod.yaml` for production settings (e.g., Ingress enabled, Resource Limits, Horizontal Autoscaling).

```bash
helm upgrade --install erp-system ./erp-system \
  -f ./erp-system/values-prod.yaml \
  --namespace erp-prod \
  --create-namespace
```

### 3. Uninstall
To remove the application:

```bash
helm uninstall erp-system --namespace erp-dev
```

## ⚙️ Configuration

The `values.yaml` file contains the default values. Key sections include:

| Key | Description | Default |
|-----|-------------|---------|
| `global.env` | Environment tag | `dev` |
| `auth-api.replicaCount` | Replicas for Auth Service | `1` |
| `product-api.image.tag` | Docker image tag | `latest` |
| `ingress.enabled` | Enable Ingress Controller | `false` |

## 🔄 Updating Subcharts

If you modify a subchart (e.g., inside `k8s/erp-system/charts/auth-api`), you need to update the dependencies:

```bash
cd k8s/erp-system
helm dependency update
```
