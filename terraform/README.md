# infrastructure-as-code (Terraform)

This directory contains the Terraform configuration for managing the cloud infrastructure of the ERP System on AWS. It follows a modular, multi-environment architecture designed for scalability and maintainability.

## 🏗 Directory Structure

```text
terraform/
├── live/                   # Environment implementations
│   ├── common/             # Global resources (ECR, IAM) shared across envs
│   ├── dev/                # Development environment (Spot instances, Single AZ)
│   ├── staging/            # Staging environment (Spot instances, Multi AZ)
│   └── prod/               # Production environment (On-Demand, Multi AZ, HA)
├── modules/                # Reusable Infrastructure Modules
│   ├── eks/                # EKS Cluster, Node Groups, OIDC
│   ├── vpc/                # VPC, Subnets, Internet Gateways, NAT
│   ├── addons/             # K8s Addons (CoreDNS, VPC-CNI, EBS CSI)
│   └── iam/                # IAM Roles and Policies
├── policies/               # JSON Policy Documents (Audit friendly)
└── scripts/                # Utility scripts (Validation, Hooks)
```

## 🚀 Environments

| Environment | VPC CIDR | Cluster Spec | Node Strategy | Purpose |
|-------------|----------|--------------|---------------|---------|
| **Dev** | `10.0.0.0/16` | v1.30, Single Zone | Spot (t3.medium) | Cost-effective development and testing |
| **Staging** | `10.1.0.0/16` | v1.30, Multi Zone | Spot (t3.medium) | Pre-production testing, mirrors Prod topology |
| **Prod** | `10.2.0.0/16` | v1.30, Multi Zone | On-Demand (m5.large) | Critical workloads, High Availability, Stability |

## 🛠 Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) >= 1.0
- [AWS CLI](https://aws.amazon.com/cli/) configured with appropriate credentials.

## ⚡ Usage

### 1. Initialize
Navigate to the desired environment and download provider plugins.

```bash
cd live/dev
terraform init
```

### 2. Plan
Preview the changes Terraform will make.

```bash
terraform plan
```

### 3. Apply
Create or update the infrastructure.

```bash
terraform apply
```

### 4. Validate (CI/CD)
Run the validation script to ensure code quality across all environments.

```bash
./scripts/validate_all.sh
```

## 📦 State Management

Each environment has a `versions.tf` file configured for remote state storage (S3 + DynamoDB locking).
*Note: The backend configuration is currently commented out for local testing. Uncomment and configure the bucket name in `versions.tf` before team usage.*

## 🔧 Modules

- **VPC Module**: Creates a secure network topology with public/private subnets and NAT gateways.
- **EKS Module**: Provisions the Control Plane and Managed Node Groups. Supports flexible node selections (Spot/On-Demand).
- **Addons**: Manages operational components like `vpc-cni`, `coredns`, and `ebs-csi-driver`.
