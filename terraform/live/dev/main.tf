provider "aws" {
  region = var.region
}

locals {
  cluster_name = "erp-eks-dev"
  environment  = "dev"
}

module "vpc" {
  source = "../../modules/vpc"

  vpc_name        = "erp-vpc-dev"
  vpc_cidr        = "10.0.0.0/16"
  azs             = ["us-east-1a", "us-east-1b"]
  public_subnets  = ["10.0.1.0/24", "10.0.2.0/24"]
  private_subnets = ["10.0.3.0/24", "10.0.4.0/24"]
  environment     = local.environment
  cluster_name    = local.cluster_name
}

module "eks" {
  source = "../../modules/eks"

  cluster_name    = local.cluster_name
  cluster_version = "1.31"
  vpc_id          = module.vpc.vpc_id
  subnet_ids      = module.vpc.private_subnets
  environment     = local.environment
}

output "cluster_endpoint" {
  value = module.eks.cluster_endpoint
}

output "cluster_name" {
  value = module.eks.cluster_name
}
