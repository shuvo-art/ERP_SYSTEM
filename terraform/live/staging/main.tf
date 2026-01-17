provider "aws" {
  region = "us-east-1"
}

locals {
  environment = "staging"
  region      = "us-east-1"
}

module "vpc" {
  source = "../../modules/vpc"

  vpc_name        = "erp-vpc-staging"
  vpc_cidr        = "10.1.0.0/16"
  azs             = ["us-east-1a", "us-east-1b"]
  public_subnets  = ["10.1.1.0/24", "10.1.2.0/24"]
  private_subnets = ["10.1.3.0/24", "10.1.4.0/24"]
  environment     = local.environment
  cluster_name    = "erp-eks-staging"
}

module "eks" {
  source = "../../modules/eks"

  cluster_name    = "erp-eks-staging"
  cluster_version = "1.30"
  vpc_id          = module.vpc.vpc_id
  subnet_ids      = module.vpc.private_subnets
  environment     = local.environment

  node_groups = {
    general = {
      desired_size = 2
      min_size     = 1
      max_size     = 3

      instance_types = ["t3.medium"]
      capacity_type  = "SPOT"
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
