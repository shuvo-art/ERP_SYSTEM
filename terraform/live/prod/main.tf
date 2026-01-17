provider "aws" {
  region = "us-east-1"
}

locals {
  environment = "prod"
  region      = "us-east-1"
}

module "vpc" {
  source = "../../modules/vpc"

  vpc_name        = "erp-vpc-prod"
  vpc_cidr        = "10.2.0.0/16"
  azs             = ["us-east-1a", "us-east-1b", "us-east-1c"]
  public_subnets  = ["10.2.1.0/24", "10.2.2.0/24", "10.2.5.0/24"]
  private_subnets = ["10.2.3.0/24", "10.2.4.0/24", "10.2.6.0/24"]
  environment     = local.environment
  cluster_name    = "erp-eks-prod"
}

module "eks" {
  source = "../../modules/eks"

  cluster_name    = "erp-eks-prod"
  cluster_version = "1.30"
  vpc_id          = module.vpc.vpc_id
  subnet_ids      = module.vpc.private_subnets
  environment     = local.environment

  node_groups = {
    general = {
      name         = "general-nodes"
      desired_size = 3
      min_size     = 2
      max_size     = 5

      instance_types = ["m5.large"]
      capacity_type  = "ON_DEMAND"
    }
  }
}

module "addons" {
  source = "../../modules/addons"

  cluster_name = module.eks.cluster_name
  tags = {
    Environment = local.environment
  }
}

output "cluster_endpoint" { value = module.eks.cluster_endpoint }
output "cluster_name" { value = module.eks.cluster_name }
